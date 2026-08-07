using BuildingBlocks.Infrastructure.MultiTenancy;
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
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/scalar");
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseTenantContext();
        app.UseAuthorization();
        app.UseFastEndpoints(c =>
        {
            c.Serializer.Options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
        Modules.Financial.Endpoints.AgreementEndpoints.MapAgreementEndpoints(app);
        Modules.Financial.Endpoints.DunningEndpoints.MapDunningEndpoints(app);
        Modules.Financial.Endpoints.DigitalBinderEndpoints.MapDigitalBinderEndpoints(app);
        Modules.Financial.Endpoints.BankReconciliationEndpoints.MapBankReconciliationEndpoints(app);
        Modules.Financial.Endpoints.ConsolidatedReportEndpoints.MapConsolidatedReportEndpoints(app);

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
