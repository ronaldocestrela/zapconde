using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.Services;

namespace Modules.Financial.Endpoints;

public static class DunningEndpoints
{
    public static void MapDunningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/dunning")
            .WithTags("Financial Dunning");

        group.MapGet("/config", async (
            int condoId,
            IReguaInadimplenciaAppService service,
            CancellationToken ct) =>
        {
            var result = await service.ObterConfiguracaoReguaAsync(condoId, ct);
            return Results.Ok(result);
        })
        .WithName("GetDunningConfig")
        .WithSummary("Obtém a configuração das etapas da régua de cobrança por condomínio.");

        group.MapPut("/config", async (
            int condoId,
            List<SalvarEtapaReguaDto> etapas,
            IReguaInadimplenciaAppService service,
            CancellationToken ct) =>
        {
            var result = await service.SalvarConfiguracaoReguaAsync(condoId, etapas, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("SaveDunningConfig")
        .WithSummary("Salva as etapas da régua de inadimplência do condomínio.");

        group.MapPost("/process", async (
            int condoId,
            IReguaInadimplenciaAppService service,
            CancellationToken ct) =>
        {
            var result = await service.ProcessarReguaCobrancaAsync(condoId, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ProcessDunningEngine")
        .WithSummary("Executa o motor de régua de cobrança para faturas vencidas em atraso.");

        group.MapGet("/dashboard", async (
            int condoId,
            IReguaInadimplenciaAppService service,
            CancellationToken ct) =>
        {
            var result = await service.ObterDashboardInadimplenciaAsync(condoId, ct);
            return Results.Ok(result);
        })
        .WithName("GetDunningDashboard")
        .WithSummary("Retorna os dados consolidados do painel de inadimplência e Aging List.");
    }
}
