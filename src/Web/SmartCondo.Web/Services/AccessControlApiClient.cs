using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Endpoints;

namespace SmartCondo.Web.Services;

public sealed class AccessControlApiClient(HttpClient httpClient, AuthSession session)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public async Task<ApiResult<IEnumerable<VisitanteDto>>> GetVisitantesAsync(
        TipoVisitante? tipo = null,
        StatusVisitante? status = null,
        int? unidadeId = null,
        string? busca = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string>();
            if (tipo.HasValue) queryParams.Add($"tipo={(int)tipo.Value}");
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
            if (unidadeId.HasValue && unidadeId.Value > 0) queryParams.Add($"unidadeId={unidadeId.Value}");
            if (!string.IsNullOrWhiteSpace(busca)) queryParams.Add($"busca={Uri.EscapeDataString(busca)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/access-control/visitors{queryString}", null, ct);
            return await ParseAsync<IEnumerable<VisitanteDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<VisitanteDto>>(ex);
        }
    }

    public async Task<ApiResult<VisitanteDto>> GetVisitanteByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/access-control/visitors/{id}", null, ct);
            return await ParseAsync<VisitanteDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<VisitanteDto>(ex);
        }
    }

    public async Task<ApiResult<VisitanteDto>> AuthorizeVisitanteAsync(CreateVisitanteRequestDto request, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/access-control/visitors", request, ct);
            return await ParseAsync<VisitanteDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<VisitanteDto>(ex);
        }
    }

    public async Task<ApiResult<VisitanteDto>> RegistrarEntradaAsync(int id, int? operadorId = null, CancellationToken ct = default)
    {
        try
        {
            var body = new RegistrarEntradaRequestDto(operadorId);
            using var response = await SendAuthorizedAsync(HttpMethod.Post, $"/api/access-control/visitors/{id}/entry", body, ct);
            return await ParseAsync<VisitanteDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<VisitanteDto>(ex);
        }
    }

    public async Task<ApiResult<VisitanteDto>> RegistrarSaidaAsync(int id, int? operadorId = null, CancellationToken ct = default)
    {
        try
        {
            var body = new RegistrarSaidaRequestDto(operadorId);
            using var response = await SendAuthorizedAsync(HttpMethod.Post, $"/api/access-control/visitors/{id}/exit", body, ct);
            return await ParseAsync<VisitanteDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<VisitanteDto>(ex);
        }
    }

    public async Task<ApiResult<VisitanteDto>> CancelarAutorizacaoAsync(int id, string? motivo = null, CancellationToken ct = default)
    {
        try
        {
            var body = new CancelarVisitanteRequest(id, motivo);
            using var response = await SendAuthorizedAsync(new HttpMethod("PATCH"), $"/api/access-control/visitors/{id}/cancel", body, ct);
            return await ParseAsync<VisitanteDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<VisitanteDto>(ex);
        }
    }

    public async Task<ApiResult<VisitanteSummaryDto>> GetSummaryAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "/api/access-control/visitors/summary", null, ct);
            return await ParseAsync<VisitanteSummaryDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<VisitanteSummaryDto>(ex);
        }
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct)
    {
        await session.EnsureLoadedAsync();
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (!string.IsNullOrWhiteSpace(session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        if (session.Context?.TenantId > 0)
        {
            request.Headers.TryAddWithoutValidation("X-Tenant-Id", session.Context.TenantId.ToString());
        }

        if (session.Context?.CondoId > 0)
        {
            request.Headers.TryAddWithoutValidation("X-Condo-Id", session.Context.CondoId.ToString());
        }

        return await httpClient.SendAsync(request, ct);
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
        new(false, default, $"Servidor backend (SmartCondo.Api) não está acessível. ({ex.Message})", 503);
}
