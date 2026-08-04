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
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Tests.Integration.Identity;

public sealed class IdentityAuthIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityWebApplicationFactory _factory;

    public IdentityAuthIntegrationTests(IdentityWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_Should_ReturnTokensAndProfiles()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("accessToken");
        json.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Should_Return401()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "errada"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithBlockedUser_Should_Return403()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "bloqueado@zapcond.com",
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SelectProfile_Should_ReturnJwtWithRequiredClaims()
    {
        await _factory.ResetDatabaseAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sindico@zapcond.com",
            password = "Senha@123"
        });

        var loginDoc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
        var membershipId = loginDoc.RootElement.GetProperty("data").GetProperty("profiles")[0].GetProperty("membershipId").GetString()!;

        using var profileClient = _factory.CreateClient();
        profileClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await profileClient.PostAsJsonAsync("/api/auth/select-profile", new
        {
            membershipId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var contextualToken = profileDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(contextualToken);
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.TenantId);
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.CondoId);
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.UserId);
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.Role);
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_Should_Return200()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = "sindico@zapcond.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("E-mail enviado");
    }
}

public sealed class IdentityWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await Modules.Identity.Infrastructure.IdentityDataSeeder.SeedAsync(Services);
    }
}
