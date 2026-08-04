using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Services;
using BuildingBlocks.Infrastructure.MultiTenancy;

namespace Tests.Unit.Identity;

public class IdentityTokenServiceTests
{
    [Fact]
    public async Task CreateContextTokensAsync_Should_IncludeRequiredClaims()
    {
        await using var db = CreateDbContext();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@zapcond.com",
            UserName = "test@zapcond.com",
            IsActive = true
        };
        db.Users.Add(user);

        var membership = new UserCondoMembership
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = 1,
            CondoId = 10,
            Role = SmartCondoRoles.Sindico,
            IsActive = true,
            IsTenantActive = true
        };
        db.UserCondoMemberships.Add(membership);
        await db.SaveChangesAsync();

        var service = new IdentityTokenService(db, Options.Create(new AuthOptions()));
        var (accessToken, refreshToken, _) = await service.CreateContextTokensAsync(user, membership);

        accessToken.Should().NotBeNullOrWhiteSpace();
        refreshToken.Should().NotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.TenantId && c.Value == "1");
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.CondoId && c.Value == "10");
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.UserId && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == SmartCondoClaimTypes.Role && c.Value == SmartCondoRoles.Sindico);
    }

    private static IdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options, new CurrentTenantService());
    }
}
