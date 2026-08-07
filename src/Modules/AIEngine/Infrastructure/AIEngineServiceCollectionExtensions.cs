using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.AIEngine.Application.Services;
using Modules.AIEngine.Infrastructure.Persistence;
using Modules.AIEngine.Infrastructure.Services;

namespace Modules.AIEngine.Infrastructure;

public static class AIEngineServiceCollectionExtensions
{
    public static IServiceCollection AddAIEngineModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=smartcondo_dev;Username=postgres;Password=postgres";

        services.AddDbContext<AiDbContext>((sp, options) =>
        {
            options.UseNpgsqlWithVector(connectionString);
        });

        services.AddSingleton<IAiKernelFactory, AiKernelFactory>();
        services.AddScoped<IAiOrchestratorService, AiOrchestratorService>();
        services.AddSingleton<ITextChunkerService, TextChunkerService>();
        services.AddScoped<ITextEmbeddingService, TextEmbeddingService>();
        services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();

        return services;
    }
}
