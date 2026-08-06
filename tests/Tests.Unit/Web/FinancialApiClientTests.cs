using System.Net;
using System.Text;
using FluentAssertions;
using SmartCondo.Web.Services;

namespace Tests.Unit.Web;

public sealed class FinancialApiClientTests
{
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public HttpResponseMessage ResponseToReturn { get; set; } = new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "isSuccess": true,
                  "message": "Sucesso",
                  "data": []
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(ResponseToReturn);
        }
    }

    [Fact]
    public async Task GetInvoicesAsync_Should_Attach_Authorization_And_Tenant_Headers()
    {
        // Arrange
        var handler = new TestHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7195")
        };

        var session = new AuthSession(null!)
        {
            AccessToken = "valid-jwt-token",
            Context = new AuthContextModel(42, 99, "user-1", "Admin")
        };

        var client = new FinancialApiClient(httpClient, session);

        // Act
        var result = await client.GetInvoicesAsync(competencia: "2026-08");

        // Assert
        result.IsSuccess.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        handler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be("valid-jwt-token");

        handler.LastRequest.Headers.Contains("X-Tenant-Id").Should().BeTrue();
        handler.LastRequest.Headers.GetValues("X-Tenant-Id").First().Should().Be("42");

        handler.LastRequest.Headers.Contains("X-Condo-Id").Should().BeTrue();
        handler.LastRequest.Headers.GetValues("X-Condo-Id").First().Should().Be("99");
    }
}
