using Modules.AccessControl.Domain.Entities;
using Modules.AccessControl.Domain.Enums;

namespace Modules.AccessControl.Application.DTOs;

public sealed record VisitanteDto(
    int Id,
    int TenantId,
    int CondoId,
    string NomeCompleto,
    string Documento,
    string? Telefone,
    TipoVisitante Tipo,
    StatusVisitante Status,
    string? Empresa,
    string? PlacaVeiculo,
    int UnidadeId,
    string BlocoUnidade,
    int? MoradorId,
    DateTimeOffset? DataHoraInicioAutorizacao,
    DateTimeOffset? DataHoraFimAutorizacao,
    DateTimeOffset? DataHoraEntrada,
    DateTimeOffset? DataHoraSaida,
    string? Observacoes,
    int? OperadorEntradaId,
    int? OperadorSaidaId,
    DateTimeOffset CriadoEm)
{
    public static VisitanteDto FromDomain(Visitante domain) => new(
        domain.Id,
        domain.TenantId,
        domain.CondoId,
        domain.NomeCompleto,
        domain.Documento,
        domain.Telefone,
        domain.Tipo,
        domain.Status,
        domain.Empresa,
        domain.PlacaVeiculo,
        domain.UnidadeId,
        domain.BlocoUnidade,
        domain.MoradorId,
        domain.DataHoraInicioAutorizacao,
        domain.DataHoraFimAutorizacao,
        domain.DataHoraEntrada,
        domain.DataHoraSaida,
        domain.Observacoes,
        domain.OperadorEntradaId,
        domain.OperadorSaidaId,
        domain.CriadoEm
    );
}

public sealed record CreateVisitanteRequestDto(
    string NomeCompleto,
    string Documento,
    string? Telefone,
    TipoVisitante Tipo,
    int UnidadeId,
    string? BlocoUnidade,
    int? MoradorId,
    DateTimeOffset? DataHoraInicioAutorizacao,
    DateTimeOffset? DataHoraFimAutorizacao,
    string? Empresa,
    string? PlacaVeiculo,
    string? Observacoes,
    bool RegistrarEntradaImediata = false);

public sealed record RegistrarEntradaRequestDto(int? OperadorId);

public sealed record RegistrarSaidaRequestDto(int? OperadorId);

public sealed record VisitanteSummaryDto(
    int TotalHoje,
    int PresentesAgora,
    int AgendadosPendentes,
    int EntradasHoje,
    int SaidasHoje);
