using System.Net.Http.Json;
using BuildingBlocks.Shared;
using Modules.WhatsApp.Application.DTOs;

namespace SmartCondo.Web.Services;

public class WhatsAppApiClient
{
    private readonly HttpClient _httpClient;

    public WhatsAppApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<WhatsAppWebhookSummaryDto>?> GetSummaryAsync(int? condoId = null)
    {
        try
        {
            var url = "/api/whatsapp/summary";
            if (condoId.HasValue && condoId.Value > 0)
            {
                url += $"?condoId={condoId.Value}";
            }
            return await _httpClient.GetFromJsonAsync<Result<WhatsAppWebhookSummaryDto>>(url);
        }
        catch (Exception ex)
        {
            return Result<WhatsAppWebhookSummaryDto>.Failure($"Erro ao obter resumo de WhatsApp: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WhatsAppInstanceConfigDto>>?> GetInstancesAsync(int? condoId = null)
    {
        try
        {
            var url = "/api/whatsapp/instances";
            if (condoId.HasValue && condoId.Value > 0)
            {
                url += $"?condoId={condoId.Value}";
            }
            return await _httpClient.GetFromJsonAsync<Result<IEnumerable<WhatsAppInstanceConfigDto>>>(url);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<WhatsAppInstanceConfigDto>>.Failure($"Erro ao carregar instâncias: {ex.Message}");
        }
    }

    public async Task<Result<WhatsAppInstanceConfigDto>?> CreateInstanceAsync(CreateWhatsAppInstanceCommand command)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/whatsapp/instances", command);
            return await response.Content.ReadFromJsonAsync<Result<WhatsAppInstanceConfigDto>>();
        }
        catch (Exception ex)
        {
            return Result<WhatsAppInstanceConfigDto>.Failure($"Erro ao cadastrar instância: {ex.Message}");
        }
    }

    public async Task<Result<WhatsAppInstanceConfigDto>?> ToggleInstanceStatusAsync(int instanceId)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"/api/whatsapp/instances/{instanceId}/status", null);
            return await response.Content.ReadFromJsonAsync<Result<WhatsAppInstanceConfigDto>>();
        }
        catch (Exception ex)
        {
            return Result<WhatsAppInstanceConfigDto>.Failure($"Erro ao alternar status da instância: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WhatsAppWebhookLogDto>>?> GetWebhookLogsAsync(
        string? instanceName = null,
        string? status = null,
        string? phone = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = $"/api/whatsapp/logs?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(instanceName)) query += $"&instanceName={Uri.EscapeDataString(instanceName)}";
            if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status)}";
            if (!string.IsNullOrWhiteSpace(phone)) query += $"&phone={Uri.EscapeDataString(phone)}";

            return await _httpClient.GetFromJsonAsync<Result<IEnumerable<WhatsAppWebhookLogDto>>>(query);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<WhatsAppWebhookLogDto>>.Failure($"Erro ao consultar logs de webhooks: {ex.Message}");
        }
    }
}
