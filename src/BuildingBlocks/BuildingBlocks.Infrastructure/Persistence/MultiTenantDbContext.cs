using BuildingBlocks.Shared.MultiTenancy;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Contexto base abstrato para aplicação multi-tenant com isolamento automático por tenant_id.
/// Aplica Global Query Filter do EF Core em todas as entidades que implementam ITenantScoped.
/// Mapeia entidades de Transactional Outbox Pattern do MassTransit.
/// </summary>
public abstract class MultiTenantDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    /// <summary>
    /// Construtor protegido para uso por contextos derivados dos módulos.
    /// </summary>
    /// <param name="options">Opções de configuração do DbContext</param>
    /// <param name="currentTenantService">Serviço de resolução do tenant atual</param>
    protected MultiTenantDbContext(
        DbContextOptions options,
        ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    /// <summary>
    /// Tenant ID resolvido no contexto atual (null quando não identificado).
    /// </summary>
    public int? CurrentTenantId => _currentTenantService?.TenantId;

    /// <summary>
    /// Aplica configuração de modelo incluindo Global Query Filter para entidades multi-tenant
    /// e mapeamento das tabelas do Transactional Outbox do MassTransit.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeia tabelas de outbox e inbox do MassTransit
        modelBuilder.AddTransactionalOutboxEntities();

        // Aplica filtro global para todas as entidades ITenantScoped
        ApplyGlobalFilters(modelBuilder);
    }

    /// <summary>
    /// Varre todas as entidades do modelo e aplica HasQueryFilter para aquelas que implementam ITenantScoped.
    /// Garante isolamento automático por tenant com fallback seguro (consultas vazias quando tenant não resolvido).
    /// </summary>
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Verifica se a entidade implementa ITenantScoped
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                // Aplica filtro global dinamicamente usando reflexão
                var method = typeof(MultiTenantDbContext)
                    .GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    /// <summary>
    /// Define o filtro global para uma entidade específica que implementa ITenantScoped.
    /// Filtro: e.TenantId == _currentTenantService.TenantId
    /// Comportamento seguro: quando TenantId é null, a comparação retorna false (nenhum registro).
    /// </summary>
    private void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        // Expressão Lambda: e => e.TenantId == _currentTenantService.TenantId
        // Comportamento: quando TenantId é null, a comparação (int == null) retorna false (deny-by-default)
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity =>
            entity.TenantId == _currentTenantService.TenantId);
    }
}
