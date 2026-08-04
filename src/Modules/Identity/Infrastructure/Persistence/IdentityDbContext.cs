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
    }
}
