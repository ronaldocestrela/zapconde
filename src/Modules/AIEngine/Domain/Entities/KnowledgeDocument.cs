using BuildingBlocks.Shared.MultiTenancy;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Domain.Exceptions;

namespace Modules.AIEngine.Domain.Entities;

/// <summary>
/// Representa um documento normativo (Regimento Interno, Convenção, Regulamento) cadastrado para RAG.
/// </summary>
public class KnowledgeDocument : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Title { get; private set; } = string.Empty;
    public KnowledgeDocumentType DocumentType { get; private set; } = KnowledgeDocumentType.RegimentoInterno;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int ChunkCount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CriadoEm { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AtualizadoEm { get; private set; }

    private readonly List<KnowledgeChunk> _chunks = new();
    public IReadOnlyCollection<KnowledgeChunk> Chunks => _chunks.AsReadOnly();

    private KnowledgeDocument() { }

    public static KnowledgeDocument Criar(
        int tenantId,
        string title,
        KnowledgeDocumentType documentType,
        string content,
        string? originalFileName = null)
    {
        if (tenantId <= 0)
            throw new AiEngineDomainException("TenantId é obrigatório.");

        if (string.IsNullOrWhiteSpace(title))
            throw new AiEngineDomainException("O título do documento é obrigatório.");

        if (string.IsNullOrWhiteSpace(content))
            throw new AiEngineDomainException("O conteúdo do documento não pode ser vazio.");

        return new KnowledgeDocument
        {
            TenantId = tenantId,
            Title = title.Trim(),
            DocumentType = documentType,
            Content = content.Trim(),
            OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? $"{title.Trim()}.txt" : originalFileName.Trim(),
            IsActive = true,
            ChunkCount = 0,
            CriadoEm = DateTimeOffset.UtcNow
        };
    }

    public void DefinirChunkCount(int count)
    {
        if (count < 0)
            throw new AiEngineDomainException("A contagem de chunks não pode ser negativa.");

        ChunkCount = count;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    public void Desativar()
    {
        IsActive = false;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    public void Ativar()
    {
        IsActive = true;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }
}
