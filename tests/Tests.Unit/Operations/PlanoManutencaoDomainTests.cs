using FluentAssertions;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;
using Xunit;

namespace Tests.Unit.Operations;

public class PlanoManutencaoDomainTests
{
    [Fact]
    public void Create_ShouldInitializePlanoManutencao_WhenValidParametersProvided()
    {
        // Arrange
        var tenantId = 1;
        var condoId = 1;
        var titulo = "Manutenção Preventiva de Elevador Social";
        var categoria = CategoriaManutencao.Elevadores;
        var periodicidade = PeriodicidadeManutencao.Mensal;
        var dataProxima = DateTime.Today.AddDays(30);

        // Act
        var plano = PlanoManutencao.Create(
            tenantId, condoId, titulo, categoria, periodicidade, dataProxima,
            descricao: "Vistoria técnica mensal nos cabos e motor",
            responsavelTecnico: "Eng. Roberto",
            empresaContratada: "Atlas Schindler",
            custoEstimado: 850.00m);

        // Assert
        plano.Should().NotBeNull();
        plano.Id.Should().NotBeEmpty();
        plano.TenantId.Should().Be(tenantId);
        plano.CondoId.Should().Be(condoId);
        plano.Titulo.Should().Be(titulo);
        plano.Categoria.Should().Be(CategoriaManutencao.Elevadores);
        plano.Periodicidade.Should().Be(PeriodicidadeManutencao.Mensal);
        plano.Status.Should().Be(StatusManutencao.EmDia);
        plano.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTituloIsEmpty()
    {
        // Act
        Action act = () => PlanoManutencao.Create(
            1, 1, "", CategoriaManutencao.BombasDagua, PeriodicidadeManutencao.Trimestral, DateTime.Today.AddDays(10));

        // Assert
        act.Should().Throw<PlanoManutencaoDomainException>()
            .WithMessage("*título*não pode ser vazio*");
    }

    [Fact]
    public void CalcularStatus_ShouldSetProxima_WhenDataProximaIsWithin15Days()
    {
        // Arrange
        var dataProxima = DateTime.Today.AddDays(7);
        var plano = PlanoManutencao.Create(
            1, 1, "Inspeção de Bombas", CategoriaManutencao.BombasDagua,
            PeriodicidadeManutencao.Mensal, dataProxima);

        // Act
        plano.CalcularStatus(DateTime.Today);

        // Assert
        plano.Status.Should().Be(StatusManutencao.Proxima);
    }

    [Fact]
    public void CalcularStatus_ShouldSetAtrasada_WhenDataProximaIsInThePast()
    {
        // Arrange
        var dataProxima = DateTime.Today.AddDays(-2);
        var plano = PlanoManutencao.Create(
            1, 1, "Vistoria Para-raios", CategoriaManutencao.ParaRaios,
            PeriodicidadeManutencao.Anual, dataProxima);

        // Act
        plano.CalcularStatus(DateTime.Today);

        // Assert
        plano.Status.Should().Be(StatusManutencao.Atrasada);
    }

    [Fact]
    public void ConcluirManutencao_ShouldUpdateDataUltimaAndAdvanceNextDate_WhenAgendarProximaIsTrue()
    {
        // Arrange
        var dataProximaOriginal = DateTime.Today.AddDays(-5);
        var plano = PlanoManutencao.Create(
            1, 1, "Revisão Gerador", CategoriaManutencao.Geradores,
            PeriodicidadeManutencao.Semestral, dataProximaOriginal);

        plano.Status.Should().Be(StatusManutencao.Atrasada);

        var dataHoje = DateTime.Today;

        // Act
        plano.ConcluirManutencao(dataHoje, custoReal: 1500.00m, observacoes: "Filtros e óleo trocados", agendarProxima: true);

        // Assert
        plano.DataUltimaManutencao.Should().Be(dataHoje);
        plano.CustoReal.Should().Be(1500.00m);
        plano.DataProximaManutencao.Should().Be(dataHoje.AddMonths(6));
        plano.Status.Should().Be(StatusManutencao.EmDia);
        plano.Observacoes.Should().Contain("Filtros e óleo trocados");
    }

    [Fact]
    public void CalcularProximaData_ShouldCalculateCorrectly_ForDifferentPeriodicidades()
    {
        var baseDate = new DateTime(2026, 1, 15);

        PlanoManutencao.CalcularProximaData(baseDate, PeriodicidadeManutencao.Semanal)
            .Should().Be(new DateTime(2026, 1, 22));

        PlanoManutencao.CalcularProximaData(baseDate, PeriodicidadeManutencao.Mensal)
            .Should().Be(new DateTime(2026, 2, 15));

        PlanoManutencao.CalcularProximaData(baseDate, PeriodicidadeManutencao.Semestral)
            .Should().Be(new DateTime(2026, 7, 15));

        PlanoManutencao.CalcularProximaData(baseDate, PeriodicidadeManutencao.Anual)
            .Should().Be(new DateTime(2027, 1, 15));
    }
}
