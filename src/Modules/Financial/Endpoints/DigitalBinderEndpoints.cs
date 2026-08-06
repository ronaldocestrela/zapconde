using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;

namespace Modules.Financial.Endpoints;

public static class DigitalBinderEndpoints
{
    public static void MapDigitalBinderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/digital-binders")
            .WithTags("Financial Digital Binders");

        group.MapPost("/generate", async (
            CriarPastaDigitalRequestDto request,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.CriarPastaDigitalAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/financial/digital-binders/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        })
        .WithName("GenerateDigitalBinder")
        .WithSummary("Inicia e gera a pasta digital mensal de prestação de contas.");

        group.MapGet("/{id:int}", async (
            int id,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ObterPorIdAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetDigitalBinderById")
        .WithSummary("Obtém os detalhes da pasta digital por ID.");

        group.MapGet("/", async (
            int condoId,
            int? ano,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.ListarPorCondominioAsync(condoId, ano, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ListDigitalBinders")
        .WithSummary("Lista as pastas digitais por condomínio e ano.");

        group.MapPost("/{id:int}/items", async (
            int id,
            AdicionarItemBalanceteRequestDto request,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.AdicionarItemBalanceteAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("AddBalanceteItem")
        .WithSummary("Adiciona um lançamento ao balancete da pasta digital.");

        group.MapPost("/{id:int}/documents", async (
            int id,
            AnexarDocumentoRequestDto request,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.AnexarDocumentoAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("AttachDocument")
        .WithSummary("Anexa um documento comprobatório à pasta digital.");

        group.MapPost("/{id:int}/submit", async (
            int id,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.SubmeterParaConselhoAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("SubmitDigitalBinder")
        .WithSummary("Submete a pasta digital para apreciação do Conselho Fiscal.");

        group.MapPost("/{id:int}/approve", async (
            int id,
            AprovarPastaDigitalRequestDto request,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.AprovarPastaDigitalAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ApproveDigitalBinder")
        .WithSummary("Registra a aprovação da pasta digital pelo Conselho.");

        group.MapPost("/{id:int}/reject", async (
            int id,
            RejeitarPastaDigitalRequestDto request,
            IPastaDigitalApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.RejeitarPastaDigitalAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("RejectDigitalBinder")
        .WithSummary("Registra a rejeição da pasta digital pelo Conselho.");
    }
}
