using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.RabbitMq;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Testes de integração da Subfase 1.3.1 para validar
/// bootstrap de MassTransit com RabbitMQ real e health check de readiness.
/// </summary>
public sealed class RabbitMqMessagingBootstrapIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
        await WaitForRabbitMqPortAsync(_rabbitMqContainer.Hostname, _rabbitMqContainer.GetMappedPublicPort(5672));
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }

    [Fact]
    public async Task Api_Should_Expose_ReadinessEndpoint_When_RabbitMq_Is_Configured()
    {
        // Arrange
        await using var factory = CreateFactoryConfiguredWithRabbitMq();
        using var client = factory.CreateClient();

        // Act
        var readinessResponse = await client.GetAsync("/health/ready");

        // Assert
        readinessResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Api_HealthEndpoint_Should_Remain_Available_With_Readiness_Checks()
    {
        // Arrange
        await using var factory = CreateFactoryConfiguredWithRabbitMq();
        using var client = factory.CreateClient();

        // Act
        var functionalHealthResponse = await client.GetAsync("/api/health");

        // Assert
        functionalHealthResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "o endpoint funcional /api/health não deve conflitar com /health/ready");
    }

    private WebApplicationFactory<Program> CreateFactoryConfiguredWithRabbitMq()
    {
        var rabbitHost = _rabbitMqContainer.Hostname;
        var rabbitPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        var rabbitConnectionString = _rabbitMqContainer.GetConnectionString();

        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=smartcondo_test;Username=postgres;Password=postgres",
            ["ConnectionStrings:RabbitMQ"] = rabbitConnectionString,
            ["RabbitMQ:Host"] = rabbitHost,
            ["RabbitMQ:Port"] = rabbitPort.ToString(),
            ["RabbitMQ:VirtualHost"] = "/",
            ["RabbitMQ:Username"] = "guest",
            ["RabbitMQ:Password"] = "guest"
        };

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(settings);
            });
        });
    }

    private static async Task WaitForRabbitMqPortAsync(string host, int port)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(host, port);
                if (tcpClient.Connected)
                {
                    return;
                }
            }
            catch
            {
                await Task.Delay(1000);
            }
        }

        throw new TimeoutException($"RabbitMQ não ficou acessível em {host}:{port} dentro do tempo esperado.");
    }
}
