using FluentAssertions;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;
using Xunit;

namespace Tests.Unit.Operations;

public class AssembleiaDomainTests
{
    [Fact]
    public void Create_ShouldInitializeAssembleia_WhenValidParametersProvided()
    {
        // Arrange
        var tenantId = 1;
        var condoId = 10;
        var titulo = "Assembleia Geral Ordinária 2026";
        var tipo = TipoAssembleia.Ordinaria;
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(48);
        var criadorId = "user-sindico-1";

        // Act
        var assembleia = AssembleiaVirtual.Create(tenantId, condoId, titulo, tipo, inicio, fim, criadorId, "Prestação de contas do ano anterior.");

        // Assert
        assembleia.Should().NotBeNull();
        assembleia.Id.Should().NotBeEmpty();
        assembleia.TenantId.Should().Be(tenantId);
        assembleia.CondoId.Should().Be(condoId);
        assembleia.Titulo.Should().Be(titulo);
        assembleia.Tipo.Should().Be(TipoAssembleia.Ordinaria);
        assembleia.Status.Should().Be(StatusAssembleia.Agendada);
        assembleia.Pautas.Should().BeEmpty();
        assembleia.AtaTexto.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDataFimIsBeforeDataInicio()
    {
        // Arrange
        var inicio = DateTime.UtcNow.AddDays(5);
        var fim = inicio.AddHours(-2);

        // Act
        Action act = () => AssembleiaVirtual.Create(1, 10, "Assembleia Inválida", TipoAssembleia.Extraordinaria, inicio, fim, "user-1");

        // Assert
        act.Should().Throw<AssembleiaDomainException>()
            .WithMessage("*posterior à data inicial*");
    }

    [Fact]
    public void AdicionarPauta_ShouldAddPautaToAssembleia()
    {
        // Arrange
        var assembleia = AssembleiaVirtual.Create(1, 10, "Assembleia 2026", TipoAssembleia.Ordinaria, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), "user-1");

        // Act
        var pauta1 = assembleia.AdicionarPauta("Aprovação de Contas 2025", TipoVotacao.MaioriaSimples, "Votação do balanço anual.");
        var pauta2 = assembleia.AdicionarPauta("Eleição do Síndico", TipoVotacao.MaioriaSimples);

        // Assert
        assembleia.Pautas.Should().HaveCount(2);
        pauta1.Ordem.Should().Be(1);
        pauta2.Ordem.Should().Be(2);
        pauta1.Status.Should().Be(StatusPauta.Aberta);
    }

    [Fact]
    public void RegistrarVoto_ShouldComputeVoteSuccessfully_WhenAssemblyIsEmAndamento()
    {
        // Arrange
        var assembleia = AssembleiaVirtual.Create(1, 10, "Assembleia 2026", TipoAssembleia.Ordinaria, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), "user-1");
        var pauta = assembleia.AdicionarPauta("Aprovação de Contas 2025", TipoVotacao.MaioriaSimples);
        assembleia.IniciarAssembleia();

        // Act
        var voto = assembleia.RegistrarVoto(pauta.Id, "user-morador-101", "101", "Sim");

        // Assert
        voto.Should().NotBeNull();
        voto.UnidadeId.Should().Be("101");
        voto.OpcaoEscolhida.Should().Be("Sim");
        pauta.Votos.Should().HaveCount(1);
    }

    [Fact]
    public void RegistrarVoto_ShouldThrowVotoDuplicadoException_WhenSameUnitVotesTwiceOnSamePauta()
    {
        // Arrange
        var assembleia = AssembleiaVirtual.Create(1, 10, "Assembleia 2026", TipoAssembleia.Ordinaria, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), "user-1");
        var pauta = assembleia.AdicionarPauta("Aprovação de Contas 2025", TipoVotacao.MaioriaSimples);
        assembleia.IniciarAssembleia();

        assembleia.RegistrarVoto(pauta.Id, "user-morador-101", "101", "Sim");

        // Act
        Action act = () => assembleia.RegistrarVoto(pauta.Id, "outro-morador-da-mesma-unidade", "101", "Não");

        // Assert
        act.Should().Throw<VotoDuplicadoException>()
            .WithMessage("*'101' já registrou voto*");
        pauta.Votos.Should().HaveCount(1);
    }

    [Fact]
    public void RegistrarVoto_ShouldThrowAssembleiaEncerradaException_WhenAssemblyIsNotEmAndamento()
    {
        // Arrange
        var assembleia = AssembleiaVirtual.Create(1, 10, "Assembleia 2026", TipoAssembleia.Ordinaria, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), "user-1");
        var pauta = assembleia.AdicionarPauta("Aprovação de Contas 2025", TipoVotacao.MaioriaSimples);
        // Status remains Agendada

        // Act
        Action act = () => assembleia.RegistrarVoto(pauta.Id, "user-morador-101", "101", "Sim");

        // Assert
        act.Should().Throw<AssembleiaEncerradaException>()
            .WithMessage("*não está aberta para votações*");
    }

    [Fact]
    public void EncerrarEGerarAta_ShouldFinalizeAssemblyAndGenerateOfficialAtaText()
    {
        // Arrange
        var assembleia = AssembleiaVirtual.Create(1, 10, "Assembleia 2026", TipoAssembleia.Ordinaria, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), "user-1");
        var pauta = assembleia.AdicionarPauta("Aprovação de Contas 2025", TipoVotacao.MaioriaSimples);
        assembleia.IniciarAssembleia();

        assembleia.RegistrarVoto(pauta.Id, "user-101", "101", "Sim");
        assembleia.RegistrarVoto(pauta.Id, "user-102", "102", "Sim");
        assembleia.RegistrarVoto(pauta.Id, "user-103", "103", "Não");

        // Act
        assembleia.EncerrarEGerarAta();

        // Assert
        assembleia.Status.Should().Be(StatusAssembleia.Encerrada);
        assembleia.DataEncerramento.Should().NotBeNull();
        assembleia.AtaTexto.Should().NotBeNullOrWhiteSpace();
        assembleia.AtaTexto.Should().Contain("ATA OFICIAL DA ASSEMBLEIA 2026");
        assembleia.AtaTexto.Should().Contain("Quórum Total de Unidades Participantes: 3 unidade(s)");
        assembleia.AtaTexto.Should().Contain("Opção 'Sim': 2 voto(s)");
        assembleia.AtaTexto.Should().Contain("Opção 'Não': 1 voto(s)");
        pauta.Status.Should().Be(StatusPauta.Encerrada);
    }
}
