using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartCondo.Web.Services;

public sealed class PhoneVerificationApiClient(HttpClient httpClient, AuthSession session)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<ApiResult<PhoneVerificationModel>> GetStatusAsync(int moradorId, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Get, $"/api/residents/{moradorId}/phone/status", null, ct);

    public Task<ApiResult<PhoneVerificationModel>> RequestCodeAsync(
        int moradorId,
        string phoneNumber,
        CancellationToken ct = default) =>
        SendAsync(
            HttpMethod.Post,
            $"/api/residents/{moradorId}/phone/request-code",
            new { moradorId, phoneNumber },
            ct);

    public Task<ApiResult<PhoneVerificationModel>> VerifyAsync(
        int moradorId,
        string code,
        CancellationToken ct = default) =>
        SendAsync(
            HttpMethod.Post,
            $"/api/residents/{moradorId}/phone/verify",
            new { moradorId, code },
            ct);

    public Task<ApiResult<PhoneVerificationModel>> ResendAsync(int moradorId, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Post, $"/api/residents/{moradorId}/phone/resend", null, ct);

    private async Task<ApiResult<PhoneVerificationModel>> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct)
    {
        try
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

            using var response = await httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new(false, null, $"Resposta vazia da API (HTTP {(int)response.StatusCode}).", (int)response.StatusCode);
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var success = root.GetProperty("isSuccess").GetBoolean();
            var message = root.TryGetProperty("message", out var messageNode)
                ? messageNode.GetString() ?? string.Empty
                : string.Empty;
            var data = success && root.TryGetProperty("data", out var dataNode)
                ? JsonSerializer.Deserialize<PhoneVerificationModel>(dataNode.GetRawText(), JsonOptions)
                : null;
            return new(success, data, message, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return new(false, null, $"Não foi possível conectar à API. {ex.Message}", 0);
        }
    }
}

public sealed class PhoneVerificationModel
{
    [JsonPropertyName("moradorId")] public int MoradorId { get; set; }
    [JsonPropertyName("phoneNumber")] public string? PhoneNumber { get; set; }
    [JsonPropertyName("maskedPhoneNumber")] public string? MaskedPhoneNumber { get; set; }
    [JsonPropertyName("status")] public int Status { get; set; }
    [JsonPropertyName("requestedAtUtc")] public DateTime? RequestedAtUtc { get; set; }
    [JsonPropertyName("verifiedAtUtc")] public DateTime? VerifiedAtUtc { get; set; }
    [JsonPropertyName("resendAvailableInSeconds")] public int? ResendAvailableInSeconds { get; set; }
    [JsonPropertyName("debugCode")] public string? DebugCode { get; set; }
}
