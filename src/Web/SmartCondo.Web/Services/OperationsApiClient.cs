using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace SmartCondo.Web.Services;

public sealed class OperationsApiClient(HttpClient httpClient, AuthSession session)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/common-areas{queryString}", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/common-areas/{id}", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/operations/common-areas", request, ct);
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

            using var response = await SendAuthorizedAsync(HttpMethod.Put, $"/api/operations/common-areas/{id}", body, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Patch, $"/api/operations/common-areas/{id}/status", body, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/common-areas/summary?condoId={condoId}", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/reservations{queryString}", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/reservations/{id}", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/operations/reservations", request, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Patch, $"/api/operations/reservations/{id}/cancel", body, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Patch, $"/api/operations/reservations/{id}/approve", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/reservations/summary?condoId={condoId}", null, ct);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/reservations/calendar{queryString}", null, ct);
            return await ParseAsync<IEnumerable<ReservaCalendarSlotDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ReservaCalendarSlotDto>>(ex);
        }
    }

    // --- Módulo de Ocorrências e Chamados ---

    public async Task<ApiResult<IEnumerable<OcorrenciaDto>>> GetTicketsAsync(
        int condoId = 1,
        StatusOcorrencia? status = null,
        CategoriaOcorrencia? categoria = null,
        PrioridadeOcorrencia? prioridade = null,
        string? moradorId = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
            if (categoria.HasValue) queryParams.Add($"categoria={(int)categoria.Value}");
            if (prioridade.HasValue) queryParams.Add($"prioridade={(int)prioridade.Value}");
            if (!string.IsNullOrWhiteSpace(moradorId)) queryParams.Add($"moradorId={moradorId}");

            var queryString = "?" + string.Join("&", queryParams);
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/tickets{queryString}", null, ct);
            return await ParseAsync<IEnumerable<OcorrenciaDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<OcorrenciaDto>>(ex);
        }
    }

    public async Task<ApiResult<OcorrenciaDto>> GetTicketByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/tickets/{id}", null, ct);
            return await ParseAsync<OcorrenciaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<OcorrenciaDto>(ex);
        }
    }

    public async Task<ApiResult<OcorrenciaDto>> CreateTicketAsync(CriarOcorrenciaRequest request, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/operations/tickets", request, ct);
            return await ParseAsync<OcorrenciaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<OcorrenciaDto>(ex);
        }
    }

    public async Task<ApiResult<OcorrenciaDto>> UpdateTicketStatusAsync(
        Guid id,
        StatusOcorrencia novoStatus,
        string comentario,
        string usuarioId,
        string usuarioNome,
        string? observacaoResolucao = null,
        CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                Id = id,
                NovoStatus = novoStatus,
                Comentario = comentario,
                UsuarioId = usuarioId,
                UsuarioNome = usuarioNome,
                ObservacaoResolucao = observacaoResolucao
            };

            using var response = await SendAuthorizedAsync(HttpMethod.Patch, $"/api/operations/tickets/{id}/status", body, ct);
            return await ParseAsync<OcorrenciaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<OcorrenciaDto>(ex);
        }
    }

    public async Task<ApiResult<OcorrenciaSummaryDto>> GetTicketSummaryAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/tickets/summary?condoId={condoId}", null, ct);
            return await ParseAsync<OcorrenciaSummaryDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<OcorrenciaSummaryDto>(ex);
        }
    }

    // --- Módulo de Manutenção Preventiva ---

    public async Task<ApiResult<IEnumerable<PlanoManutencaoDto>>> GetMaintenancePlansAsync(
        int condoId = 1,
        CategoriaManutencao? categoria = null,
        StatusManutencao? status = null,
        PeriodicidadeManutencao? periodicidade = null,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (categoria.HasValue) queryParams.Add($"categoria={(int)categoria.Value}");
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
            if (periodicidade.HasValue) queryParams.Add($"periodicidade={(int)periodicidade.Value}");
            if (inicio.HasValue) queryParams.Add($"inicio={inicio.Value:o}");
            if (fim.HasValue) queryParams.Add($"fim={fim.Value:o}");

            var queryString = "?" + string.Join("&", queryParams);
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/maintenance{queryString}", null, ct);
            return await ParseAsync<IEnumerable<PlanoManutencaoDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<PlanoManutencaoDto>>(ex);
        }
    }

    public async Task<ApiResult<PlanoManutencaoDto>> GetMaintenancePlanByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/maintenance/{id}", null, ct);
            return await ParseAsync<PlanoManutencaoDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PlanoManutencaoDto>(ex);
        }
    }

    public async Task<ApiResult<PlanoManutencaoDto>> CreateMaintenancePlanAsync(CreatePlanoManutencaoRequest request, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/operations/maintenance", request, ct);
            return await ParseAsync<PlanoManutencaoDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PlanoManutencaoDto>(ex);
        }
    }

    public async Task<ApiResult<PlanoManutencaoDto>> UpdateMaintenancePlanAsync(Guid id, UpdatePlanoManutencaoRequest request, CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                Id = id,
                request.Titulo,
                request.Descricao,
                request.Categoria,
                request.Periodicidade,
                request.DataProximaManutencao,
                request.ResponsavelTecnico,
                request.EmpresaContratada,
                request.CustoEstimado,
                request.Observacoes
            };

            using var response = await SendAuthorizedAsync(HttpMethod.Put, $"/api/operations/maintenance/{id}", body, ct);
            return await ParseAsync<PlanoManutencaoDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PlanoManutencaoDto>(ex);
        }
    }

    public async Task<ApiResult<PlanoManutencaoDto>> CompleteMaintenancePlanAsync(Guid id, ConcluirManutencaoRequest request, CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                Id = id,
                request.DataRealizacao,
                request.CustoReal,
                request.Observacoes,
                request.AgendarProxima
            };

            using var response = await SendAuthorizedAsync(HttpMethod.Post, $"/api/operations/maintenance/{id}/complete", body, ct);
            return await ParseAsync<PlanoManutencaoDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PlanoManutencaoDto>(ex);
        }
    }

    public async Task<ApiResult<PlanoManutencaoSummaryDto>> GetMaintenanceSummaryAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/maintenance/summary?condoId={condoId}", null, ct);
            return await ParseAsync<PlanoManutencaoSummaryDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PlanoManutencaoSummaryDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<ManutencaoCalendarEventDto>>> GetMaintenanceCalendarAsync(
        int condoId = 1,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (inicio.HasValue) queryParams.Add($"inicio={inicio.Value:o}");
            if (fim.HasValue) queryParams.Add($"fim={fim.Value:o}");

            var queryString = "?" + string.Join("&", queryParams);
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/operations/maintenance/calendar{queryString}", null, ct);
            return await ParseAsync<IEnumerable<ManutencaoCalendarEventDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ManutencaoCalendarEventDto>>(ex);
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
        new(false, default, $"Servidor backend (SmartCondo.Api) não está acessível em http://localhost:5127. Certifique-se de que a API está rodando. ({ex.Message})", 503);
}
