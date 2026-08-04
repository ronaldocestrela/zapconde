using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _dbContext;
    private readonly IIdentityTokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IdentityDbContext dbContext,
        IIdentityTokenService tokenService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenDto>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Result<AuthTokenDto>.ValidationFailure(["E-mail e senha são obrigatórios."]);
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return Result<AuthTokenDto>.Failure("Credenciais inválidas. Verifique e-mail e senha.");
        }

        if (!user.IsActive)
        {
            return Result<AuthTokenDto>.Failure("Usuário bloqueado. Entre em contato com o síndico.");
        }

        var memberships = await _dbContext.UserCondoMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.UserId == user.Id && m.IsActive && m.IsTenantActive)
            .ToListAsync(ct);

        if (memberships.Count == 0)
        {
            return Result<AuthTokenDto>.Failure("Tenant inativo ou sem perfil disponível.");
        }

        var (accessToken, refreshToken, expiresAt) =
            await _tokenService.CreatePreContextTokensAsync(user, memberships, ct);

        var profiles = memberships
            .Select(m => new AuthProfileDto(m.Id, m.TenantId, m.CondoId, m.Role, m.Role))
            .ToList();

        return Result<AuthTokenDto>.Success(
            new AuthTokenDto(accessToken, refreshToken, expiresAt, profiles),
            "Login realizado com sucesso.");
    }

    public async Task<Result<AuthContextTokenDto>> SelectProfileAsync(
        Guid userId,
        Guid membershipId,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result<AuthContextTokenDto>.Failure("Usuário não encontrado ou bloqueado.");
        }

        var membership = await _dbContext.UserCondoMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.UserId == userId && m.IsActive, ct);

        if (membership is null)
        {
            return Result<AuthContextTokenDto>.Failure("Perfil não encontrado.");
        }

        if (!membership.IsTenantActive)
        {
            return Result<AuthContextTokenDto>.Failure("Tenant inativo.");
        }

        var (accessToken, refreshToken, expiresAt) =
            await _tokenService.CreateContextTokensAsync(user, membership, ct);

        return Result<AuthContextTokenDto>.Success(
            new AuthContextTokenDto(
                accessToken,
                refreshToken,
                expiresAt,
                membership.TenantId,
                membership.CondoId,
                user.Id.ToString(),
                membership.Role),
            "Perfil selecionado com sucesso.");
    }

    public async Task<Result<ForgotPasswordResultDto>> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return Result<ForgotPasswordResultDto>.ValidationFailure(["Informe um e-mail válido."]);
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is not null)
        {
            _ = await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        return Result<ForgotPasswordResultDto>.Success(
            new ForgotPasswordResultDto("E-mail enviado! Verifique sua caixa de entrada para redefinir sua senha com segurança."),
            "Solicitação processada.");
    }

    public async Task<Result<AuthTokenDto>> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<AuthTokenDto>.ValidationFailure(["Refresh token é obrigatório."]);
        }

        try
        {
            var (accessToken, newRefreshToken, expiresAt) =
                await _tokenService.RefreshTokensAsync(refreshToken, ct);

            return Result<AuthTokenDto>.Success(
                new AuthTokenDto(accessToken, newRefreshToken, expiresAt, []),
                "Token renovado com sucesso.");
        }
        catch (Exception)
        {
            return Result<AuthTokenDto>.Failure("Refresh token inválido ou expirado.");
        }
    }
}
