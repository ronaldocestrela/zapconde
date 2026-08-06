using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;

namespace Modules.Financial.Endpoints;

public static class BankReconciliationEndpoints
{
    public static void MapBankReconciliationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/bank-reconciliation")
            .WithTags("Bank Reconciliation");

        group.MapPost("/accounts", async (
            CriarContaBancariaRequestDto request,
            IConciliacaoBancariaApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.CriarContaBancariaAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/financial/bank-reconciliation/accounts/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateBankAccount")
        .WithSummary("Cadastra uma nova conta bancária condominial.");

        group.MapGet("/accounts", async (
            int condoId,
            IConciliacaoBancariaApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ListarContasBancariasAsync(condoId, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ListBankAccounts")
        .WithSummary("Lista as contas bancárias do condomínio.");

        group.MapPost("/import-statement", async (
            ImportarExtratoRequestDto request,
            IConciliacaoBancariaApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ImportarExtratoAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ImportBankStatement")
        .WithSummary("Importa lançamentos do extrato bancário para conciliação.");

        group.MapPost("/auto-reconcile/{contaBancariaId:int}", async (
            int contaBancariaId,
            IConciliacaoBancariaApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ProcessarConciliacaoAutomaticaAsync(contaBancariaId, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("AutoReconcile")
        .WithSummary("Executa o motor de conciliação bancária automática em lote.");

        group.MapGet("/pending-items/{contaBancariaId:int}", async (
            int contaBancariaId,
            IConciliacaoBancariaApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ListarItensPendentesAsync(contaBancariaId, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ListPendingReconciliationItems")
        .WithSummary("Lista lançamentos pendentes de conciliação no extrato.");

        group.MapPost("/reconcile-item", async (
            ConciliarManualRequestDto request,
            IConciliacaoBancariaApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ConciliarManualAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ManualReconcile")
        .WithSummary("Efetiva a conciliação manual entre extrato e lançamento interno.");
    }
}
