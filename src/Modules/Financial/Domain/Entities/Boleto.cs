using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Representa o boleto e dados de pagamento bancário / PIX associado a uma fatura.
/// </summary>
public class Boleto : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int FaturaId { get; set; }

    public string NossoNumero { get; set; } = string.Empty;
    public string LinhaDigitavel { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string CodigoPixCopiaECola { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;

    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public DateTime? DataPagamento { get; set; }

    public StatusBoleto Status { get; set; } = StatusBoleto.Gerado;

    // Navegação EF Core
    public Fatura? Fatura { get; set; }

    protected Boleto() { }

    public static Boleto Create(
        int tenantId,
        int faturaId,
        string nossoNumero,
        string linhaDigitavel,
        string codigoBarras,
        string codigoPix,
        decimal valor,
        DateTime dataVencimento,
        string pdfUrl = "")
    {
        var utcDataVencimento = dataVencimento.Kind == DateTimeKind.Utc
            ? dataVencimento
            : DateTime.SpecifyKind(dataVencimento, DateTimeKind.Utc);

        return new Boleto
        {
            TenantId = tenantId,
            FaturaId = faturaId,
            NossoNumero = nossoNumero,
            LinhaDigitavel = linhaDigitavel,
            CodigoBarras = codigoBarras,
            CodigoPixCopiaECola = codigoPix,
            Valor = valor,
            DataVencimento = utcDataVencimento,
            DataEmissao = DateTime.UtcNow,
            Status = StatusBoleto.Gerado,
            PdfUrl = pdfUrl
        };
    }

    public void RegistrarPagamento(DateTime dataPagamento)
    {
        DataPagamento = dataPagamento.Kind == DateTimeKind.Utc
            ? dataPagamento
            : DateTime.SpecifyKind(dataPagamento, DateTimeKind.Utc);
        Status = StatusBoleto.Pago;
    }

    public void Cancelar()
    {
        Status = StatusBoleto.Cancelado;
    }
}
