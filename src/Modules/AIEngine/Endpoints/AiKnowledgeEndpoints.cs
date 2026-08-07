using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Infrastructure.Services;

namespace Modules.AIEngine.Endpoints;

/// <summary>
/// Endpoint para envio e processamento de documentos RAG (Convenção/Regimento Interno).
/// </summary>
public sealed class UploadKnowledgeDocumentEndpoint : Endpoint<UploadKnowledgeDocumentRequest, Result<KnowledgeDocumentDto>>
{
    private readonly IKnowledgeBaseService _service;

    public UploadKnowledgeDocumentEndpoint(IKnowledgeBaseService service) => _service = service;

    public override void Configure()
    {
        Post("/api/ai/knowledge/upload");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cadastrar Documento RAG";
            s.Description = "Cadastra, fragmenta e indexa vetorialmente no pgvector um documento de Regimento Interno ou Convenção Condominial.";
        });
    }

    public override async Task HandleAsync(UploadKnowledgeDocumentRequest req, CancellationToken ct)
    {
        var result = await _service.UploadAndProcessDocumentAsync(req, ct);
        var statusCode = result.IsSuccess ? 201 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para listar os documentos RAG cadastrados do condomínio.
/// </summary>
public sealed class GetKnowledgeDocumentsEndpoint : EndpointWithoutRequest<Result<IReadOnlyList<KnowledgeDocumentDto>>>
{
    private readonly IKnowledgeBaseService _service;

    public GetKnowledgeDocumentsEndpoint(IKnowledgeBaseService service) => _service = service;

    public override void Configure()
    {
        Get("/api/ai/knowledge/documents");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar Documentos RAG";
            s.Description = "Retorna todos os documentos normativos indexados do condomínio atual.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetDocumentsAsync(ct);
        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint para obter detalhes e trechos de um documento RAG.
/// </summary>
public sealed class GetKnowledgeDocumentDetailsEndpoint : EndpointWithoutRequest<Result<KnowledgeDocumentDetailDto>>
{
    private readonly IKnowledgeBaseService _service;

    public GetKnowledgeDocumentDetailsEndpoint(IKnowledgeBaseService service) => _service = service;

    public override void Configure()
    {
        Get("/api/ai/knowledge/documents/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter Detalhes do Documento RAG";
            s.Description = "Retorna os metadados, conteúdo bruto e trechos vetoriais do documento especificado pelo ID.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await _service.GetDocumentDetailsAsync(id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para inativar ou remover um documento RAG.
/// </summary>
public sealed class DeleteKnowledgeDocumentEndpoint : EndpointWithoutRequest<Result>
{
    private readonly IKnowledgeBaseService _service;

    public DeleteKnowledgeDocumentEndpoint(IKnowledgeBaseService service) => _service = service;

    public override void Configure()
    {
        Delete("/api/ai/knowledge/documents/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Excluir Documento RAG";
            s.Description = "Remove um documento RAG e todos os seus fragmentos vetoriais do pgvector.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await _service.DeleteDocumentAsync(id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para consulta de similaridade vetorial (pgvector RAG query).
/// </summary>
public sealed class SearchKnowledgeChunksEndpoint : Endpoint<KnowledgeSearchQueryRequest, Result<IReadOnlyList<KnowledgeSearchResultDto>>>
{
    private readonly IKnowledgeBaseService _service;

    public SearchKnowledgeChunksEndpoint(IKnowledgeBaseService service) => _service = service;

    public override void Configure()
    {
        Post("/api/ai/knowledge/search");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Busca Vetorial RAG no Pgvector";
            s.Description = "Realiza busca semântica por similaridade vetorial no pgvector a partir de uma dúvida em linguagem natural.";
        });
    }

    public override async Task HandleAsync(KnowledgeSearchQueryRequest req, CancellationToken ct)
    {
        var result = await _service.SearchSimilarChunksAsync(req, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para obter os dados do resumo KPI da Base de Conhecimento RAG.
/// </summary>
public sealed class GetKnowledgeSummaryEndpoint : EndpointWithoutRequest<Result<KnowledgeSummaryDto>>
{
    private readonly IKnowledgeBaseService _service;

    public GetKnowledgeSummaryEndpoint(IKnowledgeBaseService service) => _service = service;

    public override void Configure()
    {
        Get("/api/ai/knowledge/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI da Base RAG";
            s.Description = "Retorna métricas da base de conhecimento RAG: documentos ativos, total de fragmentos e modelo de embedding em uso.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(ct);
        await SendAsync(result, 200, ct);
    }
}
