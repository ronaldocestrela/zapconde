using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Identity.Domain;

/// <summary>
/// Unidade habitacional (apartamento/casa) vinculada a um bloco.
/// </summary>
public class Unidade : ITenantScoped
{
    public int Id { get; private set; }

    public int TenantId { get; set; }

    public int CondoId { get; private set; }

    public int BlocoId { get; private set; }

    public string Numero { get; private set; } = string.Empty;

    public UnidadeStatus Status { get; private set; }

    public Bloco? Bloco { get; private set; }

    public ICollection<VinculoUnidade> Vinculos { get; private set; } = [];

    private Unidade() { }

    public static Unidade Create(int tenantId, int condoId, int blocoId, string numero, UnidadeStatus status = UnidadeStatus.Vaga)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            throw new DomainValidationException("Número da unidade é obrigatório.");
        }

        return new Unidade
        {
            TenantId = tenantId,
            CondoId = condoId,
            BlocoId = blocoId,
            Numero = numero.Trim(),
            Status = status
        };
    }

    public void AtualizarStatus(UnidadeStatus status) => Status = status;

    public void RecalcularStatus()
    {
        if (Status == UnidadeStatus.EmReforma)
        {
            return;
        }

        Status = Vinculos.Any(v => v.IsActive) ? UnidadeStatus.Ocupada : UnidadeStatus.Vaga;
    }

    public VinculoUnidade? ObterVinculoAtivo(PapelVinculo papel) =>
        Vinculos.FirstOrDefault(v => v.IsActive && v.Papel == papel);

    public IEnumerable<VinculoUnidade> ObterVinculosAtivos() =>
        Vinculos.Where(v => v.IsActive);

    public void ValidarNovoVinculo(PapelVinculo papel)
    {
        if (papel == PapelVinculo.Proprietario && ObterVinculoAtivo(PapelVinculo.Proprietario) is not null)
        {
            throw new DomainValidationException("Já existe um proprietário ativo nesta unidade.");
        }
    }
}
