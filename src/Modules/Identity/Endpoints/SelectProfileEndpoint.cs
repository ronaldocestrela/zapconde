using System.Security.Claims;
using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;

namespace Modules.Identity.Endpoints;

public sealed class SelectProfileRequest
{
    public Guid MembershipId { get; set; }
}

/// <summary>
/// Seleciona perfil (tenant/condo/role) e emite JWT contextual completo.
/// </summary>
public sealed class SelectProfileEndpoint : Endpoint<SelectProfileRequest, Result<AuthContextTokenDto>>
{
    private readonly IAuthService _authService;

    public SelectProfileEndpoint(IAuthService authService) => _authService = authService;

    public override void Configure()
    {
        Post("/api/auth/select-profile");
        Summary(s =>
        {
            s.Summary = "Selecionar perfil de acesso";
            s.Description = "Emite JWT com claims TenantId, CondoId, UserId e Role.";
        });
    }

    public override async Task HandleAsync(SelectProfileRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(SmartCondoClaimTypes.UserId)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await SendAsync(Result<AuthContextTokenDto>.Failure("Usuário não autenticado."), 401, ct);
            return;
        }

        var result = await _authService.SelectProfileAsync(userId, req.MembershipId, ct);
        var status = result.IsSuccess ? 200 : result.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase) ? 404 : 403;
        await SendAsync(result, status, ct);
    }
}
