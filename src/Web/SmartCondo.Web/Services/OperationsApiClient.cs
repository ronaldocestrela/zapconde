using System.Net.Http.Json;
using System.Text.Json;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace SmartCondo.Web.Services;

public sealed class OperationsApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApiResult<IEnumerable<AreaComumDto>>> GetAreasComunsAsync(
        int condoId = 1,
        StatusAreaComum? status = null,
        TipoAreaComum? tipo = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
            if (tipo.HasValue) queryParams.Add($"tipo={(int)tipo.Value}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await httpClient.GetAsync($"/api/operations/common-areas{queryString}", ct);
            return await ParseAsync<IEnumerable<AreaComumDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<AreaComumDto>>(ex);
        }
    }

    public async Task<ApiResult<AreaComumDto>> GetAreaComumByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/operations/common-areas/{id}", ct);
            return await ParseAsync<AreaComumDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<AreaComumDto>(ex);
        }
    }

    public async Task<ApiResult<AreaComumDto>> CreateAreaComumAsync(CreateAreaComumRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/operations/common-areas", request, JsonOptions, ct);
            return await ParseAsync<AreaComumDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<AreaComumDto>(ex);
        }
    }

    public async Task<ApiResult<AreaComumDto>> UpdateAreaComumAsync(int id, UpdateAreaComumRequest request, CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                Id = id,
                request.Nome,
                request.Descricao,
                request.Tipo,
                request.CapacidadeMaxima,
                request.TaxaReserva,
                request.TaxaLimpeza,
                request.HorarioInicioFuncionamento,
                request.HorarioFimFuncionamento,
                request.TempoAntecedenciaMinimaDias,
                request.TempoAntecedenciaMaximaDias,
                request.RequerAprovacaoSindico,
                request.RegrasUso
            };

            var response = await httpClient.PutAsJsonAsync($"/api/operations/common-areas/{id}", body, JsonOptions, ct);
            return await ParseAsync<AreaComumDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<AreaComumDto>(ex);
        }
    }

    public async Task<ApiResult<AreaComumDto>> ChangeStatusAsync(int id, StatusAreaComum novoStatus, CancellationToken ct = default)
    {
        try
        {
            var body = new { Id = id, NovoStatus = novoStatus };
            var response = await httpClient.PatchAsJsonAsync($"/api/operations/common-areas/{id}/status", body, JsonOptions, ct);
            return await ParseAsync<AreaComumDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<AreaComumDto>(ex);
        }
    }

    public async Task<ApiResult<AreaComumSummaryDto>> GetSummaryAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/operations/common-areas/summary?condoId={condoId}", ct);
            return await ParseAsync<AreaComumSummaryDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<AreaComumSummaryDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<ReservaDto>>> GetReservasAsync(
        int condoId = 1,
        int? areaComumId = null,
        int? moradorId = null,
        StatusReserva? status = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (areaComumId.HasValue) queryParams.Add($"areaComumId={areaComumId.Value}");
            if (moradorId.HasValue) queryParams.Add($"moradorId={moradorId.Value}");
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
            if (dataInicio.HasValue) queryParams.Add($"dataInicio={dataInicio.Value:o}");
            if (dataFim.HasValue) queryParams.Add($"dataFim={dataFim.Value:o}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await httpClient.GetAsync($"/api/operations/reservations{queryString}", ct);
            return await ParseAsync<IEnumerable<ReservaDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ReservaDto>>(ex);
        }
    }

    public async Task<ApiResult<ReservaDto>> GetReservaByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/operations/reservations/{id}", ct);
            return await ParseAsync<ReservaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ReservaDto>(ex);
        }
    }

    public async Task<ApiResult<ReservaDto>> CreateReservaAsync(CreateReservaRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/operations/reservations", request, JsonOptions, ct);
            return await ParseAsync<ReservaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ReservaDto>(ex);
        }
    }

    public async Task<ApiResult<ReservaDto>> CancelarReservaAsync(int id, string motivo, CancellationToken ct = default)
    {
        try
        {
            var body = new CancelarReservaRequest(motivo);
            var response = await httpClient.PatchAsJsonAsync($"/api/operations/reservations/{id}/cancel", body, JsonOptions, ct);
            return await ParseAsync<ReservaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ReservaDto>(ex);
        }
    }

    public async Task<ApiResult<ReservaDto>> AprovarReservaAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PatchAsync($"/api/operations/reservations/{id}/approve", null, ct);
            return await ParseAsync<ReservaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ReservaDto>(ex);
        }
    }

    public async Task<ApiResult<ReservaSummaryDto>> GetReservaSummaryAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/operations/reservations/summary?condoId={condoId}", ct);
            return await ParseAsync<ReservaSummaryDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ReservaSummaryDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<ReservaCalendarSlotDto>>> GetReservaCalendarAsync(
        int condoId = 1,
        int? areaComumId = null,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (areaComumId.HasValue) queryParams.Add($"areaComumId={areaComumId.Value}");
            if (inicio.HasValue) queryParams.Add($"inicio={inicio.Value:o}");
            if (fim.HasValue) queryParams.Add($"fim={fim.Value:o}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await httpClient.GetAsync($"/api/operations/reservations/calendar{queryString}", ct);
            return await ParseAsync<IEnumerable<ReservaCalendarSlotDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ReservaCalendarSlotDto>>(ex);
        }
    }

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var statusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ApiResult<T>(false, default, $"HTTP {statusCode}: resposta vazia.", statusCode);
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            bool isSuccess = root.TryGetProperty("isSuccess", out var isSuccessProp) && isSuccessProp.GetBoolean();
            string message = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? string.Empty : string.Empty;

            T? data = default;
            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
            {
                data = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), JsonOptions);
            }

            return new ApiResult<T>(isSuccess && response.IsSuccessStatusCode, data, message, statusCode);
        }
        catch (JsonException ex)
        {
            return new ApiResult<T>(false, default, $"Erro ao processar JSON: {ex.Message}", statusCode);
        }
    }

    private static ApiResult<T> ConnectionFailure<T>(Exception ex) =>
        new(false, default, $"Falha de conexão com a API: {ex.Message}", 503);
}
