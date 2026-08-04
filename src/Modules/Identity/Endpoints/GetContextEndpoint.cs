using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FastEndpoints;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Retorna o contexto de tenant/condomínio resolvido pelo middleware na requisição atual.
/// </summary>
public sealed class GetContextEndpoint : EndpointWithoutRequest<Result<TenantContextDto>>
{
    private readonly ICurrentTenantService _tenantService;

    public GetContextEndpoint(ICurrentTenantService tenantService) => _tenantService = tenantService;

    public override void Configure()
    {
        Get("/api/auth/context");
        Summary(s =>
        {
            s.Summary = "Contexto de tenant ativo";
            s.Description = "Expõe TenantId/CondoId injetados pelo middleware a partir do JWT ou header de webhook.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var dto = new TenantContextDto(
            _tenantService.TenantId,
            _tenantService.CondoId,
            _tenantService.IsResolved);

        await SendAsync(Result<TenantContextDto>.Success(dto), 200, ct);
    }
}
