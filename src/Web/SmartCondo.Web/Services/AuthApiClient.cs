using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;

namespace SmartCondo.Web.Services;

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
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/auth/login", new { email, password }, ct);
            return await ParseAsync<LoginResponse>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            return ConnectionFailure<LoginResponse>(ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ConnectionFailure<LoginResponse>(ex);
        }
    }

    public async Task<ApiResult<List<AuthProfileModel>>> GetProfilesAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profiles");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.SendAsync(request, ct);
            return await ParseAsync<List<AuthProfileModel>>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            return ConnectionFailure<List<AuthProfileModel>>(ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ConnectionFailure<List<AuthProfileModel>>(ex);
        }
    }

    public async Task<ApiResult<SelectProfileResponse>> SelectProfileAsync(string accessToken, Guid membershipId, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/select-profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(new { membershipId });
            var response = await httpClient.SendAsync(request, ct);
            return await ParseAsync<SelectProfileResponse>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            return ConnectionFailure<SelectProfileResponse>(ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ConnectionFailure<SelectProfileResponse>(ex);
        }
    }

    public async Task<ApiResult<ForgotPasswordResponse>> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/auth/forgot-password", new { email }, ct);
            return await ParseAsync<ForgotPasswordResponse>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            return ConnectionFailure<ForgotPasswordResponse>(ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ConnectionFailure<ForgotPasswordResponse>(ex);
        }
    }

    private ApiResult<T> ConnectionFailure<T>(Exception ex)
    {
        var baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "API";
        return new ApiResult<T>(
            false,
            default,
            $"Não foi possível conectar à API em {baseUrl}. Verifique se a API está em execução.",
            0);
    }

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var statusCode = (int)response.StatusCode;
        var json = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new ApiResult<T>(
                false,
                default,
                $"Resposta vazia da API (HTTP {statusCode}).",
                statusCode);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return new ApiResult<T>(
                false,
                default,
                $"Resposta inválida da API (HTTP {statusCode}).",
                statusCode);
        }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("isSuccess", out var isSuccessElement))
            {
                return new ApiResult<T>(
                    false,
                    default,
                    $"Resposta inesperada da API (HTTP {statusCode}).",
                    statusCode);
            }

            var isSuccess = isSuccessElement.GetBoolean();
            var message = root.TryGetProperty("message", out var msg) ? msg.GetString() ?? string.Empty : string.Empty;

            if (!isSuccess || !root.TryGetProperty("data", out var data))
            {
                return new ApiResult<T>(false, default, message, statusCode);
            }

            var payload = data.Deserialize<T>(JsonOptions);
            if (payload is null)
            {
                return new ApiResult<T>(
                    false,
                    default,
                    string.IsNullOrWhiteSpace(message) ? "Resposta da API sem dados." : message,
                    statusCode);
            }

            if (payload is LoginResponse login && string.IsNullOrWhiteSpace(login.AccessToken))
            {
                return new ApiResult<T>(
                    false,
                    default,
                    "Login retornou sucesso, mas sem access token.",
                    statusCode);
            }

            if (payload is SelectProfileResponse profile && string.IsNullOrWhiteSpace(profile.AccessToken))
            {
                return new ApiResult<T>(
                    false,
                    default,
                    "Seleção de perfil retornou sucesso, mas sem access token.",
                    statusCode);
            }

            return new ApiResult<T>(true, payload, message, statusCode);
        }
    }
}

public sealed record ApiResult<T>(bool IsSuccess, T? Data, string Message, int StatusCode);

public sealed record LoginResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    [property: JsonPropertyName("profiles")] List<AuthProfileModel> Profiles);

public sealed record SelectProfileResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    [property: JsonPropertyName("tenantId")] int TenantId,
    [property: JsonPropertyName("condoId")] int CondoId,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("role")] string Role);

public sealed record ForgotPasswordResponse(
    [property: JsonPropertyName("message")] string Message);

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
