using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.DTOs;

public record CriarContaBancariaRequestDto(
    int CondoId,
    string NomeBanco,
    string CodigoBanco,
    string Agencia,
    string NumeroConta,
    TipoContaBancaria TipoConta = TipoContaBancaria.Corrente,
    decimal SaldoInicial = 0);

public record ContaBancariaDto(
    int Id,
    int TenantId,
    int CondoId,
    string NomeBanco,
    string CodigoBanco,
    string Agencia,
    string NumeroConta,
    TipoContaBancaria TipoConta,
    decimal SaldoAtual,
    bool Ativa);

public record ImportarExtratoItemDto(
    DateTime DataTransacao,
    string DescricaoHistorico,
    string DocumentoRef,
    decimal Valor,
    TipoTransacaoBancaria TipoTransacao);

public record ImportarExtratoRequestDto(
    int ContaBancariaId,
    List<ImportarExtratoItemDto> Itens);

public record ExtratoBancarioItemDto(
    int Id,
    int TenantId,
    int ContaBancariaId,
    DateTime DataTransacao,
    string DescricaoHistorico,
    string DocumentoRef,
    decimal Valor,
    TipoTransacaoBancaria TipoTransacao,
    StatusConciliacaoBancaria StatusConciliacao,
    int? TransacaoConciliadaId,
    decimal ScoreConciliacao);

public record ConciliarManualRequestDto(
    int ExtratoBancarioItemId,
    OrigemConciliacao OrigemTipo,
    int OrigemId,
    int ConciliadoPorUserId,
    string Observacoes = "");

public record ResultadoConciliacaoEmLoteDto(
    int TotalItensProcessados,
    int ConciliadosAutomaticamente,
    int Pendentes,
    List<ExtratoBancarioItemDto> ItensConciliados);
