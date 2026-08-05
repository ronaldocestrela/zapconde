using BuildingBlocks.Shared.Enums;
using FluentAssertions;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Infrastructure.Services;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Xunit;

namespace Tests.Unit.Financial;

public class PaymentGatewayDomainTests
{
    [Fact]
    public void Boleto_VincularCobrancaGateway_ShouldUpdatePropertiesCorrectly()
    {
        // Arrange
        var boleto = Boleto.Create(
            tenantId: 1,
            faturaId: 10,
            nossoNumero: "NOSSO-001",
            linhaDigitavel: "12345.67890",
            codigoBarras: "12345678901234567890",
            codigoPix: "pix-copia-cola-test",
            valor: 250.00m,
            dataVencimento: DateTime.UtcNow.AddDays(5)
        );

        // Act
        boleto.VincularCobrancaGateway(
            externalChargeId: "pay_asaas_999",
            provider: PaymentGatewayProvider.Asaas,
            linhaDigitavel: "34191.79001",
            codigoBarras: "341987654321",
            codigoPix: "pix-payload-atualizado",
            qrCodeBase64: "data:image/png;base64,iVBORw0KGgo...",
            pdfUrl: "https://asaas.com/pdf/pay_asaas_999"
        );

        // Assert
        boleto.ExternalChargeId.Should().Be("pay_asaas_999");
        boleto.GatewayProvider.Should().Be(PaymentGatewayProvider.Asaas);
        boleto.LinhaDigitavel.Should().Be("34191.79001");
        boleto.PixQrCodeBase64.Should().Be("data:image/png;base64,iVBORw0KGgo...");
        boleto.PdfUrl.Should().Be("https://asaas.com/pdf/pay_asaas_999");
        boleto.DataUltimaSincronizacaoGateway.Should().NotBeNull();
    }

    [Fact]
    public async Task MockPaymentGatewayService_GerarCobranca_ShouldReturnValidPayloads()
    {
        // Arrange
        var mockService = new MockPaymentGatewayService();
        var request = new BoletoCobrancaRequestDto(
            FaturaId: 101,
            Valor: 500.00m,
            DataVencimento: DateTime.UtcNow.AddDays(10),
            MoradorNome: "João Silva",
            MoradorCpfCnpj: "12345678901"
        );

        // Act
        var result = await mockService.GerarCobrancaBoletoPixAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ExternalChargeId.Should().StartWith("pay_mock_101_");
        result.Data.CodigoPixCopiaECola.Should().Contain("BR.GOV.BCB.PIX");
        result.Data.PixQrCodeBase64.Should().StartWith("data:image/svg+xml;base64,");
        result.Data.PdfUrl.Should().Contain("pay_mock_101_");
    }

    [Fact]
    public async Task MockPaymentGatewayService_GerarCobranca_WithZeroAmount_ShouldFailValidation()
    {
        // Arrange
        var mockService = new MockPaymentGatewayService();
        var request = new BoletoCobrancaRequestDto(
            FaturaId: 102,
            Valor: 0m,
            DataVencimento: DateTime.UtcNow.AddDays(5),
            MoradorNome: "Maria Souza",
            MoradorCpfCnpj: "98765432100"
        );

        // Act
        var result = await mockService.GerarCobrancaBoletoPixAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Valor da cobrança deve ser maior que zero.");
    }
}
