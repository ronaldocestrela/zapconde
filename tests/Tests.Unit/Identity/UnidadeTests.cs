using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public sealed class UnidadeTests
{
    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var unidade = Unidade.Create(1, 10, 1, "101");

        unidade.TenantId.Should().Be(1);
        unidade.CondoId.Should().Be(10);
        unidade.BlocoId.Should().Be(1);
        unidade.Numero.Should().Be("101");
        unidade.Status.Should().Be(UnidadeStatus.Vaga);
    }

    [Fact]
    public void Create_WithEmptyNumero_Should_Throw()
    {
        var act = () => Unidade.Create(1, 10, 1, "");

        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void ValidarNovoVinculo_WhenProprietarioAlreadyActive_Should_Throw()
    {
        var unidade = Unidade.Create(1, 10, 1, "101");
        var vinculo = VinculoUnidade.Create(1, 10, 0, 1, PapelVinculo.Proprietario, DateTime.UtcNow);
        unidade.Vinculos.Add(vinculo);

        var act = () => unidade.ValidarNovoVinculo(PapelVinculo.Proprietario);

        act.Should().Throw<DomainValidationException>().WithMessage("*proprietário*");
    }

    [Fact]
    public void RecalcularStatus_WithActiveVinculo_Should_SetOcupada()
    {
        var unidade = Unidade.Create(1, 10, 1, "101");
        unidade.Vinculos.Add(VinculoUnidade.Create(1, 10, 0, 1, PapelVinculo.Inquilino, DateTime.UtcNow));

        unidade.RecalcularStatus();

        unidade.Status.Should().Be(UnidadeStatus.Ocupada);
    }

    [Fact]
    public void RecalcularStatus_WhenEmReforma_Should_KeepEmReforma()
    {
        var unidade = Unidade.Create(1, 10, 1, "101", UnidadeStatus.EmReforma);
        unidade.Vinculos.Add(VinculoUnidade.Create(1, 10, 0, 1, PapelVinculo.Inquilino, DateTime.UtcNow));

        unidade.RecalcularStatus();

        unidade.Status.Should().Be(UnidadeStatus.EmReforma);
    }
}
