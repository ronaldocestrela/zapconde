using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Infrastructure.MultiTenancy;

public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantContextMiddleware>();
}
