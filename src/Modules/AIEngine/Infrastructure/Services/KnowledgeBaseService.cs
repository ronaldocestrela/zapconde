using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Infrastructure.Persistence;
using Pgvector.EntityFrameworkCore;

namespace Modules.AIEngine.Infrastructure.Services;

/// <summary>
/// Serviço de orquestração do RAG: cadastro de documentos, chunking, geração de embeddings e busca por similaridade vetorial via pgvector.
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly AiDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ITextChunkerService _textChunkerService;
    private readonly ITextEmbeddingService _textEmbeddingService;

    public KnowledgeBaseService(
        AiDbContext dbContext,
        ICurrentTenantService currentTenantService,
        ITextChunkerService textChunkerService,
        ITextEmbeddingService textEmbeddingService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _textChunkerService = textChunkerService;
        _textEmbeddingService = textEmbeddingService;
    }

    public async Task<Result<KnowledgeDocumentDto>> UploadAndProcessDocumentAsync(
        UploadKnowledgeDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<KnowledgeDocumentDto>.ValidationFailure(new[] { "O título do documento é obrigatório." });

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result<KnowledgeDocumentDto>.ValidationFailure(new[] { "O conteúdo do documento não pode ser vazio." });

        var tenantId = _currentTenantService.TenantId ?? 1;

        var document = KnowledgeDocument.Criar(
            tenantId,
            request.Title,
            request.DocumentType,
            request.Content,
            request.OriginalFileName);

        _dbContext.KnowledgeDocuments.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Realiza o chunking do texto
        var textChunks = _textChunkerService.ChunkText(request.Content, maxChunkSize: 800, overlap: 150);
        var chunksList = new List<KnowledgeChunk>();

        for (int i = 0; i < textChunks.Count; i++)
        {
            var chunkText = textChunks[i];
            var embeddingVector = await _textEmbeddingService.GenerateEmbeddingAsync(chunkText, cancellationToken);

            var chunk = KnowledgeChunk.Criar(
                tenantId,
                document.Id,
                i,
                chunkText,
                embeddingVector);

            chunksList.Add(chunk);
        }

        _dbContext.KnowledgeChunks.AddRange(chunksList);
        document.DefinirChunkCount(chunksList.Count);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(document);
        return Result<KnowledgeDocumentDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<KnowledgeDocumentDto>>> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var docs = await _dbContext.KnowledgeDocuments
            .AsNoTracking()
            .OrderByDescending(d => d.CriadoEm)
            .ToListAsync(cancellationToken);

        var dtos = docs.Select(MapToDto).ToList();
        return Result<IReadOnlyList<KnowledgeDocumentDto>>.Success(dtos);
    }

    public async Task<Result<KnowledgeDocumentDetailDto>> GetDocumentDetailsAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var doc = await _dbContext.KnowledgeDocuments
            .Include(d => d.Chunks)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (doc == null)
            return Result<KnowledgeDocumentDetailDto>.Failure($"Documento com ID {documentId} não foi encontrado.");

        var chunkDtos = doc.Chunks
            .OrderBy(c => c.ChunkIndex)
            .Select(c => new KnowledgeChunkDto(
                c.Id,
                c.DocumentId,
                c.ChunkIndex,
                c.Content,
                c.Embedding?.ToArray()?.Length ?? 0,
                c.CriadoEm))
            .ToList();

        var detailDto = new KnowledgeDocumentDetailDto(
            doc.Id,
            doc.TenantId,
            doc.Title,
            doc.DocumentType,
            GetDocumentTypeName(doc.DocumentType),
            doc.OriginalFileName,
            doc.Content,
            doc.ChunkCount,
            doc.IsActive,
            doc.CriadoEm,
            chunkDtos);

        return Result<KnowledgeDocumentDetailDto>.Success(detailDto);
    }

    public async Task<Result> DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var doc = await _dbContext.KnowledgeDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (doc == null)
            return Result.Failure($"Documento com ID {documentId} não foi encontrado.");

        _dbContext.KnowledgeDocuments.Remove(doc);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<KnowledgeSearchResultDto>>> SearchSimilarChunksAsync(
        KnowledgeSearchQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.QueryText))
            return Result<IReadOnlyList<KnowledgeSearchResultDto>>.ValidationFailure(new[] { "O texto da pesquisa não pode ser vazio." });

        var queryVector = await _textEmbeddingService.GenerateEmbeddingAsync(request.QueryText, cancellationToken);

        // EF Core 10 pgvector L2Distance query
        var chunksQuery = _dbContext.KnowledgeChunks
            .Include(c => c.Document)
            .Where(c => c.Document != null && c.Document.IsActive)
            .Where(c => c.Embedding != null);

        // Executa busca vetorial ordenada por L2Distance
        var matchedChunks = await chunksQuery
            .OrderBy(c => c.Embedding!.L2Distance(queryVector))
            .Take(Math.Max(1, request.TopK * 2)) // Busca margem para cálculo de score
            .ToListAsync(cancellationToken);

        var results = new List<KnowledgeSearchResultDto>();

        foreach (var chunk in matchedChunks)
        {
            if (chunk.Embedding == null || chunk.Document == null) continue;

            // Calcula distância L2 e converte para score de similaridade (0.0 a 1.0)
            double distance = CalculateL2Distance(chunk.Embedding, queryVector);
            // Quanto menor a distância L2, maior a similaridade
            double similarityScore = Math.Max(0.0, 1.0 - (distance / 2.0));
            // Caso especial para vetores idênticos
            if (distance < 0.05) similarityScore = 0.99;

            if (similarityScore >= request.MinSimilarity)
            {
                results.Add(new KnowledgeSearchResultDto(
                    chunk.Id,
                    chunk.DocumentId,
                    chunk.Document.Title,
                    chunk.Document.DocumentType,
                    chunk.ChunkIndex,
                    chunk.Content,
                    Math.Round(similarityScore, 4)));
            }
        }

        var topResults = results
            .OrderByDescending(r => r.SimilarityScore)
            .Take(request.TopK)
            .ToList();

        return Result<IReadOnlyList<KnowledgeSearchResultDto>>.Success(topResults);
    }

    public async Task<Result<KnowledgeSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalDocs = await _dbContext.KnowledgeDocuments.CountAsync(cancellationToken);
        var activeDocs = await _dbContext.KnowledgeDocuments.CountAsync(d => d.IsActive, cancellationToken);
        var totalChunks = await _dbContext.KnowledgeChunks.CountAsync(cancellationToken);

        var summary = new KnowledgeSummaryDto(
            totalDocs,
            activeDocs,
            totalChunks,
            "text-embedding-3-small (1536 dimensões / pgvector)");

        return Result<KnowledgeSummaryDto>.Success(summary);
    }

    private static KnowledgeDocumentDto MapToDto(KnowledgeDocument doc)
    {
        return new KnowledgeDocumentDto(
            doc.Id,
            doc.TenantId,
            doc.Title,
            doc.DocumentType,
            GetDocumentTypeName(doc.DocumentType),
            doc.OriginalFileName,
            doc.ChunkCount,
            doc.IsActive,
            doc.CriadoEm,
            doc.AtualizadoEm);
    }

    private static string GetDocumentTypeName(KnowledgeDocumentType type) => type switch
    {
        KnowledgeDocumentType.RegimentoInterno => "Regimento Interno",
        KnowledgeDocumentType.ConvencaoCondominial => "Convenção Condominial",
        KnowledgeDocumentType.RegulamentoAreaComum => "Regulamento de Áreas Comuns",
        KnowledgeDocumentType.ManualCondomino => "Manual do Condômino",
        _ => "Outros"
    };

    private static double CalculateL2Distance(Pgvector.Vector v1, Pgvector.Vector v2)
    {
        var a1 = v1.ToArray();
        var a2 = v2.ToArray();
        int len = Math.Min(a1.Length, a2.Length);
        double sum = 0;
        for (int i = 0; i < len; i++)
        {
            double diff = a1[i] - a2[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }
}
