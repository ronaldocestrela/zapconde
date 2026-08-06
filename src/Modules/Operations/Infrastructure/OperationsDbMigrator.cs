using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Operations.Infrastructure.Persistence;

namespace Modules.Operations.Infrastructure;

public static class OperationsDbMigrator
{
    public static async Task MigrateAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OperationsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Modules.Operations.Infrastructure.OperationsDbMigrator");

        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(ct);
            logger.LogInformation("Operations database ensured (non-relational/in-memory provider).");
            return;
        }

        try
        {
            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();

            if (pendingMigrations.Count > 0)
            {
                logger.LogInformation(
                    "Applying {MigrationCount} pending Operations migration(s): {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                await dbContext.Database.MigrateAsync(ct);
                logger.LogInformation("Applied Operations migrations successfully.");
            }
            else
            {
                logger.LogInformation("Operations database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha na migração automática EF Core para Operations. Executando verificação de estrutura...");
            try
            {
                var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
                if (!await databaseCreator.HasTablesAsync(ct))
                {
                    await databaseCreator.CreateTablesAsync(ct);
                    logger.LogInformation("Tabelas do módulo Operations criadas via RelationalDatabaseCreator.");
                }
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Erro ao inicializar schema do módulo Operations.");
                throw;
            }
        }
    }
}
