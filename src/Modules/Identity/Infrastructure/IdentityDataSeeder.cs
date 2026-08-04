using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using OpenIddict.Abstractions;

namespace Modules.Identity.Infrastructure;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var authOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthOptions>>().Value;

        foreach (var role in SmartCondoRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        if (await scopeManager.FindByNameAsync("smartcondo_api", ct) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "smartcondo_api",
                DisplayName = "SmartCondo API",
                Resources = { "SmartCondo" }
            }, ct);
        }

        if (await applicationManager.FindByClientIdAsync(authOptions.BlazorClientId, ct) is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = authOptions.BlazorClientId,
                DisplayName = "SmartCondo Blazor Web",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "smartcondo_api"
                }
            }, ct);
        }

        await SeedDemoUserAsync(userManager, dbContext, ct);
    }

    private static async Task SeedDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        const string email = "sindico@zapcond.com";
        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var user = await userManager.FindByEmailAsync(email)
            ?? await userManager.FindByIdAsync(demoUserId.ToString());

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = demoUserId,
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                DisplayName = "Síndico Demo",
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, "Senha@123");
            if (!createResult.Succeeded)
            {
                user = await userManager.FindByIdAsync(demoUserId.ToString())
                    ?? throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }
        }

        const string blockedEmail = "bloqueado@zapcond.com";
        if (await userManager.FindByEmailAsync(blockedEmail) is null)
        {
            var blocked = new ApplicationUser
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Email = blockedEmail,
                UserName = blockedEmail,
                EmailConfirmed = true,
                DisplayName = "Usuário Bloqueado",
                IsActive = false
            };
            await userManager.CreateAsync(blocked, "Senha@123");
        }

        await EnsureMembershipAsync(
            dbContext,
            user.Id,
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            tenantId: 1,
            condoId: 10,
            SmartCondoRoles.Sindico,
            "Condomínio Ville de Paris - Bloco A",
            ct);

        await EnsureMembershipAsync(
            dbContext,
            user.Id,
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            tenantId: 2,
            condoId: 20,
            SmartCondoRoles.Administradora,
            "Residencial Jardim das Flores",
            ct);

        await EnsureMembershipAsync(
            dbContext,
            user.Id,
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            tenantId: 3,
            condoId: 30,
            SmartCondoRoles.Sindico,
            "Edifício Belvedere",
            ct);

        await EnsureMembershipAsync(
            dbContext,
            user.Id,
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            tenantId: 1,
            condoId: 10,
            SmartCondoRoles.Portaria,
            "Condomínio Ville de Paris - Portaria",
            ct);
    }

    private static async Task EnsureMembershipAsync(
        IdentityDbContext dbContext,
        Guid userId,
        Guid membershipId,
        int tenantId,
        int condoId,
        string role,
        string displayLabel,
        CancellationToken ct)
    {
        var existing = await dbContext.UserCondoMemberships
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == membershipId, ct);

        if (existing is not null)
        {
            existing.DisplayLabel = displayLabel;
            existing.TenantId = tenantId;
            existing.CondoId = condoId;
            existing.Role = role;
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        var duplicateRole = await dbContext.UserCondoMemberships
            .IgnoreQueryFilters()
            .AnyAsync(m => m.Id != membershipId &&
                           m.UserId == userId &&
                           m.TenantId == tenantId &&
                           m.CondoId == condoId &&
                           m.Role == role, ct);

        if (duplicateRole)
        {
            return;
        }

        dbContext.UserCondoMemberships.Add(new UserCondoMembership
        {
            Id = membershipId,
            UserId = userId,
            TenantId = tenantId,
            CondoId = condoId,
            Role = role,
            DisplayLabel = displayLabel,
            IsActive = true,
            IsTenantActive = true
        });

        await dbContext.SaveChangesAsync(ct);
    }
}
