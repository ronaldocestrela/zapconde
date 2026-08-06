using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.DTOs;

public record CreateReservaRequest(
    int CondoId,
    int AreaComumId,
    int MoradorId,
    string NomeMorador,
    string UnidadeMorador,
    DateTime DataInicio,
    DateTime DataFim,
    int QuantidadePessoas,
    string Observacao = "");

public record CancelarReservaRequest(
    string Motivo);

public record RejeitarReservaRequest(
    string Motivo);

public record ReservaDto(
    int Id,
    int TenantId,
    int CondoId,
    int AreaComumId,
    string NomeAreaComum,
    int MoradorId,
    string NomeMorador,
    string UnidadeMorador,
    DateTime DataInicio,
    DateTime DataFim,
    int QuantidadePessoas,
    decimal ValorTaxaReserva,
    decimal ValorTaxaLimpeza,
    decimal ValorTotal,
    StatusReserva Status,
    string Observacao,
    string MotivoCancelamento,
    DateTime DataCriacao);

public record ReservaSummaryDto(
    int TotalReservasMes,
    int ReservasPendentes,
    int ReservasConfirmadas,
    decimal ReceitaTotalReservas);

public record ReservaCalendarSlotDto(
    int ReservaId,
    int AreaComumId,
    string NomeAreaComum,
    string NomeMorador,
    string UnidadeMorador,
    DateTime DataInicio,
    DateTime DataFim,
    StatusReserva Status);
