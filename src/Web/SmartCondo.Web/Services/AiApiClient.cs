using System.Net.Http.Json;
using BuildingBlocks.Shared;
using Modules.AIEngine.Application.DTOs;

namespace SmartCondo.Web.Services;

public class AiApiClient
{
    private readonly HttpClient _httpClient;

    public AiApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<AiKernelConfigDto>?> GetConfigAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<AiKernelConfigDto>>("/api/ai/config");
        }
        catch (Exception ex)
        {
            return Result<AiKernelConfigDto>.Failure($"Erro ao obter configuração de IA: {ex.Message}");
        }
    }

    public async Task<Result<AiKernelConfigDto>?> SaveConfigAsync(SaveAiConfigCommand command)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/config", command);
            return await response.Content.ReadFromJsonAsync<Result<AiKernelConfigDto>>();
        }
        catch (Exception ex)
        {
            return Result<AiKernelConfigDto>.Failure($"Erro ao salvar configuração de IA: {ex.Message}");
        }
    }

    public async Task<Result<ExecutePromptResponseDto>?> ExecutePromptAsync(ExecutePromptRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/prompt/execute", request);
            return await response.Content.ReadFromJsonAsync<Result<ExecutePromptResponseDto>>();
        }
        catch (Exception ex)
        {
            return Result<ExecutePromptResponseDto>.Failure($"Erro ao executar prompt: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AiExecutionLogDto>>?> GetLogsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<IEnumerable<AiExecutionLogDto>>>($"/api/ai/logs?page={page}&pageSize={pageSize}");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AiExecutionLogDto>>.Failure($"Erro ao carregar logs de IA: {ex.Message}");
        }
    }

    public async Task<Result<AiSummaryDto>?> GetSummaryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<AiSummaryDto>>("/api/ai/summary");
        }
        catch (Exception ex)
        {
            return Result<AiSummaryDto>.Failure($"Erro ao obter resumo de IA: {ex.Message}");
        }
    }

    // ================================================
    // Métodos para RAG / Base de Conhecimento
    // ================================================

    public async Task<Result<KnowledgeDocumentDto>?> UploadKnowledgeDocumentAsync(UploadKnowledgeDocumentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/knowledge/upload", request);
            return await response.Content.ReadFromJsonAsync<Result<KnowledgeDocumentDto>>();
        }
        catch (Exception ex)
        {
            return Result<KnowledgeDocumentDto>.Failure($"Erro ao cadastrar documento RAG: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<KnowledgeDocumentDto>>?> GetKnowledgeDocumentsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<IReadOnlyList<KnowledgeDocumentDto>>>("/api/ai/knowledge/documents");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<KnowledgeDocumentDto>>.Failure($"Erro ao listar documentos RAG: {ex.Message}");
        }
    }

    public async Task<Result<KnowledgeDocumentDetailDto>?> GetKnowledgeDocumentDetailsAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<KnowledgeDocumentDetailDto>>($"/api/ai/knowledge/documents/{id}");
        }
        catch (Exception ex)
        {
            return Result<KnowledgeDocumentDetailDto>.Failure($"Erro ao obter detalhes do documento RAG: {ex.Message}");
        }
    }

    public async Task<Result?> DeleteKnowledgeDocumentAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/ai/knowledge/documents/{id}");
            return await response.Content.ReadFromJsonAsync<Result>();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao excluir documento RAG: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<KnowledgeSearchResultDto>>?> SearchKnowledgeChunksAsync(KnowledgeSearchQueryRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/knowledge/search", request);
            return await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<KnowledgeSearchResultDto>>>();
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<KnowledgeSearchResultDto>>.Failure($"Erro na busca vetorial RAG: {ex.Message}");
        }
    }

    public async Task<Result<KnowledgeSummaryDto>?> GetKnowledgeSummaryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<KnowledgeSummaryDto>>("/api/ai/knowledge/summary");
        }
        catch (Exception ex)
        {
            return Result<KnowledgeSummaryDto>.Failure($"Erro ao obter resumo da base RAG: {ex.Message}");
        }
    }
}
