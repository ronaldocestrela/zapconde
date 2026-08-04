using System.Security.Claims;
using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Lista perfis (memberships) disponíveis para troca de contexto tenant/condomínio.
/// </summary>
public sealed class GetProfilesEndpoint : EndpointWithoutRequest<Result<IReadOnlyList<AuthProfileDto>>>
{
    private readonly IAuthService _authService;

    public GetProfilesEndpoint(IAuthService authService) => _authService = authService;

    public override void Configure()
    {
        Get("/api/auth/profiles");
        Summary(s =>
        {
            s.Summary = "Listar perfis disponíveis";
            s.Description = "Retorna memberships ativas do usuário autenticado para o seletor de contexto.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(SmartCondoClaimTypes.UserId)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await SendAsync(Result<IReadOnlyList<AuthProfileDto>>.Failure("Usuário não autenticado."), 401, ct);
            return;
        }

        var result = await _authService.GetProfilesAsync(userId, ct);
        await SendAsync(result, result.IsSuccess ? 200 : 403, ct);
    }
}
