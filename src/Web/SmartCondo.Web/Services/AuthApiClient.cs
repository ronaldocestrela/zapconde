using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;

namespace SmartCondo.Web.Services;

public sealed class AuthSession
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public List<AuthProfileModel> Profiles { get; set; } = [];
    public AuthContextModel? Context { get; set; }
}

public sealed record AuthProfileModel(
    [property: JsonPropertyName("membershipId")] Guid MembershipId,
    [property: JsonPropertyName("tenantId")] int TenantId,
    [property: JsonPropertyName("condoId")] int CondoId,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("displayLabel")] string DisplayLabel);

public sealed record AuthContextModel(int TenantId, int CondoId, string UserId, string Role);

public sealed class AuthApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApiResult<LoginResponse>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/login", new { email, password }, ct);
        return await ParseAsync<LoginResponse>(response, ct);
    }

    public async Task<ApiResult<SelectProfileResponse>> SelectProfileAsync(string accessToken, Guid membershipId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/select-profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new { membershipId });
        var response = await httpClient.SendAsync(request, ct);
        return await ParseAsync<SelectProfileResponse>(response, ct);
    }

    public async Task<ApiResult<ForgotPasswordResponse>> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/forgot-password", new { email }, ct);
        return await ParseAsync<ForgotPasswordResponse>(response, ct);
    }

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var isSuccess = root.GetProperty("isSuccess").GetBoolean();
        var message = root.TryGetProperty("message", out var msg) ? msg.GetString() ?? string.Empty : string.Empty;

        if (!isSuccess || !root.TryGetProperty("data", out var data))
        {
            return new ApiResult<T>(false, default, message, (int)response.StatusCode);
        }

        var payload = data.Deserialize<T>(JsonOptions);
        return new ApiResult<T>(true, payload, message, (int)response.StatusCode);
    }
}

public sealed record ApiResult<T>(bool IsSuccess, T? Data, string Message, int StatusCode);

public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, List<AuthProfileModel> Profiles);

public sealed record SelectProfileResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, int TenantId, int CondoId, string UserId, string Role);

public sealed record ForgotPasswordResponse(string Message);

public sealed class SessionAuthStateProvider(AuthSession session) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrWhiteSpace(session.AccessToken) || session.Context is null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var identity = new ClaimsIdentity(
        [
            new Claim("TenantId", session.Context.TenantId.ToString()),
            new Claim("CondoId", session.Context.CondoId.ToString()),
            new Claim("UserId", session.Context.UserId),
            new Claim(ClaimTypes.Role, session.Context.Role),
            new Claim("Role", session.Context.Role)
        ], "Bearer");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
