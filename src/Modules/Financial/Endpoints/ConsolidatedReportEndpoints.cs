using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Financial.Application.Services;

namespace Modules.Financial.Endpoints;

public static class ConsolidatedReportEndpoints
{
    public static void MapConsolidatedReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/reports")
            .WithTags("Financial Consolidated Reports");

        group.MapGet("/multi-condo-summary", async (
            IRelatorioConsolidadoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ObterRelatorioConsolidadoAsync(ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("GetMultiCondoSummaryReport")
        .WithSummary("Retorna o relatório financeiro consolidado multicondomínio da administradora.");
    }
}
