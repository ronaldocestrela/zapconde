using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Testes de integração para validar o comportamento do MultiTenantDbContext com PostgreSQL real.
/// Utiliza Testcontainers para garantir isolamento e testes realistas de multi-tenancy.
/// </summary>
public sealed class MultiTenantDbContextIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_multitenancy_test")
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
    public async Task PostgreSQL_Should_Filter_Queries_By_TenantId()
    {
        // Arrange - Tenant 1 resolvido
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestMultiTenantDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        await using var context = new TestMultiTenantDbContext(options, currentTenantService);
        await context.Database.EnsureCreatedAsync();

        // Adiciona dados de diferentes tenants
        context.TenantProducts.AddRange(
            new TenantProduct { Id = 1, TenantId = 1, Name = "Product1-Tenant1" },
            new TenantProduct { Id = 2, TenantId = 2, Name = "Product2-Tenant2" },
            new TenantProduct { Id = 3, TenantId = 1, Name = "Product3-Tenant1" },
            new TenantProduct { Id = 4, TenantId = 3, Name = "Product4-Tenant3" }
        );
        await context.SaveChangesAsync();

        // Act - Consulta com tenant 1 resolvido
        var result = await context.TenantProducts.ToListAsync();

        // Assert - Deve retornar apenas produtos do tenant 1
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(1, p.TenantId));
        Assert.Contains(result, p => p.Name == "Product1-Tenant1");
        Assert.Contains(result, p => p.Name == "Product3-Tenant1");
    }

    [Fact]
    public async Task PostgreSQL_Should_Return_Empty_When_TenantId_Not_Resolved()
    {
        // Arrange - Tenant não resolvido
        var currentTenantService = new TestCurrentTenantService { TenantId = null };
        var options = new DbContextOptionsBuilder<TestMultiTenantDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        await using var context = new TestMultiTenantDbContext(options, currentTenantService);
        await context.Database.EnsureCreatedAsync();

        // Adiciona dados de diferentes tenants
        context.TenantProducts.AddRange(
            new TenantProduct { Id = 10, TenantId = 1, Name = "ProductA" },
            new TenantProduct { Id = 20, TenantId = 2, Name = "ProductB" }
        );
        await context.SaveChangesAsync();

        // Act - Consulta sem tenant resolvido
        var result = await context.TenantProducts.ToListAsync();

        // Assert - Deve retornar vazio (deny-by-default)
        Assert.Empty(result);
    }

    [Fact]
    public async Task PostgreSQL_Should_Isolate_Tenants_On_Save_And_Query()
    {
        // Arrange - Simula requisições de diferentes tenants
        var tenant1Service = new TestCurrentTenantService { TenantId = 1 };
        var tenant2Service = new TestCurrentTenantService { TenantId = 2 };

        var options = new DbContextOptionsBuilder<TestMultiTenantDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        // Tenant 1 salva dados
        await using (var contextTenant1 = new TestMultiTenantDbContext(options, tenant1Service))
        {
            await contextTenant1.Database.EnsureCreatedAsync();
            contextTenant1.TenantProducts.Add(new TenantProduct { Id = 100, TenantId = 1, Name = "Tenant1Product" });
            await contextTenant1.SaveChangesAsync();
        }

        // Tenant 2 salva dados
        await using (var contextTenant2 = new TestMultiTenantDbContext(options, tenant2Service))
        {
            contextTenant2.TenantProducts.Add(new TenantProduct { Id = 200, TenantId = 2, Name = "Tenant2Product" });
            await contextTenant2.SaveChangesAsync();
        }

        // Act & Assert - Tenant 1 consulta e vê apenas seus dados
        await using (var contextTenant1Read = new TestMultiTenantDbContext(options, tenant1Service))
        {
            var tenant1Products = await contextTenant1Read.TenantProducts.ToListAsync();
            Assert.Single(tenant1Products);
            Assert.Equal("Tenant1Product", tenant1Products[0].Name);
        }

        // Act & Assert - Tenant 2 consulta e vê apenas seus dados
        await using (var contextTenant2Read = new TestMultiTenantDbContext(options, tenant2Service))
        {
            var tenant2Products = await contextTenant2Read.TenantProducts.ToListAsync();
            Assert.Single(tenant2Products);
            Assert.Equal("Tenant2Product", tenant2Products[0].Name);
        }
    }

    [Fact]
    public async Task PostgreSQL_Should_Not_Filter_NonTenantScoped_Entities()
    {
        // Arrange
        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<TestMultiTenantDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        await using var context = new TestMultiTenantDbContext(options, currentTenantService);
        await context.Database.EnsureCreatedAsync();

        // Adiciona entidades globais (não-tenant)
        context.GlobalSettings.AddRange(
            new GlobalSetting { Id = 1, Key = "Setting1", Value = "Value1" },
            new GlobalSetting { Id = 2, Key = "Setting2", Value = "Value2" }
        );
        await context.SaveChangesAsync();

        // Act - Consulta entidades globais
        var result = await context.GlobalSettings.ToListAsync();

        // Assert - Deve retornar todas (sem filtro de tenant)
        Assert.Equal(2, result.Count);
    }
}

// ============================================
// Classes de teste auxiliares para integração
// ============================================

/// <summary>
/// Implementação de teste do ICurrentTenantService
/// </summary>
internal class TestCurrentTenantService : ICurrentTenantService
{
    public int? TenantId { get; set; }
    public int? CondoId { get; set; }
    public bool IsResolved => TenantId.HasValue;

    public void SetTenantId(int tenantId) => TenantId = tenantId;

    public void SetCondoId(int condoId) => CondoId = condoId;

    public void Clear()
    {
        TenantId = null;
        CondoId = null;
    }
}

/// <summary>
/// DbContext de teste para integração com PostgreSQL
/// </summary>
internal class TestMultiTenantDbContext : MultiTenantDbContext
{
    public DbSet<TenantProduct> TenantProducts => Set<TenantProduct>();
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();

    public TestMultiTenantDbContext(DbContextOptions<TestMultiTenantDbContext> options, ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }
}

/// <summary>
/// Entidade de teste que implementa ITenantScoped
/// </summary>
internal class TenantProduct : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Entidade de teste global que NÃO implementa ITenantScoped
/// </summary>
internal class GlobalSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
