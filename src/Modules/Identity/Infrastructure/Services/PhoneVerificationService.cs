using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Caching;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public sealed class PhoneVerificationService(
    IdentityDbContext dbContext,
    ICacheService cache,
    IWhatsAppOtpSender sender,
    IHostEnvironment environment) : IPhoneVerificationService
{
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan RateWindow = TimeSpan.FromMinutes(15);
    private const int MaxSendsPerWindow = 5;

    public async Task<Result<PhoneVerificationStatusDto>> RequestCodeAsync(
        int moradorId,
        string phoneNumber,
        bool isResend,
        CancellationToken ct = default)
    {
        if (!PhoneNumberValidator.TryNormalizeBrazilianMobile(phoneNumber, out var normalized))
        {
            return Result<PhoneVerificationStatusDto>.ValidationFailure(
                "Número de celular brasileiro inválido.",
                ["Informe um celular com DDD no formato (00) 00000-0000."]);
        }

        var morador = await dbContext.Moradores.FirstOrDefaultAsync(x => x.Id == moradorId, ct);
        if (morador is null)
        {
            return Result<PhoneVerificationStatusDto>.Failure("Morador não encontrado.");
        }

        var conflict = await dbContext.Moradores.AnyAsync(x =>
            x.Id != moradorId &&
            x.TelefoneWhatsAppE164 == normalized &&
            x.PhoneVerificationStatus == PhoneVerificationStatus.Validado, ct);
        if (conflict)
        {
            return Result<PhoneVerificationStatusDto>.Failure("Número já vinculado a outro morador.");
        }

        var now = DateTime.UtcNow;
        var existing = await cache.GetAsync<OtpCacheEntry>(OtpKey(morador), ct);
        if (existing is not null && existing.ResendAvailableAtUtc > now)
        {
            var seconds = Math.Max(1, (int)Math.Ceiling((existing.ResendAvailableAtUtc - now).TotalSeconds));
            return Result<PhoneVerificationStatusDto>.Failure(
                $"Reenvio disponível em {seconds} segundos.");
        }

        var rateKey = RateKey(morador, normalized);
        var rate = await cache.GetAsync<RateLimitEntry>(rateKey, ct);
        if (rate is null || rate.WindowStartedAtUtc.Add(RateWindow) <= now)
        {
            rate = new RateLimitEntry(now, 0);
        }

        if (rate.Count >= MaxSendsPerWindow)
        {
            return Result<PhoneVerificationStatusDto>.Failure(
                "Limite de envios atingido. Aguarde 15 minutos antes de solicitar novo código.");
        }

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var otp = new OtpCacheEntry(
            normalized,
            Hash(code),
            now.Add(OtpTtl),
            now.Add(ResendCooldown));

        await cache.SetAsync(OtpKey(morador), otp, OtpTtl, ct);
        await cache.SetAsync(rateKey, rate with { Count = rate.Count + 1 }, RateWindow, ct);
        await sender.SendOtpAsync(normalized, code, ct);

        morador.IniciarVerificacaoTelefone(normalized, now);
        await dbContext.SaveChangesAsync(ct);

        var dto = ToDto(morador, (int)ResendCooldown.TotalSeconds);
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            dto.DebugCode = code;
        }

        return Result<PhoneVerificationStatusDto>.Success(
            dto,
            isResend ? "Código reenviado via WhatsApp." : "Código enviado via WhatsApp.");
    }

    public async Task<Result<PhoneVerificationStatusDto>> VerifyAsync(
        int moradorId,
        string code,
        CancellationToken ct = default)
    {
        if (code.Length != 6 || code.Any(c => !char.IsDigit(c)))
        {
            return Result<PhoneVerificationStatusDto>.ValidationFailure(
                "Código inválido.",
                ["Informe os seis dígitos recebidos pelo WhatsApp."]);
        }

        var morador = await dbContext.Moradores
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == moradorId, ct);
        if (morador is null)
        {
            return Result<PhoneVerificationStatusDto>.Failure("Morador não encontrado.");
        }

        var entry = await cache.GetAsync<OtpCacheEntry>(OtpKey(morador), ct);
        if (entry is null || entry.ExpiresAtUtc <= DateTime.UtcNow)
        {
            morador.MarcarCodigoExpirado();
            await dbContext.SaveChangesAsync(ct);
            return Result<PhoneVerificationStatusDto>.ValidationFailure(
                "Código expirado.",
                ["Solicite um novo código para continuar."]);
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(entry.CodeHash),
                Convert.FromHexString(Hash(code))))
        {
            return Result<PhoneVerificationStatusDto>.ValidationFailure(
                "Código inválido.",
                ["Confira o código recebido e tente novamente."]);
        }

        var verifiedAt = DateTime.UtcNow;
        morador.ConfirmarTelefone(verifiedAt);
        if (morador.User is not null)
        {
            morador.User.PhoneNumber = morador.TelefoneWhatsAppE164;
            morador.User.PhoneNumberConfirmed = true;
        }

        await dbContext.SaveChangesAsync(ct);
        await cache.RemoveAsync(OtpKey(morador), ct);

        return Result<PhoneVerificationStatusDto>.Success(
            ToDto(morador),
            "Número validado e vinculado com sucesso.");
    }

    public async Task<Result<PhoneVerificationStatusDto>> GetStatusAsync(
        int moradorId,
        CancellationToken ct = default)
    {
        var morador = await dbContext.Moradores.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == moradorId, ct);
        return morador is null
            ? Result<PhoneVerificationStatusDto>.Failure("Morador não encontrado.")
            : Result<PhoneVerificationStatusDto>.Success(ToDto(morador));
    }

    private static PhoneVerificationStatusDto ToDto(Morador morador, int? resendSeconds = null) => new()
    {
        MoradorId = morador.Id,
        PhoneNumber = morador.TelefoneWhatsAppE164,
        MaskedPhoneNumber = string.IsNullOrWhiteSpace(morador.TelefoneWhatsAppE164)
            ? null
            : PhoneNumberValidator.FormatBrazilianMobile(morador.TelefoneWhatsAppE164),
        Status = morador.PhoneVerificationStatus,
        RequestedAtUtc = morador.PhoneVerificationRequestedAtUtc,
        VerifiedAtUtc = morador.PhoneVerifiedAtUtc,
        ResendAvailableInSeconds = resendSeconds
    };

    private static string OtpKey(Morador morador) =>
        $"phone:otp:{morador.TenantId}:{morador.Id}";

    private static string RateKey(Morador morador, string phone) =>
        $"phone:otp:rate:{morador.TenantId}:{phone}";

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record OtpCacheEntry(
        string PhoneNumber,
        string CodeHash,
        DateTime ExpiresAtUtc,
        DateTime ResendAvailableAtUtc);

    private sealed record RateLimitEntry(DateTime WindowStartedAtUtc, int Count);
}
