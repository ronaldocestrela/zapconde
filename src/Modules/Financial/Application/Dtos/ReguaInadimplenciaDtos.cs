using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.Dtos;

public record EtapaReguaDto(
    int Id,
    int CondoId,
    int Ordem,
    int DiasAtrasoMinimo,
    int DiasAtrasoMaximo,
    string NomeEtapa,
    CanalCobranca Canal,
    TipoAcaoCobranca TipoAcao,
    string TemplateMensagem,
    bool Ativo
);

public record SalvarEtapaReguaDto(
    int? Id,
    int Ordem,
    int DiasAtrasoMinimo,
    int DiasAtrasoMaximo,
    string NomeEtapa,
    CanalCobranca Canal,
    TipoAcaoCobranca TipoAcao,
    string TemplateMensagem,
    bool Ativo
);

public record HistoricoCobrancaDto(
    int Id,
    int CondoId,
    int UnidadeId,
    int MoradorId,
    int FaturaId,
    int EtapaReguaId,
    DateTime DataExecucao,
    CanalCobranca Canal,
    TipoAcaoCobranca TipoAcao,
    string MensagemEnviada,
    bool Sucesso,
    string Observacao
);

public record AgingListDto(
    decimal TotalVencido1A30Dias,
    decimal TotalVencido31A60Dias,
    decimal TotalVencido61A90Dias,
    decimal TotalVencidoAcima90Dias,
    decimal TotalGeralVencido,
    int QuantidadeUnidadesInadimplentes
);

public record DashboardInadimplenciaDto(
    int CondoId,
    decimal TaxaInadimplenciaPercentual,
    decimal ValorTotalInadimplente,
    decimal ValorTotalRecuperadoAcordos,
    int QuantidadeAcordosAtivos,
    AgingListDto AgingList,
    List<HistoricoCobrancaDto> UltimosHistoricos
);

public record ProcessamentoReguaResultadoDto(
    int TotalAcoesProcessadas,
    int TotalSucessos,
    int TotalFalhas,
    List<HistoricoCobrancaDto> HistoricosGerados
);
