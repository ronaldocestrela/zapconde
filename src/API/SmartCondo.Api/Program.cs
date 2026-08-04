using Modules.Identity.Infrastructure;
using SmartCondo.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApiDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        await IdentityDbMigrator.MigrateAsync(app.Services, app.Configuration);
    }

    if (app.Configuration.GetValue<bool>("Identity:SeedOnStartup"))
    {
        await IdentityDataSeeder.SeedAsync(app.Services);
    }
}

app.UseApiPipeline();

app.Run();

public partial class Program { }
