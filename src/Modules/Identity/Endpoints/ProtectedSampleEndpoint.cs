using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Modules.Identity.Endpoints;

public sealed class ProtectedSampleDto
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint protegido de exemplo para validar autorização e fluxo 403.
/// </summary>
[Authorize(Roles = "Administradora")]
public sealed class ProtectedSampleEndpoint : EndpointWithoutRequest<Result<ProtectedSampleDto>>
{
    public override void Configure()
    {
        Get("/api/auth/protected-sample");
        Summary(s => s.Summary = "Recurso protegido de exemplo (requer role Administradora)");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(
            Result<ProtectedSampleDto>.Success(new ProtectedSampleDto { Message = "Acesso autorizado." }),
            200,
            ct);
    }
}
