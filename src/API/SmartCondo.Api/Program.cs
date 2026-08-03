using SmartCondo.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços (delegado para extensões)
builder.Services
    .AddApiServices()
    .AddApiDocumentation();

var app = builder.Build();

// Configurar pipeline HTTP (delegado para extensões)
app.UseApiPipeline();

app.Run();

// Expor Program para testes de integração
public partial class Program { }
