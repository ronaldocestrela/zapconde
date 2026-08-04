using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FastEndpoints;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Endpoint de sonda para validar resolução de tenant via header em rotas de webhook.
/// </summary>
public sealed class WebhookContextProbeEndpoint : EndpointWithoutRequest<Result<TenantContextDto>>
{
    private readonly ICurrentTenantService _tenantService;

    public WebhookContextProbeEndpoint(ICurrentTenantService tenantService) => _tenantService = tenantService;

    public override void Configure()
    {
        Get("/api/webhooks/context-probe");
        AllowAnonymous();
        Summary(s => s.Summary = "Sonda de contexto tenant para webhooks (header X-Tenant-Id)");
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
