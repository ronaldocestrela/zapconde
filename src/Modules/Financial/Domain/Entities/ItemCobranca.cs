using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Representa um item individual de cobrança em uma fatura condominial.
/// </summary>
public class ItemCobranca : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int FaturaId { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public TipoItemCobranca Tipo { get; set; }
    public decimal ValorUnitario { get; set; }
    public int Quantidade { get; set; } = 1;

    public decimal Subtotal => ValorUnitario * Quantidade;

    // Navegação EF Core
    public Fatura? Fatura { get; set; }

    protected ItemCobranca() { }

    public static ItemCobranca Create(int tenantId, int faturaId, string descricao, TipoItemCobranca tipo, decimal valorUnitario, int quantidade = 1)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do item é obrigatória.", nameof(descricao));

        if (valorUnitario < 0)
            throw new ArgumentException("Valor unitário não pode ser negativo.", nameof(valorUnitario));

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));

        return new ItemCobranca
        {
            TenantId = tenantId,
            FaturaId = faturaId,
            Descricao = descricao.Trim(),
            Tipo = tipo,
            ValorUnitario = valorUnitario,
            Quantidade = quantidade
        };
    }
}
