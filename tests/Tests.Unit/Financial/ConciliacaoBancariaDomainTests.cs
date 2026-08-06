using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Xunit;

namespace Tests.Unit.Financial;

public class ConciliacaoBancariaDomainTests
{
    private readonly ConciliacaoBancariaDomainService _domainService = new();

    [Fact]
    public void Should_AutoReconcile_CreditItem_With_MatchingFatura()
    {
        // Arrange
        var itemExtrato = ExtratoBancarioItem.Create(
            tenantId: 1,
            contaBancariaId: 5,
            dataTransacao: new DateTime(2026, 7, 10),
            descricaoHistorico: "DEP PIX TAXA CONDO 101",
            documentoRef: "PIX123",
            valor: 500.00m,
            tipoTransacao: TipoTransacaoBancaria.Credito);

        var fatura = Fatura.Create(
            tenantId: 1,
            condoId: 10,
            unidadeId: 101,
            moradorId: 200,
            competencia: "2026-07",
            dataVencimento: new DateTime(2026, 7, 10));

        fatura.AddItem("Taxa Condominial Julho", TipoItemCobranca.TaxaCondominial, 500.00m);
        fatura.RegistrarPagamento(new DateTime(2026, 7, 10), 500.00m);

        // Act
        var matches = _domainService.ProcessarConciliacaoAutomatica(
            new[] { itemExtrato },
            new[] { fatura },
            Array.Empty<ItemBalancete>()).ToList();

        // Assert
        matches.Should().HaveCount(1);
        matches[0].ScoreMatch.Should().Be(100m);
        itemExtrato.StatusConciliacao.Should().Be(StatusConciliacaoBancaria.ConciliadoAutomatico);
        itemExtrato.TransacaoConciliadaId.Should().Be(fatura.Id);
    }

    [Fact]
    public void Should_AutoReconcile_DebitItem_With_MatchingDespesa()
    {
        // Arrange
        var itemExtrato = ExtratoBancarioItem.Create(
            tenantId: 1,
            contaBancariaId: 5,
            dataTransacao: new DateTime(2026, 7, 15),
            descricaoHistorico: "PAGTO MANUTENCAO ELEVADOR",
            documentoRef: "NF999",
            valor: 1200.00m,
            tipoTransacao: TipoTransacaoBancaria.Debito);

        var despesa = ItemBalancete.Create(
            tenantId: 1,
            pastaDigitalId: 1,
            tipoLancamento: TipoLancamentoBalancete.Despesa,
            categoria: CategoriaPlanoContas.DespesaManutencao,
            descricao: "Manutenção Preventiva de Elevador",
            valorOrcado: 1200m,
            valorRealizado: 1200m,
            dataLancamento: new DateTime(2026, 7, 15));

        // Act
        var matches = _domainService.ProcessarConciliacaoAutomatica(
            new[] { itemExtrato },
            Array.Empty<Fatura>(),
            new[] { despesa }).ToList();

        // Assert
        matches.Should().HaveCount(1);
        itemExtrato.StatusConciliacao.Should().Be(StatusConciliacaoBancaria.ConciliadoAutomatico);
        despesa.Conciliado.Should().BeTrue();
    }
}
