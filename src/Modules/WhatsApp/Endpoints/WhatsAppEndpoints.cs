using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.WhatsApp.Application.DTOs;
using Modules.WhatsApp.Application.Services;

namespace Modules.WhatsApp.Endpoints;

/// <summary>
/// Endpoint público/autenticado para recepção de payloads de Webhook da Evolution API.
/// </summary>
public sealed class ReceiveEvolutionWebhookEndpoint : EndpointWithoutRequest<Result<WebhookIngestionResultDto>>
{
    private readonly IWhatsAppApplicationService _service;

    public ReceiveEvolutionWebhookEndpoint(IWhatsAppApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/whatsapp/webhook/evolution");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Recepção de Webhook da Evolution API";
            s.Description = "Recebe e processa payloads de mensagens recebidas no WhatsApp via Evolution API de forma assíncrona e idempotente.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var rawJson = await reader.ReadToEndAsync(ct);

        string? apiKey = HttpContext.Request.Headers["apikey"].FirstOrDefault()
            ?? HttpContext.Request.Headers["X-Evolution-Api-Key"].FirstOrDefault()
            ?? HttpContext.Request.Query["token"].FirstOrDefault();

        var result = await _service.IngestEvolutionWebhookAsync(rawJson, apiKey, ct);
        var statusCode = result.IsSuccess ? 200 : 400;

        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para criação/configuração de uma nova instância WhatsApp.
/// </summary>
public sealed class CreateWhatsAppInstanceEndpoint : Endpoint<CreateWhatsAppInstanceCommand, Result<WhatsAppInstanceConfigDto>>
{
    private readonly IWhatsAppApplicationService _service;

    public CreateWhatsAppInstanceEndpoint(IWhatsAppApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/whatsapp/instances");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cadastrar Instância WhatsApp";
            s.Description = "Configura os parâmetros de conexão de uma instância da Evolution API para o condomínio.";
        });
    }

    public override async Task HandleAsync(CreateWhatsAppInstanceCommand req, CancellationToken ct)
    {
        var result = await _service.CreateInstanceAsync(req, ct);
        var statusCode = result.IsSuccess ? 201 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para listar as instâncias ativas de WhatsApp do condomínio.
/// </summary>
public sealed class GetWhatsAppInstancesEndpoint : EndpointWithoutRequest<Result<IEnumerable<WhatsAppInstanceConfigDto>>>
{
    private readonly IWhatsAppApplicationService _service;

    public GetWhatsAppInstancesEndpoint(IWhatsAppApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/whatsapp/instances");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar Instâncias WhatsApp";
            s.Description = "Retorna a lista de instâncias de WhatsApp configuradas.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var condoIdStr = HttpContext.Request.Query["condoId"].FirstOrDefault();
        int? condoId = int.TryParse(condoIdStr, out var cid) ? cid : null;

        var result = await _service.GetInstancesAsync(condoId, ct);
        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint para alternar status (Ativo/Inativo) de uma instância.
/// </summary>
public sealed class ToggleWhatsAppInstanceStatusEndpoint : EndpointWithoutRequest<Result<WhatsAppInstanceConfigDto>>
{
    private readonly IWhatsAppApplicationService _service;

    public ToggleWhatsAppInstanceStatusEndpoint(IWhatsAppApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/whatsapp/instances/{id}/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Alternar Status da Instância";
            s.Description = "Ativa ou desativa uma instância de WhatsApp.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idStr = Route<string>("id");
        if (!int.TryParse(idStr, out var id))
        {
            await SendAsync(Result<WhatsAppInstanceConfigDto>.Failure("ID inválido"), 400, ct);
            return;
        }

        var result = await _service.ToggleInstanceStatusAsync(id, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para consulta paginada de logs de webhooks recebidos.
/// </summary>
public sealed class GetWhatsAppWebhookLogsEndpoint : EndpointWithoutRequest<Result<IEnumerable<WhatsAppWebhookLogDto>>>
{
    private readonly IWhatsAppApplicationService _service;

    public GetWhatsAppWebhookLogsEndpoint(IWhatsAppApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/whatsapp/logs");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar Logs de Webhooks";
            s.Description = "Retorna os registros de webhooks recebidos do WhatsApp com suporte a filtros e paginação.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var instanceName = HttpContext.Request.Query["instanceName"].FirstOrDefault();
        var status = HttpContext.Request.Query["status"].FirstOrDefault();
        var phone = HttpContext.Request.Query["phone"].FirstOrDefault();
        int.TryParse(HttpContext.Request.Query["page"].FirstOrDefault(), out var page);
        int.TryParse(HttpContext.Request.Query["pageSize"].FirstOrDefault(), out var pageSize);

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var result = await _service.GetWebhookLogsAsync(instanceName, status, phone, page, pageSize, ct);
        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint para obter resumo de indicadores KPI do módulo de WhatsApp.
/// </summary>
public sealed class GetWhatsAppWebhookSummaryEndpoint : EndpointWithoutRequest<Result<WhatsAppWebhookSummaryDto>>
{
    private readonly IWhatsAppApplicationService _service;

    public GetWhatsAppWebhookSummaryEndpoint(IWhatsAppApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/whatsapp/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI de WhatsApp";
            s.Description = "Retorna os totais de webhooks recebidos hoje, taxas de sucesso, ignorados e instâncias ativas.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var condoIdStr = HttpContext.Request.Query["condoId"].FirstOrDefault();
        int? condoId = int.TryParse(condoIdStr, out var cid) ? cid : null;

        var result = await _service.GetSummaryAsync(condoId, ct);
        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint para obter as métricas de resolução e performance do consumidor em background (WhatsAppInboundConsumer).
/// </summary>
public sealed class GetWhatsAppConsumerMetricsEndpoint : EndpointWithoutRequest<Result<WhatsAppConsumerMetricsDto>>
{
    private readonly IWhatsAppInboundProcessorService _processorService;

    public GetWhatsAppConsumerMetricsEndpoint(IWhatsAppInboundProcessorService processorService)
    {
        _processorService = processorService;
    }

    public override void Configure()
    {
        Get("/api/whatsapp/consumer/metrics");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Métricas do Consumidor em Background WhatsApp";
            s.Description = "Retorna contadores de processamento, taxa de identificação de moradores e métricas de cache do Redis.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tenantIdStr = HttpContext.Request.Query["tenantId"].FirstOrDefault();
        int? tenantId = int.TryParse(tenantIdStr, out var tid) ? tid : null;

        var metrics = await _processorService.GetMetricsAsync(tenantId, ct);
        await SendAsync(Result<WhatsAppConsumerMetricsDto>.Success(metrics), 200, ct);
    }
}
