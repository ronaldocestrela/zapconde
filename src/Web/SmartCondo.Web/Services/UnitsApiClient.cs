using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SmartCondo.Web.Services;

public sealed class UnitsApiClient(HttpClient httpClient, AuthSession session)
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApiResult<List<BlockModel>>> GetBlocksAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await SendAuthorizedAsync(HttpMethod.Get, "/api/blocks", null, ct);
            return await ParseAsync<List<BlockModel>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<List<BlockModel>>(ex);
        }
    }

    public async Task<ApiResult<List<UnitListItemModel>>> GetUnitsAsync(UnitListQueryModel query, CancellationToken ct = default)
    {
        try
        {
            var qs = BuildQuery(query);
            var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/units{qs}", null, ct);
            return await ParseAsync<List<UnitListItemModel>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<List<UnitListItemModel>>(ex);
        }
    }

    public async Task<ApiResult<UnitCreatedModel>> CreateUnitAsync(CreateUnitModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/units", request, ct);
            return await ParseAsync<UnitCreatedModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<UnitCreatedModel>(ex);
        }
    }

    public async Task<ApiResult<UnitListItemModel>> UpdateUnitAsync(int unitId, UpdateUnitModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await SendAuthorizedAsync(HttpMethod.Put, $"/api/units/{unitId}", request, ct);
            return await ParseAsync<UnitListItemModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<UnitListItemModel>(ex);
        }
    }

    public async Task<ApiResult<object>> TransferOwnershipAsync(int unitId, TransferOwnershipModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await SendAuthorizedAsync(HttpMethod.Post, $"/api/units/{unitId}/transfer", request, ct);
            return await ParseAsync<object>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<object>(ex);
        }
    }

    public async Task<ApiResult<List<UnitHistoryItemModel>>> GetHistoryAsync(int unitId, CancellationToken ct = default)
    {
        try
        {
            var response = await SendAuthorizedAsync(HttpMethod.Get, $"/api/units/{unitId}/history", null, ct);
            return await ParseAsync<List<UnitHistoryItemModel>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<List<UnitHistoryItemModel>>(ex);
        }
    }

    public async Task<ApiResult<ImportPreviewModel>> PreviewImportAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        try
        {
            await session.EnsureLoadedAsync();
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            content.Add(streamContent, "file", fileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/units/import/preview") { Content = content };
            if (!string.IsNullOrWhiteSpace(session.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
            }

            var response = await httpClient.SendAsync(request, ct);
            return await ParseAsync<ImportPreviewModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ImportPreviewModel>(ex);
        }
    }

    public async Task<ApiResult<ImportCommitResultModel>> CommitImportAsync(ImportCommitModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/units/import/commit", request, ct);
            return await ParseAsync<ImportCommitResultModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ImportCommitResultModel>(ex);
        }
    }

    public string GetTemplateUrl() =>
        $"{httpClient.BaseAddress?.ToString().TrimEnd('/')}/api/units/import/template";

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        await session.EnsureLoadedAsync();
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        if (!string.IsNullOrWhiteSpace(session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        return await httpClient.SendAsync(request, ct);
    }

    private static string BuildQuery(UnitListQueryModel query)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(query.Q)) parts.Add($"q={Uri.EscapeDataString(query.Q)}");
        if (query.BlockId.HasValue) parts.Add($"blockId={query.BlockId}");
        if (!string.IsNullOrWhiteSpace(query.Status)) parts.Add($"status={query.Status}");
        if (!string.IsNullOrWhiteSpace(query.Papel)) parts.Add($"papel={query.Papel}");
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private ApiResult<T> ConnectionFailure<T>(Exception ex)
    {
        var baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "API";
        return new ApiResult<T>(false, default, $"Não foi possível conectar à API em {baseUrl}. {ex.Message}", 0);
    }

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var statusCode = (int)response.StatusCode;
        var json = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new ApiResult<T>(false, default, $"Resposta vazia da API (HTTP {statusCode}).", statusCode);
        }

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        var isSuccess = root.GetProperty("isSuccess").GetBoolean();
        var message = root.TryGetProperty("message", out var msg) ? msg.GetString() ?? string.Empty : string.Empty;

        if (!isSuccess)
        {
            return new ApiResult<T>(false, default, message, statusCode);
        }

        if (!root.TryGetProperty("data", out var data))
        {
            return new ApiResult<T>(true, default, message, statusCode);
        }

        var payload = System.Text.Json.JsonSerializer.Deserialize<T>(data.GetRawText(), JsonOptions);
        return new ApiResult<T>(true, payload, message, statusCode);
    }
}

public sealed class BlockModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
}

public sealed class UnitListItemModel
{
    [JsonPropertyName("unitId")] public int UnitId { get; set; }
    [JsonPropertyName("blocoId")] public int BlocoId { get; set; }
    [JsonPropertyName("blocoCodigo")] public string BlocoCodigo { get; set; } = string.Empty;
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("moradorNome")] public string? MoradorNome { get; set; }
    [JsonPropertyName("papel")] public string? Papel { get; set; }
    [JsonPropertyName("moradorTelefone")] public string? MoradorTelefone { get; set; }
    [JsonPropertyName("phoneVerificationStatus")] public int PhoneVerificationStatus { get; set; }
    [JsonPropertyName("dataInicio")] public DateTime? DataInicio { get; set; }
    [JsonPropertyName("moradorId")] public int? MoradorId { get; set; }
}

public sealed class CreateUnitModel
{
    [JsonPropertyName("blocoId")] public int? BlocoId { get; set; }
    [JsonPropertyName("blocoCodigo")] public string? BlocoCodigo { get; set; }
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = "Vaga";
    [JsonPropertyName("moradorNome")] public string MoradorNome { get; set; } = string.Empty;
    [JsonPropertyName("moradorCpf")] public string MoradorCpf { get; set; } = string.Empty;
    [JsonPropertyName("moradorEmail")] public string MoradorEmail { get; set; } = string.Empty;
    [JsonPropertyName("moradorTelefone")] public string MoradorTelefone { get; set; } = string.Empty;
    [JsonPropertyName("papel")] public string Papel { get; set; } = "Proprietario";
    [JsonPropertyName("dataInicio")] public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("dependencias")] public List<string> Dependencias { get; set; } = [];
}

public sealed class UpdateUnitModel
{
    [JsonPropertyName("status")] public string Status { get; set; } = "Ocupada";
    [JsonPropertyName("moradorNome")] public string MoradorNome { get; set; } = string.Empty;
    [JsonPropertyName("moradorCpf")] public string MoradorCpf { get; set; } = string.Empty;
    [JsonPropertyName("moradorEmail")] public string MoradorEmail { get; set; } = string.Empty;
    [JsonPropertyName("moradorTelefone")] public string MoradorTelefone { get; set; } = string.Empty;
    [JsonPropertyName("papel")] public string Papel { get; set; } = "Proprietario";
    [JsonPropertyName("dependencias")] public List<string> Dependencias { get; set; } = [];
}

public sealed class TransferOwnershipModel
{
    [JsonPropertyName("dataEncerramento")] public DateTime DataEncerramento { get; set; }
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = string.Empty;
    [JsonPropertyName("papel")] public string Papel { get; set; } = "Proprietario";
    [JsonPropertyName("novoMoradorNome")] public string NovoMoradorNome { get; set; } = string.Empty;
    [JsonPropertyName("novoMoradorCpf")] public string NovoMoradorCpf { get; set; } = string.Empty;
    [JsonPropertyName("novoMoradorEmail")] public string NovoMoradorEmail { get; set; } = string.Empty;
    [JsonPropertyName("novoMoradorTelefone")] public string NovoMoradorTelefone { get; set; } = string.Empty;
    [JsonPropertyName("dataInicio")] public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("dependencias")] public List<string> Dependencias { get; set; } = [];
}

public sealed class UnitHistoryItemModel
{
    [JsonPropertyName("vinculoId")] public int VinculoId { get; set; }
    [JsonPropertyName("moradorNome")] public string MoradorNome { get; set; } = string.Empty;
    [JsonPropertyName("papel")] public string Papel { get; set; } = string.Empty;
    [JsonPropertyName("dataInicio")] public DateTime DataInicio { get; set; }
    [JsonPropertyName("dataFim")] public DateTime? DataFim { get; set; }
    [JsonPropertyName("motivoEncerramento")] public string? MotivoEncerramento { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("createdByUserId")] public string? CreatedByUserId { get; set; }
}

public sealed class UnitListQueryModel
{
    public string? Q { get; set; }
    public int? BlockId { get; set; }
    public string? Status { get; set; }
    public string? Papel { get; set; }
}

public sealed class ImportPreviewModel
{
    [JsonPropertyName("totalRows")] public int TotalRows { get; set; }
    [JsonPropertyName("validRows")] public int ValidRows { get; set; }
    [JsonPropertyName("invalidRows")] public int InvalidRows { get; set; }
    [JsonPropertyName("rows")] public List<ImportPreviewRowModel> Rows { get; set; } = [];
}

public sealed class ImportPreviewRowModel
{
    [JsonPropertyName("rowNumber")] public int RowNumber { get; set; }
    [JsonPropertyName("blocoCodigo")] public string BlocoCodigo { get; set; } = string.Empty;
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("moradorNome")] public string MoradorNome { get; set; } = string.Empty;
    [JsonPropertyName("moradorCpf")] public string MoradorCpf { get; set; } = string.Empty;
    [JsonPropertyName("moradorEmail")] public string MoradorEmail { get; set; } = string.Empty;
    [JsonPropertyName("moradorTelefone")] public string MoradorTelefone { get; set; } = string.Empty;
    [JsonPropertyName("papel")] public string Papel { get; set; } = string.Empty;
    [JsonPropertyName("isValid")] public bool IsValid { get; set; }
    [JsonPropertyName("errors")] public List<string> Errors { get; set; } = [];
}

public sealed class ImportCommitModel
{
    [JsonPropertyName("rows")] public List<ImportPreviewRowModel> Rows { get; set; } = [];
}

public sealed class ImportCommitResultModel
{
    [JsonPropertyName("importedCount")] public int ImportedCount { get; set; }
    [JsonPropertyName("skippedCount")] public int SkippedCount { get; set; }
}

public sealed record UnitCreatedModel(
    [property: JsonPropertyName("unitId")] int UnitId,
    [property: JsonPropertyName("residentId")] int ResidentId,
    [property: JsonPropertyName("vinculoId")] int VinculoId);
