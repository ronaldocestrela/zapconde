using FluentAssertions;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Xunit;

namespace Tests.Unit.Operations;

public class AreaComumDomainTests
{
    [Fact]
    public void Should_CreateAreaComum_When_AllParametersAreValid()
    {
        // Arrange
        var tenantId = 1;
        var condoId = 10;
        var nome = "Salão de Festas Principal";
        var descricao = "Salão amplo para até 100 pessoas";
        var tipo = TipoAreaComum.Eventos;
        var capacidade = 100;
        var taxaReserva = 150.00m;
        var taxaLimpeza = 50.00m;
        var inicio = new TimeSpan(8, 0, 0);
        var fim = new TimeSpan(22, 0, 0);

        // Act
        var area = AreaComum.Create(
            tenantId,
            condoId,
            nome,
            descricao,
            tipo,
            capacidade,
            taxaReserva,
            taxaLimpeza,
            inicio,
            fim,
            tempoAntecedenciaMinimaDias: 2,
            tempoAntecedenciaMaximaDias: 30,
            requerAprovacaoSindico: true,
            regrasUso: "Proibido som alto após 22h");

        // Assert
        area.Should().NotBeNull();
        area.TenantId.Should().Be(tenantId);
        area.CondoId.Should().Be(condoId);
        area.Nome.Should().Be(nome);
        area.Descricao.Should().Be(descricao);
        area.Tipo.Should().Be(tipo);
        area.CapacidadeMaxima.Should().Be(capacidade);
        area.TaxaReserva.Should().Be(taxaReserva);
        area.TaxaLimpeza.Should().Be(taxaLimpeza);
        area.CustoTotalReserva.Should().Be(200.00m);
        area.HorarioInicioFuncionamento.Should().Be(inicio);
        area.HorarioFimFuncionamento.Should().Be(fim);
        area.Status.Should().Be(StatusAreaComum.Ativa);
        area.RequerAprovacaoSindico.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_ThrowArgumentException_When_CapacidadeIsInvalid(int capacidadeInvalida)
    {
        // Act
        Action act = () => AreaComum.Create(
            1, 1, "Churrasqueira VIP", "Desc", TipoAreaComum.Churrasqueira,
            capacidadeInvalida, 50m, 20m, new TimeSpan(10, 0, 0), new TimeSpan(20, 0, 0));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*capacidade máxima deve ser maior que zero*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_TaxaReservaIsNegative()
    {
        // Act
        Action act = () => AreaComum.Create(
            1, 1, "Quadra", "Desc", TipoAreaComum.Esportes,
            20, -10m, 20m, new TimeSpan(10, 0, 0), new TimeSpan(20, 0, 0));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*taxa de reserva não pode ser negativa*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_HorarioInicioIsAfterOrEqualFim()
    {
        // Act
        Action act = () => AreaComum.Create(
            1, 1, "Espaço Gourmet", "Desc", TipoAreaComum.Gourmet,
            30, 100m, 30m, new TimeSpan(22, 0, 0), new TimeSpan(8, 0, 0));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*horário de início*anterior ao horário de término*");
    }

    [Fact]
    public void Should_UpdateStatus_When_AlterarStatusIsCalled()
    {
        // Arrange
        var area = AreaComum.Create(
            1, 1, "Piscina", "Piscina olímpica", TipoAreaComum.Lazer,
            50, 0m, 0m, new TimeSpan(8, 0, 0), new TimeSpan(20, 0, 0));

        // Act
        area.AlterarStatus(StatusAreaComum.Manutencao);

        // Assert
        area.Status.Should().Be(StatusAreaComum.Manutencao);
        area.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Should_ValidateCapacidadeElegibility()
    {
        // Arrange
        var area = AreaComum.Create(
            1, 1, "Salão de Jogos", "Desc", TipoAreaComum.Lazer,
            15, 30m, 10m, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0));

        // Act & Assert
        area.ValidarElegibilidadeCapacidade(10).Should().BeTrue();
        area.ValidarElegibilidadeCapacidade(15).Should().BeTrue();
        area.ValidarElegibilidadeCapacidade(20).Should().BeFalse();
        area.ValidarElegibilidadeCapacidade(0).Should().BeFalse();
    }

    [Fact]
    public void Should_CalculateCorrectCustoTotalReserva_When_RegrasECustosUpdated()
    {
        // Arrange
        var area = AreaComum.Create(
            1, 1, "Churrasqueira 1", "Desc", TipoAreaComum.Churrasqueira,
            30, 80m, 40m, new TimeSpan(10, 0, 0), new TimeSpan(22, 0, 0));

        // Act
        area.AtualizarRegrasECustos(100m, 50m, 3, 45);

        // Assert
        area.TaxaReserva.Should().Be(100m);
        area.TaxaLimpeza.Should().Be(50m);
        area.CustoTotalReserva.Should().Be(150m);
        area.TempoAntecedenciaMinimaDias.Should().Be(3);
        area.TempoAntecedenciaMaximaDias.Should().Be(45);
    }
}
