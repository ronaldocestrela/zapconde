using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Domain.Entities;

public class HistoricoOcorrencia : ITenantScoped
{
    public Guid Id { get; private set; }
    public int TenantId { get; set; }
    public int CondoId { get; private set; }
    public Guid OcorrenciaId { get; private set; }
    public StatusOcorrencia? StatusAnterior { get; private set; }
    public StatusOcorrencia StatusNovo { get; private set; }
    public string Comentario { get; private set; } = string.Empty;
    public DateTime DataAlteracao { get; private set; }
    public string AlteradoPorUserId { get; private set; } = string.Empty;
    public string AlteradoPorNome { get; private set; } = string.Empty;

    // EF Core Constructor
    private HistoricoOcorrencia() { }

    public static HistoricoOcorrencia Create(
        int tenantId,
        int condoId,
        Guid ocorrenciaId,
        StatusOcorrencia? statusAnterior,
        StatusOcorrencia statusNovo,
        string comentario,
        string alteradoPorUserId,
        string alteradoPorNome)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId deve ser maior que zero.", nameof(tenantId));
        if (condoId <= 0) throw new ArgumentException("CondoId deve ser maior que zero.", nameof(condoId));
        if (ocorrenciaId == Guid.Empty) throw new ArgumentException("OcorrenciaId é obrigatório.", nameof(ocorrenciaId));

        return new HistoricoOcorrencia
        {
            Id = Guid.Empty, // Default value so EF Core change tracker recognizes as Added
            TenantId = tenantId,
            CondoId = condoId,
            OcorrenciaId = ocorrenciaId,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            Comentario = comentario ?? string.Empty,
            DataAlteracao = DateTime.UtcNow,
            AlteradoPorUserId = alteradoPorUserId ?? string.Empty,
            AlteradoPorNome = alteradoPorNome ?? "Sistema"
        };
    }
}
