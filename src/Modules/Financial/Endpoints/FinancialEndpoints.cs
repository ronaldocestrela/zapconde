using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Endpoints;

public record GetInvoicesQueryRequest(
    int? CondoId,
    int? UnidadeId,
    string? Competencia,
    StatusFatura? Status
);

/// <summary>
/// Consulta lista paginada e filtrada de faturas.
/// </summary>
public sealed class GetInvoicesEndpoint : Endpoint<GetInvoicesQueryRequest, Result<IEnumerable<FaturaSummaryDto>>>
{
    private readonly IInvoiceService _invoiceService;

    public GetInvoicesEndpoint(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    public override void Configure()
    {
        Get("/api/financial/invoices");
        AllowAnonymous(); // Para testes de integracao / dev context
        Summary(s =>
        {
            s.Summary = "Listar faturas condominiais";
            s.Description = "Retorna lista de faturas filtradas por condomínio, unidade, competência ou status com isolamento multi-tenant.";
        });
    }

    public override async Task HandleAsync(GetInvoicesQueryRequest req, CancellationToken ct)
    {
        var result = await _invoiceService.GetInvoicesAsync(
            condoId: req.CondoId,
            unidadeId: req.UnidadeId,
            competencia: req.Competencia,
            status: req.Status,
            ct: ct);

        await SendAsync(result, result.IsSuccess ? 200 : 400, ct);
    }
}

public record GetInvoiceByIdRequest(int Id);

/// <summary>
/// Consulta detalhes completos de uma fatura e seus itens/boleto.
/// </summary>
public sealed class GetInvoiceByIdEndpoint : Endpoint<GetInvoiceByIdRequest, Result<FaturaDetailDto>>
{
    private readonly IInvoiceService _invoiceService;

    public GetInvoiceByIdEndpoint(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    public override void Configure()
    {
        Get("/api/financial/invoices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter detalhes da fatura";
            s.Description = "Retorna o detalhamento completo dos itens de cobrança e dados do boleto/PIX.";
        });
    }

    public override async Task HandleAsync(GetInvoiceByIdRequest req, CancellationToken ct)
    {
        var result = await _invoiceService.GetInvoiceByIdAsync(req.Id, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não foi encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}

/// <summary>
/// Emissão de nova fatura condominial.
/// </summary>
public sealed class CreateInvoiceEndpoint : Endpoint<CreateFaturaRequest, Result<FaturaDetailDto>>
{
    private readonly IInvoiceService _invoiceService;

    public CreateInvoiceEndpoint(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    public override void Configure()
    {
        Post("/api/financial/invoices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Emitir fatura condominial";
            s.Description = "Cria uma nova fatura com itens de cobrança e gera boleto/PIX automático.";
        });
    }

    public override async Task HandleAsync(CreateFaturaRequest req, CancellationToken ct)
    {
        var result = await _invoiceService.CreateInvoiceAsync(req, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 201, ct);
        }
        else
        {
            var status = result.Errors.Any() ? 422 : 400;
            await SendAsync(result, status, ct);
        }
    }
}

public record CancelInvoiceRequest(int Id);

/// <summary>
/// Cancelamento de fatura pendente.
/// </summary>
public sealed class CancelInvoiceEndpoint : Endpoint<CancelInvoiceRequest, Result>
{
    private readonly IInvoiceService _invoiceService;

    public CancelInvoiceEndpoint(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    public override void Configure()
    {
        Post("/api/financial/invoices/{id}/cancel");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cancelar fatura";
            s.Description = "Altera o status de uma fatura pendente para Cancelado.";
        });
    }

    public override async Task HandleAsync(CancelInvoiceRequest req, CancellationToken ct)
    {
        var result = await _invoiceService.CancelInvoiceAsync(req.Id, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não foi encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}
