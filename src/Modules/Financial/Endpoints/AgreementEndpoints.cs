using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Endpoints;

public static class AgreementEndpoints
{
    public static void MapAgreementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/agreements")
            .WithTags("Financial Agreements");

        group.MapPost("/simulate", async (
            SimulacaoAcordoRequest request,
            IAcordoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.SimularAcordoAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("SimulateAgreement")
        .WithSummary("Simula propostas de acordo de renegociação de débitos.");

        group.MapPost("/", async (
            CriarAcordoRequest request,
            IAcordoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.CriarAcordoAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/financial/agreements/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateAgreement")
        .WithSummary("Cria e efetiva um novo acordo de renegociação.");

        group.MapGet("/", async (
            int condoId,
            int? unidadeId,
            StatusAcordo? status,
            IAcordoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ObterAcordosPorCondominioAsync(condoId, unidadeId, status, ct);
            return Results.Ok(result);
        })
        .WithName("GetAgreements")
        .WithSummary("Lista acordos de renegociação do condomínio.");

        group.MapGet("/{id:int}", async (
            int id,
            IAcordoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ObterDetalhesAcordoAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetAgreementById")
        .WithSummary("Obtém detalhes e parcelas de um acordo específico.");

        group.MapPost("/{id:int}/cancel", async (
            int id,
            string motivo,
            IAcordoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.CancelarAcordoAsync(id, motivo, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("CancelAgreement")
        .WithSummary("Cancela um acordo ativo e reativa as faturas originais.");

        group.MapPost("/{id:int}/pay-installment", async (
            int id,
            int numeroParcela,
            DateTime dataPagamento,
            IAcordoApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.RegistrarPagamentoParcelaAsync(id, numeroParcela, dataPagamento, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("PayAgreementInstallment")
        .WithSummary("Registra o pagamento de uma parcela do acordo.");
    }
}
