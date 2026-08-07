using System.Net.Http.Json;
using BuildingBlocks.Shared;
using Modules.AIEngine.Application.DTOs;
using Modules.Financial.Application.DTOs;

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

    // ================================================
    // Métodos para Plugins / Function Calling
    // ================================================

    public async Task<Result<BoletoPluginExecutionResultDto>?> ExecuteBoletoPluginAsync(int moradorId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/plugins/boletos/execute", new { MoradorId = moradorId });
            return await response.Content.ReadFromJsonAsync<Result<BoletoPluginExecutionResultDto>>();
        }
        catch (Exception ex)
        {
            return Result<BoletoPluginExecutionResultDto>.Failure($"Erro ao executar plugin de boleto: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Modules.Financial.Application.DTOs.PendingBoletoDto>>?> GetPendingBoletosByMoradorAsync(int moradorId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<IEnumerable<Modules.Financial.Application.DTOs.PendingBoletoDto>>>($"/api/ai/plugins/boletos/pending/{moradorId}");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Modules.Financial.Application.DTOs.PendingBoletoDto>>.Failure($"Erro ao consultar boletos pendentes: {ex.Message}");
        }
    }

    public async Task<Result<Modules.AIEngine.Endpoints.ReservaPluginExecutionResultDto>?> ExecuteReservaPluginAsync(Modules.AIEngine.Endpoints.ExecuteReservaPluginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/plugins/reservas/execute", request);
            return await response.Content.ReadFromJsonAsync<Result<Modules.AIEngine.Endpoints.ReservaPluginExecutionResultDto>>();
        }
        catch (Exception ex)
        {
            return Result<Modules.AIEngine.Endpoints.ReservaPluginExecutionResultDto>.Failure($"Erro ao executar plugin de reserva de área comum: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Modules.Operations.Application.DTOs.AreaComumDto>>?> GetActiveAreasComunsAsync(int condoId = 1)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Result<IEnumerable<Modules.Operations.Application.DTOs.AreaComumDto>>>($"/api/ai/plugins/reservas/areas?condoId={condoId}");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Modules.Operations.Application.DTOs.AreaComumDto>>.Failure($"Erro ao consultar áreas comuns ativas: {ex.Message}");
        }
    }

    public async Task<Result<Modules.AIEngine.Endpoints.AuthorizeGuestPluginExecutionResultDto>?> ExecutePortariaPluginAsync(Modules.AIEngine.Endpoints.ExecuteAuthorizeGuestPluginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/plugins/portaria/execute", request);
            return await response.Content.ReadFromJsonAsync<Result<Modules.AIEngine.Endpoints.AuthorizeGuestPluginExecutionResultDto>>();
        }
        catch (Exception ex)
        {
            return Result<Modules.AIEngine.Endpoints.AuthorizeGuestPluginExecutionResultDto>.Failure($"Erro ao executar plugin de portaria: {ex.Message}");
        }
    }
}

