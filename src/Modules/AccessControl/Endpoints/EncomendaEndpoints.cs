using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Domain.Enums;

namespace Modules.AccessControl.Endpoints;

/// <summary>
/// Endpoint para registrar o recebimento de uma nova encomenda na portaria.
/// </summary>
public sealed class CreateEncomendaEndpoint : Endpoint<RegistrarRecebimentoEncomendaRequest, Result<EncomendaDto>>
{
    private readonly IEncomendaApplicationService _service;

    public CreateEncomendaEndpoint(IEncomendaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/access-control/packages");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Registrar Recebimento de Encomenda";
            s.Description = "Cadastra o recebimento de um pacote, caixa ou correspondência na portaria do condomínio.";
        });
    }

    public override async Task HandleAsync(RegistrarRecebimentoEncomendaRequest req, CancellationToken ct)
    {
        var result = await _service.RegistrarRecebimentoAsync(req, ct);
        var statusCode = result.IsSuccess ? 201 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record GetEncomendasQueryRequest(
    StatusEncomenda? Status = null,
    TipoEncomenda? Tipo = null,
    int? UnidadeId = null,
    string? Busca = null);

/// <summary>
/// Endpoint para listar encomendas filtradas.
/// </summary>
public sealed class GetEncomendasEndpoint : Endpoint<GetEncomendasQueryRequest, Result<IEnumerable<EncomendaDto>>>
{
    private readonly IEncomendaApplicationService _service;

    public GetEncomendasEndpoint(IEncomendaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/access-control/packages");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar Encomendas";
            s.Description = "Retorna a lista de encomendas recebidas com filtros por status, tipo, unidade ou busca por código/descrição.";
        });
    }

    public override async Task HandleAsync(GetEncomendasQueryRequest req, CancellationToken ct)
    {
        var result = await _service.GetEncomendasAsync(req.Status, req.Tipo, req.UnidadeId, req.Busca, ct);
        await SendAsync(result, 200, ct);
    }
}

public record GetEncomendaByIdRequest(int Id);

/// <summary>
/// Endpoint para obter os detalhes de uma encomenda por ID.
/// </summary>
public sealed class GetEncomendaByIdEndpoint : Endpoint<GetEncomendaByIdRequest, Result<EncomendaDto>>
{
    private readonly IEncomendaApplicationService _service;

    public GetEncomendaByIdEndpoint(IEncomendaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/access-control/packages/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter Encomenda por ID";
            s.Description = "Retorna os detalhes completos de um registro de encomenda.";
        });
    }

    public override async Task HandleAsync(GetEncomendaByIdRequest req, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

public record RegistrarBaixaRouteRequest
{
    public int Id { get; init; }
    [FromBody]
    public RegistrarBaixaEncomendaRequest Body { get; init; } = default!;
}

/// <summary>
/// Endpoint para dar baixa/entrega de encomenda ao morador ou pessoa autorizada.
/// </summary>
public sealed class RegistrarBaixaEncomendaEndpoint : Endpoint<RegistrarBaixaRouteRequest, Result<EncomendaDto>>
{
    private readonly IEncomendaApplicationService _service;

    public RegistrarBaixaEncomendaEndpoint(IEncomendaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/access-control/packages/{Id}/pickup");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Registrar Retirada de Encomenda";
            s.Description = "Registra a baixa de entrega da encomenda na portaria informando o retirante.";
        });
    }

    public override async Task HandleAsync(RegistrarBaixaRouteRequest req, CancellationToken ct)
    {
        var result = await _service.RegistrarBaixaAsync(req.Id, req.Body, ct);
        var statusCode = result.IsSuccess ? 200 : (result.Message.Contains("não foi encontrada") ? 404 : 400);
        await SendAsync(result, statusCode, ct);
    }
}

public record NotificarMoradorRouteRequest(int Id);

/// <summary>
/// Endpoint para disparar notificação ao morador sobre a encomenda pendente.
/// </summary>
public sealed class NotificarMoradorEncomendaEndpoint : Endpoint<NotificarMoradorRouteRequest, Result<EncomendaDto>>
{
    private readonly IEncomendaApplicationService _service;

    public NotificarMoradorEncomendaEndpoint(IEncomendaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/access-control/packages/{Id}/notify");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Notificar Morador";
            s.Description = "Dispara um aviso ao morador referente à encomenda disponível para retirada.";
        });
    }

    public override async Task HandleAsync(NotificarMoradorRouteRequest req, CancellationToken ct)
    {
        var result = await _service.NotificarMoradorAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : (result.Message.Contains("não foi encontrada") ? 404 : 400);
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para obter o resumo KPI das encomendas.
/// </summary>
public sealed class GetEncomendaSummaryEndpoint : EndpointWithoutRequest<Result<EncomendaSummaryDto>>
{
    private readonly IEncomendaApplicationService _service;

    public GetEncomendaSummaryEndpoint(IEncomendaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/access-control/packages/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter Resumo KPI de Encomendas";
            s.Description = "Retorna métricas consolidadas de correspondências e encomendas na portaria.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(ct);
        await SendAsync(result, 200, ct);
    }
}
