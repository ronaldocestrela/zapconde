using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Renova access token usando refresh token válido.
/// </summary>
public sealed class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, Result<AuthTokenDto>>
{
    private readonly IAuthService _authService;

    public RefreshTokenEndpoint(IAuthService authService) => _authService = authService;

    public override void Configure()
    {
        Post("/api/auth/refresh");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Renovar token";
            s.Description = "Troca refresh token por novo par access/refresh.";
        });
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var result = await _authService.RefreshAsync(req.RefreshToken, ct);
        var status = result.IsSuccess ? 200 : 401;
        await SendAsync(result, status, ct);
    }
}
