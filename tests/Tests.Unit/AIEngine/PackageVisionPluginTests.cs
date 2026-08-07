using BuildingBlocks.Shared;
using FluentAssertions;
using Moq;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Domain.Enums;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Application.Plugins;
using Modules.AIEngine.Application.Services;
using Xunit;

namespace Tests.Unit.AIEngine;

public class PackageVisionPluginTests
{
    private readonly Mock<IPackageVisionOcrService> _visionOcrServiceMock;
    private readonly PackageVisionPlugin _plugin;

    public PackageVisionPluginTests()
    {
        _visionOcrServiceMock = new Mock<IPackageVisionOcrService>();
        _plugin = new PackageVisionPlugin(_visionOcrServiceMock.Object);
    }

    [Fact]
    public async Task ReadPackageLabel_ShouldReturnJsonWithExtractedLabelData()
    {
        // Arrange
        var extractionData = new PackageLabelExtractionResultDto(
            Sucesso: true,
            Mensagem: "Sucesso",
            NomeDestinatario: "Mariana Oliveira",
            BlocoUnidade: "Bloco B - Apto 204",
            CodigoRastreio: "TRK-998877",
            Transportadora: "Amazon Logistics",
            Remetente: "Vendedor Oficial",
            TipoSugerido: TipoEncomenda.Caixa,
            ConfiancaPercentual: 94.5,
            UnidadeIdIdentificada: 204,
            MoradorIdentificadoNome: "Mariana Oliveira",
            FotoEtiquetaUrl: "https://example.com/etiqueta.jpg",
            NotificacaoEnviada: false,
            CamposDetectadosJson: "{}"
        );

        _visionOcrServiceMock
            .Setup(s => s.ProcessLabelImageAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PackageLabelExtractionResultDto>.Success(extractionData));

        // Act
        var jsonResult = await _plugin.ReadPackageLabelAsync("https://example.com/etiqueta.jpg", condoId: 1);

        // Assert
        jsonResult.Should().Contain("Mariana Oliveira");
        jsonResult.Should().Contain("Bloco B - Apto 204");
        jsonResult.Should().Contain("TRK-998877");
        jsonResult.Should().Contain("Amazon Logistics");
    }

    [Fact]
    public async Task ReadPackageLabelAndNotify_ShouldRegisterPackageAndReturnSuccessJson()
    {
        // Arrange
        var encomendaDto = new EncomendaDto(
            Id: 42,
            TenantId: 1,
            CondoId: 1,
            UnidadeId: 204,
            BlocoUnidade: "Bloco B - Apto 204",
            CodigoRastreio: "TRK-998877",
            Descricao: "Encomenda Caixa - Amazon",
            Remetente: "Vendedor Oficial",
            Transportadora: "Amazon Logistics",
            LocalArmazenamento: "Portaria",
            Tipo: TipoEncomenda.Caixa,
            TipoDescricao: "Caixa",
            Status: StatusEncomenda.AguardandoRetirada,
            StatusDescricao: "AguardandoRetirada",
            DataRecebimento: DateTimeOffset.UtcNow,
            RecebidoPorNome: "Portaria IA",
            DataRetirada: null,
            RetiradoPorNome: null,
            NotificadoEm: DateTimeOffset.UtcNow,
            Observacoes: "[OCR IA]",
            FotoEtiquetaUrl: "https://example.com/etiqueta.jpg",
            ConfiancaOcr: 94.5,
            DadosOcrJson: "{}",
            CriadoEm: DateTimeOffset.UtcNow,
            AtualizadoEm: DateTimeOffset.UtcNow
        );

        _visionOcrServiceMock
            .Setup(s => s.ProcessLabelAndRegisterAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EncomendaDto>.Success(encomendaDto));

        // Act
        var jsonResult = await _plugin.ReadPackageLabelAndNotifyAsync("https://example.com/etiqueta.jpg", enviarNotificacao: true);

        // Assert
        jsonResult.Should().Contain("Encomenda registrada com sucesso");
        jsonResult.Should().Contain("42");
        jsonResult.Should().Contain("TRK-998877");
        jsonResult.Should().Contain("notificacaoMoradorEnviada\":true");
    }
}
