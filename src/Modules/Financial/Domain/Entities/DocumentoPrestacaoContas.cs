using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Documento anexo à Pasta Digital de Prestação de Contas.
/// </summary>
public class DocumentoPrestacaoContas : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int PastaDigitalId { get; set; }
    public CategoriaDocumentoPrestacao Categoria { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string NomeArquivo { get; set; } = string.Empty;
    public string UrlArquivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public long TamanhoBytes { get; set; }
    public DateTime DataUpload { get; set; } = DateTime.UtcNow;
    public int UploadPorUserId { get; set; }

    protected DocumentoPrestacaoContas() { }

    public static DocumentoPrestacaoContas Create(
        int tenantId,
        int pastaDigitalId,
        CategoriaDocumentoPrestacao categoria,
        string titulo,
        string nomeArquivo,
        string urlArquivo,
        string contentType,
        long tamanhoBytes,
        int uploadPorUserId)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("Título é obrigatório.", nameof(titulo));
        if (string.IsNullOrWhiteSpace(nomeArquivo)) throw new ArgumentException("Nome do arquivo é obrigatório.", nameof(nomeArquivo));

        return new DocumentoPrestacaoContas
        {
            TenantId = tenantId,
            PastaDigitalId = pastaDigitalId,
            Categoria = categoria,
            Titulo = titulo,
            NomeArquivo = nomeArquivo,
            UrlArquivo = urlArquivo,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType,
            TamanhoBytes = tamanhoBytes,
            DataUpload = DateTime.UtcNow,
            UploadPorUserId = uploadPorUserId
        };
    }
}
