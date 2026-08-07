using BuildingBlocks.Shared.MultiTenancy;
using Modules.AIEngine.Domain.Exceptions;
using Pgvector;

namespace Modules.AIEngine.Domain.Entities;

/// <summary>
/// Representa um fragmento de documento com seu embedding vetorial correspondente para RAG no pgvector.
/// </summary>
public class KnowledgeChunk : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int DocumentId { get; set; }
    public KnowledgeDocument? Document { get; private set; }
    public int ChunkIndex { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Vector? Embedding { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; } = DateTimeOffset.UtcNow;

    private KnowledgeChunk() { }

    public static KnowledgeChunk Criar(
        int tenantId,
        int documentId,
        int chunkIndex,
        string content,
        Vector? embedding = null)
    {
        if (tenantId <= 0)
            throw new AiEngineDomainException("TenantId é obrigatório.");

        if (chunkIndex < 0)
            throw new AiEngineDomainException("O índice do chunk deve ser maior ou igual a zero.");

        if (string.IsNullOrWhiteSpace(content))
            throw new AiEngineDomainException("O conteúdo do chunk não pode ser vazio.");

        return new KnowledgeChunk
        {
            TenantId = tenantId,
            DocumentId = documentId,
            ChunkIndex = chunkIndex,
            Content = content.Trim(),
            Embedding = embedding,
            CriadoEm = DateTimeOffset.UtcNow
        };
    }

    public void DefinirEmbedding(Vector embedding)
    {
        Embedding = embedding ?? throw new AiEngineDomainException("Embedding vetorial não pode ser nulo.");
    }
}
