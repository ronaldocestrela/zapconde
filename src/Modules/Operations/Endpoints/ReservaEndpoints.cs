using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Endpoints;

/// <summary>
/// Endpoint para criar uma reserva de área comum com verificação de colisão e Redis Distributed Lock.
/// </summary>
public sealed class CreateReservaEndpoint : Endpoint<CreateReservaRequest, Result<ReservaDto>>
{
    private readonly IReservaApplicationService _service;

    public CreateReservaEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/reservations");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Criar reserva de área comum";
            s.Description = "Realiza o agendamento de uma área comum evitando colisão de horários com Redis Lock.";
        });
    }

    public override async Task HandleAsync(CreateReservaRequest req, CancellationToken ct)
    {
        var result = await _service.CriarReservaAsync(req, ct);
        int statusCode;

        if (result.IsSuccess)
        {
            statusCode = 201;
        }
        else if (result.Message.Contains("concorrência", StringComparison.OrdinalIgnoreCase) ||
                 result.Message.Contains("Já existe uma reserva", StringComparison.OrdinalIgnoreCase))
        {
            statusCode = 409; // 409 Conflict
        }
        else
        {
            statusCode = 400; // 400 Bad Request
        }

        await SendAsync(result, statusCode, ct);
    }
}

public record ListReservasRequest(
    int CondoId = 1,
    int? AreaComumId = null,
    int? MoradorId = null,
    StatusReserva? Status = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null);

/// <summary>
/// Endpoint para listar reservas com filtros por condomínio, área, morador, status e datas.
/// </summary>
public sealed class ListReservasEndpoint : Endpoint<ListReservasRequest, Result<IEnumerable<ReservaDto>>>
{
    private readonly IReservaApplicationService _service;

    public ListReservasEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/reservations");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar reservas de áreas comuns";
            s.Description = "Retorna reservas filtradas com isolamento por tenant.";
        });
    }

    public override async Task HandleAsync(ListReservasRequest req, CancellationToken ct)
    {
        var result = await _service.ListarReservasAsync(
            req.CondoId,
            req.AreaComumId,
            req.MoradorId,
            req.Status,
            req.DataInicio,
            req.DataFim,
            ct);

        await SendAsync(result, 200, ct);
    }
}

public record GetReservaByIdRequest(int Id);

/// <summary>
/// Endpoint para obter os detalhes de uma reserva por ID.
/// </summary>
public sealed class GetReservaByIdEndpoint : Endpoint<GetReservaByIdRequest, Result<ReservaDto>>
{
    private readonly IReservaApplicationService _service;

    public GetReservaByIdEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/reservations/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter reserva por ID";
            s.Description = "Retorna detalhes da reserva especificada.";
        });
    }

    public override async Task HandleAsync(GetReservaByIdRequest req, CancellationToken ct)
    {
        var result = await _service.ObterPorIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

public record CancelarReservaEndpointRequest(
    int Id,
    CancelarReservaRequest Body);

/// <summary>
/// Endpoint para cancelar uma reserva existente.
/// </summary>
public sealed class CancelarReservaEndpoint : Endpoint<CancelarReservaEndpointRequest, Result<ReservaDto>>
{
    private readonly IReservaApplicationService _service;

    public CancelarReservaEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/operations/reservations/{Id}/cancel");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cancelar reserva";
            s.Description = "Cancela uma reserva de área comum pendente ou confirmada.";
        });
    }

    public override async Task HandleAsync(CancelarReservaEndpointRequest req, CancellationToken ct)
    {
        var result = await _service.CancelarReservaAsync(req.Id, req.Body, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record AprovarReservaEndpointRequest(int Id);

/// <summary>
/// Endpoint para o síndico/administradora aprovar uma reserva pendente.
/// </summary>
public sealed class AprovarReservaEndpoint : Endpoint<AprovarReservaEndpointRequest, Result<ReservaDto>>
{
    private readonly IReservaApplicationService _service;

    public AprovarReservaEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/operations/reservations/{Id}/approve");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Aprovar reserva pendente";
            s.Description = "Altera o status de uma reserva pendente para confirmada.";
        });
    }

    public override async Task HandleAsync(AprovarReservaEndpointRequest req, CancellationToken ct)
    {
        var result = await _service.AprovarReservaAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record GetReservaSummaryRequest(int CondoId = 1);

/// <summary>
/// Endpoint para obter o resumo KPI das reservas do condomínio.
/// </summary>
public sealed class GetReservaSummaryEndpoint : Endpoint<GetReservaSummaryRequest, Result<ReservaSummaryDto>>
{
    private readonly IReservaApplicationService _service;

    public GetReservaSummaryEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/reservations/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI de reservas";
            s.Description = "Retorna contagens e totais financeiros de reservas do mês.";
        });
    }

    public override async Task HandleAsync(GetReservaSummaryRequest req, CancellationToken ct)
    {
        var result = await _service.ObterResumoAsync(req.CondoId, ct);
        await SendAsync(result, 200, ct);
    }
}

public record GetReservaCalendarRequest(
    int CondoId = 1,
    int? AreaComumId = null,
    DateTime? Inicio = null,
    DateTime? Fim = null);

/// <summary>
/// Endpoint para obter dados do calendário de ocupação de reservas.
/// </summary>
public sealed class GetReservaCalendarEndpoint : Endpoint<GetReservaCalendarRequest, Result<IEnumerable<ReservaCalendarSlotDto>>>
{
    private readonly IReservaApplicationService _service;

    public GetReservaCalendarEndpoint(IReservaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/reservations/calendar");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Calendário de ocupação de reservas";
            s.Description = "Retorna agendamentos ocupados no período para visualização em grade ou calendário.";
        });
    }

    public override async Task HandleAsync(GetReservaCalendarRequest req, CancellationToken ct)
    {
        var inicio = req.Inicio ?? DateTime.UtcNow.Date;
        var fim = req.Fim ?? inicio.AddMonths(1);

        var result = await _service.ObterCalendarioAsync(req.CondoId, req.AreaComumId, inicio, fim, ct);
        await SendAsync(result, 200, ct);
    }
}
