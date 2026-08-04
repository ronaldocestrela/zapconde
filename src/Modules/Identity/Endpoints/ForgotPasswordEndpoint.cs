using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Solicita recuperação de senha com resposta genérica anti-enumeration.
/// </summary>
public sealed class ForgotPasswordEndpoint : Endpoint<ForgotPasswordRequest, Result<ForgotPasswordResultDto>>
{
    private readonly IAuthService _authService;

    public ForgotPasswordEndpoint(IAuthService authService) => _authService = authService;

    public override void Configure()
    {
        Post("/api/auth/forgot-password");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Recuperação de senha";
            s.Description = "Envia instruções de redefinição de senha (mensagem genérica de sucesso).";
        });
    }

    public override async Task HandleAsync(ForgotPasswordRequest req, CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(req.Email, ct);
        var status = result.IsSuccess ? 200 : 400;
        await SendAsync(result, status, ct);
    }
}
