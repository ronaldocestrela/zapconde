using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartCondo.Web.Services;

public sealed class OnboardingApiClient(HttpClient httpClient)
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApiResult<OnboardingDraftSaveResult>> SaveDraftAsync(OnboardingDraftModel draft, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/tenants/onboarding/draft", draft, ct);
            return await ParseAsync<OnboardingDraftSaveResult>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<OnboardingDraftSaveResult>(ex);
        }
    }

    public async Task<ApiResult<CnpjStatusModel>> GetCnpjStatusAsync(string cnpj, CancellationToken ct = default)
    {
        try
        {
            var digits = new string(cnpj.Where(char.IsDigit).ToArray());
            var response = await httpClient.GetAsync($"/api/tenants/cnpj/{digits}/status", ct);
            return await ParseAsync<CnpjStatusModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<CnpjStatusModel>(ex);
        }
    }

    public async Task<ApiResult<CepLookupModel>> LookupCepAsync(string cep, CancellationToken ct = default)
    {
        try
        {
            var digits = new string(cep.Where(char.IsDigit).ToArray());
            var response = await httpClient.GetAsync($"/api/tenants/cep/{digits}", ct);
            return await ParseAsync<CepLookupModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<CepLookupModel>(ex);
        }
    }

    public async Task<ApiResult<TenantCreatedModel>> CreateTenantAsync(CreateTenantModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/tenants/onboarding", request, ct);
            return await ParseAsync<TenantCreatedModel>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<TenantCreatedModel>(ex);
        }
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

        if (!root.TryGetProperty("isSuccess", out var isSuccessElement))
        {
            return new ApiResult<T>(false, default, $"Resposta inesperada da API (HTTP {statusCode}).", statusCode);
        }

        var isSuccess = isSuccessElement.GetBoolean();
        var message = root.TryGetProperty("message", out var msg) ? msg.GetString() ?? string.Empty : string.Empty;

        if (!isSuccess)
        {
            return new ApiResult<T>(false, default, message, statusCode);
        }

        if (!root.TryGetProperty("data", out var data))
        {
            return new ApiResult<T>(true, default, message, statusCode);
        }

        var payload = JsonSerializer.Deserialize<T>(data.GetRawText(), JsonOptions);
        return new ApiResult<T>(true, payload, message, statusCode);
    }
}

public class OnboardingDraftModel
{
    [JsonPropertyName("draftId")] public Guid DraftId { get; set; }
    [JsonPropertyName("administradora")] public OnboardingAdministradoraModel Administradora { get; set; } = new();
    [JsonPropertyName("condominio")] public OnboardingCondominioModel Condominio { get; set; } = new();
    [JsonPropertyName("endereco")] public OnboardingEnderecoModel Endereco { get; set; } = new();
    [JsonPropertyName("contatos")] public OnboardingContatosModel Contatos { get; set; } = new();
    [JsonPropertyName("configuracoes")] public OnboardingConfiguracoesModel Configuracoes { get; set; } = new();
    [JsonPropertyName("currentStep")] public int CurrentStep { get; set; } = 1;
}

public sealed class OnboardingAdministradoraModel
{
    [JsonPropertyName("razaoSocial")] public string RazaoSocial { get; set; } = string.Empty;
    [JsonPropertyName("cnpj")] public string Cnpj { get; set; } = string.Empty;
    [JsonPropertyName("nomeFantasia")] public string NomeFantasia { get; set; } = string.Empty;
    [JsonPropertyName("licensePlan")] public int LicensePlan { get; set; }
}

public sealed class OnboardingCondominioModel
{
    [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("tipo")] public int Tipo { get; set; }
    [JsonPropertyName("totalUnits")] public int TotalUnits { get; set; }
    [JsonPropertyName("numberOfBlocks")] public int NumberOfBlocks { get; set; } = 1;
}

public sealed class OnboardingEnderecoModel
{
    [JsonPropertyName("cep")] public string Cep { get; set; } = string.Empty;
    [JsonPropertyName("logradouro")] public string Logradouro { get; set; } = string.Empty;
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("bairro")] public string Bairro { get; set; } = string.Empty;
    [JsonPropertyName("cidade")] public string Cidade { get; set; } = string.Empty;
    [JsonPropertyName("uf")] public string Uf { get; set; } = string.Empty;
}

public sealed class OnboardingContatosModel
{
    [JsonPropertyName("masterAdminName")] public string MasterAdminName { get; set; } = string.Empty;
    [JsonPropertyName("corporateEmail")] public string CorporateEmail { get; set; } = string.Empty;
    [JsonPropertyName("phoneWhatsApp")] public string PhoneWhatsApp { get; set; } = string.Empty;
    [JsonPropertyName("emergencyPhone")] public string EmergencyPhone { get; set; } = string.Empty;
    [JsonPropertyName("masterRole")] public string MasterRole { get; set; } = "Sindico";
}

public sealed class OnboardingConfiguracoesModel
{
    [JsonPropertyName("diaVencimento")] public int DiaVencimento { get; set; } = 10;
    [JsonPropertyName("jurosEnabled")] public bool JurosEnabled { get; set; }
    [JsonPropertyName("multaEnabled")] public bool MultaEnabled { get; set; }
    [JsonPropertyName("bankGateway")] public int BankGateway { get; set; }
    [JsonPropertyName("whatsAppAiEnabled")] public bool WhatsAppAiEnabled { get; set; }
}

public sealed class CreateTenantModel : OnboardingDraftModel
{
    [JsonPropertyName("simulateRollback")] public bool SimulateRollback { get; set; }
}

public sealed record OnboardingDraftSaveResult(
    [property: JsonPropertyName("draftId")] Guid DraftId,
    [property: JsonPropertyName("savedAt")] DateTime SavedAt);

public sealed record CnpjStatusModel(
    [property: JsonPropertyName("cnpj")] string Cnpj,
    [property: JsonPropertyName("isAvailable")] bool IsAvailable,
    [property: JsonPropertyName("status")] string Status);

public sealed record CepLookupModel(
    [property: JsonPropertyName("cep")] string Cep,
    [property: JsonPropertyName("logradouro")] string Logradouro,
    [property: JsonPropertyName("bairro")] string Bairro,
    [property: JsonPropertyName("cidade")] string Cidade,
    [property: JsonPropertyName("uf")] string Uf);

public sealed record TenantCreatedModel(
    [property: JsonPropertyName("tenantId")] int TenantId,
    [property: JsonPropertyName("condoId")] int CondoId,
    [property: JsonPropertyName("masterEmail")] string MasterEmail,
    [property: JsonPropertyName("credentialsDispatchedMessage")] string CredentialsDispatchedMessage,
    [property: JsonPropertyName("condominioNome")] string CondominioNome);
