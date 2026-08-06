using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Entidade que representa uma parcela individual do acordo.
/// </summary>
public class ParcelaAcordo : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int AcordoId { get; set; }
    public int NumeroParcela { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal ValorParcela { get; set; }
    public StatusParcelaAcordo Status { get; set; } = StatusParcelaAcordo.Pendente;
    public DateTime? DataPagamento { get; set; }
    public int? FaturaGeradaId { get; set; }

    protected ParcelaAcordo() { }

    public static ParcelaAcordo Create(
        int tenantId,
        int acordoId,
        int numeroParcela,
        DateTime dataVencimento,
        decimal valorParcela)
    {
        if (numeroParcela <= 0)
            throw new ArgumentException("Número da parcela deve ser maior que zero.", nameof(numeroParcela));

        if (valorParcela <= 0)
            throw new ArgumentException("Valor da parcela deve ser maior que zero.", nameof(valorParcela));

        var utcVencimento = dataVencimento.Kind == DateTimeKind.Utc
            ? dataVencimento
            : DateTime.SpecifyKind(dataVencimento, DateTimeKind.Utc);

        return new ParcelaAcordo
        {
            TenantId = tenantId,
            AcordoId = acordoId,
            NumeroParcela = numeroParcela,
            DataVencimento = utcVencimento,
            ValorParcela = valorParcela,
            Status = StatusParcelaAcordo.Pendente
        };
    }

    public void RegistrarPagamento(DateTime dataPagamento)
    {
        DataPagamento = dataPagamento.Kind == DateTimeKind.Utc
            ? dataPagamento
            : DateTime.SpecifyKind(dataPagamento, DateTimeKind.Utc);
        Status = StatusParcelaAcordo.Paga;
    }

    public void MarcarAtrasada()
    {
        if (Status == StatusParcelaAcordo.Pendente)
            Status = StatusParcelaAcordo.Atrasada;
    }

    public void Cancelar()
    {
        if (Status != StatusParcelaAcordo.Paga)
            Status = StatusParcelaAcordo.Cancelada;
    }
}
