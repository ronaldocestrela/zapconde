using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Domain.Enums;

namespace Modules.AccessControl.Endpoints;

/// <summary>
/// Endpoint para autorizar ou cadastrar um novo visitante ou prestador de serviço.
/// </summary>
public sealed class CreateVisitanteEndpoint : Endpoint<CreateVisitanteRequestDto, Result<VisitanteDto>>
{
    private readonly IVisitanteApplicationService _service;

    public CreateVisitanteEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/access-control/visitors");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Autorizar Visitante ou Prestador";
            s.Description = "Cadastra uma autorização prévia de visitante social ou prestador de serviço na portaria.";
        });
    }

    public override async Task HandleAsync(CreateVisitanteRequestDto req, CancellationToken ct)
    {
        var result = await _service.AuthorizeVisitanteAsync(req, ct);
        var statusCode = result.IsSuccess ? 201 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record GetVisitantesQueryRequest(
    TipoVisitante? Tipo = null,
    StatusVisitante? Status = null,
    int? UnidadeId = null,
    string? Busca = null);

/// <summary>
/// Endpoint para listar visitantes e prestadores de serviço com filtros.
/// </summary>
public sealed class GetVisitantesEndpoint : Endpoint<GetVisitantesQueryRequest, Result<IEnumerable<VisitanteDto>>>
{
    private readonly IVisitanteApplicationService _service;

    public GetVisitantesEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/access-control/visitors");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar visitantes e prestadores";
            s.Description = "Retorna os registros de acesso filtrados por tipo, status, unidade ou termo de busca.";
        });
    }

    public override async Task HandleAsync(GetVisitantesQueryRequest req, CancellationToken ct)
    {
        var result = await _service.GetVisitantesAsync(req.Tipo, req.Status, req.UnidadeId, req.Busca, ct);
        await SendAsync(result, 200, ct);
    }
}

public record GetVisitanteByIdRequest(int Id);

/// <summary>
/// Endpoint para obter os detalhes de um visitante por ID.
/// </summary>
public sealed class GetVisitanteByIdEndpoint : Endpoint<GetVisitanteByIdRequest, Result<VisitanteDto>>
{
    private readonly IVisitanteApplicationService _service;

    public GetVisitanteByIdEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/access-control/visitors/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter visitante por ID";
            s.Description = "Retorna os detalhes de um cadastro de visitante ou prestador pelo seu ID.";
        });
    }

    public override async Task HandleAsync(GetVisitanteByIdRequest req, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

public record RegistrarEntradaRouteRequest
{
    public int Id { get; init; }
    [FromBody]
    public RegistrarEntradaRequestDto? Body { get; init; }
}

/// <summary>
/// Endpoint para registrar a entrada de um visitante na portaria.
/// </summary>
public sealed class RegistrarEntradaEndpoint : Endpoint<RegistrarEntradaRouteRequest, Result<VisitanteDto>>
{
    private readonly IVisitanteApplicationService _service;

    public RegistrarEntradaEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/access-control/visitors/{Id}/entry");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Registrar entrada na portaria";
            s.Description = "Muda o status do visitante para 'Presente' e registra a data/hora exata de entrada.";
        });
    }

    public override async Task HandleAsync(RegistrarEntradaRouteRequest req, CancellationToken ct)
    {
        var result = await _service.RegistrarEntradaAsync(req.Id, req.Body?.OperadorId, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record RegistrarSaidaRouteRequest
{
    public int Id { get; init; }
    [FromBody]
    public RegistrarSaidaRequestDto? Body { get; init; }
}

/// <summary>
/// Endpoint para registrar a saída de um visitante na portaria.
/// </summary>
public sealed class RegistrarSaidaEndpoint : Endpoint<RegistrarSaidaRouteRequest, Result<VisitanteDto>>
{
    private readonly IVisitanteApplicationService _service;

    public RegistrarSaidaEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/access-control/visitors/{Id}/exit");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Registrar saída na portaria";
            s.Description = "Muda o status do visitante para 'Finalizado' e registra a data/hora exata de saída.";
        });
    }

    public override async Task HandleAsync(RegistrarSaidaRouteRequest req, CancellationToken ct)
    {
        var result = await _service.RegistrarSaidaAsync(req.Id, req.Body?.OperadorId, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record CancelarVisitanteRequest(int Id, string? Motivo = null);

/// <summary>
/// Endpoint para cancelar uma autorização de visitante.
/// </summary>
public sealed class CancelarVisitanteEndpoint : Endpoint<CancelarVisitanteRequest, Result<VisitanteDto>>
{
    private readonly IVisitanteApplicationService _service;

    public CancelarVisitanteEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/access-control/visitors/{Id}/cancel");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cancelar autorização";
            s.Description = "Muda o status da autorização para 'Cancelado'.";
        });
    }

    public override async Task HandleAsync(CancelarVisitanteRequest req, CancellationToken ct)
    {
        var result = await _service.CancelarAutorizacaoAsync(req.Id, req.Motivo, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para obter o resumo de KPIs do fluxo de visitantes e portaria.
/// </summary>
public sealed class GetVisitanteSummaryEndpoint : EndpointWithoutRequest<Result<VisitanteSummaryDto>>
{
    private readonly IVisitanteApplicationService _service;

    public GetVisitanteSummaryEndpoint(IVisitanteApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/access-control/visitors/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI de portaria";
            s.Description = "Retorna os indicadores diários de visitantes, prestadores presentes, entradas e saídas.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(ct);
        await SendAsync(result, 200, ct);
    }
}
