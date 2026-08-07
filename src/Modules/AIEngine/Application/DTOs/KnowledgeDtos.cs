using Modules.AIEngine.Domain.Enums;

namespace Modules.AIEngine.Application.DTOs;

public record UploadKnowledgeDocumentRequest(
    string Title,
    KnowledgeDocumentType DocumentType,
    string Content,
    string? OriginalFileName = null);

public record KnowledgeDocumentDto(
    int Id,
    int TenantId,
    string Title,
    KnowledgeDocumentType DocumentType,
    string DocumentTypeName,
    string OriginalFileName,
    int ChunkCount,
    bool IsActive,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm);

public record KnowledgeDocumentDetailDto(
    int Id,
    int TenantId,
    string Title,
    KnowledgeDocumentType DocumentType,
    string DocumentTypeName,
    string OriginalFileName,
    string Content,
    int ChunkCount,
    bool IsActive,
    DateTimeOffset CriadoEm,
    IReadOnlyList<KnowledgeChunkDto> Chunks);

public record KnowledgeChunkDto(
    int Id,
    int DocumentId,
    int ChunkIndex,
    string Content,
    int VectorDimensions,
    DateTimeOffset CriadoEm);

public record KnowledgeSearchQueryRequest(
    string QueryText,
    int TopK = 3,
    double MinSimilarity = 0.0);

public record KnowledgeSearchResultDto(
    int ChunkId,
    int DocumentId,
    string DocumentTitle,
    KnowledgeDocumentType DocumentType,
    int ChunkIndex,
    string Content,
    double SimilarityScore);

public record KnowledgeSummaryDto(
    int TotalDocuments,
    int ActiveDocuments,
    int TotalChunks,
    string EmbeddingModel);
