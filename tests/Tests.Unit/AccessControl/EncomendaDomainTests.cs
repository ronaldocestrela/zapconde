using System;
using FluentAssertions;
using Modules.AccessControl.Domain.Entities;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Domain.Exceptions;
using Xunit;

namespace Tests.Unit.AccessControl;

public class EncomendaDomainTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveInstanciarEncomendaComStatusAguardandoRetirada()
    {
        // Arrange
        int tenantId = 1;
        int condoId = 10;
        int unidadeId = 202;
        string bloco = "Bloco B - Apt 202";
        string codigo = "LOG123456789";
        string desc = "Caixa de ferramentas";
        string remetente = "Amazon";
        string transportadora = "Loggi";
        string local = "Prateleira A2";
        var tipo = TipoEncomenda.Caixa;
        string porteiro = "José Porteiro";
        var dataRecebimento = DateTimeOffset.UtcNow.AddMinutes(-10);

        // Act
        var encomenda = Encomenda.Criar(
            tenantId,
            condoId,
            unidadeId,
            bloco,
            codigo,
            desc,
            remetente,
            transportadora,
            local,
            tipo,
            porteiro,
            dataRecebimento);

        // Assert
        encomenda.Should().NotBeNull();
        encomenda.TenantId.Should().Be(tenantId);
        encomenda.CondoId.Should().Be(condoId);
        encomenda.UnidadeId.Should().Be(unidadeId);
        encomenda.BlocoUnidade.Should().Be(bloco);
        encomenda.CodigoRastreio.Should().Be(codigo);
        encomenda.Descricao.Should().Be(desc);
        encomenda.Remetente.Should().Be(remetente);
        encomenda.Transportadora.Should().Be(transportadora);
        encomenda.LocalArmazenamento.Should().Be(local);
        encomenda.Tipo.Should().Be(tipo);
        encomenda.Status.Should().Be(StatusEncomenda.AguardandoRetirada);
        encomenda.RecebidoPorNome.Should().Be(porteiro);
        encomenda.DataRecebimento.Should().Be(dataRecebimento);
        encomenda.NotificadoEm.Should().BeNull();
        encomenda.DataRetirada.Should().BeNull();
        encomenda.RetiradoPorNome.Should().BeNull();
    }

    [Fact]
    public void Criar_ComTenantIdInvalido_DeveLancarEncomendaDomainException()
    {
        // Act
        Action act = () => Encomenda.Criar(
            tenantId: 0,
            condoId: 1,
            unidadeId: 101,
            blocoUnidade: "Apt 101",
            codigoRastreio: "123",
            descricao: "Pacote",
            remetente: null,
            transportadora: null,
            localArmazenamento: null,
            tipo: TipoEncomenda.Pacote,
            recebidoPorNome: "Porteiro",
            dataRecebimento: DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<EncomendaDomainException>()
           .WithMessage("*TenantId é obrigatório*");
    }

    [Fact]
    public void Criar_ComDataFutura_DeveLancarEncomendaDomainException()
    {
        // Act
        Action act = () => Encomenda.Criar(
            tenantId: 1,
            condoId: 1,
            unidadeId: 101,
            blocoUnidade: "Apt 101",
            codigoRastreio: "123",
            descricao: "Pacote",
            remetente: null,
            transportadora: null,
            localArmazenamento: null,
            tipo: TipoEncomenda.Pacote,
            recebidoPorNome: "Porteiro",
            dataRecebimento: DateTimeOffset.UtcNow.AddHours(2));

        // Assert
        act.Should().Throw<EncomendaDomainException>()
           .WithMessage("*futuro*");
    }

    [Fact]
    public void MarcarComoEntregue_ComDadosValidos_DeveTransicionarParaEntregue()
    {
        // Arrange
        var encomenda = Encomenda.Criar(
            tenantId: 1,
            condoId: 1,
            unidadeId: 101,
            blocoUnidade: "Apt 101",
            codigoRastreio: "XYZ999",
            descricao: "Envelope",
            remetente: "Banco",
            transportadora: "Correios",
            localArmazenamento: "Gaveta 1",
            tipo: TipoEncomenda.Envelope,
            recebidoPorNome: "Porteiro Silva",
            dataRecebimento: DateTimeOffset.UtcNow.AddHours(-2));

        var dataRetirada = DateTimeOffset.UtcNow;
        string morador = "Ana Maria";

        // Act
        encomenda.MarcarComoEntregue(morador, dataRetirada);

        // Assert
        encomenda.Status.Should().Be(StatusEncomenda.Entregue);
        encomenda.RetiradoPorNome.Should().Be(morador);
        encomenda.DataRetirada.Should().Be(dataRetirada);
    }

    [Fact]
    public void MarcarComoEntregue_QuandoJaEntregue_DeveLancarEncomendaDomainException()
    {
        // Arrange
        var encomenda = Encomenda.Criar(
            tenantId: 1,
            condoId: 1,
            unidadeId: 101,
            blocoUnidade: "Apt 101",
            codigoRastreio: "XYZ999",
            descricao: "Envelope",
            remetente: "Banco",
            transportadora: "Correios",
            localArmazenamento: "Gaveta 1",
            tipo: TipoEncomenda.Envelope,
            recebidoPorNome: "Porteiro Silva",
            dataRecebimento: DateTimeOffset.UtcNow.AddHours(-2));

        encomenda.MarcarComoEntregue("Ana Maria", DateTimeOffset.UtcNow.AddHours(-1));

        // Act
        Action act = () => encomenda.MarcarComoEntregue("Carlos", DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<EncomendaDomainException>()
           .WithMessage("*Apenas encomendas aguardando retirada podem ser entregues*");
    }

    [Fact]
    public void NotificarMorador_DevePreencherNotificadoEm()
    {
        // Arrange
        var encomenda = Encomenda.Criar(
            tenantId: 1,
            condoId: 1,
            unidadeId: 101,
            blocoUnidade: "Apt 101",
            codigoRastreio: "123",
            descricao: "Pacote",
            remetente: null,
            transportadora: null,
            localArmazenamento: null,
            tipo: TipoEncomenda.Pacote,
            recebidoPorNome: "Porteiro",
            dataRecebimento: DateTimeOffset.UtcNow);

        // Act
        encomenda.NotificarMorador();

        // Assert
        encomenda.NotificadoEm.Should().NotBeNull();
    }
}
