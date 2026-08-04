using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentTenantService _currentTenantService;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<UserCondoMembership> UserCondoMemberships => Set<UserCondoMembership>();

    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

    public DbSet<Administradora> Administradoras => Set<Administradora>();

    public DbSet<Condominio> Condominios => Set<Condominio>();

    public DbSet<Bloco> Blocos => Set<Bloco>();

    public DbSet<Unidade> Unidades => Set<Unidade>();

    public DbSet<Morador> Moradores => Set<Morador>();

    public DbSet<VinculoUnidade> VinculosUnidade => Set<VinculoUnidade>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        builder.UseOpenIddict();

        builder.Entity<UserCondoMembership>().HasQueryFilter(m =>
            m.TenantId == _currentTenantService.TenantId);

        builder.Entity<Administradora>().HasQueryFilter(a =>
            !_currentTenantService.TenantId.HasValue ||
            a.Id == _currentTenantService.TenantId);

        builder.Entity<Condominio>().HasQueryFilter(c =>
            !_currentTenantService.TenantId.HasValue ||
            c.TenantId == _currentTenantService.TenantId);

        builder.Entity<Bloco>().HasQueryFilter(b =>
            !_currentTenantService.TenantId.HasValue ||
            (b.TenantId == _currentTenantService.TenantId &&
             (!_currentTenantService.CondoId.HasValue || b.CondoId == _currentTenantService.CondoId)));

        builder.Entity<Unidade>().HasQueryFilter(u =>
            !_currentTenantService.TenantId.HasValue ||
            (u.TenantId == _currentTenantService.TenantId &&
             (!_currentTenantService.CondoId.HasValue || u.CondoId == _currentTenantService.CondoId)));

        builder.Entity<Morador>().HasQueryFilter(m =>
            !_currentTenantService.TenantId.HasValue ||
            (m.TenantId == _currentTenantService.TenantId &&
             (!_currentTenantService.CondoId.HasValue || m.CondoId == _currentTenantService.CondoId)));

        builder.Entity<VinculoUnidade>().HasQueryFilter(v =>
            !_currentTenantService.TenantId.HasValue ||
            (v.TenantId == _currentTenantService.TenantId &&
             (!_currentTenantService.CondoId.HasValue || v.CondoId == _currentTenantService.CondoId)));
    }
}
