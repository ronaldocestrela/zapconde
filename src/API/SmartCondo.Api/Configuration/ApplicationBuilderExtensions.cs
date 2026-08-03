using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace SmartCondo.Api.Configuration;

/// <summary>
/// Extensões para configuração do pipeline HTTP da aplicação.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configura o pipeline HTTP da API
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // Documentação OpenAPI apenas em desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/scalar");
        }

        // Redirecionamento HTTPS
        app.UseHttpsRedirection();

        // Pipeline do FastEndpoints
        app.UseFastEndpoints();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });

        return app;
    }
}
