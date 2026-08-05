using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Xunit;

namespace Tests.Unit.Financial;

public class FinancialDomainTests
{
    [Fact]
    public void Fatura_Create_ShouldInitializeWithDefaultStatusPendenteAndCalculatedTotals()
    {
        // Arrange & Act
        var fatura = Fatura.Create(
            tenantId: 1,
            condoId: 10,
            unidadeId: 101,
            moradorId: 5,
            competencia: "2026-08",
            dataVencimento: DateTime.Today.AddDays(10),
            observacoes: "Taxa ordinária referente ao mês 08/2026"
        );

        // Assert
        fatura.TenantId.Should().Be(1);
        fatura.CondoId.Should().Be(10);
        fatura.UnidadeId.Should().Be(101);
        fatura.MoradorId.Should().Be(5);
        fatura.Competencia.Should().Be("2026-08");
        fatura.NumeroFatura.Should().Be("FAT-202608-101");
        fatura.Status.Should().Be(StatusFatura.Pendente);
        fatura.ValorOriginal.Should().Be(0);
        fatura.TotalFinal.Should().Be(0);
        fatura.Itens.Should().BeEmpty();
        fatura.Boleto.Should().BeNull();
    }

    [Fact]
    public void AddItem_ShouldAddItemsAndRecalculateValorOriginalAndTotalFinal()
    {
        // Arrange
        var fatura = Fatura.Create(1, 10, 101, 5, "2026-08", DateTime.Today.AddDays(10));

        // Act
        fatura.AddItem("Taxa Condominial Ordinária", TipoItemCobranca.TaxaCondominial, 450.00m, 1);
        fatura.AddItem("Fundo de Reserva (10%)", TipoItemCobranca.FundoReserva, 45.00m, 1);
        fatura.AddItem("Consumo de Gás Mês Anterior", TipoItemCobranca.Gas, 35.50m, 1);

        // Assert
        fatura.Itens.Should().HaveCount(3);
        fatura.ValorOriginal.Should().Be(530.50m);
        fatura.TotalFinal.Should().Be(530.50m);
    }

    [Fact]
    public void AnexarBoleto_ShouldLinkBoletoToFaturaCorrectly()
    {
        // Arrange
        var fatura = Fatura.Create(1, 10, 101, 5, "2026-08", DateTime.Today.AddDays(10));
        fatura.AddItem("Taxa Condominial", TipoItemCobranca.TaxaCondominial, 500.00m);

        var boleto = Boleto.Create(
            tenantId: 1,
            faturaId: fatura.Id,
            nossoNumero: "34190123456",
            linhaDigitavel: "34191.79001 01234.567890 12345.678901 8 90000000050000",
            codigoBarras: "34198900000000050000",
            codigoPix: "00020126580014br.gov.bcb.pix...",
            valor: 500.00m,
            dataVencimento: fatura.DataVencimento
        );

        // Act
        fatura.AnexarBoleto(boleto);

        // Assert
        fatura.Boleto.Should().NotBeNull();
        fatura.Boleto!.NossoNumero.Should().Be("34190123456");
        fatura.Boleto.Status.Should().Be(StatusBoleto.Gerado);
    }

    [Fact]
    public void Cancelar_ShouldChangeStatusToCanceladoAndCancelBoleto()
    {
        // Arrange
        var fatura = Fatura.Create(1, 10, 101, 5, "2026-08", DateTime.Today.AddDays(10));
        var boleto = Boleto.Create(1, fatura.Id, "123", "456", "789", "pix", 100m, DateTime.Today);
        fatura.AnexarBoleto(boleto);

        // Act
        fatura.Cancelar();

        // Assert
        fatura.Status.Should().Be(StatusFatura.Cancelado);
        fatura.Boleto!.Status.Should().Be(StatusBoleto.Cancelado);
    }

    [Fact]
    public void RegistrarPagamento_WhenFullyPaid_ShouldSetStatusPago()
    {
        // Arrange
        var fatura = Fatura.Create(1, 10, 101, 5, "2026-08", DateTime.Today.AddDays(10));
        fatura.AddItem("Taxa Condominial", TipoItemCobranca.TaxaCondominial, 300.00m);
        var boleto = Boleto.Create(1, fatura.Id, "123", "456", "789", "pix", 300m, DateTime.Today);
        fatura.AnexarBoleto(boleto);

        // Act
        fatura.RegistrarPagamento(DateTime.UtcNow, 300.00m);

        // Assert
        fatura.Status.Should().Be(StatusFatura.Pago);
        fatura.DataPagamento.Should().NotBeNull();
        fatura.Boleto!.Status.Should().Be(StatusBoleto.Pago);
        fatura.Boleto.DataPagamento.Should().NotBeNull();
    }
}
