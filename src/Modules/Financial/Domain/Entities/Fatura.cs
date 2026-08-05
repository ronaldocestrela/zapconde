using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Entidade raiz de cobrança (Fatura) condominial.
/// </summary>
public class Fatura : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public int UnidadeId { get; set; }
    public int MoradorId { get; set; }

    public string Competencia { get; set; } = string.Empty; // ex: "2026-08"
    public string NumeroFatura { get; set; } = string.Empty; // ex: "FAT-202608-101"
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public DateTime DataVencimento { get; set; }

    public decimal ValorOriginal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorJuros { get; set; }

    public StatusFatura Status { get; set; } = StatusFatura.Pendente;
    public DateTime? DataPagamento { get; set; }
    public string Observacoes { get; set; } = string.Empty;

    // Relacionamentos e Coleções
    private readonly List<ItemCobranca> _itens = new();
    public IReadOnlyCollection<ItemCobranca> Itens => _itens.AsReadOnly();

    public Boleto? Boleto { get; set; }

    public decimal TotalFinal => ValorOriginal + ValorMulta + ValorJuros - ValorDesconto;

    protected Fatura() { }

    public static Fatura Create(
        int tenantId,
        int condoId,
        int unidadeId,
        int moradorId,
        string competencia,
        DateTime dataVencimento,
        string observacoes = "")
    {
        if (string.IsNullOrWhiteSpace(competencia))
            throw new ArgumentException("Competência é obrigatória.", nameof(competencia));

        if (unidadeId <= 0)
            throw new ArgumentException("UnidadeId inválido.", nameof(unidadeId));

        if (moradorId <= 0)
            throw new ArgumentException("MoradorId inválido.", nameof(moradorId));

        var utcDataVencimento = dataVencimento.Kind == DateTimeKind.Utc
            ? dataVencimento
            : DateTime.SpecifyKind(dataVencimento, DateTimeKind.Utc);

        return new Fatura
        {
            TenantId = tenantId,
            CondoId = condoId,
            UnidadeId = unidadeId,
            MoradorId = moradorId,
            Competencia = competencia.Trim(),
            NumeroFatura = $"FAT-{competencia.Replace("-", "")}-{unidadeId:D3}",
            DataEmissao = DateTime.UtcNow,
            DataVencimento = utcDataVencimento,
            Status = StatusFatura.Pendente,
            Observacoes = observacoes
        };
    }

    public void AddItem(string descricao, TipoItemCobranca tipo, decimal valorUnitario, int quantidade = 1)
    {
        var item = ItemCobranca.Create(TenantId, Id, descricao, tipo, valorUnitario, quantidade);
        _itens.Add(item);
        RecalcularValores();
    }

    public void RecalcularValores()
    {
        ValorOriginal = _itens.Sum(i => i.Subtotal);
    }

    public void AnexarBoleto(Boleto boleto)
    {
        Boleto = boleto;
        boleto.FaturaId = Id;
        boleto.TenantId = TenantId;
    }

    public void Cancelar()
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Não é possível cancelar uma fatura já paga.");

        Status = StatusFatura.Cancelado;
        Boleto?.Cancelar();
    }

    public void RegistrarPagamento(DateTime dataPagamento, decimal valorPago)
    {
        DataPagamento = dataPagamento.Kind == DateTimeKind.Utc
            ? dataPagamento
            : DateTime.SpecifyKind(dataPagamento, DateTimeKind.Utc);

        if (valorPago >= TotalFinal)
        {
            Status = StatusFatura.Pago;
            Boleto?.RegistrarPagamento(dataPagamento);
        }
        else
        {
            Status = StatusFatura.ParcialmentePago;
        }
    }
}
