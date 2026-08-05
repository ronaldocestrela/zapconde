using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Tests.Integration.Identity;

[Collection("IdentityIntegration")]
public sealed class UnitResidentIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly IdentityWebApplicationFactory _factory;

    public UnitResidentIntegrationTests(IdentityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateUnit_WithValidPayload_Should_Return201()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/units", ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("unitId");
        json.Should().Contain("residentId");
    }

    [Fact]
    public async Task CreateUnit_WithPayloadSerializedByBlazor_Should_Return201()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();
        var payload = new
        {
            blocoCodigo = "Bloco A",
            numero = "103",
            status = "Vaga",
            moradorNome = "Maria Souza",
            moradorCpf = "11144477735",
            moradorEmail = "maria@test.com",
            moradorTelefone = "+5511988887777",
            papel = "Proprietario",
            dataInicio = new DateTime(2024, 2, 1),
            dependencias = new[] { "Pets" }
        };

        var response = await client.PostAsJsonAsync("/api/units", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"unitId\":");
        json.Should().Contain("\"residentId\":");
    }

    [Fact]
    public async Task CreateUnit_WithDuplicateNumber_Should_Return409()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/units", ValidCreateRequest());
        var response = await client.PostAsJsonAsync("/api/units", ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUnit_WithInvalidCpf_Should_Return422()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();

        var request = ValidCreateRequest();
        request.MoradorCpf = "00000000000";

        var response = await client.PostAsJsonAsync("/api/units", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ListUnits_WithBlockFilter_Should_ReturnFiltered()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/units", ValidCreateRequest());

        var blocksResponse = await client.GetAsync("/api/blocks");
        var blocksDoc = JsonDocument.Parse(await blocksResponse.Content.ReadAsStringAsync());
        var blockId = blocksDoc.RootElement.GetProperty("data")[0].GetProperty("id").GetInt32();

        var response = await client.GetAsync($"/api/units?blockId={blockId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Bloco A");
    }

    [Fact]
    public async Task TransferOwnership_Should_ArchiveOldVinculo()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/units", ValidCreateRequest());
        var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var unitId = createDoc.RootElement.GetProperty("data").GetProperty("unitId").GetInt32();

        var transferResponse = await client.PostAsJsonAsync($"/api/units/{unitId}/transfer", new TransferOwnershipRequestDto
        {
            DataEncerramento = new DateTime(2025, 10, 31),
            Motivo = "Contrato Encerrado",
            Papel = PapelVinculo.Proprietario,
            NovoMoradorNome = "Ana Costa",
            NovoMoradorCpf = "39053344705",
            NovoMoradorEmail = "ana@test.com",
            NovoMoradorTelefone = "+5511988887777",
            DataInicio = new DateTime(2025, 11, 1)
        });

        transferResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var historyResponse = await client.GetAsync($"/api/units/{unitId}/history");
        var historyJson = await historyResponse.Content.ReadAsStringAsync();
        historyJson.Should().Contain("Contrato Encerrado");
        historyJson.Should().Contain("Ana Costa");
    }

    [Fact]
    public async Task GetHistory_Should_ReturnTimeline()
    {
        await _factory.ResetDatabaseAsync();
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/units", ValidCreateRequest());
        var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var unitId = createDoc.RootElement.GetProperty("data").GetProperty("unitId").GetInt32();

        var response = await client.GetAsync($"/api/units/{unitId}/history");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("João Silva");
    }

    [Fact]
    public async Task ListUnits_Should_IsolateTenants()
    {
        await _factory.ResetDatabaseAsync();
        var clientTenant1 = await CreateAuthenticatedClientAsync();

        await clientTenant1.PostAsJsonAsync("/api/units", ValidCreateRequest());

        var clientTenant2 = await CreateAuthenticatedClientForTenant2Async();
        var response = await clientTenant2.GetAsync("/api/units");
        var json = await response.Content.ReadAsStringAsync();

        json.Should().NotContain("101");
    }

    private static CreateUnitRequestDto ValidCreateRequest() => new()
    {
        BlocoCodigo = "Bloco A",
        Numero = "101",
        MoradorNome = "João Silva",
        MoradorCpf = "52998224725",
        MoradorEmail = "joao@test.com",
        MoradorTelefone = "+5511999999999",
        Papel = PapelVinculo.Proprietario,
        DataInicio = new DateTime(2024, 1, 15),
        Dependencias = ["Vagas de Garagem"]
    };

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var token = await GetContextualTokenAsync(tenantId: 1, condoId: 10);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<HttpClient> CreateAuthenticatedClientForTenant2Async()
    {
        var token = await GetContextualTokenAsync(tenantId: 2, condoId: 20);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> GetContextualTokenAsync(int tenantId, int condoId)
    {
        using var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        });

        var loginDoc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var profiles = loginDoc.RootElement.GetProperty("data").GetProperty("profiles");
        Guid? membershipId = null;

        foreach (var profile in profiles.EnumerateArray())
        {
            if (profile.GetProperty("tenantId").GetInt32() == tenantId &&
                profile.GetProperty("condoId").GetInt32() == condoId)
            {
                membershipId = Guid.Parse(profile.GetProperty("membershipId").GetString()!);
                break;
            }
        }

        membershipId.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            loginDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!);

        var selectResponse = await client.PostAsJsonAsync("/api/auth/select-profile", new { membershipId });
        var selectDoc = JsonDocument.Parse(await selectResponse.Content.ReadAsStringAsync());
        return selectDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
}
