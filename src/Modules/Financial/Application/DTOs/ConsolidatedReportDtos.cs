namespace Modules.Financial.Application.DTOs;

public record ResumoCondominioDto(
    int CondoId,
    string NomeCondominio,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal SaldoAtual,
    decimal TaxaInadimplenciaPercentual,
    int TotalInadimplentes,
    int PastasPendentesAprovacao);

public record RelatorioConsolidadoMulticondominioDto(
    int TenantId,
    DateTime DataGeracao,
    int TotalCondominios,
    decimal ReceitaTotalConsolidada,
    decimal DespesaTotalConsolidada,
    decimal SaldoTotalConsolidado,
    decimal TaxaInadimplenciaMedia,
    int TotalPastasPendentesGeral,
    List<ResumoCondominioDto> Condominios);
