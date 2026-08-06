using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Xunit;

namespace Tests.Unit.Financial;

public class AcordoDomainTests
{
    private readonly CalculadoraAcordoDomainService _calculadora = new();

    [Fact]
    public void SimularAcordo_DeveDividirParcelasComAjusteExatoDeCentavos()
    {
        // Arrange
        var valorOriginal = 1000m;
        var desconto = 100m; // total = 900
        var parcelas = 3;
        var vencimento = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var resultado = _calculadora.SimularAcordo(valorOriginal, desconto, parcelas, vencimento);

        // Assert
        resultado.ValorTotalOriginal.Should().Be(1000m);
        resultado.ValorDesconto.Should().Be(100m);
        resultado.ValorTotalAcordo.Should().Be(900m);
        resultado.QuantidadeParcelas.Should().Be(3);
        resultado.Parcelas.Should().HaveCount(3);
        resultado.Parcelas.Sum(p => p.ValorParcela).Should().Be(900m);
    }

    [Fact]
    public void CriarAcordo_DeveInicializarEmPropostaEVincularFaturas()
    {
        // Arrange & Act
        var acordo = Acordo.Create(
            tenantId: 1,
            condoId: 1,
            unidadeId: 101,
            moradorId: 5,
            dataPrimeiroVencimento: DateTime.UtcNow.AddDays(5),
            valorTotalOriginal: 600m,
            valorDesconto: 50m,
            quantidadeParcelas: 2
        );

        acordo.VincularFaturaOriginal(faturaId: 10, valorOriginal: 300m);
        acordo.VincularFaturaOriginal(faturaId: 11, valorOriginal: 300m);

        // Assert
        acordo.Status.Should().Be(StatusAcordo.Proposta);
        acordo.ValorTotalOriginal.Should().Be(600m);
        acordo.ValorTotalAcordo.Should().Be(550m);
        acordo.FaturasVinculadas.Should().HaveCount(2);
    }

    [Fact]
    public void EfetivarAcordo_DeveAlterarStatusParaAtivo()
    {
        // Arrange
        var acordo = Acordo.Create(1, 1, 101, 5, DateTime.UtcNow.AddDays(5), 500m, 50m, 2);

        // Act
        acordo.EfetivarAcordo(DateTime.UtcNow);

        // Assert
        acordo.Status.Should().Be(StatusAcordo.Ativo);
        acordo.DataAceite.Should().NotBeNull();
    }

    [Fact]
    public void RegistrarPagamentoParcela_QuandoTodasPagas_DeveQuitarAcordo()
    {
        // Arrange
        var acordo = Acordo.Create(1, 1, 101, 5, DateTime.UtcNow.AddDays(5), 400m, 0m, 2);
        var p1 = ParcelaAcordo.Create(1, acordo.Id, 1, DateTime.UtcNow.AddDays(5), 200m);
        var p2 = ParcelaAcordo.Create(1, acordo.Id, 2, DateTime.UtcNow.AddMonths(1), 200m);
        acordo.AdicionarParcela(p1);
        acordo.AdicionarParcela(p2);
        acordo.EfetivarAcordo(DateTime.UtcNow);

        // Act
        acordo.RegistrarPagamentoParcela(1, DateTime.UtcNow);
        acordo.Status.Should().Be(StatusAcordo.Ativo);

        acordo.RegistrarPagamentoParcela(2, DateTime.UtcNow);

        // Assert
        acordo.Status.Should().Be(StatusAcordo.Quitado);
    }

    [Fact]
    public void MarcarDescumprido_DeveAlterarStatusECancelarParcelasPendentes()
    {
        // Arrange
        var acordo = Acordo.Create(1, 1, 101, 5, DateTime.UtcNow.AddDays(5), 400m, 0m, 2);
        var p1 = ParcelaAcordo.Create(1, acordo.Id, 1, DateTime.UtcNow.AddDays(5), 200m);
        var p2 = ParcelaAcordo.Create(1, acordo.Id, 2, DateTime.UtcNow.AddMonths(1), 200m);
        acordo.AdicionarParcela(p1);
        acordo.AdicionarParcela(p2);
        acordo.EfetivarAcordo(DateTime.UtcNow);

        // Act
        acordo.MarcarDescumprido();

        // Assert
        acordo.Status.Should().Be(StatusAcordo.Descumprido);
        p1.Status.Should().Be(StatusParcelaAcordo.Cancelada);
        p2.Status.Should().Be(StatusParcelaAcordo.Cancelada);
    }
}
