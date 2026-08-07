using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.AccessControl.Infrastructure.Persistence;

namespace Modules.AccessControl.Infrastructure;

public static class AccessControlDbMigrator
{
    public static async Task MigrateAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccessControlDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Modules.AccessControl.Infrastructure.AccessControlDbMigrator");

        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(ct);
            logger.LogInformation("AccessControl database ensured (non-relational/in-memory provider).");
            return;
        }

        try
        {
            var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
            if (!await databaseCreator.ExistsAsync(ct))
            {
                logger.LogInformation("AccessControl database does not exist. Creating database...");
                await databaseCreator.CreateAsync(ct);
                logger.LogInformation("AccessControl database created successfully.");
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();

            if (pendingMigrations.Count > 0)
            {
                logger.LogInformation(
                    "Applying {MigrationCount} pending AccessControl migration(s): {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                await dbContext.Database.MigrateAsync(ct);
                logger.LogInformation("Applied AccessControl migrations successfully.");
            }
            else
            {
                logger.LogInformation("No pending AccessControl migrations found. Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply AccessControl database migrations.");
            throw;
        }
    }
}
