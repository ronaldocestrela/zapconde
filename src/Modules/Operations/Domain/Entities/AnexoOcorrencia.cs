using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Operations.Domain.Entities;

public class AnexoOcorrencia : ITenantScoped
{
    public Guid Id { get; private set; }
    public int TenantId { get; set; }
    public int CondoId { get; private set; }
    public Guid OcorrenciaId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string NomeArquivo { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public DateTime DataUpload { get; private set; }
    public string UploadPorUserId { get; private set; } = string.Empty;

    // EF Core Constructor
    private AnexoOcorrencia() { }

    public static AnexoOcorrencia Create(
        int tenantId,
        int condoId,
        Guid ocorrenciaId,
        string url,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        string uploadPorUserId)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId deve ser maior que zero.", nameof(tenantId));
        if (condoId <= 0) throw new ArgumentException("CondoId deve ser maior que zero.", nameof(condoId));
        if (ocorrenciaId == Guid.Empty) throw new ArgumentException("OcorrenciaId é obrigatório.", nameof(ocorrenciaId));
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url é obrigatória.", nameof(url));
        if (string.IsNullOrWhiteSpace(nomeArquivo)) throw new ArgumentException("NomeArquivo é obrigatório.", nameof(nomeArquivo));

        return new AnexoOcorrencia
        {
            Id = Guid.Empty, // Default value so EF Core change tracker recognizes as Added
            TenantId = tenantId,
            CondoId = condoId,
            OcorrenciaId = ocorrenciaId,
            Url = url,
            NomeArquivo = nomeArquivo,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            TamanhoBytes = tamanhoBytes < 0 ? 0 : tamanhoBytes,
            DataUpload = DateTime.UtcNow,
            UploadPorUserId = uploadPorUserId ?? string.Empty
        };
    }
}
