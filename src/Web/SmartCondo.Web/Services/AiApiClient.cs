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
}
