using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public sealed class VinculoUnidadeTests
{
    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var vinculo = VinculoUnidade.Create(
            1, 10, 1, 1, PapelVinculo.Proprietario,
            new DateTime(2024, 1, 15),
            ["Vagas de Garagem", "Pets"],
            "user-1");

        vinculo.IsActive.Should().BeTrue();
        vinculo.Papel.Should().Be(PapelVinculo.Proprietario);
        vinculo.Dependencias.Should().Contain("Vagas de Garagem");
        vinculo.Dependencias.Should().Contain("Pets");
    }

    [Fact]
    public void Encerrar_WithValidData_Should_ArchiveVinculo()
    {
        var vinculo = VinculoUnidade.Create(1, 10, 1, 1, PapelVinculo.Proprietario, new DateTime(2024, 1, 1));

        vinculo.Encerrar(new DateTime(2025, 10, 31), "Contrato Encerrado");

        vinculo.IsActive.Should().BeFalse();
        vinculo.DataFim.Should().Be(new DateTime(2025, 10, 31));
        vinculo.MotivoEncerramento.Should().Be("Contrato Encerrado");
    }

    [Fact]
    public void Encerrar_WhenAlreadyClosed_Should_Throw()
    {
        var vinculo = VinculoUnidade.Create(1, 10, 1, 1, PapelVinculo.Inquilino, new DateTime(2024, 1, 1));
        vinculo.Encerrar(new DateTime(2025, 1, 1), "Saída");

        var act = () => vinculo.Encerrar(new DateTime(2025, 2, 1), "Duplicado");

        act.Should().Throw<DomainValidationException>().WithMessage("*encerrado*");
    }

    [Fact]
    public void Encerrar_WithDataBeforeInicio_Should_Throw()
    {
        var vinculo = VinculoUnidade.Create(1, 10, 1, 1, PapelVinculo.Proprietario, new DateTime(2024, 6, 1));

        var act = () => vinculo.Encerrar(new DateTime(2024, 1, 1), "Inválido");

        act.Should().Throw<DomainValidationException>().WithMessage("*encerramento*");
    }
}

public sealed class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25", true)]
    [InlineData("52998224725", true)]
    [InlineData("00000000000", false)]
    [InlineData("123", false)]
    public void IsValid_Should_ValidateCpf(string cpf, bool expected)
    {
        CpfValidator.IsValid(cpf).Should().Be(expected);
    }
}

public sealed class MoradorTests
{
    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var morador = Morador.Create(1, 10, "Maria Silva", "52998224725", "maria@test.com", "+5511999999999");

        morador.Nome.Should().Be("Maria Silva");
        morador.Cpf.Should().Be("52998224725");
        morador.Email.Should().Be("maria@test.com");
    }

    [Fact]
    public void Create_WithInvalidCpf_Should_Throw()
    {
        var act = () => Morador.Create(1, 10, "Maria", "00000000000", "maria@test.com", "");

        act.Should().Throw<DomainValidationException>().WithMessage("*CPF*");
    }
}
