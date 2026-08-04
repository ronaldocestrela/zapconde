using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;

namespace Tests.Integration.Identity;

[Collection("IdentityIntegration")]
public sealed class PhoneVerificationIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly IdentityWebApplicationFactory _factory;

    public PhoneVerificationIntegrationTests(IdentityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RequestAndVerify_WithValidCode_Should_ValidatePhone()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();
        var moradorId = await CreateResidentAsync(client);

        var requestResponse = await client.PostAsJsonAsync(
            $"/api/residents/{moradorId}/phone/request-code",
            new RequestPhoneVerificationDto
            {
                MoradorId = moradorId,
                PhoneNumber = "(11) 98765-4321"
            });

        requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var requestDoc = JsonDocument.Parse(await requestResponse.Content.ReadAsStringAsync());
        var code = requestDoc.RootElement.GetProperty("data").GetProperty("debugCode").GetString();
        code.Should().HaveLength(6);

        var verifyResponse = await client.PostAsJsonAsync(
            $"/api/residents/{moradorId}/phone/verify",
            new VerifyPhoneDto { MoradorId = moradorId, Code = code! });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await verifyResponse.Content.ReadAsStringAsync();
        payload.Should().Contain("\"status\":2");
        payload.Should().Contain("+5511987654321");
    }

    [Fact]
    public async Task PhoneStatus_FromAnotherTenant_Should_Return404()
    {
        await _factory.ResetDatabaseAsync();
        var tenant1 = await CreateAuthenticatedClientAsync();
        var moradorId = await CreateResidentAsync(tenant1);
        var tenant2 = await CreateAuthenticatedClientAsync(2, 20);

        var response = await tenant2.GetAsync($"/api/residents/{moradorId}/phone/status");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<int> CreateResidentAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/units", new CreateUnitRequestDto
        {
            BlocoCodigo = "Bloco OTP",
            Numero = Guid.NewGuid().ToString("N")[..6],
            MoradorNome = "Morador OTP",
            MoradorCpf = "52998224725",
            MoradorEmail = "otp@test.com",
            MoradorTelefone = "",
            Papel = PapelVinculo.Proprietario,
            DataInicio = new DateTime(2026, 1, 1)
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("residentId").GetInt32();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(int tenantId = 1, int condoId = 10)
    {
        using var loginClient = _factory.CreateClient();
        var login = await loginClient.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        });
        var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var profiles = loginDoc.RootElement.GetProperty("data").GetProperty("profiles");
        var membershipId = profiles.EnumerateArray()
            .Where(x => x.GetProperty("tenantId").GetInt32() == tenantId &&
                        x.GetProperty("condoId").GetInt32() == condoId)
            .Select(x => Guid.Parse(x.GetProperty("membershipId").GetString()!))
            .First();

        loginClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
        var selected = await loginClient.PostAsJsonAsync(
            "/api/auth/select-profile",
            new { membershipId });
        var selectedDoc = JsonDocument.Parse(await selected.Content.ReadAsStringAsync());

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            selectedDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
        return client;
    }
}
