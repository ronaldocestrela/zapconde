using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Infrastructure.Persistence;
using Tests.Integration.Identity;

namespace Tests.Integration.Identity;

[Collection("IdentityIntegration")]
public sealed class TenantOnboardingIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityWebApplicationFactory _factory;

    public TenantOnboardingIntegrationTests(IdentityWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTenant_WithValidPayload_Should_Return201()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/tenants/onboarding", OnboardingTestData.ValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("tenantId");
        json.Should().Contain("condoId");

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        (await db.Administradoras.IgnoreQueryFilters().CountAsync()).Should().BeGreaterThan(0);
        (await db.Condominios.IgnoreQueryFilters().CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCnpjStatus_WithExistingCnpj_Should_Return409()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/tenants/cnpj/07526557000100/status");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("CNPJ já cadastrado");
    }

    [Fact]
    public async Task GetCnpjStatus_WithInvalidCnpj_Should_Return422()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/tenants/cnpj/00000000000000/status");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateTenant_WithInvalidVencimento_Should_Return422()
    {
        await _factory.ResetDatabaseAsync();

        var request = OnboardingTestData.ValidRequest();
        request.Configuracoes.DiaVencimento = 32;

        var response = await _client.PostAsJsonAsync("/api/tenants/onboarding", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateTenant_WithSimulateRollback_Should_Return500()
    {
        await _factory.ResetDatabaseAsync();

        var request = OnboardingTestData.ValidRequest();
        request.SimulateRollback = true;

        var response = await _client.PostAsJsonAsync("/api/tenants/onboarding", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("rollback");

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var count = await db.Administradoras.IgnoreQueryFilters()
            .CountAsync(a => a.Cnpj == "11222333000181");
        count.Should().Be(0);
    }

    [Fact]
    public async Task SaveAndGetDraft_Should_Work()
    {
        await _factory.ResetDatabaseAsync();

        var draft = new
        {
            draftId = Guid.Empty,
            administradora = new { razaoSocial = "Rascunho LTDA", cnpj = "11.222.333/0001-81" },
            currentStep = 1
        };

        var saveResponse = await _client.PostAsJsonAsync("/api/tenants/onboarding/draft", draft);
        saveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var saveDoc = JsonDocument.Parse(await saveResponse.Content.ReadAsStringAsync());
        var draftId = saveDoc.RootElement.GetProperty("data").GetProperty("draftId").GetString()!;

        var getResponse = await _client.GetAsync($"/api/tenants/onboarding/draft/{draftId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getJson = await getResponse.Content.ReadAsStringAsync();
        getJson.Should().Contain("Rascunho LTDA");
    }

    [Fact]
    public async Task GetCepLookup_InTesting_Should_Return200()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/tenants/cep/01310100");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

internal static class OnboardingTestData
{
    public static CreateTenantRequestDto ValidRequest() => new()
    {
        Administradora = new OnboardingAdministradoraDto
        {
            RazaoSocial = "Nova Administradora LTDA",
            Cnpj = "11.222.333/0001-81",
            NomeFantasia = "Nova Admin",
            LicensePlan = Modules.Identity.Domain.LicensePlan.Starter
        },
        Condominio = new OnboardingCondominioDto
        {
            Nome = "Condomínio Integração",
            Tipo = Modules.Identity.Domain.CondominioTipo.Residencial,
            TotalUnits = 80,
            NumberOfBlocks = 3
        },
        Endereco = new OnboardingEnderecoDto
        {
            Cep = "01310-100",
            Logradouro = "Av Paulista",
            Numero = "500",
            Bairro = "Bela Vista",
            Cidade = "São Paulo",
            Uf = "SP"
        },
        Contatos = new OnboardingContatosDto
        {
            MasterAdminName = "Ana Master",
            CorporateEmail = "ana.master@integracao.com",
            PhoneWhatsApp = "+5511999999999",
            EmergencyPhone = "+5511888888888",
            MasterRole = "Sindico"
        },
        Configuracoes = new OnboardingConfiguracoesDto
        {
            DiaVencimento = 10,
            JurosEnabled = true,
            MultaEnabled = true,
            BankGateway = Modules.Identity.Domain.BankGateway.None,
            WhatsAppAiEnabled = true
        }
    };
}
