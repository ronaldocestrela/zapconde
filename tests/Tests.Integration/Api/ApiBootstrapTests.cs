using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Integration.Api;

/// <summary>
/// Testes de integração do bootstrap da API SmartCondo
/// conforme Subfase 1.1.2 do ROADMAP.md
/// </summary>
public class ApiBootstrapTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiBootstrapTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Api_Should_StartSuccessfully()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.Should().NotBeNull("a API deve responder");
    }

    [Fact]
    public async Task HealthEndpoint_Should_Return200Ok()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            "o endpoint de health deve retornar 200 OK");
    }

    [Fact]
    public async Task HealthEndpoint_Should_ReturnResultEnvelope()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.Should().NotBeNull();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
            "a resposta deve ser JSON");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("isSuccess", "o envelope Result deve conter isSuccess");
        content.Should().Contain("message", "o envelope Result deve conter message");
        content.Should().Contain("data", "o envelope Result deve conter data");
    }

    [Fact]
    public async Task HealthEndpoint_Should_ReturnSuccessTrue()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        json.Should().Contain("\"isSuccess\":true", 
            "o health check deve retornar isSuccess=true");
    }

    [Fact]
    public async Task HealthEndpoint_Should_RespondQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/health");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, 
            "o endpoint de health deve responder em menos de 100ms");
    }

    [Fact]
    public async Task WeatherForecastEndpoint_Should_NotExist()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, 
            "o endpoint de template WeatherForecast deve ter sido removido");
    }
}
