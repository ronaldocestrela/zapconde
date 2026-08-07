using BuildingBlocks.Shared;
using Modules.AIEngine.Application.DTOs;

namespace Modules.AIEngine.Infrastructure.Services;

/// <summary>
/// Interface do serviço de gerenciamento da Base de Conhecimento RAG e buscas vetoriais.
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Cadastra, particiona e indexa vetorialmente um documento normativo (Regimento/Convenção).
    /// </summary>
    Task<Result<KnowledgeDocumentDto>> UploadAndProcessDocumentAsync(UploadKnowledgeDocumentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista os documentos cadastrados do tenant atual.
    /// </summary>
    Task<Result<IReadOnlyList<KnowledgeDocumentDto>>> GetDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém detalhes de um documento e seus trechos vetoriais.
    /// </summary>
    Task<Result<KnowledgeDocumentDetailDto>> GetDocumentDetailsAsync(int documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inativa/exclui um documento e seus fragmentos.
    /// </summary>
    Task<Result> DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa busca por similaridade vetorial (RAG) no pgvector.
    /// </summary>
    Task<Result<IReadOnlyList<KnowledgeSearchResultDto>>> SearchSimilarChunksAsync(KnowledgeSearchQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o resumo dos indicadores da base RAG.
    /// </summary>
    Task<Result<KnowledgeSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
