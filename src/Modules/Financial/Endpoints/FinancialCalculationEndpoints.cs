using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FastEndpoints;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;

namespace Modules.Financial.Endpoints;

/// <summary>
/// Endpoint para simulação ad-hoc de encargos financeiros (multa, juros pró-rata e desconto).
/// </summary>
public sealed class CalculateSimulationEndpoint : Endpoint<SimularCalculoRequestDto, Result<CalculoFinanceiroDto>>
{
    private readonly IFinancialCalculationService _calculationService;

    public CalculateSimulationEndpoint(IFinancialCalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    public override void Configure()
    {
        Post("/api/financial/simulator/calculate");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Simular cálculo de encargos financeiros";
            s.Description = "Calcula multa por atraso, juros pró-rata dia e desconto de pontualidade com trilha de auditoria.";
        });
    }

    public override async Task HandleAsync(SimularCalculoRequestDto req, CancellationToken ct)
    {
        var result = await _calculationService.CalcularSimulacaoAsync(req, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Errors != null && result.Errors.Any() ? 400 : 422;
            await SendAsync(result, status, ct);
        }
    }
}

public record SimulateInvoiceRequest(int Id, DateTime DataSimulacao);

/// <summary>
/// Endpoint para simular encargos de uma fatura cadastrada em data futura de pagamento.
/// </summary>
public sealed class SimulateInvoiceEndpoint : Endpoint<SimulateInvoiceRequest, Result<CalculoFinanceiroDto>>
{
    private readonly IFinancialCalculationService _calculationService;
    private readonly ICurrentTenantService _currentTenantService;

    public SimulateInvoiceEndpoint(
        IFinancialCalculationService calculationService,
        ICurrentTenantService currentTenantService)
    {
        _calculationService = calculationService;
        _currentTenantService = currentTenantService;
    }

    public override void Configure()
    {
        Post("/api/financial/invoices/{id}/simulate");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Simular acréscimos de fatura existente";
            s.Description = "Simula o valor atualizado a ser pago para uma fatura existente em uma data futura informada.";
        });
    }

    public override async Task HandleAsync(SimulateInvoiceRequest req, CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        var result = await _calculationService.SimularFaturaExistenteAsync(req.Id, req.DataSimulacao, tenantId, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}

public record GetInvoiceProjectionRequest(int Id);

/// <summary>
/// Endpoint para obter projeção de valores a pagar em 0, 7, 15, 30 e 60 dias de atraso.
/// </summary>
public sealed class GetInvoiceProjectionEndpoint : Endpoint<GetInvoiceProjectionRequest, Result<IEnumerable<ProjecaoCalculoDto>>>
{
    private readonly IFinancialCalculationService _calculationService;
    private readonly ICurrentTenantService _currentTenantService;

    public GetInvoiceProjectionEndpoint(
        IFinancialCalculationService calculationService,
        ICurrentTenantService currentTenantService)
    {
        _calculationService = calculationService;
        _currentTenantService = currentTenantService;
    }

    public override void Configure()
    {
        Get("/api/financial/invoices/{id}/projection");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Projeção futura de débitos";
            s.Description = "Retorna uma timeline de projeção futura de encargos para 0, 7, 15, 30 e 60 dias de atraso.";
        });
    }

    public override async Task HandleAsync(GetInvoiceProjectionRequest req, CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        var result = await _calculationService.ObterProjecaoFuturaAsync(req.Id, tenantId, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
        }
        else
        {
            var status = result.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            await SendAsync(result, status, ct);
        }
    }
}
