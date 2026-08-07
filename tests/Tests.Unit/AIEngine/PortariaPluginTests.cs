using System.Text.Json;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Moq;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Domain.Enums;
using Modules.AIEngine.Application.Plugins;
using Xunit;

namespace Tests.Unit.AIEngine;

public class PortariaPluginTests
{
    private readonly Mock<IVisitanteApplicationService> _visitanteServiceMock;
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly PortariaPlugin _plugin;

    public PortariaPluginTests()
    {
        _visitanteServiceMock = new Mock<IVisitanteApplicationService>();
        _tenantServiceMock = new Mock<ICurrentTenantService>();

        _tenantServiceMock.Setup(t => t.TenantId).Returns(1);
        _tenantServiceMock.Setup(t => t.CondoId).Returns(1);

        _plugin = new PortariaPlugin(_visitanteServiceMock.Object, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task Should_ReturnSuccessJson_When_GuestAuthorizationIsCreated()
    {
        // Arrange
        var visitanteDto = new VisitanteDto(
            Id: 100,
            TenantId: 1,
            CondoId: 1,
            NomeCompleto: "Carlos Eduardo",
            Documento: "123.456.789-00",
            Telefone: "+5575988887777",
            Tipo: TipoVisitante.VisitanteSocial,
            Status: StatusVisitante.Agendado,
            Empresa: null,
            PlacaVeiculo: "ABC-1234",
            UnidadeId: 102,
            BlocoUnidade: "Bloco A - Apto 102",
            MoradorId: 10,
            DataHoraInicioAutorizacao: DateTimeOffset.UtcNow,
            DataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(4),
            DataHoraEntrada: null,
            DataHoraSaida: null,
            Observacoes: "Jantar com morador",
            OperadorEntradaId: null,
            OperadorSaidaId: null,
            CriadoEm: DateTimeOffset.UtcNow
        );

        _visitanteServiceMock
            .Setup(s => s.AuthorizeVisitanteAsync(It.IsAny<CreateVisitanteRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisitanteDto>.Success(visitanteDto, "Visitante cadastrado com sucesso"));

        // Act
        var jsonResult = await _plugin.AuthorizeGuestAsync(
            nome: "Carlos Eduardo",
            documento: "123.456.789-00",
            dataInicio: "2026-09-20 14:00",
            dataFim: "2026-09-20 18:00",
            tipo: "Visitante",
            unidadeId: 102,
            blocoUnidade: "Bloco A - Apto 102",
            moradorId: 10,
            telefone: "+5575988887777",
            placaVeiculo: "ABC-1234",
            observacoes: "Jantar com morador");

        // Assert
        jsonResult.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        root.GetProperty("autorizacaoId").GetInt32().Should().Be(100);
        root.GetProperty("nomeCompleto").GetString().Should().Be("Carlos Eduardo");
        root.GetProperty("documento").GetString().Should().Be("123.456.789-00");
        root.GetProperty("status").GetString().Should().Be("Agendado");
    }

    [Fact]
    public async Task Should_ReturnFailureJson_When_PrestadorServicoMissingEmpresa()
    {
        // Act
        var jsonResult = await _plugin.AuthorizeGuestAsync(
            nome: "Roberto Alencar",
            documento: "987.654.321-11",
            tipo: "PrestadorServico",
            empresa: null);

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        root.GetProperty("mensagem").GetString().Should().Contain("empresa");
    }

    [Fact]
    public async Task Should_ReturnFailureJson_When_NomeOrDocumentoIsEmpty()
    {
        // Act
        var jsonResult = await _plugin.AuthorizeGuestAsync(
            nome: "",
            documento: "");

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        root.GetProperty("mensagem").GetString().Should().Contain("obrigatório");
    }
}
