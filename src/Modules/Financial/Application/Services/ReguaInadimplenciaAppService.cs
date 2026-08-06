using BuildingBlocks.Shared.MultiTenancy;
using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public class ReguaInadimplenciaAppService : IReguaInadimplenciaAppService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ReguaInadimplenciaEngine _engine;

    public ReguaInadimplenciaAppService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService,
        ReguaInadimplenciaEngine engine)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _engine = engine;
    }

    public async Task<Result<IEnumerable<EtapaReguaDto>>> ObterConfiguracaoReguaAsync(int condoId, CancellationToken ct = default)
    {
        var etapas = await _dbContext.EtapasReguaInadimplencia
            .Where(e => e.CondoId == condoId)
            .OrderBy(e => e.Ordem)
            .ToListAsync(ct);

        // Se não houver configuração para o condomínio, provisiona etapas default
        if (!etapas.Any())
        {
            etapas = ProvisionarEtapasDefault(condoId);
            _dbContext.EtapasReguaInadimplencia.AddRange(etapas);
            await _dbContext.SaveChangesAsync(ct);
        }

        var dtos = etapas.Select(MapearEtapaParaDto);
        return Result<IEnumerable<EtapaReguaDto>>.Success(dtos);
    }

    public async Task<Result> SalvarConfiguracaoReguaAsync(int condoId, IEnumerable<SalvarEtapaReguaDto> etapasDtos, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        var etapasExistentes = await _dbContext.EtapasReguaInadimplencia
            .Where(e => e.CondoId == condoId)
            .ToListAsync(ct);

        foreach (var dto in etapasDtos)
        {
            if (dto.Id.HasValue && dto.Id.Value > 0)
            {
                var etapa = etapasExistentes.FirstOrDefault(e => e.Id == dto.Id.Value);
                if (etapa != null)
                {
                    etapa.AtualizarConfiguracao(
                        dto.Ordem,
                        dto.DiasAtrasoMinimo,
                        dto.DiasAtrasoMaximo,
                        dto.NomeEtapa,
                        dto.Canal,
                        dto.TipoAcao,
                        dto.TemplateMensagem,
                        dto.Ativo
                    );
                }
            }
            else
            {
                var novaEtapa = EtapaReguaInadimplencia.Create(
                    tenantId,
                    condoId,
                    dto.Ordem,
                    dto.DiasAtrasoMinimo,
                    dto.DiasAtrasoMaximo,
                    dto.NomeEtapa,
                    dto.Canal,
                    dto.TipoAcao,
                    dto.TemplateMensagem
                );
                _dbContext.EtapasReguaInadimplencia.Add(novaEtapa);
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success("Configuração da régua de inadimplência salva com sucesso.");
    }

    public async Task<Result<ProcessamentoReguaResultadoDto>> ProcessarReguaCobrancaAsync(int condoId, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        var hoje = DateTime.UtcNow;

        var etapas = await _dbContext.EtapasReguaInadimplencia
            .Where(e => e.CondoId == condoId && e.Ativo)
            .ToListAsync(ct);

        if (!etapas.Any())
        {
            etapas = ProvisionarEtapasDefault(condoId);
            _dbContext.EtapasReguaInadimplencia.AddRange(etapas);
            await _dbContext.SaveChangesAsync(ct);
        }

        var faturasVencidas = await _dbContext.Faturas
            .Where(f => f.CondoId == condoId && (f.Status == StatusFatura.Vencido || (f.Status == StatusFatura.Pendente && f.DataVencimento < hoje)))
            .ToListAsync(ct);

        var historicosExistentes = await _dbContext.HistoricosCobranca
            .Where(h => h.CondoId == condoId)
            .ToListAsync(ct);

        var acoesElegiveis = _engine.AvaliarFaturasElegiveis(faturasVencidas, etapas, historicosExistentes, hoje);
        var novosHistoricos = new List<HistoricoCobranca>();

        foreach (var (fatura, etapa) in acoesElegiveis)
        {
            var mensagem = string.IsNullOrWhiteSpace(etapa.TemplateMensagem)
                ? $"Prezado morador da unidade {fatura.UnidadeId}, identificamos a fatura {fatura.NumeroFatura} em atraso desde {fatura.DataVencimento:dd/MM/yyyy}."
                : etapa.TemplateMensagem.Replace("{NumeroFatura}", fatura.NumeroFatura).Replace("{Vencimento}", fatura.DataVencimento.ToString("dd/MM/yyyy"));

            var historico = HistoricoCobranca.Create(
                tenantId,
                condoId,
                fatura.UnidadeId,
                fatura.MoradorId,
                fatura.Id,
                etapa.Id,
                etapa.Canal,
                etapa.TipoAcao,
                mensagem,
                sucesso: true,
                observacao: $"Execução da etapa {etapa.NomeEtapa} para fatura {fatura.NumeroFatura}"
            );

            novosHistoricos.Add(historico);
            _dbContext.HistoricosCobranca.Add(historico);
        }

        await _dbContext.SaveChangesAsync(ct);

        var dtos = novosHistoricos.Select(MapearHistoricoParaDto).ToList();
        var resultado = new ProcessamentoReguaResultadoDto(
            TotalAcoesProcessadas: dtos.Count,
            TotalSucessos: dtos.Count,
            TotalFalhas: 0,
            HistoricosGerados: dtos
        );

        return Result<ProcessamentoReguaResultadoDto>.Success(resultado, "Processamento da régua concluído com sucesso.");
    }

    public async Task<Result<DashboardInadimplenciaDto>> ObterDashboardInadimplenciaAsync(int condoId, CancellationToken ct = default)
    {
        var hoje = DateTime.UtcNow;

        var faturasVencidas = await _dbContext.Faturas
            .Where(f => f.CondoId == condoId && (f.Status == StatusFatura.Vencido || (f.Status == StatusFatura.Pendente && f.DataVencimento < hoje)))
            .ToListAsync(ct);

        var totalGeralFaturas = await _dbContext.Faturas
            .Where(f => f.CondoId == condoId)
            .SumAsync(f => (decimal?)(f.ValorOriginal + f.ValorMulta + f.ValorJuros - f.ValorDesconto), ct) ?? 0m;

        var totalInadimplente = faturasVencidas.Sum(f => f.TotalFinal);
        var unidadesInadimplentes = faturasVencidas.Select(f => f.UnidadeId).Distinct().Count();

        var taxaInadimplencia = totalGeralFaturas > 0 ? (totalInadimplente / totalGeralFaturas) * 100m : 0m;

        var acordosAtivos = await _dbContext.Acordos
            .Where(a => a.CondoId == condoId && (a.Status == StatusAcordo.Ativo || a.Status == StatusAcordo.Quitado))
            .ToListAsync(ct);

        var valorRecuperado = acordosAtivos.Sum(a => a.ValorTotalAcordo);
        var qtdAcordosAtivos = acordosAtivos.Count(a => a.Status == StatusAcordo.Ativo);

        // Aging List
        decimal d1a30 = 0m, d31a60 = 0m, d61a90 = 0m, dAcima90 = 0m;
        foreach (var fatura in faturasVencidas)
        {
            var diasAtraso = (int)(hoje.Date - fatura.DataVencimento.Date).TotalDays;
            if (diasAtraso <= 30) d1a30 += fatura.TotalFinal;
            else if (diasAtraso <= 60) d31a60 += fatura.TotalFinal;
            else if (diasAtraso <= 90) d61a90 += fatura.TotalFinal;
            else dAcima90 += fatura.TotalFinal;
        }

        var agingList = new AgingListDto(
            TotalVencido1A30Dias: d1a30,
            TotalVencido31A60Dias: d31a60,
            TotalVencido61A90Dias: d61a90,
            TotalVencidoAcima90Dias: dAcima90,
            TotalGeralVencido: totalInadimplente,
            QuantidadeUnidadesInadimplentes: unidadesInadimplentes
        );

        var ultimosHistoricos = await _dbContext.HistoricosCobranca
            .Where(h => h.CondoId == condoId)
            .OrderByDescending(h => h.DataExecucao)
            .Take(10)
            .ToListAsync(ct);

        var dashboard = new DashboardInadimplenciaDto(
            CondoId: condoId,
            TaxaInadimplenciaPercentual: Math.Round(taxaInadimplencia, 2),
            ValorTotalInadimplente: totalInadimplente,
            ValorTotalRecuperadoAcordos: valorRecuperado,
            QuantidadeAcordosAtivos: qtdAcordosAtivos,
            AgingList: agingList,
            UltimosHistoricos: ultimosHistoricos.Select(MapearHistoricoParaDto).ToList()
        );

        return Result<DashboardInadimplenciaDto>.Success(dashboard);
    }

    private List<EtapaReguaInadimplencia> ProvisionarEtapasDefault(int condoId)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        return new List<EtapaReguaInadimplencia>
        {
            EtapaReguaInadimplencia.Create(tenantId, condoId, 1, 3, 9, "Lembrete Amigável", CanalCobranca.WhatsApp, TipoAcaoCobranca.LembreteAmigavel, "Olá! Lembramos que sua fatura {NumeroFatura} venceu em {Vencimento}. Acesse o portal para 2ª via."),
            EtapaReguaInadimplencia.Create(tenantId, condoId, 2, 10, 29, "Notificação de Cobrança", CanalCobranca.WhatsApp, TipoAcaoCobranca.NotificacaoCobranca, "Aviso de Cobrança: A fatura {NumeroFatura} encontra-se pendente desde {Vencimento}. Favor efetuar o pagamento."),
            EtapaReguaInadimplencia.Create(tenantId, condoId, 3, 30, 59, "Proposta de Acordo", CanalCobranca.Email, TipoAcaoCobranca.PropostaAcordo, "Identificamos débitos pendentes superiores a 30 dias. Disponibilizamos uma proposta de acordo e parcelamento via portal."),
            EtapaReguaInadimplencia.Create(tenantId, condoId, 4, 60, 0, "Encaminhamento Jurídico", CanalCobranca.CartaNotificacao, TipoAcaoCobranca.EncaminhamentoJuridico, "Notificação Extrajudicial: Seu débito será encaminhado para cobrança jurídica caso não haja renegociação em 5 dias úteis.")
        };
    }

    private static EtapaReguaDto MapearEtapaParaDto(EtapaReguaInadimplencia etapa)
    {
        return new EtapaReguaDto(
            etapa.Id, etapa.CondoId, etapa.Ordem, etapa.DiasAtrasoMinimo, etapa.DiasAtrasoMaximo,
            etapa.NomeEtapa, etapa.Canal, etapa.TipoAcao, etapa.TemplateMensagem, etapa.Ativo
        );
    }

    private static HistoricoCobrancaDto MapearHistoricoParaDto(HistoricoCobranca h)
    {
        return new HistoricoCobrancaDto(
            h.Id, h.CondoId, h.UnidadeId, h.MoradorId, h.FaturaId, h.EtapaReguaId,
            h.DataExecucao, h.Canal, h.TipoAcao, h.MensagemEnviada, h.Sucesso, h.Observacao
        );
    }
}
