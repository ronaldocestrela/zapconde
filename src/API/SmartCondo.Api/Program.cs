using Modules.AccessControl.Infrastructure;
using Modules.Financial.Infrastructure;
using Modules.Identity.Infrastructure;
using Modules.Operations.Infrastructure;
using Modules.WhatsApp.Infrastructure;
using SmartCondo.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApiDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            app.Logger.LogInformation("Executando migrações do banco de dados no startup...");
            await IdentityDbMigrator.MigrateAsync(app.Services, app.Configuration);
            await FinancialDbMigrator.MigrateAsync(app.Services, app.Configuration);
            await OperationsDbMigrator.MigrateAsync(app.Services, app.Configuration);
            await AccessControlDbMigrator.MigrateAsync(app.Services, app.Configuration);
            await WhatsAppDbMigrator.MigrateAsync(app.Services, app.Configuration);
        }

        if (app.Configuration.GetValue<bool>("Identity:SeedOnStartup"))
        {
            app.Logger.LogInformation("Executando seeder inicial de dados...");
            await IdentityDataSeeder.SeedAsync(app.Services);
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "FALHA CRÍTICA NO STARTUP DA API: Ocorreu um erro ao aplicar migrações ou seeder de dados.");
        throw;
    }
}

app.UseApiPipeline();

app.Run();

public partial class Program { }
