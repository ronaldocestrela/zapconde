using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Lançamento do balancete mensal (Receita ou Despesa) da Pasta Digital.
/// </summary>
public class ItemBalancete : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int PastaDigitalId { get; set; }
    public TipoLancamentoBalancete TipoLancamento { get; set; }
    public CategoriaPlanoContas Categoria { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorOrcado { get; set; }
    public decimal ValorRealizado { get; set; }
    public DateTime DataLancamento { get; set; }
    public bool Conciliado { get; set; }

    protected ItemBalancete() { }

    public static ItemBalancete Create(
        int tenantId,
        int pastaDigitalId,
        TipoLancamentoBalancete tipoLancamento,
        CategoriaPlanoContas categoria,
        string descricao,
        decimal valorOrcado,
        decimal valorRealizado,
        DateTime dataLancamento,
        bool conciliado = false)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));

        var utcDataLancamento = dataLancamento.Kind == DateTimeKind.Utc
            ? dataLancamento
            : DateTime.SpecifyKind(dataLancamento, DateTimeKind.Utc);

        return new ItemBalancete
        {
            TenantId = tenantId,
            PastaDigitalId = pastaDigitalId,
            TipoLancamento = tipoLancamento,
            Categoria = categoria,
            Descricao = descricao,
            ValorOrcado = valorOrcado,
            ValorRealizado = valorRealizado,
            DataLancamento = utcDataLancamento,
            Conciliado = conciliado
        };
    }
}
