using BuildingBlocks.Shared;
using FluentAssertions;
using Moq;
using Modules.AIEngine.Application.Plugins;
using Modules.AIEngine.Application.Services;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;
using Xunit;

namespace Tests.Unit.AIEngine;

public class OcorrenciaTriagemPluginTests
{
    private readonly Mock<IOcorrenciaTriagemService> _triagemServiceMock;
    private readonly OcorrenciaTriagemPlugin _plugin;

    public OcorrenciaTriagemPluginTests()
    {
        _triagemServiceMock = new Mock<IOcorrenciaTriagemService>();
        _plugin = new OcorrenciaTriagemPlugin(_triagemServiceMock.Object);
    }

    [Fact]
    public async Task TriarEAbrirOcorrencia_ComSucesso_DeveRetornarJsonComDadosDaOcorrencia()
    {
        // Arrange
        var triagemResult = new ResultadoTriagemOcorrenciaDto(
            TituloSugerido: "Infiltração com vazamento constante na garagem",
            DescricaoDetalhada: "Infiltração identificada por foto com pingos d'água no subsolo.",
            CategoriaInferida: CategoriaOcorrencia.Manutencao,
            PrioridadeInferida: PrioridadeOcorrencia.Alta,
            LocalizacaoSugerida: "Subsolo 2 - Vaga 42",
            SetorResponsavelSugerido: "Zeladoria / Manutenção Predial",
            NivelConfianca: 0.92,
            JustificativaIa: "Risco de dano veicular e degradação da estrutura.",
            OrigemTriagem: "IA_Foto",
            OcorrenciaCriadaId: Guid.NewGuid()
        );

        _triagemServiceMock
            .Setup(s => s.TriarEAbrirOcorrenciaAsync(It.IsAny<TriagemOcorrenciaRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ResultadoTriagemOcorrenciaDto>.Success(triagemResult));

        // Act
        var jsonResult = await _plugin.TriarEAbrirOcorrenciaAsync(
            fotoUrl: "https://example.com/foto.jpg",
            relatoTexto: "Infiltração na garagem",
            moradorId: "morador-1",
            moradorNome: "João Silva",
            condoId: 1
        );

        // Assert
        jsonResult.Should().Contain("sucesso\":true");
        jsonResult.Should().Contain("Infiltração com vazamento constante na garagem");
        jsonResult.Should().Contain("Manutencao");
        jsonResult.Should().Contain("Alta");
        jsonResult.Should().Contain("Subsolo 2 - Vaga 42");
    }

    [Fact]
    public async Task AnalisarOcorrenciaMultimodal_ComSucesso_DeveRetornarAnalisePreviaSemIdOcorrencia()
    {
        // Arrange
        var analiseResult = new ResultadoTriagemOcorrenciaDto(
            TituloSugerido: "Som alto e perturbação no Bloco A Ap 504",
            DescricaoDetalhada: "Som alto de madrugada",
            CategoriaInferida: CategoriaOcorrencia.Barulho,
            PrioridadeInferida: PrioridadeOcorrencia.Media,
            LocalizacaoSugerida: "Bloco A - Ap 504",
            SetorResponsavelSugerido: "Administração / Portaria",
            NivelConfianca: 0.88,
            JustificativaIa: "Perturbação do sossego",
            OrigemTriagem: "IA_Audio"
        );

        _triagemServiceMock
            .Setup(s => s.AnalisarOcorrenciaAsync(It.IsAny<TriagemOcorrenciaRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ResultadoTriagemOcorrenciaDto>.Success(analiseResult));

        // Act
        var jsonResult = await _plugin.AnalisarOcorrenciaMultimodalAsync(
            audioUrl: "https://example.com/audio.mp3",
            relatoTexto: "Música alta",
            condoId: 1
        );

        // Assert
        jsonResult.Should().Contain("sucesso\":true");
        jsonResult.Should().Contain("Som alto e perturbação no Bloco A Ap 504");
        jsonResult.Should().Contain("Barulho");
        jsonResult.Should().Contain("IA_Audio");
    }
}
