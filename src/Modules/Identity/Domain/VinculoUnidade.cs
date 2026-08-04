using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Identity.Domain;

/// <summary>
/// Vínculo temporal de um morador a uma unidade com papel e histórico auditável.
/// </summary>
public class VinculoUnidade : ITenantScoped
{
    public int Id { get; private set; }

    public int TenantId { get; set; }

    public int CondoId { get; private set; }

    public int UnidadeId { get; private set; }

    public int MoradorId { get; private set; }

    public PapelVinculo Papel { get; private set; }

    public DateTime DataInicio { get; private set; }

    public DateTime? DataFim { get; private set; }

    public string? MotivoEncerramento { get; private set; }

    public bool IsActive { get; private set; }

    public List<string> Dependencias { get; private set; } = [];

    public void AtualizarDependencias(IEnumerable<string> dependencias) =>
        Dependencias = dependencias.Select(d => d.Trim()).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().ToList();

    public string? CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Unidade? Unidade { get; private set; }

    public Morador? Morador { get; private set; }

    private VinculoUnidade() { }

    public static VinculoUnidade Create(
        int tenantId,
        int condoId,
        int unidadeId,
        int moradorId,
        PapelVinculo papel,
        DateTime dataInicio,
        IEnumerable<string>? dependencias = null,
        string? createdByUserId = null)
    {
        if (dataInicio.Date > DateTime.UtcNow.Date.AddDays(1))
        {
            throw new DomainValidationException("Data de início não pode ser futura.");
        }

        return new VinculoUnidade
        {
            TenantId = tenantId,
            CondoId = condoId,
            UnidadeId = unidadeId,
            MoradorId = moradorId,
            Papel = papel,
            DataInicio = dataInicio.Date,
            IsActive = true,
            Dependencias = dependencias?.Select(d => d.Trim()).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().ToList() ?? [],
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Encerrar(DateTime dataFim, string motivo)
    {
        if (!IsActive)
        {
            throw new DomainValidationException("Vínculo já está encerrado.");
        }

        if (dataFim.Date < DataInicio.Date)
        {
            throw new DomainValidationException("Data de encerramento não pode ser anterior ao início.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new DomainValidationException("Motivo de encerramento é obrigatório.");
        }

        DataFim = dataFim.Date;
        MotivoEncerramento = motivo.Trim();
        IsActive = false;
    }
}
