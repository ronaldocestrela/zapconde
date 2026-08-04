using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Tests.Unit.Infrastructure;

/// <summary>
/// Testes unitários para validar o mapeamento e comportamento do Transactional Outbox Pattern no MultiTenantDbContext.
/// </summary>
public class TransactionalOutboxUnitTests
{
    [Fact]
    public void MultiTenantDbContext_Should_Map_MassTransit_Outbox_Entities()
    {
        // Arrange
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestOutboxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOutboxDbContext(options, currentTenantService);

        // Act
        var model = context.Model;
        var outboxEntityNames = model.GetEntityTypes()
            .Select(e => e.ClrType.Name)
            .Where(name => name.Contains("Outbox") || name.Contains("Inbox"))
            .ToList();

        // Assert
        Assert.NotEmpty(outboxEntityNames);
        Assert.Contains(outboxEntityNames, name => name.Contains("OutboxMessage"));
        Assert.Contains(outboxEntityNames, name => name.Contains("OutboxState"));
        Assert.Contains(outboxEntityNames, name => name.Contains("InboxState"));
    }

    [Fact]
    public void Outbox_Entities_Should_Not_Have_Tenant_Query_Filter()
    {
        // Arrange
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestOutboxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOutboxDbContext(options, currentTenantService);

        // Act
        var model = context.Model;
        var outboxEntities = model.GetEntityTypes()
            .Where(e => e.ClrType.Name.Contains("Outbox") || e.ClrType.Name.Contains("Inbox"))
            .ToList();

        // Assert
        Assert.NotEmpty(outboxEntities);
#pragma warning disable CS0618
        Assert.All(outboxEntities, entity => Assert.Null(entity.GetQueryFilter()));
#pragma warning restore CS0618
    }

    private class TestOutboxDbContext : MultiTenantDbContext
    {
        public TestOutboxDbContext(DbContextOptions options, ICurrentTenantService currentTenantService)
            : base(options, currentTenantService)
        {
        }
    }
}
