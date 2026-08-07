using FluentAssertions;
using Modules.AccessControl.Domain.Entities;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Domain.Exceptions;

namespace Tests.Unit.AccessControl;

public class VisitanteDomainTests
{
    private const int TenantIdDefault = 1;
    private const int CondoIdDefault = 10;
    private const int UnidadeIdDefault = 101;

    [Fact]
    public void CriarAutorizacao_DeveCriarVisitanteComStatusAgendado()
    {
        // Act
        var visitante = Visitante.CriarAutorizacao(
            tenantId: TenantIdDefault,
            condoId: CondoIdDefault,
            nomeCompleto: "Carlos Silva",
            documento: "123.456.789-00",
            telefone: "+5575999998888",
            tipo: TipoVisitante.VisitanteSocial,
            unidadeId: UnidadeIdDefault,
            blocoUnidade: "Bloco A - Apt 101",
            moradorId: 5,
            dataHoraInicioAutorizacao: DateTimeOffset.UtcNow.AddMinutes(-5),
            dataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(5),
            empresa: null,
            placaVeiculo: "ABC-1234",
            observacoes: "Visitante da família"
        );

        // Assert
        visitante.Should().NotBeNull();
        visitante.TenantId.Should().Be(TenantIdDefault);
        visitante.CondoId.Should().Be(CondoIdDefault);
        visitante.NomeCompleto.Should().Be("Carlos Silva");
        visitante.Status.Should().Be(StatusVisitante.Agendado);
        visitante.DataHoraEntrada.Should().BeNull();
        visitante.DataHoraSaida.Should().BeNull();
    }

    [Fact]
    public void CriarAutorizacao_QuandoPrestadorSemEmpresa_DeveLancarVisitanteDomainException()
    {
        // Act
        Action act = () => Visitante.CriarAutorizacao(
            tenantId: TenantIdDefault,
            condoId: CondoIdDefault,
            nomeCompleto: "João Eletricista",
            documento: "987.654.321-11",
            telefone: "+5575988887777",
            tipo: TipoVisitante.PrestadorServico,
            unidadeId: UnidadeIdDefault,
            blocoUnidade: "Bloco A - Apt 101",
            moradorId: 5,
            dataHoraInicioAutorizacao: DateTimeOffset.UtcNow.AddMinutes(-5),
            dataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(5),
            empresa: "   ", // Inválido para prestador
            placaVeiculo: null,
            observacoes: null
        );

        // Assert
        act.Should().Throw<VisitanteDomainException>()
            .WithMessage("*Empresa*obrigatória*Prestador*");
    }

    [Fact]
    public void RegistrarEntrada_QuandoStatusAgendado_DeveMudarStatusParaPresenteEDefinirDataEntrada()
    {
        // Arrange
        var visitante = Visitante.CriarAutorizacao(
            tenantId: TenantIdDefault,
            condoId: CondoIdDefault,
            nomeCompleto: "Maria Souza",
            documento: "111.222.333-44",
            telefone: null,
            tipo: TipoVisitante.VisitanteSocial,
            unidadeId: UnidadeIdDefault,
            blocoUnidade: "Bloco B - Apt 202",
            moradorId: null,
            dataHoraInicioAutorizacao: DateTimeOffset.UtcNow.AddHours(-1),
            dataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(2),
            empresa: null,
            placaVeiculo: null,
            observacoes: null
        );

        // Act
        visitante.RegistrarEntrada(operadorId: 99);

        // Assert
        visitante.Status.Should().Be(StatusVisitante.Presente);
        visitante.DataHoraEntrada.Should().NotBeNull();
        visitante.OperadorEntradaId.Should().Be(99);
    }

    [Fact]
    public void RegistrarEntrada_QuandoJaPresente_DeveLancarVisitanteDomainException()
    {
        // Arrange
        var visitante = Visitante.CriarAutorizacao(
            tenantId: TenantIdDefault,
            condoId: CondoIdDefault,
            nomeCompleto: "Maria Souza",
            documento: "111.222.333-44",
            telefone: null,
            tipo: TipoVisitante.VisitanteSocial,
            unidadeId: UnidadeIdDefault,
            blocoUnidade: "Bloco B - Apt 202",
            moradorId: null,
            dataHoraInicioAutorizacao: DateTimeOffset.UtcNow.AddHours(-1),
            dataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(2),
            empresa: null,
            placaVeiculo: null,
            observacoes: null
        );
        visitante.RegistrarEntrada(operadorId: 99);

        // Act
        Action act = () => visitante.RegistrarEntrada(operadorId: 99);

        // Assert
        act.Should().Throw<VisitanteDomainException>()
            .WithMessage("*já se encontra no condomínio*");
    }

    [Fact]
    public void RegistrarSaida_QuandoPresente_DeveMudarStatusParaFinalizadoEDefinirDataSaida()
    {
        // Arrange
        var visitante = Visitante.CriarAutorizacao(
            tenantId: TenantIdDefault,
            condoId: CondoIdDefault,
            nomeCompleto: "Maria Souza",
            documento: "111.222.333-44",
            telefone: null,
            tipo: TipoVisitante.VisitanteSocial,
            unidadeId: UnidadeIdDefault,
            blocoUnidade: "Bloco B - Apt 202",
            moradorId: null,
            dataHoraInicioAutorizacao: DateTimeOffset.UtcNow.AddHours(-1),
            dataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(2),
            empresa: null,
            placaVeiculo: null,
            observacoes: null
        );
        visitante.RegistrarEntrada(operadorId: 99);

        // Act
        visitante.RegistrarSaida(operadorId: 100);

        // Assert
        visitante.Status.Should().Be(StatusVisitante.Finalizado);
        visitante.DataHoraSaida.Should().NotBeNull();
        visitante.OperadorSaidaId.Should().Be(100);
    }

    [Fact]
    public void CancelarAutorizacao_QuandoAgendado_DeveMudarStatusParaCancelado()
    {
        // Arrange
        var visitante = Visitante.CriarAutorizacao(
            tenantId: TenantIdDefault,
            condoId: CondoIdDefault,
            nomeCompleto: "Lucas Oliveira",
            documento: "555.666.777-88",
            telefone: null,
            tipo: TipoVisitante.VisitanteSocial,
            unidadeId: UnidadeIdDefault,
            blocoUnidade: "Bloco A - Apt 101",
            moradorId: 5,
            dataHoraInicioAutorizacao: DateTimeOffset.UtcNow,
            dataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(2),
            empresa: null,
            placaVeiculo: null,
            observacoes: null
        );

        // Act
        visitante.CancelarAutorizacao("Cancelado pelo morador");

        // Assert
        visitante.Status.Should().Be(StatusVisitante.Cancelado);
        visitante.Observacoes.Should().Contain("Cancelado pelo morador");
    }
}
