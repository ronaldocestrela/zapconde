using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;

namespace Modules.AIEngine.Application.Plugins;

/// <summary>
/// Plugin do Microsoft.SemanticKernel (Function Calling / Tools) para consulta de boletos pendentes.
/// </summary>
public class BoletoPlugin
{
    private readonly IInvoiceService _invoiceService;

    public BoletoPlugin(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
    }

    [KernelFunction("GetPendingBoletos")]
    [Description("Consulta e retorna a lista de faturas e boletos em aberto/pendentes de pagamento para um determinado morador pelo seu moradorId, contendo código PIX Copia e Cola, linha digitável, valor, data de vencimento e link do PDF.")]
    public async Task<string> GetPendingBoletosAsync(
        [Description("ID numérico do morador registrado no condomínio (morador_id)")] int moradorId,
        CancellationToken cancellationToken = default)
    {
        if (moradorId <= 0)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = "ID do morador inválido para consulta."
            });
        }

        var result = await _invoiceService.GetPendingBoletosByMoradorAsync(moradorId, cancellationToken);

        if (!result.IsSuccess)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? "Erro ao consultar boletos no serviço financeiro."
            });
        }

        var list = result.Data?.ToList() ?? new List<PendingBoletoDto>();

        if (!list.Any())
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = true,
                moradorId,
                totalPendencias = 0,
                valorTotal = 0.00m,
                mensagem = $"O morador #{moradorId} está totalmente em dia! Nenhuma fatura ou boleto pendente foi encontrado.",
                boletos = Array.Empty<object>()
            });
        }

        var responseObj = new
        {
            sucesso = true,
            moradorId,
            totalPendencias = list.Count,
            valorTotal = list.Sum(b => b.ValorTotal),
            mensagem = $"Foram encontradas {list.Count} fatura(s) pendente(s) para o morador #{moradorId}.",
            boletos = list.Select(b => new
            {
                faturaId = b.FaturaId,
                boletoId = b.BoletoId,
                competencia = b.Competencia,
                numeroFatura = b.NumeroFatura,
                valor = b.ValorTotal,
                dataVencimento = b.DataVencimento.ToString("dd/MM/yyyy"),
                status = b.StatusFaturaDescricao,
                vencido = b.Vencido,
                pixCopiaECola = b.CodigoPixCopiaECola,
                linhaDigitavel = b.LinhaDigitavel,
                codigoBarras = b.CodigoBarras,
                pdfUrl = b.PdfUrl
            })
        };

        return JsonSerializer.Serialize(responseObj, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }
}
