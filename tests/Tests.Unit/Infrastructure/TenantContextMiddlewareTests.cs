using System.Security.Claims;
using BuildingBlocks.Infrastructure.MultiTenancy;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Tests.Unit.Infrastructure;

public class TenantContextMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithAuthenticatedTenantClaim_Should_SetTenantContext()
    {
        var tenantService = new CurrentTenantService();
        var context = CreateHttpContext("/api/auth/context");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("TenantId", "1"),
            new Claim("CondoId", "10"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        ], authenticationType: "Bearer"));

        var middleware = new TenantContextMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantService);

        tenantService.TenantId.Should().Be(1);
        tenantService.CondoId.Should().Be(10);
        tenantService.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithoutAuthOrHeader_Should_LeaveContextUnresolved()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(99);
        var context = CreateHttpContext("/api/auth/context");

        var middleware = new TenantContextMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantService);

        tenantService.IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WebhookPathWithHeader_Should_SetTenantFromHeader()
    {
        var tenantService = new CurrentTenantService();
        var context = CreateHttpContext("/api/webhooks/context-probe");
        context.Request.Headers[TenantHttpHeaders.TenantId] = "2";
        context.Request.Headers[TenantHttpHeaders.CondoId] = "20";

        var middleware = new TenantContextMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantService);

        tenantService.TenantId.Should().Be(2);
        tenantService.CondoId.Should().Be(20);
    }

    [Fact]
    public async Task InvokeAsync_NonWebhookPathWithHeaderOnly_Should_NotResolveTenant()
    {
        var tenantService = new CurrentTenantService();
        var context = CreateHttpContext("/api/auth/context");
        context.Request.Headers[TenantHttpHeaders.TenantId] = "2";

        var middleware = new TenantContextMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantService);

        tenantService.IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_JwtAndWebhookHeader_Should_PreferJwtClaims()
    {
        var tenantService = new CurrentTenantService();
        var context = CreateHttpContext("/api/webhooks/context-probe");
        context.Request.Headers[TenantHttpHeaders.TenantId] = "99";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("TenantId", "1"),
            new Claim("CondoId", "10")
        ], authenticationType: "Bearer"));

        var middleware = new TenantContextMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantService);

        tenantService.TenantId.Should().Be(1);
        tenantService.CondoId.Should().Be(10);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }
}
