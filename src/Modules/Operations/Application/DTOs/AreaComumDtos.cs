using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.DTOs;

public record AreaComumDto(
    int Id,
    int TenantId,
    int CondoId,
    string Nome,
    string Descricao,
    TipoAreaComum Tipo,
    string TipoDescricao,
    StatusAreaComum Status,
    string StatusDescricao,
    int CapacidadeMaxima,
    decimal TaxaReserva,
    decimal TaxaLimpeza,
    decimal CustoTotalReserva,
    string HorarioInicioFuncionamento,
    string HorarioFimFuncionamento,
    int TempoAntecedenciaMinimaDias,
    int TempoAntecedenciaMaximaDias,
    bool RequerAprovacaoSindico,
    string RegrasUso,
    DateTime DataCriacao,
    DateTime? DataAtualizacao);

public record CreateAreaComumRequest(
    int CondoId,
    string Nome,
    string Descricao,
    TipoAreaComum Tipo,
    int CapacidadeMaxima,
    decimal TaxaReserva,
    decimal TaxaLimpeza,
    string HorarioInicioFuncionamento,
    string HorarioFimFuncionamento,
    int TempoAntecedenciaMinimaDias = 1,
    int TempoAntecedenciaMaximaDias = 60,
    bool RequerAprovacaoSindico = false,
    string RegrasUso = "");

public record UpdateAreaComumRequest(
    string Nome,
    string Descricao,
    TipoAreaComum Tipo,
    int CapacidadeMaxima,
    decimal TaxaReserva,
    decimal TaxaLimpeza,
    string HorarioInicioFuncionamento,
    string HorarioFimFuncionamento,
    int TempoAntecedenciaMinimaDias,
    int TempoAntecedenciaMaximaDias,
    bool RequerAprovacaoSindico,
    string RegrasUso);

public record ChangeAreaComumStatusRequest(
    StatusAreaComum NovoStatus);

public record AreaComumSummaryDto(
    int TotalAreas,
    int AreasAtivas,
    int AreasEmManutencao,
    int AreasInativas,
    decimal TaxaMediaReserva,
    decimal TaxaMediaLimpeza);
