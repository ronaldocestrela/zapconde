using System.Security.Claims;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.MultiTenancy;

/// <summary>
/// Middleware que injeta TenantId/CondoId no <see cref="ICurrentTenantService"/>.
/// Prioridade: claims JWT autenticadas; headers X-Tenant-ID / webhooks; fallback dev mode (TenantId = 1).
/// </summary>
public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        tenantService.Clear();

        if (TryResolveFromClaims(context.User, tenantService))
        {
            await next(context);
            return;
        }

        if (IsWebhookPath(context.Request.Path) &&
            TryResolveFromHeaders(context.Request.Headers, tenantService))
        {
            await next(context);
            return;
        }

        await next(context);
    }

    private static bool TryResolveFromClaims(ClaimsPrincipal user, ICurrentTenantService tenantService)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var tenantClaim = user.FindFirst("TenantId")?.Value;
        if (!int.TryParse(tenantClaim, out var tenantId))
        {
            return false;
        }

        tenantService.SetTenantId(tenantId);

        var condoClaim = user.FindFirst("CondoId")?.Value;
        if (int.TryParse(condoClaim, out var condoId))
        {
            tenantService.SetCondoId(condoId);
        }

        return true;
    }

    private static bool TryResolveFromHeaders(IHeaderDictionary headers, ICurrentTenantService tenantService)
    {
        if (!headers.TryGetValue(TenantHttpHeaders.TenantId, out var tenantValues) ||
            !int.TryParse(tenantValues.FirstOrDefault(), out var tenantId))
        {
            return false;
        }

        tenantService.SetTenantId(tenantId);

        if (headers.TryGetValue(TenantHttpHeaders.CondoId, out var condoValues) &&
            int.TryParse(condoValues.FirstOrDefault(), out var condoId))
        {
            tenantService.SetCondoId(condoId);
        }

        return true;
    }

    private static bool IsWebhookPath(PathString path) =>
        path.StartsWithSegments("/api/webhooks", StringComparison.OrdinalIgnoreCase);
}
