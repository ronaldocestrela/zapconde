using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Domain.ValueObjects;
using Xunit;

namespace Tests.Unit.Financial;

public class FinancialCalculationDomainTests
{
    private readonly CalculadoraFinanceira _calculadora = new();

    [Fact]
    public void Should_Apply_EarlyPaymentDiscount_When_PaymentDate_Is_On_Or_Before_DueDate()
    {
        // Arrange
        var vencimento = new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);
        var dataCalculo = new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);

        var parametros = new ParametrosCalculoFinanceiro(
            valorOriginal: 500.00m,
            dataVencimento: vencimento,
            dataCalculo: dataCalculo,
            percentualMulta: 2.0m,
            percentualJurosMensal: 1.0m,
            valorDescontoPontualidade: 20.00m
        );

        // Act
        var resultado = _calculadora.CalcularEncargos(parametros);

        // Assert
        resultado.DiasAtraso.Should().Be(0);
        resultado.ValorMulta.Should().Be(0m);
        resultado.ValorJuros.Should().Be(0m);
        resultado.ValorDesconto.Should().Be(20.00m);
        resultado.ValorTotalCalculado.Should().Be(480.00m);
        resultado.MemoriaCalculoTextual.Should().Contain("Em dia");
        resultado.MemoriaCalculoTextual.Should().Contain("Desconto de Pontualidade Aplicado: -R$ 20,00");
    }

    [Fact]
    public void Should_Not_Apply_Discount_And_Apply_Fine_And_ProRataInterest_When_Overdue()
    {
        // Arrange
        var vencimento = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var dataCalculo = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Utc); // 10 dias de atraso

        var parametros = new ParametrosCalculoFinanceiro(
            valorOriginal: 1000.00m,
            dataVencimento: vencimento,
            dataCalculo: dataCalculo,
            percentualMulta: 2.0m,
            percentualJurosMensal: 1.0m,
            valorDescontoPontualidade: 50.00m
        );

        // Act
        var resultado = _calculadora.CalcularEncargos(parametros);

        // Assert
        resultado.DiasAtraso.Should().Be(10);
        resultado.ValorMulta.Should().Be(20.00m); // 2% de 1000
        // Juros: 1000 * (1% / 30 / 100) * 10 = 1000 * 0.000333333 * 10 = 3.33333 -> R$ 3,33
        resultado.ValorJuros.Should().Be(3.33m);
        resultado.ValorDesconto.Should().Be(0m); // expirado
        resultado.ValorTotalCalculado.Should().Be(1023.33m);
        resultado.MemoriaCalculoTextual.Should().Contain("Em Atraso (10 dia(s) corrido(s))");
        resultado.MemoriaCalculoTextual.Should().Contain("Multa por Atraso (2,0%): R$ 20,00");
        resultado.MemoriaCalculoTextual.Should().Contain("TOTAL FINAL A PAGAR: R$ 1.023,33");
    }

    [Theory]
    [InlineData(1, 20.00, 0.33, 1020.33)]  // 1 dia atraso: juros 1000 * (1%/30)*1 = 0.333 -> 0.33
    [InlineData(15, 20.00, 5.00, 1025.00)] // 15 dias atraso: juros 1000 * (1%/30)*15 = 5.00
    [InlineData(30, 20.00, 10.00, 1030.00)]// 30 dias atraso: juros 1000 * (1%/30)*30 = 10.00
    [InlineData(45, 20.00, 15.00, 1035.00)]// 45 dias atraso: juros 1000 * (1%/30)*45 = 15.00
    public void Should_Calculate_Exact_ProRata_Interest_For_Different_Day_Ranges(
        int diasAtraso,
        decimal expectedMulta,
        decimal expectedJuros,
        decimal expectedTotal)
    {
        // Arrange
        var vencimento = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var dataCalculo = vencimento.AddDays(diasAtraso);

        var parametros = new ParametrosCalculoFinanceiro(
            valorOriginal: 1000.00m,
            dataVencimento: vencimento,
            dataCalculo: dataCalculo,
            percentualMulta: 2.0m,
            percentualJurosMensal: 1.0m
        );

        // Act
        var resultado = _calculadora.CalcularEncargos(parametros);

        // Assert
        resultado.DiasAtraso.Should().Be(diasAtraso);
        resultado.ValorMulta.Should().Be(expectedMulta);
        resultado.ValorJuros.Should().Be(expectedJuros);
        resultado.ValorTotalCalculado.Should().Be(expectedTotal);
    }

    [Fact]
    public void Should_ThrowException_When_ValorOriginal_IsZero_Or_Negative()
    {
        // Arrange
        var act = () => new ParametrosCalculoFinanceiro(
            valorOriginal: 0m,
            dataVencimento: DateTime.UtcNow,
            dataCalculo: DateTime.UtcNow
        );

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*Valor original deve ser maior que zero.*");
    }

    [Fact]
    public void Should_Apply_Calculation_To_Fatura_Entity()
    {
        // Arrange
        var fatura = Fatura.Create(
            tenantId: 1,
            condoId: 10,
            unidadeId: 101,
            moradorId: 5,
            competencia: "2026-08",
            dataVencimento: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)
        );
        fatura.AddItem("Taxa Condominial", TipoItemCobranca.TaxaCondominial, 800.00m);

        var dataSimulacao = new DateTime(2026, 8, 16, 0, 0, 0, DateTimeKind.Utc); // 15 dias atraso

        // Act
        var resultado = fatura.AplicarCalculoAtualizado(_calculadora, dataSimulacao);

        // Assert
        resultado.DiasAtraso.Should().Be(15);
        fatura.ValorMulta.Should().Be(16.00m); // 2% de 800
        fatura.ValorJuros.Should().Be(4.00m);  // 800 * (1%/30)*15 = 4.00
        fatura.TotalFinal.Should().Be(820.00m);
    }
}
