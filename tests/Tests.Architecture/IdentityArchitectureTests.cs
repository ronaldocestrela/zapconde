using FluentAssertions;
using Modules.Identity.Domain;
using OpenIddict.Abstractions;

namespace Tests.Architecture;

public class IdentityArchitectureTests
{
    [Fact]
    public void IdentityModule_Should_ReferenceOpenIddictPackages()
    {
        typeof(OpenIddictConstants).Assembly.GetName().Name.Should().Be("OpenIddict.Abstractions");
        typeof(ApplicationUser).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Should()
            .Contain(name => name != null && name.StartsWith("OpenIddict", StringComparison.Ordinal));
    }

    [Fact]
    public void SmartCondoClaimTypes_Should_DefineRequiredClaims()
    {
        SmartCondoClaimTypes.TenantId.Should().Be("TenantId");
        SmartCondoClaimTypes.CondoId.Should().Be("CondoId");
        SmartCondoClaimTypes.UserId.Should().Be("UserId");
        SmartCondoClaimTypes.Role.Should().Be("Role");
    }

    [Fact]
    public void Api_Should_ReferenceIdentityModule()
    {
        var apiAssembly = typeof(Program).Assembly;
        var referenced = apiAssembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        referenced.Should().Contain("Modules.Identity");
    }
}
