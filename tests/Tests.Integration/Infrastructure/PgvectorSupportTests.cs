using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Testes de integração para validar suporte a Pgvector no PostgreSQL.
/// Comprova habilitação da extensão vector, mapeamento EF Core e consultas de similaridade vetorial.
/// Conformidade com Subfase 1.2.3 do ROADMAP.
/// </summary>
public sealed class PgvectorSupportTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_vector_test")
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

    /// <summary>
    /// Método auxiliar para habilitar a extensão pgvector no PostgreSQL
    /// </summary>
    private async Task EnablePgvectorExtensionAsync()
    {
        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE EXTENSION IF NOT EXISTS vector;";
        await command.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task PostgreSQL_Should_Support_Pgvector_Extension()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();

        // Act - Habilita extensão pgvector
        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE EXTENSION IF NOT EXISTS vector;";
        await command.ExecuteNonQueryAsync();

        // Assert - Verifica se a extensão está disponível
        command.CommandText = "SELECT COUNT(*) FROM pg_extension WHERE extname = 'vector';";
        var extensionCount = (long?)await command.ExecuteScalarAsync();

        Assert.NotNull(extensionCount);
        Assert.Equal(1, extensionCount.Value);
    }

    [Fact]
    public async Task EfCore_Should_Map_Vector_Type_With_Pgvector()
    {
        // Arrange - Habilita extensão pgvector primeiro
        await EnablePgvectorExtensionAsync();

        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<VectorTestDbContext>()
            .UseNpgsql(
                _postgresContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.UseVector())
            .Options;

        await using var context = new VectorTestDbContext(options, currentTenantService);
        await context.Database.EnsureCreatedAsync();

        // Act - Persiste entidade com embedding vetorial
        var document = new DocumentEmbedding
        {
            Id = 1,
            TenantId = 1,
            Content = "Regra para uso da churrasqueira: necessário reserva com 48h de antecedência",
            Embedding = new Vector(new[] { 0.1f, 0.2f, 0.3f })
        };

        context.DocumentEmbeddings.Add(document);
        await context.SaveChangesAsync();

        // Assert - Recupera entidade e valida embedding
        var savedDocument = await context.DocumentEmbeddings.FirstOrDefaultAsync(d => d.Id == 1);

        Assert.NotNull(savedDocument);
        Assert.Equal("Regra para uso da churrasqueira: necessário reserva com 48h de antecedência", savedDocument.Content);
        Assert.NotNull(savedDocument.Embedding);
        Assert.Equal(3, savedDocument.Embedding.ToArray().Length);
    }

    [Fact]
    public async Task PostgreSQL_Should_Execute_Vector_Similarity_Query()
    {
        // Arrange - Habilita extensão pgvector primeiro
        await EnablePgvectorExtensionAsync();

        var currentTenantService = new TestCurrentTenantService { TenantId = 1 };
        var options = new DbContextOptionsBuilder<VectorTestDbContext>()
            .UseNpgsql(
                _postgresContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.UseVector())
            .Options;

        await using var context = new VectorTestDbContext(options, currentTenantService);
        await context.Database.EnsureCreatedAsync();

        // Adiciona documentos com diferentes embeddings
        context.DocumentEmbeddings.AddRange(
            new DocumentEmbedding
            {
                Id = 10,
                TenantId = 1,
                Content = "Área comum: Churrasqueira",
                Embedding = new Vector(new[] { 0.1f, 0.2f, 0.3f })
            },
            new DocumentEmbedding
            {
                Id = 20,
                TenantId = 1,
                Content = "Regra: Silêncio após 22h",
                Embedding = new Vector(new[] { 0.9f, 0.8f, 0.7f })
            },
            new DocumentEmbedding
            {
                Id = 30,
                TenantId = 1,
                Content = "Reserva de salão de festas",
                Embedding = new Vector(new[] { 0.15f, 0.25f, 0.35f })
            }
        );
        await context.SaveChangesAsync();

        // Act - Busca vetorial por similaridade (distância L2)
        var queryEmbedding = new Vector(new[] { 0.12f, 0.22f, 0.32f });

        var results = await context.DocumentEmbeddings
            .OrderBy(d => d.Embedding!.L2Distance(queryEmbedding))
            .Take(2)
            .ToListAsync();

        // Assert - Valida resultados ordenados por similaridade
        Assert.Equal(2, results.Count);
        Assert.Equal(10, results[0].Id); // Churrasqueira (mais similar)
        Assert.Equal(30, results[1].Id); // Salão de festas (segunda mais similar)
    }

    [Fact]
    public async Task Pgvector_Should_Respect_MultiTenant_Isolation()
    {
        // Arrange - Habilita extensão pgvector primeiro
        await EnablePgvectorExtensionAsync();

        // Dois tenants diferentes
        var tenant1Service = new TestCurrentTenantService { TenantId = 1 };
        var tenant2Service = new TestCurrentTenantService { TenantId = 2 };

        var options = new DbContextOptionsBuilder<VectorTestDbContext>()
            .UseNpgsql(
                _postgresContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.UseVector())
            .Options;

        // Tenant 1 persiste embeddings
        await using (var contextTenant1 = new VectorTestDbContext(options, tenant1Service))
        {
            await contextTenant1.Database.EnsureCreatedAsync();
            contextTenant1.DocumentEmbeddings.Add(new DocumentEmbedding
            {
                Id = 100,
                TenantId = 1,
                Content = "Tenant1 Document",
                Embedding = new Vector(new[] { 0.1f, 0.2f, 0.3f })
            });
            await contextTenant1.SaveChangesAsync();
        }

        // Tenant 2 persiste embeddings
        await using (var contextTenant2 = new VectorTestDbContext(options, tenant2Service))
        {
            contextTenant2.DocumentEmbeddings.Add(new DocumentEmbedding
            {
                Id = 200,
                TenantId = 2,
                Content = "Tenant2 Document",
                Embedding = new Vector(new[] { 0.9f, 0.8f, 0.7f })
            });
            await contextTenant2.SaveChangesAsync();
        }

        // Act & Assert - Tenant 1 vê apenas seus embeddings
        await using (var contextTenant1Read = new VectorTestDbContext(options, tenant1Service))
        {
            var tenant1Embeddings = await contextTenant1Read.DocumentEmbeddings.ToListAsync();
            Assert.Single(tenant1Embeddings);
            Assert.Equal("Tenant1 Document", tenant1Embeddings[0].Content);
        }

        // Act & Assert - Tenant 2 vê apenas seus embeddings
        await using (var contextTenant2Read = new VectorTestDbContext(options, tenant2Service))
        {
            var tenant2Embeddings = await contextTenant2Read.DocumentEmbeddings.ToListAsync();
            Assert.Single(tenant2Embeddings);
            Assert.Equal("Tenant2 Document", tenant2Embeddings[0].Content);
        }
    }
}

// ============================================
// Classes de teste auxiliares
// ============================================

/// <summary>
/// DbContext de teste para validação de suporte a pgvector
/// </summary>
internal class VectorTestDbContext : MultiTenantDbContext
{
    public DbSet<DocumentEmbedding> DocumentEmbeddings => Set<DocumentEmbedding>();

    public VectorTestDbContext(DbContextOptions<VectorTestDbContext> options, ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DocumentEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Embedding).HasColumnType("vector(3)");
        });
    }
}

/// <summary>
/// Entidade de teste para documentos com embeddings vetoriais
/// </summary>
internal class DocumentEmbedding : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
}
