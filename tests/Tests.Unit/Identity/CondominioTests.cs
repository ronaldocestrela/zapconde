using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public sealed class CondominioTests
{
    private static Endereco ValidEndereco() => new()
    {
        Cep = "01310100",
        Logradouro = "Av Paulista",
        Numero = "1000",
        Bairro = "Bela Vista",
        Cidade = "São Paulo",
        Uf = "SP"
    };

    private static ConfiguracoesIniciais ValidConfig() => new()
    {
        DiaVencimento = 10,
        JurosEnabled = true,
        MultaEnabled = true,
        BankGateway = BankGateway.Asaas,
        WhatsAppAiEnabled = true
    };

    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var condo = Condominio.Create(
            10,
            1,
            "Residencial Jardim",
            CondominioTipo.Residencial,
            120,
            4,
            ValidEndereco(),
            "Maria Silva",
            "maria@condo.com",
            "+5511999999999",
            "+5511888888888",
            ValidConfig());

        condo.Id.Should().Be(10);
        condo.TenantId.Should().Be(1);
        condo.TotalUnits.Should().Be(120);
        condo.Configuracoes.DiaVencimento.Should().Be(10);
    }

    [Fact]
    public void Create_WithInvalidCep_Should_Throw()
    {
        var endereco = ValidEndereco();
        endereco.Cep = "123";

        var act = () => Condominio.Create(
            10, 1, "Nome", CondominioTipo.Misto, 10, 1,
            endereco, "Admin", "a@b.com", "", "", ValidConfig());

        act.Should().Throw<DomainValidationException>().WithMessage("*CEP*");
    }

    [Fact]
    public void Create_WithInvalidVencimentoDay_Should_Throw()
    {
        var config = ValidConfig();
        config.DiaVencimento = 32;

        var act = () => Condominio.Create(
            10, 1, "Nome", CondominioTipo.Residencial, 10, 1,
            ValidEndereco(), "Admin", "a@b.com", "", "", config);

        act.Should().Throw<DomainValidationException>().WithMessage("*vencimento*");
    }

    [Fact]
    public void Create_WithZeroUnits_Should_Throw()
    {
        var act = () => Condominio.Create(
            10, 1, "Nome", CondominioTipo.Residencial, 0, 1,
            ValidEndereco(), "Admin", "a@b.com", "", "", ValidConfig());

        act.Should().Throw<DomainValidationException>();
    }
}
