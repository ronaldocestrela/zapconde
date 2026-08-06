using FluentAssertions;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Xunit;

namespace Tests.Unit.Operations;

public class ReservaDomainTests
{
    private AreaComum CreateDefaultAreaComum()
    {
        return AreaComum.Create(
            tenantId: 1,
            condoId: 1,
            nome: "Salão de Festas Principal",
            descricao: "Salão de festas bloco A",
            tipo: TipoAreaComum.Eventos,
            capacidadeMaxima: 50,
            taxaReserva: 100.00m,
            taxaLimpeza: 40.00m,
            horarioInicioFuncionamento: new TimeSpan(8, 0, 0),
            horarioFimFuncionamento: new TimeSpan(22, 0, 0),
            tempoAntecedenciaMinimaDias: 1,
            tempoAntecedenciaMaximaDias: 60,
            requerAprovacaoSindico: false);
    }

    [Fact]
    public void Should_Create_Reserva_Successfully_When_Valid()
    {
        // Arrange
        var area = CreateDefaultAreaComum();
        var dataRef = new DateTime(2026, 8, 6, 12, 0, 0, DateTimeKind.Utc);
        var dataInicio = dataRef.Date.AddDays(2).AddHours(14); // 2026-08-08 14:00
        var dataFim = dataRef.Date.AddDays(2).AddHours(18);    // 2026-08-08 18:00

        // Act
        var reserva = Reserva.Create(
            tenantId: 1,
            condoId: 1,
            areaComum: area,
            moradorId: 10,
            nomeMorador: "Carlos Silva",
            unidadeMorador: "Apt 101",
            dataInicio: dataInicio,
            dataFim: dataFim,
            quantidadePessoas: 30,
            observacao: "Aniversário de família",
            dataReferenciaCalculo: dataRef);

        // Assert
        reserva.Should().NotBeNull();
        reserva.TenantId.Should().Be(1);
        reserva.CondoId.Should().Be(1);
        reserva.MoradorId.Should().Be(10);
        reserva.Status.Should().Be(StatusReserva.Confirmada);
        reserva.ValorTaxaReserva.Should().Be(100.00m);
        reserva.ValorTaxaLimpeza.Should().Be(40.00m);
        reserva.ValorTotal.Should().Be(140.00m);
    }

    [Fact]
    public void Should_Set_Status_PendenteAprovacao_When_Area_Requires_Sindico_Approval()
    {
        // Arrange
        var area = AreaComum.Create(
            tenantId: 1, condoId: 1, nome: "Churrasqueira VIP", descricao: "Área VIP",
            tipo: TipoAreaComum.Churrasqueira, capacidadeMaxima: 20, taxaReserva: 50m, taxaLimpeza: 30m,
            horarioInicioFuncionamento: new TimeSpan(8, 0, 0), horarioFimFuncionamento: new TimeSpan(22, 0, 0),
            tempoAntecedenciaMinimaDias: 1, tempoAntecedenciaMaximaDias: 30, requerAprovacaoSindico: true);

        var dataRef = new DateTime(2026, 8, 6, 12, 0, 0, DateTimeKind.Utc);
        var dataInicio = dataRef.Date.AddDays(3).AddHours(12);
        var dataFim = dataRef.Date.AddDays(3).AddHours(16);

        // Act
        var reserva = Reserva.Create(
            tenantId: 1, condoId: 1, areaComum: area, moradorId: 12,
            nomeMorador: "Ana Maria", unidadeMorador: "Apt 202",
            dataInicio: dataInicio, dataFim: dataFim, quantidadePessoas: 15,
            dataReferenciaCalculo: dataRef);

        // Assert
        reserva.Status.Should().Be(StatusReserva.PendenteAprovacao);
    }

    [Fact]
    public void Should_ThrowException_When_Capacity_Exceeds_Max()
    {
        // Arrange
        var area = CreateDefaultAreaComum(); // Cap 50
        var dataRef = new DateTime(2026, 8, 6, 12, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Reserva.Create(
            tenantId: 1, condoId: 1, areaComum: area, moradorId: 10,
            nomeMorador: "Carlos Silva", unidadeMorador: "Apt 101",
            dataInicio: dataRef.AddDays(2).AddHours(14), dataFim: dataRef.AddDays(2).AddHours(18),
            quantidadePessoas: 60, dataReferenciaCalculo: dataRef));
    }

    [Fact]
    public void Should_ThrowException_When_Outside_Operating_Hours()
    {
        // Arrange (Funcionamento: 08:00 às 22:00)
        var area = CreateDefaultAreaComum();
        var dataRef = new DateTime(2026, 8, 6, 12, 0, 0, DateTimeKind.Utc);

        // Act & Assert (Fim às 23:00 - fora do expediente)
        Assert.Throws<ArgumentException>(() => Reserva.Create(
            tenantId: 1, condoId: 1, areaComum: area, moradorId: 10,
            nomeMorador: "Carlos Silva", unidadeMorador: "Apt 101",
            dataInicio: dataRef.Date.AddDays(2).AddHours(20), dataFim: dataRef.Date.AddDays(2).AddHours(23),
            quantidadePessoas: 20, dataReferenciaCalculo: dataRef));
    }

    [Fact]
    public void Should_Detect_Overlapping_Reservations()
    {
        // Arrange
        var area = CreateDefaultAreaComum();
        var dataRef = new DateTime(2026, 8, 6, 12, 0, 0, DateTimeKind.Utc);
        var dataInicio = dataRef.Date.AddDays(2).AddHours(14); // 14:00
        var dataFim = dataRef.Date.AddDays(2).AddHours(18);    // 18:00

        var reserva = Reserva.Create(
            tenantId: 1, condoId: 1, areaComum: area, moradorId: 10,
            nomeMorador: "Carlos Silva", unidadeMorador: "Apt 101",
            dataInicio: dataInicio, dataFim: dataFim, quantidadePessoas: 20,
            dataReferenciaCalculo: dataRef);

        // Act & Assert
        // Sobreposição parcial início (12:00 às 15:00)
        reserva.Overlaps(dataRef.Date.AddDays(2).AddHours(12), dataRef.Date.AddDays(2).AddHours(15)).Should().BeTrue();

        // Sobreposição total interna (15:00 às 17:00)
        reserva.Overlaps(dataRef.Date.AddDays(2).AddHours(15), dataRef.Date.AddDays(2).AddHours(17)).Should().BeTrue();

        // Sem sobreposição antes (10:00 às 14:00)
        reserva.Overlaps(dataRef.Date.AddDays(2).AddHours(10), dataRef.Date.AddDays(2).AddHours(14)).Should().BeFalse();

        // Sem sobreposição depois (18:00 às 21:00)
        reserva.Overlaps(dataRef.Date.AddDays(2).AddHours(18), dataRef.Date.AddDays(2).AddHours(21)).Should().BeFalse();
    }
}
