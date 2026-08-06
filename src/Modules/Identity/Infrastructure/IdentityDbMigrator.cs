using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Identity.Infrastructure.Persistence;
using Npgsql;

namespace Modules.Identity.Infrastructure;

public static class IdentityDbMigrator
{
    public static async Task MigrateAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Modules.Identity.Infrastructure.IdentityDbMigrator");

        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(ct);
            logger.LogInformation("Identity database ensured (in-memory provider).");
            return;
        }

        var connectionString = configuration.GetConnectionString("Postgres");
        var databaseTarget = DescribeDatabaseTarget(connectionString);

        try
        {
            var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
            if (!await databaseCreator.ExistsAsync(ct))
            {
                logger.LogInformation("Database physical instance does not exist ({DatabaseTarget}). Creating database...", databaseTarget);
                await databaseCreator.CreateAsync(ct);
                logger.LogInformation("Database created successfully ({DatabaseTarget}).", databaseTarget);
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();

            if (pendingMigrations.Count == 0)
            {
                logger.LogInformation(
                    "Identity database is already up to date ({DatabaseTarget}).",
                    databaseTarget);
                return;
            }

            logger.LogInformation(
                "Applying {MigrationCount} pending Identity migration(s) to {DatabaseTarget}: {Migrations}",
                pendingMigrations.Count,
                databaseTarget,
                string.Join(", ", pendingMigrations));

            await dbContext.Database.MigrateAsync(ct);

            logger.LogInformation(
                "Applied {MigrationCount} Identity migration(s) to {DatabaseTarget}.",
                pendingMigrations.Count,
                databaseTarget);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException(
                $"Failed to apply Identity migrations on {databaseTarget}. " +
                $"Ensure PostgreSQL is running (`docker compose up -d`) and accessible. " +
                $"Provider error: {innerMessage}",
                ex);
        }
    }

    private static string DescribeDatabaseTarget(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "Postgres (connection string not configured)";
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var host = string.IsNullOrWhiteSpace(builder.Host) ? "localhost" : builder.Host;
            var port = builder.Port <= 0 ? 5432 : builder.Port;
            var database = string.IsNullOrWhiteSpace(builder.Database) ? "(default)" : builder.Database;

            return $"Host={host};Port={port};Database={database}";
        }
        catch
        {
            return "Postgres (invalid connection string)";
        }
    }
}
