using System.Text.Json;
using BuildingBlocks.Shared;
using FluentAssertions;
using Moq;
using Modules.AIEngine.Application.Plugins;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;
using Xunit;

namespace Tests.Unit.AIEngine;

public class BoletoPluginTests
{
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly BoletoPlugin _plugin;

    public BoletoPluginTests()
    {
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _plugin = new BoletoPlugin(_invoiceServiceMock.Object);
    }

    [Fact]
    public async Task Should_ReturnBoletosJson_When_MoradorHasPendingBoletos()
    {
        // Arrange
        var moradorId = 10;
        var pendingBoletos = new List<PendingBoletoDto>
        {
            new(
                FaturaId: 1,
                BoletoId: 100,
                MoradorId: moradorId,
                UnidadeId: 101,
                Competencia: "2026-08",
                NumeroFatura: "FAT-202608-101",
                ValorTotal: 250.00m,
                DataVencimento: DateTime.UtcNow.AddDays(5),
                StatusFatura: StatusFatura.Pendente,
                StatusFaturaDescricao: "Pendente",
                CodigoPixCopiaECola: "00020126580014br.gov.bcb.pix0136zapcondo-pix-1-fat1",
                LinhaDigitavel: "34191.79001 12345.67890",
                CodigoBarras: "341981234567890",
                PdfUrl: "/api/financial/invoices/1/pdf",
                Vencido: false
            )
        };

        _invoiceServiceMock
            .Setup(s => s.GetPendingBoletosByMoradorAsync(moradorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<PendingBoletoDto>>.Success(pendingBoletos));

        // Act
        var jsonResult = await _plugin.GetPendingBoletosAsync(moradorId);

        // Assert
        jsonResult.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;
        root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        root.GetProperty("totalPendencias").GetInt32().Should().Be(1);
        root.GetProperty("valorTotal").GetDecimal().Should().Be(250.00m);

        var boletos = root.GetProperty("boletos");
        boletos.GetArrayLength().Should().Be(1);
        boletos[0].GetProperty("pixCopiaECola").GetString().Should().Contain("zapcondo-pix-1-fat1");
        boletos[0].GetProperty("pdfUrl").GetString().Should().Be("/api/financial/invoices/1/pdf");
    }

    [Fact]
    public async Task Should_ReturnAdimplenteJson_When_MoradorHasNoPendingBoletos()
    {
        // Arrange
        var moradorId = 20;
        _invoiceServiceMock
            .Setup(s => s.GetPendingBoletosByMoradorAsync(moradorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<PendingBoletoDto>>.Success(new List<PendingBoletoDto>()));

        // Act
        var jsonResult = await _plugin.GetPendingBoletosAsync(moradorId);

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;
        root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        root.GetProperty("totalPendencias").GetInt32().Should().Be(0);
        root.GetProperty("mensagem").GetString().Should().Contain("totalmente em dia");
    }

    [Fact]
    public async Task Should_ReturnErrorJson_When_MoradorIdIsInvalid()
    {
        // Act
        var jsonResult = await _plugin.GetPendingBoletosAsync(0);

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;
        root.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        root.GetProperty("mensagem").GetString().Should().Contain("inválido");
    }
}
