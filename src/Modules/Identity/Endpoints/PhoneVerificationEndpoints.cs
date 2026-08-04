using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Solicita o primeiro código de verificação do celular do morador.
/// </summary>
[Authorize]
public sealed class RequestPhoneVerificationEndpoint
    : Endpoint<RequestPhoneVerificationDto, Result<PhoneVerificationStatusDto>>
{
    private readonly IPhoneVerificationService _service;

    public RequestPhoneVerificationEndpoint(IPhoneVerificationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/residents/{moradorId}/phone/request-code");
        Summary(s => s.Summary = "Enviar código de verificação via WhatsApp");
    }

    public override async Task HandleAsync(RequestPhoneVerificationDto req, CancellationToken ct)
    {
        var result = await _service.RequestCodeAsync(req.MoradorId, req.PhoneNumber, false, ct);
        await SendAsync(result, result.IsSuccess ? 200 : PhoneEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Confirma o celular com o código de seis dígitos.
/// </summary>
[Authorize]
public sealed class VerifyPhoneEndpoint : Endpoint<VerifyPhoneDto, Result<PhoneVerificationStatusDto>>
{
    private readonly IPhoneVerificationService _service;

    public VerifyPhoneEndpoint(IPhoneVerificationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/residents/{moradorId}/phone/verify");
        Summary(s => s.Summary = "Validar código recebido pelo WhatsApp");
    }

    public override async Task HandleAsync(VerifyPhoneDto req, CancellationToken ct)
    {
        var result = await _service.VerifyAsync(req.MoradorId, req.Code, ct);
        await SendAsync(result, result.IsSuccess ? 200 : PhoneEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Reenvia o código respeitando cooldown e limite de tentativas.
/// </summary>
[Authorize]
public sealed class ResendPhoneVerificationEndpoint
    : EndpointWithoutRequest<Result<PhoneVerificationStatusDto>>
{
    private readonly IPhoneVerificationService _service;

    public ResendPhoneVerificationEndpoint(IPhoneVerificationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/residents/{moradorId}/phone/resend");
        Summary(s => s.Summary = "Reenviar código de verificação via WhatsApp");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var moradorId = Route<int>("moradorId");
        var status = await _service.GetStatusAsync(moradorId, ct);
        if (!status.IsSuccess || string.IsNullOrWhiteSpace(status.Data?.PhoneNumber))
        {
            await SendAsync(status, PhoneEndpointStatus.Map(status), ct);
            return;
        }

        var result = await _service.RequestCodeAsync(moradorId, status.Data.PhoneNumber, true, ct);
        await SendAsync(result, result.IsSuccess ? 200 : PhoneEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Consulta o estado atual da verificação do celular.
/// </summary>
[Authorize]
public sealed class GetPhoneVerificationStatusEndpoint
    : EndpointWithoutRequest<Result<PhoneVerificationStatusDto>>
{
    private readonly IPhoneVerificationService _service;

    public GetPhoneVerificationStatusEndpoint(IPhoneVerificationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/residents/{moradorId}/phone/status");
        Summary(s => s.Summary = "Consultar status da verificação do celular");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetStatusAsync(Route<int>("moradorId"), ct);
        await SendAsync(result, result.IsSuccess ? 200 : PhoneEndpointStatus.Map(result), ct);
    }
}

internal static class PhoneEndpointStatus
{
    public static int Map(Result result)
    {
        if (result.Errors.Any())
        {
            return 422;
        }

        if (result.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
        {
            return 404;
        }

        if (result.Message.Contains("vinculado", StringComparison.OrdinalIgnoreCase) ||
            result.Message.Contains("Reenvio", StringComparison.OrdinalIgnoreCase) ||
            result.Message.Contains("Limite", StringComparison.OrdinalIgnoreCase))
        {
            return 409;
        }

        return 500;
    }
}
