using BuildingBlocks.Infrastructure.Email;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.Email;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Infrastructure;

public sealed class OutlookSmtpEmailIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OutlookSmtpEmailIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:MigrateOnStartup"] = "false",
                    ["Identity:SeedOnStartup"] = "false",
                    ["Infrastructure:UseInMemoryCache"] = "true"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                var mockMultiplexer = new Mock<IConnectionMultiplexer>();
                var mockDatabase = new Mock<IDatabase>();
                mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
                services.AddSingleton(mockMultiplexer.Object);

                var mockChatSession = new Mock<IChatSessionService>();
                services.AddScoped(_ => mockChatSession.Object);
            });
        });
    }

    [Fact]
    public void DependencyInjection_ShouldResolveIEmailServiceAndOptions_Successfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var emailService = scope.ServiceProvider.GetService<IEmailService>();
        var options = scope.ServiceProvider.GetService<IOptions<OutlookSmtpOptions>>();

        // Assert
        emailService.Should().NotBeNull();
        emailService.Should().BeOfType<OutlookSmtpEmailService>();
        options.Should().NotBeNull();
        options!.Value.Host.Should().Be("smtp.office365.com");
        options.Value.Port.Should().Be(587);
    }

    [Fact]
    public async Task Post_SendTestEmail_ShouldReturnResultResponse()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var payload = new
        {
            To = "test-recipient@domain.com",
            Subject = "E-mail de Teste de Integração",
            BodyHtml = "<h1>Teste de Integração</h1>"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/email/send-test", payload);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<Result<string>>();
        result.Should().NotBeNull();
    }
}
