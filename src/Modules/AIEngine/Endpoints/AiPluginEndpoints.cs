using System.Text.Json;
using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AIEngine.Application.Plugins;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;

namespace Modules.AIEngine.Endpoints;

public record GetPendingBoletosRequest(int MoradorId);

/// <summary>
/// Endpoint para consultar diretamente os boletos pendentes de um morador pelo seu moradorId.
/// </summary>
public sealed class GetPendingBoletosEndpoint : Endpoint<GetPendingBoletosRequest, Result<IEnumerable<PendingBoletoDto>>>
{
    private readonly IInvoiceService _invoiceService;

    public GetPendingBoletosEndpoint(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    public override void Configure()
    {
        Get("/api/ai/plugins/boletos/pending/{MoradorId:int}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Consultar Boletos Pendentes do Morador";
            s.Description = "Retorna os boletos e faturas pendentes de um morador para consumo do plugin de IA ou chamadas diretas.";
        });
    }

    public override async Task HandleAsync(GetPendingBoletosRequest req, CancellationToken ct)
    {
        var result = await _invoiceService.GetPendingBoletosByMoradorAsync(req.MoradorId, ct);
        var statusCode = result.IsSuccess ? 200 : (result.Errors.Any() ? 400 : 404);
        await SendAsync(result, statusCode, ct);
    }
}

public record ExecuteBoletoPluginRequest(int MoradorId);

/// <summary>
/// Endpoint para simular a invocação do Plugin BoletoPlugin (Function Calling do Semantic Kernel).
/// </summary>
public sealed class ExecuteBoletoPluginEndpoint : Endpoint<ExecuteBoletoPluginRequest, Result<BoletoPluginExecutionResultDto>>
{
    private readonly BoletoPlugin _boletoPlugin;
    private readonly IInvoiceService _invoiceService;

    public ExecuteBoletoPluginEndpoint(BoletoPlugin boletoPlugin, IInvoiceService invoiceService)
    {
        _boletoPlugin = boletoPlugin;
        _invoiceService = invoiceService;
    }

    public override void Configure()
    {
        Post("/api/ai/plugins/boletos/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Executar Function Calling do BoletoPlugin";
            s.Description = "Invoca o plugin GetPendingBoletos do Semantic Kernel simulando a resposta em linguagem natural do agente.";
        });
    }

    public override async Task HandleAsync(ExecuteBoletoPluginRequest req, CancellationToken ct)
    {
        if (req.MoradorId <= 0)
        {
            await SendAsync(Result<BoletoPluginExecutionResultDto>.ValidationFailure(["MoradorId é obrigatório e deve ser maior que zero."]), 400, ct);
            return;
        }

        var jsonResult = await _boletoPlugin.GetPendingBoletosAsync(req.MoradorId, ct);
        var boletosResult = await _invoiceService.GetPendingBoletosByMoradorAsync(req.MoradorId, ct);

        var list = boletosResult.Data?.ToList() ?? new List<PendingBoletoDto>();
        var totalValor = list.Sum(b => b.ValorTotal);

        string mensagemFormatada;
        if (!list.Any())
        {
            mensagemFormatada = $"Olá! O morador #{req.MoradorId} está totalmente em dia com as taxas do condomínio. Nenhuma fatura em aberto foi encontrada.";
        }
        else
        {
            var primeiro = list.First();
            mensagemFormatada = $"Olá! Identifiquei {list.Count} fatura(s) pendente(s) no valor total de R$ {totalValor:F2}.\n" +
                                $"Vencimento: {primeiro.DataVencimento:dd/MM/yyyy}\n" +
                                $"Chave PIX Copia e Cola: {primeiro.CodigoPixCopiaECola}\n" +
                                $"Link do PDF: {primeiro.PdfUrl}";
        }

        var dto = new BoletoPluginExecutionResultDto(
            MoradorId: req.MoradorId,
            QuantidadePendencias: list.Count,
            ValorTotalPendencias: totalValor,
            Boletos: list,
            MensagemFormatadaFormatadaIa: mensagemFormatada
        );

        await SendAsync(Result<BoletoPluginExecutionResultDto>.Success(dto, "Plugin GetPendingBoletos executado com sucesso."), 200, ct);
    }
}
