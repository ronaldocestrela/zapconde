using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Tests.Unit.Infrastructure;

/// <summary>
/// Testes unitários para validar o comportamento do MultiTenantDbContext.
/// Valida aplicação correta de Global Query Filter em entidades ITenantScoped.
/// </summary>
public class MultiTenantDbContextTests
{
    [Fact]
    public void DbContext_Should_Apply_GlobalFilter_To_TenantScoped_Entities()
    {
        // Arrange
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, currentTenantService);

        // Act - Obtém o modelo do EF Core
        var model = context.Model;
        var tenantEntityType = model.FindEntityType(typeof(TestTenantEntity));

        // Assert - Verifica se a entidade tem query filter configurado
        Assert.NotNull(tenantEntityType);
#pragma warning disable CS0618
        var queryFilter = tenantEntityType.GetQueryFilter();
#pragma warning restore CS0618
        Assert.NotNull(queryFilter);
    }

    [Fact]
    public void DbContext_Should_Not_Apply_GlobalFilter_To_NonTenantScoped_Entities()
    {
        // Arrange
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, currentTenantService);

        // Act - Obtém o modelo do EF Core
        var model = context.Model;
        var nonTenantEntityType = model.FindEntityType(typeof(TestNonTenantEntity));

        // Assert - Verifica se a entidade NÃO tem query filter configurado
        Assert.NotNull(nonTenantEntityType);
#pragma warning disable CS0618
        var queryFilter = nonTenantEntityType.GetQueryFilter();
#pragma warning restore CS0618
        Assert.Null(queryFilter);
    }

    [Fact]
    public async Task DbContext_Should_Return_Empty_When_TenantId_Is_Null()
    {
        // Arrange - Tenant não resolvido
        var currentTenantService = new TestCurrentTenantService { TenantId = null };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, currentTenantService);

        // Adiciona dados de diferentes tenants
        context.TenantEntities.AddRange(
            new TestTenantEntity { Id = 1, TenantId = 1, Name = "Tenant1Entity" },
            new TestTenantEntity { Id = 2, TenantId = 2, Name = "Tenant2Entity" }
        );
        await context.SaveChangesAsync();

        // Act - Consulta sem tenant resolvido
        var result = await context.TenantEntities.ToListAsync();

        // Assert - Deve retornar vazio (deny-by-default)
        Assert.Empty(result);
    }

    [Fact]
    public async Task DbContext_Should_Filter_By_TenantId_When_Tenant_Is_Resolved()
    {
        // Arrange - Tenant resolvido
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, currentTenantService);

        // Adiciona dados de diferentes tenants
        context.TenantEntities.AddRange(
            new TestTenantEntity { Id = 1, TenantId = 1, Name = "Tenant1Entity" },
            new TestTenantEntity { Id = 2, TenantId = 2, Name = "Tenant2Entity" },
            new TestTenantEntity { Id = 3, TenantId = 1, Name = "AnotherTenant1Entity" }
        );
        await context.SaveChangesAsync();

        // Act - Consulta com tenant resolvido
        var result = await context.TenantEntities.ToListAsync();

        // Assert - Deve retornar apenas entidades do tenant 1
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(1, e.TenantId));
    }

    [Fact]
    public async Task DbContext_Should_Not_Filter_NonTenantScoped_Entities()
    {
        // Arrange
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, currentTenantService);

        // Adiciona dados não-tenant
        context.NonTenantEntities.AddRange(
            new TestNonTenantEntity { Id = 1, Name = "GlobalEntity1" },
            new TestNonTenantEntity { Id = 2, Name = "GlobalEntity2" }
        );
        await context.SaveChangesAsync();

        // Act - Consulta entidades não-tenant
        var result = await context.NonTenantEntities.ToListAsync();

        // Assert - Deve retornar todas as entidades (sem filtro)
        Assert.Equal(2, result.Count);
    }
}

// ============================================
// Classes de teste auxiliares
// ============================================

/// <summary>
/// Implementação de teste do ICurrentTenantService
/// </summary>
internal class TestCurrentTenantService : ICurrentTenantService
{
    public int? TenantId { get; set; }
}

/// <summary>
/// DbContext de teste que herda de MultiTenantDbContext
/// </summary>
internal class TestDbContext : MultiTenantDbContext
{
    public DbSet<TestTenantEntity> TenantEntities => Set<TestTenantEntity>();
    public DbSet<TestNonTenantEntity> NonTenantEntities => Set<TestNonTenantEntity>();

    public TestDbContext(DbContextOptions<TestDbContext> options, ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }
}

/// <summary>
/// Entidade de teste que implementa ITenantScoped
/// </summary>
internal class TestTenantEntity : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Entidade de teste que NÃO implementa ITenantScoped
/// </summary>
internal class TestNonTenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
