using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.AIEngine.Infrastructure.Persistence;

namespace Modules.AIEngine.Infrastructure;

public static class AiDbMigrator
{
    public static async Task MigrateAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Modules.AIEngine.Infrastructure.AiDbMigrator");

        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(ct);
            logger.LogInformation("AI Engine database ensured (non-relational/in-memory provider).");
            return;
        }

        try
        {
            var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
            if (!await databaseCreator.ExistsAsync(ct))
            {
                logger.LogInformation("AI Engine database does not exist. Creating database...");
                await databaseCreator.CreateAsync(ct);
                logger.LogInformation("AI Engine database created successfully.");
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();

            if (pendingMigrations.Count > 0)
            {
                logger.LogInformation(
                    "Applying {MigrationCount} pending AI Engine migration(s): {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                await dbContext.Database.MigrateAsync(ct);
                logger.LogInformation("Applied AI Engine migrations successfully.");
            }
            else
            {
                logger.LogInformation("No pending AI Engine migrations found. Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply AI Engine database migrations.");
            throw;
        }
    }
}
