using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.WhatsApp.Infrastructure.Persistence;

namespace Modules.WhatsApp.Infrastructure;

public static class WhatsAppDbMigrator
{
    public static async Task MigrateAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Modules.WhatsApp.Infrastructure.WhatsAppDbMigrator");

        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(ct);
            logger.LogInformation("WhatsApp database ensured (non-relational/in-memory provider).");
            return;
        }

        try
        {
            var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
            if (!await databaseCreator.ExistsAsync(ct))
            {
                logger.LogInformation("WhatsApp database does not exist. Creating database...");
                await databaseCreator.CreateAsync(ct);
                logger.LogInformation("WhatsApp database created successfully.");
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();

            if (pendingMigrations.Count > 0)
            {
                logger.LogInformation(
                    "Applying {MigrationCount} pending WhatsApp migration(s): {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                await dbContext.Database.MigrateAsync(ct);
                logger.LogInformation("Applied WhatsApp migrations successfully.");
            }
            else
            {
                logger.LogInformation("No pending WhatsApp migrations found. Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply WhatsApp database migrations.");
            throw;
        }
    }
}
