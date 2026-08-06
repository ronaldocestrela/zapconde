using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Xunit;

namespace Tests.Unit.Financial;

public class ReguaInadimplenciaDomainTests
{
    private readonly ReguaInadimplenciaEngine _engine = new();

    [Fact]
    public void AvaliarFaturasElegiveis_DeveIdentificarEtapaPorDiasAtraso()
    {
        // Arrange
        var hoje = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc);
        var fatura = Fatura.Create(1, 1, 101, 5, "2026-07", hoje.AddDays(-12)); // 12 dias de atraso
        fatura.Id = 10;
        fatura.Status = StatusFatura.Vencido;

        var etapa1 = EtapaReguaInadimplencia.Create(1, 1, 1, 3, 9, "Lembrete", CanalCobranca.WhatsApp, TipoAcaoCobranca.LembreteAmigavel, "Msg 1");
        etapa1.Id = 1;

        var etapa2 = EtapaReguaInadimplencia.Create(1, 1, 2, 10, 29, "Notificação", CanalCobranca.WhatsApp, TipoAcaoCobranca.NotificacaoCobranca, "Msg 2");
        etapa2.Id = 2;

        var etapas = new[] { etapa1, etapa2 };
        var historicos = Array.Empty<HistoricoCobranca>();

        // Act
        var elegiveis = _engine.AvaliarFaturasElegiveis(new[] { fatura }, etapas, historicos, hoje).ToList();

        // Assert
        elegiveis.Should().HaveCount(1);
        elegiveis.First().Etapa.Id.Should().Be(2);
        elegiveis.First().Etapa.NomeEtapa.Should().Be("Notificação");
    }

    [Fact]
    public void AvaliarFaturasElegiveis_NaoDeveRepetirEtapaJaExecutadaComSucesso()
    {
        // Arrange
        var hoje = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc);
        var fatura = Fatura.Create(1, 1, 101, 5, "2026-07", hoje.AddDays(-12));
        fatura.Id = 10;
        fatura.Status = StatusFatura.Vencido;

        var etapa2 = EtapaReguaInadimplencia.Create(1, 1, 2, 10, 29, "Notificação", CanalCobranca.WhatsApp, TipoAcaoCobranca.NotificacaoCobranca, "Msg 2");
        etapa2.Id = 2;

        var historicoJaEnviado = HistoricoCobranca.Create(1, 1, 101, 5, fatura.Id, etapa2.Id, CanalCobranca.WhatsApp, TipoAcaoCobranca.NotificacaoCobranca, "Enviado", sucesso: true);

        // Act
        var elegiveis = _engine.AvaliarFaturasElegiveis(new[] { fatura }, new[] { etapa2 }, new[] { historicoJaEnviado }, hoje);

        // Assert
        elegiveis.Should().BeEmpty();
    }
}
