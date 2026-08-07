using System.Text.Json;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Moq;
using Modules.AIEngine.Application.Plugins;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;
using Xunit;

namespace Tests.Unit.AIEngine;

public class ReservaPluginTests
{
    private readonly Mock<IReservaApplicationService> _reservaServiceMock;
    private readonly Mock<IAreaComumApplicationService> _areaComumServiceMock;
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly ReservaPlugin _plugin;

    public ReservaPluginTests()
    {
        _reservaServiceMock = new Mock<IReservaApplicationService>();
        _areaComumServiceMock = new Mock<IAreaComumApplicationService>();
        _tenantServiceMock = new Mock<ICurrentTenantService>();

        _tenantServiceMock.Setup(t => t.TenantId).Returns(1);
        _tenantServiceMock.Setup(t => t.CondoId).Returns(1);

        _plugin = new ReservaPlugin(
            _reservaServiceMock.Object,
            _areaComumServiceMock.Object,
            _tenantServiceMock.Object);
    }

    [Fact]
    public async Task Should_ReturnSuccessJson_When_ReservationIsCreatedSuccessfully()
    {
        // Arrange
        var areaId = 1;
        var moradorId = 10;
        var dataInicio = "2026-09-15 18:00";
        var dataFim = "2026-09-15 22:00";

        var reservaDto = new ReservaDto(
            Id: 50,
            TenantId: 1,
            CondoId: 1,
            AreaComumId: areaId,
            NomeAreaComum: "Salão de Festas",
            MoradorId: moradorId,
            NomeMorador: "Morador #10",
            UnidadeMorador: "Unidade 101",
            DataInicio: DateTime.Parse("2026-09-15T18:00:00Z"),
            DataFim: DateTime.Parse("2026-09-15T22:00:00Z"),
            QuantidadePessoas: 25,
            ValorTaxaReserva: 150.00m,
            ValorTaxaLimpeza: 50.00m,
            ValorTotal: 200.00m,
            Status: StatusReserva.PendenteAprovacao,
            Observacao: "Festa de Aniversário",
            MotivoCancelamento: "",
            DataCriacao: DateTime.UtcNow
        );

        _reservaServiceMock
            .Setup(s => s.CriarReservaAsync(It.IsAny<CreateReservaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReservaDto>.Success(reservaDto, "Reserva criada com sucesso"));

        // Act
        var jsonResult = await _plugin.ReserveCommonAreaAsync(
            areaId, dataInicio, dataFim, moradorId, quantidadePessoas: 25, observacao: "Festa de Aniversário");

        // Assert
        jsonResult.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        root.GetProperty("reservaId").GetInt32().Should().Be(50);
        root.GetProperty("nomeAreaComum").GetString().Should().Be("Salão de Festas");
        root.GetProperty("status").GetString().Should().Be("PendenteAprovacao");
        root.GetProperty("valorTotal").GetDecimal().Should().Be(200.00m);
    }

    [Fact]
    public async Task Should_ReturnFailureJson_When_ReservationCollidesOrServiceFails()
    {
        // Arrange
        var areaId = 2;
        var moradorId = 15;

        _reservaServiceMock
            .Setup(s => s.CriarReservaAsync(It.IsAny<CreateReservaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReservaDto>.Failure("Já existe uma reserva confirmada para este horário."));

        // Act
        var jsonResult = await _plugin.ReserveCommonAreaAsync(
            areaId, "2026-09-20 12:00", "2026-09-20 16:00", moradorId);

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        root.GetProperty("mensagem").GetString().Should().Contain("Já existe uma reserva confirmada");
    }

    [Fact]
    public async Task Should_ReturnErrorJson_When_AreaIdOrMoradorIdIsInvalid()
    {
        // Act
        var jsonResult = await _plugin.ReserveCommonAreaAsync(
            0, "2026-09-20 12:00", "2026-09-20 16:00", 0);

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        root.GetProperty("mensagem").GetString().Should().Contain("inválido");
    }

    [Fact]
    public async Task Should_ReturnAvailableAreas_When_GetAvailableCommonAreasIsInvoked()
    {
        // Arrange
        var areas = new List<AreaComumDto>
        {
            new(
                Id: 1,
                TenantId: 1,
                CondoId: 1,
                Nome: "Salão de Festas",
                Descricao: "Salão principal",
                Tipo: TipoAreaComum.Eventos,
                TipoDescricao: "Salão de Festas",
                Status: StatusAreaComum.Ativa,
                StatusDescricao: "Ativa",
                CapacidadeMaxima: 100,
                TaxaReserva: 200.00m,
                TaxaLimpeza: 80.00m,
                CustoTotalReserva: 280.00m,
                HorarioInicioFuncionamento: "08:00",
                HorarioFimFuncionamento: "23:00",
                TempoAntecedenciaMinimaDias: 1,
                TempoAntecedenciaMaximaDias: 60,
                RequerAprovacaoSindico: true,
                RegrasUso: "Sem barulho após 22h",
                DataCriacao: DateTime.UtcNow,
                DataAtualizacao: null
            )
        };

        _areaComumServiceMock
            .Setup(s => s.GetAllAsync(1, StatusAreaComum.Ativa, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<AreaComumDto>>.Success(areas));

        // Act
        var jsonResult = await _plugin.GetAvailableCommonAreasAsync(1);

        // Assert
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        root.GetProperty("totalAreas").GetInt32().Should().Be(1);

        var list = root.GetProperty("areas");
        list.GetArrayLength().Should().Be(1);
        list[0].GetProperty("nome").GetString().Should().Be("Salão de Festas");
    }
}
