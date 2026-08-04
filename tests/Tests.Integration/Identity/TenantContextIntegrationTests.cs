using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Infrastructure.Persistence;

namespace Tests.Integration.Identity;

[Collection("IdentityIntegration")]
public sealed class TenantContextIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly IdentityWebApplicationFactory _factory;

    public TenantContextIntegrationTests(IdentityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetContext_WithContextualJwt_Should_ReturnResolvedTenant()
    {
        await _factory.ResetDatabaseAsync();
        var contextualToken = await GetContextualTokenAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", contextualToken);

        var response = await client.GetAsync("/api/auth/context");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"tenantId\":1");
        json.Should().Contain("\"condoId\":10");
        json.Should().Contain("\"isResolved\":true");
    }

    [Fact]
    public async Task GetContext_WithoutAuth_Should_Return401()
    {
        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/auth/context");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WebhookProbe_WithTenantHeader_Should_ReturnResolvedTenant()
    {
        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "2");
        client.DefaultRequestHeaders.Add("X-Condo-Id", "20");

        var response = await client.GetAsync("/api/webhooks/context-probe");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"tenantId\":2");
        json.Should().Contain("\"isResolved\":true");
    }

    [Fact]
    public async Task GetContext_WithJwtAndConflictingHeader_Should_KeepJwtTenant()
    {
        await _factory.ResetDatabaseAsync();
        var contextualToken = await GetContextualTokenAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", contextualToken);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "99");

        var response = await client.GetAsync("/api/auth/context");
        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain("\"tenantId\":1");
        json.Should().NotContain("\"tenantId\":99");
    }

    [Fact]
    public async Task SelectProfile_ThenGetContext_Should_ReturnNewTenant()
    {
        await _factory.ResetDatabaseAsync();
        var loginToken = await GetLoginTokenAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginToken);

        var loginDoc = JsonDocument.Parse(await (await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        })).Content.ReadAsStringAsync());

        var profiles = loginDoc.RootElement.GetProperty("data").GetProperty("profiles");
        Guid? targetMembership = null;
        foreach (var profile in profiles.EnumerateArray())
        {
            if (profile.GetProperty("tenantId").GetInt32() == 2)
            {
                targetMembership = Guid.Parse(profile.GetProperty("membershipId").GetString()!);
                break;
            }
        }

        targetMembership.Should().NotBeNull();

        var selectResponse = await client.PostAsJsonAsync("/api/auth/select-profile", new { membershipId = targetMembership!.Value });
        selectResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var selectDoc = JsonDocument.Parse(await selectResponse.Content.ReadAsStringAsync());
        var newToken = selectDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        var contextResponse = await client.GetAsync("/api/auth/context");
        var contextJson = await contextResponse.Content.ReadAsStringAsync();

        contextJson.Should().Contain("\"tenantId\":2");
    }

    [Fact]
    public async Task GetProfiles_WithContextualJwt_Should_ReturnMultipleProfiles()
    {
        await _factory.ResetDatabaseAsync();
        var contextualToken = await GetContextualTokenAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", contextualToken);

        var response = await client.GetAsync("/api/auth/profiles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Ville de Paris");
        json.Should().Contain("Jardim das Flores");
        json.Should().Contain("Belvedere");
    }

    private async Task<string> GetLoginTokenAsync()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        });

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    private async Task<string> GetContextualTokenAsync()
    {
        var loginToken = await GetLoginTokenAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginToken);

        var loginDoc = JsonDocument.Parse(await (await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        })).Content.ReadAsStringAsync());

        var membershipId = loginDoc.RootElement.GetProperty("data").GetProperty("profiles")[0].GetProperty("membershipId").GetString()!;

        var selectResponse = await client.PostAsJsonAsync("/api/auth/select-profile", new { membershipId });
        var selectDoc = JsonDocument.Parse(await selectResponse.Content.ReadAsStringAsync());
        return selectDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
}
