using BuildingBlocks.Shared;
using FastEndpoints;

namespace SmartCondo.Api.Endpoints.System;

/// <summary>
/// Endpoint de health check do sistema.
/// Retorna informações básicas de status da API usando o padrão Result<T>.
/// Implementado conforme Subfase 1.1.2 do ROADMAP.md e diretrizes do AGENTS.md.
/// </summary>
public class HealthEndpoint : EndpointWithoutRequest<Result<HealthDto>>
{
    public override void Configure()
    {
        Get("/api/health");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Health check endpoint";
            s.Description = "Retorna o status básico da API para verificação de disponibilidade";
            s.Response<Result<HealthDto>>(200, "Status da API retornado com sucesso");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var healthData = new HealthDto
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        };

        var result = Result<HealthDto>.Success(
            healthData,
            "API SmartCondo está operacional"
        );

        await SendOkAsync(result, ct);
    }
}
