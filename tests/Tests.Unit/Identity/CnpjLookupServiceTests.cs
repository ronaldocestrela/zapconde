using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Services;

namespace Tests.Unit.Identity;

public sealed class CnpjLookupServiceTests
{
    [Fact]
    public async Task GetStatusAsync_WithExistingCnpj_Should_ReturnConflictMessage()
    {
        await using var db = CreateDbContext();
        db.Administradoras.Add(Administradora.Create(
            1, "Existente LTDA", "07.526.557/0001-00", "Existente", LicensePlan.Starter));
        await db.SaveChangesAsync();

        var service = new CnpjLookupService(db);
        var result = await service.GetStatusAsync("07.526.557/0001-00");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("CNPJ já cadastrado");
    }

    [Fact]
    public async Task GetStatusAsync_WithAvailableCnpj_Should_ReturnSuccess()
    {
        await using var db = CreateDbContext();
        var service = new CnpjLookupService(db);

        var result = await service.GetStatusAsync("11.222.333/0001-81");

        result.IsSuccess.Should().BeTrue();
        result.Data!.IsAvailable.Should().BeTrue();
    }

    private static IdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantService = new BuildingBlocks.Infrastructure.MultiTenancy.CurrentTenantService();
        return new IdentityDbContext(options, tenantService);
    }
}
