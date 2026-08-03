using FastEndpoints;

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
        }

        // Redirecionamento HTTPS
        app.UseHttpsRedirection();

        // Pipeline do FastEndpoints
        app.UseFastEndpoints();

        return app;
    }
}
