using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public sealed class BlocoTests
{
    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var bloco = Bloco.Create(1, 10, "Bloco A", "Torre A", 1);

        bloco.TenantId.Should().Be(1);
        bloco.CondoId.Should().Be(10);
        bloco.Codigo.Should().Be("Bloco A");
        bloco.Nome.Should().Be("Torre A");
        bloco.Ordem.Should().Be(1);
    }

    [Fact]
    public void Create_WithEmptyCodigo_Should_Throw()
    {
        var act = () => Bloco.Create(1, 10, "  ");

        act.Should().Throw<DomainValidationException>().WithMessage("*bloco*");
    }
}
