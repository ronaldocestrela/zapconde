using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public class SmartCondoRolesTests
{
    [Theory]
    [InlineData("Porteiro", SmartCondoRoles.Portaria)]
    [InlineData("Morador", SmartCondoRoles.Condomino)]
    [InlineData("Sindico", SmartCondoRoles.Sindico)]
    [InlineData("Administradora", SmartCondoRoles.Administradora)]
    public void FromStitchLabel_Should_MapUiLabelsToCanonicalRoles(string stitchLabel, string expectedRole)
    {
        SmartCondoRoles.FromStitchLabel(stitchLabel).Should().Be(expectedRole);
    }

    [Fact]
    public void All_Should_ContainFiveRoles()
    {
        SmartCondoRoles.All.Should().HaveCount(5);
        SmartCondoRoles.All.Should().Contain(SmartCondoRoles.Portaria);
        SmartCondoRoles.All.Should().Contain(SmartCondoRoles.Condomino);
    }
}
