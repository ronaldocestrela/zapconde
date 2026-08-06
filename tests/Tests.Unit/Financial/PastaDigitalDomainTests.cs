using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Xunit;

namespace Tests.Unit.Financial;

public class PastaDigitalDomainTests
{
    private readonly PastaDigitalDomainService _domainService = new();

    [Fact]
    public void Should_CreatePastaDigital_With_StatusRascunho_And_ZeroSaldos()
    {
        // Act
        var pasta = PastaDigital.Create(
            tenantId: 1,
            condoId: 10,
            ano: 2026,
            mes: 7,
            saldoAnterior: 1000m);

        // Assert
        pasta.TenantId.Should().Be(1);
        pasta.CondoId.Should().Be(10);
        pasta.Ano.Should().Be(2026);
        pasta.Mes.Should().Be(7);
        pasta.Status.Should().Be(StatusPastaDigital.Rascunho);
        pasta.SaldoAnterior.Should().Be(1000m);
        pasta.SaldoAcumulado.Should().Be(1000m);
    }

    [Fact]
    public void Should_CalculateCorrectSaldos_When_AddingItensBalancete()
    {
        // Arrange
        var pasta = PastaDigital.Create(1, 10, 2026, 7, saldoAnterior: 5000m);

        // Act
        pasta.AdicionarItemBalancete(
            TipoLancamentoBalancete.Receita,
            CategoriaPlanoContas.ReceitaOrdinaria,
            "Taxas de Condomínio Recebidas",
            valorOrcado: 20000m,
            valorRealizado: 22000m,
            dataLancamento: DateTime.Today);

        pasta.AdicionarItemBalancete(
            TipoLancamentoBalancete.Despesa,
            CategoriaPlanoContas.DespesaManutencao,
            "Manutenção Elevadores",
            valorOrcado: 3000m,
            valorRealizado: 3500m,
            dataLancamento: DateTime.Today);

        // Assert
        pasta.TotalReceitas.Should().Be(22000m);
        pasta.TotalDespesas.Should().Be(3500m);
        pasta.SaldoMes.Should().Be(18500m);
        pasta.SaldoAcumulado.Should().Be(23500m);
    }

    [Fact]
    public void Should_TransitionStatus_Correctly_In_ApprovalCycle()
    {
        // Arrange
        var pasta = PastaDigital.Create(1, 10, 2026, 7, 0);

        // Act & Assert Submeter
        pasta.SubmeterParaConselho();
        pasta.Status.Should().Be(StatusPastaDigital.EmAnaliseConselho);

        // Act & Assert Aprovar
        pasta.Aprovar(aprovadoPorUserId: 99, parecer: "Aprovado em assembleia");
        pasta.Status.Should().Be(StatusPastaDigital.Aprovada);
        pasta.AprovadoPorUserId.Should().Be(99);

        // Act & Assert Publicar
        pasta.Publicar();
        pasta.Status.Should().Be(StatusPastaDigital.Publicada);
    }

    [Fact]
    public void Should_ThrowException_When_ModifyingPublishedPasta()
    {
        // Arrange
        var pasta = PastaDigital.Create(1, 10, 2026, 7, 0);
        pasta.SubmeterParaConselho();
        pasta.Aprovar(1);
        pasta.Publicar();

        // Act & Assert
        Action act = () => pasta.AdicionarItemBalancete(
            TipoLancamentoBalancete.Receita,
            CategoriaPlanoContas.ReceitaOrdinaria,
            "Teste", 100, 100, DateTime.Today);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*publicada*");
    }
}
