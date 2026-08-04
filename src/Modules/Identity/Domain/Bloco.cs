using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Identity.Domain;

/// <summary>
/// Bloco ou torre dentro de um condomínio.
/// </summary>
public class Bloco : ITenantScoped
{
    public int Id { get; private set; }

    public int TenantId { get; set; }

    public int CondoId { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Nome { get; private set; } = string.Empty;

    public int Ordem { get; private set; }

    public ICollection<Unidade> Unidades { get; private set; } = [];

    private Bloco() { }

    public static Bloco Create(int tenantId, int condoId, string codigo, string? nome = null, int ordem = 0)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new DomainValidationException("Código do bloco é obrigatório.");
        }

        return new Bloco
        {
            TenantId = tenantId,
            CondoId = condoId,
            Codigo = codigo.Trim(),
            Nome = string.IsNullOrWhiteSpace(nome) ? codigo.Trim() : nome.Trim(),
            Ordem = ordem
        };
    }
}
