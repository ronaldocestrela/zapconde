using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Aggregate Root de Acordo de Renegociação Condominial.
/// </summary>
public class Acordo : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public int UnidadeId { get; set; }
    public int MoradorId { get; set; }

    public string NumeroAcordo { get; set; } = string.Empty; // ex: ACD-202608-101
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAceite { get; set; }
    public DateTime DataPrimeiroVencimento { get; set; }

    public decimal ValorTotalOriginal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotalAcordo { get; set; }
    public int QuantidadeParcelas { get; set; }

    public StatusAcordo Status { get; set; } = StatusAcordo.Proposta;
    public string Observacoes { get; set; } = string.Empty;

    // Coleções
    private readonly List<ParcelaAcordo> _parcelas = new();
    public IReadOnlyCollection<ParcelaAcordo> Parcelas => _parcelas.AsReadOnly();

    private readonly List<AcordoFaturaVinculada> _faturasVinculadas = new();
    public IReadOnlyCollection<AcordoFaturaVinculada> FaturasVinculadas => _faturasVinculadas.AsReadOnly();

    protected Acordo() { }

    public static Acordo Create(
        int tenantId,
        int condoId,
        int unidadeId,
        int moradorId,
        DateTime dataPrimeiroVencimento,
        decimal valorTotalOriginal,
        decimal valorDesconto,
        int quantidadeParcelas,
        string observacoes = "")
    {
        if (unidadeId <= 0)
            throw new ArgumentException("UnidadeId inválido.", nameof(unidadeId));

        if (moradorId <= 0)
            throw new ArgumentException("MoradorId inválido.", nameof(moradorId));

        if (valorTotalOriginal <= 0)
            throw new ArgumentException("Valor total original deve ser positivo.", nameof(valorTotalOriginal));

        if (quantidadeParcelas <= 0)
            throw new ArgumentException("Quantidade de parcelas deve ser maior que zero.", nameof(quantidadeParcelas));

        var valorTotalAcordo = valorTotalOriginal - valorDesconto;
        if (valorTotalAcordo <= 0)
            throw new ArgumentException("O valor final do acordo deve ser maior que zero.", nameof(valorDesconto));

        var utcVencimento = dataPrimeiroVencimento.Kind == DateTimeKind.Utc
            ? dataPrimeiroVencimento
            : DateTime.SpecifyKind(dataPrimeiroVencimento, DateTimeKind.Utc);

        var acordo = new Acordo
        {
            TenantId = tenantId,
            CondoId = condoId,
            UnidadeId = unidadeId,
            MoradorId = moradorId,
            NumeroAcordo = $"ACD-{DateTime.UtcNow:yyyyMM}-{unidadeId:D3}-{Random.Shared.Next(100, 999)}",
            DataCriacao = DateTime.UtcNow,
            DataPrimeiroVencimento = utcVencimento,
            ValorTotalOriginal = valorTotalOriginal,
            ValorDesconto = valorDesconto,
            ValorTotalAcordo = valorTotalAcordo,
            QuantidadeParcelas = quantidadeParcelas,
            Status = StatusAcordo.Proposta,
            Observacoes = observacoes
        };

        return acordo;
    }

    public void VincularFaturaOriginal(int faturaId, decimal valorOriginal)
    {
        var vinculo = AcordoFaturaVinculada.Create(TenantId, Id, faturaId, valorOriginal);
        _faturasVinculadas.Add(vinculo);
    }

    public void AdicionarParcela(ParcelaAcordo parcela)
    {
        _parcelas.Add(parcela);
    }

    public void EfetivarAcordo(DateTime dataAceite)
    {
        if (Status != StatusAcordo.Proposta)
            throw new InvalidOperationException("Apenas propostas de acordo podem ser efetivadas.");

        DataAceite = dataAceite.Kind == DateTimeKind.Utc
            ? dataAceite
            : DateTime.SpecifyKind(dataAceite, DateTimeKind.Utc);

        Status = StatusAcordo.Ativo;
    }

    public void RegistrarPagamentoParcela(int numeroParcela, DateTime dataPagamento)
    {
        var parcela = _parcelas.FirstOrDefault(p => p.NumeroParcela == numeroParcela);
        if (parcela == null)
            throw new InvalidOperationException($"Parcela {numeroParcela} não encontrada no acordo.");

        parcela.RegistrarPagamento(dataPagamento);

        if (_parcelas.All(p => p.Status == StatusParcelaAcordo.Paga))
        {
            Status = StatusAcordo.Quitado;
        }
    }

    public void MarcarDescumprido()
    {
        if (Status == StatusAcordo.Ativo)
        {
            Status = StatusAcordo.Descumprido;
            foreach (var parcela in _parcelas.Where(p => p.Status == StatusParcelaAcordo.Pendente))
            {
                parcela.Cancelar();
            }
        }
    }

    public void Cancelar()
    {
        if (Status == StatusAcordo.Quitado)
            throw new InvalidOperationException("Não é possível cancelar um acordo já quitado.");

        Status = StatusAcordo.Cancelado;
        foreach (var parcela in _parcelas.Where(p => p.Status == StatusParcelaAcordo.Pendente))
        {
            parcela.Cancelar();
        }
    }
}
