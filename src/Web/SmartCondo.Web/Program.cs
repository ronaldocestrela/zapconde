using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.MultiTenancy;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.AspNetCore.Components.Authorization;
using Modules.Identity.Infrastructure;
using Modules.AccessControl.Infrastructure;
using Modules.WhatsApp.Infrastructure;
using Modules.Financial.Infrastructure;
using Modules.Operations.Infrastructure;
using SmartCondo.Web.Components;
using SmartCondo.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<SessionAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SessionAuthStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7195";
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<OnboardingApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<UnitsApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<PhoneVerificationApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<FinancialApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<OperationsApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<AccessControlApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<WhatsAppApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<Modules.Financial.Domain.Services.CalculadoraFinanceira>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddFinancialModule(builder.Configuration);
builder.Services.AddOperationsModule(builder.Configuration);
builder.Services.AddAccessControlModule(builder.Configuration);
builder.Services.AddWhatsAppModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        await IdentityDbMigrator.MigrateAsync(app.Services, app.Configuration);
        await FinancialDbMigrator.MigrateAsync(app.Services, app.Configuration);
        await OperationsDbMigrator.MigrateAsync(app.Services, app.Configuration);
        await AccessControlDbMigrator.MigrateAsync(app.Services, app.Configuration);
        await WhatsAppDbMigrator.MigrateAsync(app.Services, app.Configuration);
    }
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
