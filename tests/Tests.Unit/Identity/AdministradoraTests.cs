using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public sealed class AdministradoraTests
{
    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var admin = Administradora.Create(
            1,
            "Administradora Exemplo LTDA",
            "07.526.557/0001-00",
            "Admin Exemplo",
            LicensePlan.Professional);

        admin.Id.Should().Be(1);
        admin.RazaoSocial.Should().Be("Administradora Exemplo LTDA");
        admin.Cnpj.Should().Be("07526557000100");
        admin.LicensePlan.Should().Be(LicensePlan.Professional);
    }

    [Fact]
    public void Create_WithInvalidCnpj_Should_Throw()
    {
        var act = () => Administradora.Create(1, "Razao", "00000000000000", "Fantasia", LicensePlan.Starter);
        act.Should().Throw<DomainValidationException>().WithMessage("*CNPJ*");
    }

    [Fact]
    public void Cnpj_Normalize_Should_StripFormatting()
    {
        CnpjValidator.Normalize("07.526.557/0001-00").Should().Be("07526557000100");
    }

    [Fact]
    public void Cnpj_IsValid_Should_ValidateCheckDigits()
    {
        CnpjValidator.IsValid("07.526.557/0001-00").Should().BeTrue();
        CnpjValidator.IsValid("11.222.333/0001-81").Should().BeTrue();
        CnpjValidator.IsValid("00000000000000").Should().BeFalse();
    }
}
