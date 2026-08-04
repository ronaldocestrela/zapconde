using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.Messaging;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Testes de integração para validar o Transactional Outbox Pattern com PostgreSQL e MassTransit real.
/// </summary>
public sealed class TransactionalOutboxIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_outbox_test")
        .WithUsername("smartcondo")
        .WithPassword("smartcondo")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task PostgreSQL_Should_Create_Outbox_Tables_And_Persist_Outbox_Messages()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentTenantService = new TestCurrentTenantService { TenantId = 10 };
        services.AddSingleton<ICurrentTenantService>(currentTenantService);

        var connectionString = _postgresContainer.GetConnectionString();
        services.AddDbContext<OutboxTestDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<OutboxTestDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        await using var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            // Act - Publica evento no endpoint com Outbox ativo no DbContext
            var sampleEvent = new TestSampleIntegrationEvent
            {
                EventId = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Payload = "Outbox Integration Test"
            };

            await publishEndpoint.Publish(sampleEvent);
            dbContext.TestEntities.Add(new OutboxTestEntity { Id = 1, TenantId = 10, Name = "Test Entity" });
            await dbContext.SaveChangesAsync();
        }

        // Assert - Verifica se a mensagem foi gravada na tabela outbox_message
        using (var scope = provider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();

            var outboxMessageType = dbContext.Model.GetEntityTypes()
                .First(e => e.ClrType.Name.Contains("OutboxMessage"))
                .ClrType;

            var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(outboxMessageType);
            var outboxQueryable = (IQueryable)setMethod.Invoke(dbContext, null)!;

            var count = 0;
            foreach (var _ in outboxQueryable)
            {
                count++;
            }
            count.Should().BeGreaterThan(0, "o evento deve ser gravado na tabela outbox_message durante o commit");

            var domainEntities = await dbContext.TestEntities.ToListAsync();
            domainEntities.Should().HaveCount(1);
            domainEntities[0].Name.Should().Be("Test Entity");
        }
    }
}

// ============================================
// Classes auxiliares de teste de integração
// ============================================

internal class OutboxTestDbContext : MultiTenantDbContext
{
    public DbSet<OutboxTestEntity> TestEntities => Set<OutboxTestEntity>();

    public OutboxTestDbContext(DbContextOptions<OutboxTestDbContext> options, ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddTransactionalOutboxEntities();
    }
}

internal class OutboxTestEntity : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record TestSampleIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public string Payload { get; init; } = string.Empty;
}
