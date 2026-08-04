using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Autentica usuário por e-mail e senha, retornando tokens JWT e perfis disponíveis.
/// </summary>
public sealed class LoginEndpoint : Endpoint<LoginRequest, Result<AuthTokenDto>>
{
    private readonly IAuthService _authService;

    public LoginEndpoint(IAuthService authService) => _authService = authService;

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Login com e-mail e senha";
            s.Description = "Valida credenciais e retorna access/refresh token com lista de perfis (tenant/condo/role).";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(req.Email, req.Password, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
            return;
        }

        var status = result.Message.Contains("bloqueado", StringComparison.OrdinalIgnoreCase) ||
                     result.Message.Contains("Tenant inativo", StringComparison.OrdinalIgnoreCase)
            ? 403
            : result.Message.Contains("Credenciais", StringComparison.OrdinalIgnoreCase)
                ? 401
                : 400;

        await SendAsync(result, status, ct);
    }
}
