using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Application.Services;

namespace Modules.AIEngine.Endpoints;

/// <summary>
/// Endpoint para obter a configuração do Semantic Kernel do condomínio.
/// </summary>
public sealed class GetAiConfigEndpoint : EndpointWithoutRequest<Result<AiKernelConfigDto>>
{
    private readonly IAiOrchestratorService _service;

    public GetAiConfigEndpoint(IAiOrchestratorService service) => _service = service;

    public override void Configure()
    {
        Get("/api/ai/config");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter Configuração do Semantic Kernel";
            s.Description = "Retorna a configuração ativa do Semantic Kernel (OpenAI, Azure OpenAI ou MockLocal) para o condomínio atual.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetConfigAsync(ct);
        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint para salvar ou atualizar a configuração do Semantic Kernel.
/// </summary>
public sealed class SaveAiConfigEndpoint : Endpoint<SaveAiConfigCommand, Result<AiKernelConfigDto>>
{
    private readonly IAiOrchestratorService _service;

    public SaveAiConfigEndpoint(IAiOrchestratorService service) => _service = service;

    public override void Configure()
    {
        Post("/api/ai/config");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Salvar Configuração do Semantic Kernel";
            s.Description = "Cria ou atualiza os parâmetros do Semantic Kernel, modelos e credenciais do condomínio.";
        });
    }

    public override async Task HandleAsync(SaveAiConfigCommand req, CancellationToken ct)
    {
        var result = await _service.SaveConfigAsync(req, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para executar prompts interativos via Semantic Kernel.
/// </summary>
public sealed class ExecutePromptEndpoint : Endpoint<ExecutePromptRequestDto, Result<ExecutePromptResponseDto>>
{
    private readonly IAiOrchestratorService _service;

    public ExecutePromptEndpoint(IAiOrchestratorService service) => _service = service;

    public override void Configure()
    {
        Post("/api/ai/prompt/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Executar Prompt no Semantic Kernel";
            s.Description = "Processa um prompt através da instância configurada do Semantic Kernel e registra a auditoria e métricas de consumo.";
        });
    }

    public override async Task HandleAsync(ExecutePromptRequestDto req, CancellationToken ct)
    {
        var result = await _service.ExecutePromptAsync(req, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

/// <summary>
/// Endpoint para listar logs de auditoria de chamadas de IA.
/// </summary>
public sealed class GetAiLogsEndpoint : EndpointWithoutRequest<Result<IEnumerable<AiExecutionLogDto>>>
{
    private readonly IAiOrchestratorService _service;

    public GetAiLogsEndpoint(IAiOrchestratorService service) => _service = service;

    public override void Configure()
    {
        Get("/api/ai/logs");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar Logs de Execução de IA";
            s.Description = "Retorna o histórico paginado de chamadas e execuções de prompts no Semantic Kernel.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int.TryParse(HttpContext.Request.Query["page"].FirstOrDefault(), out var page);
        int.TryParse(HttpContext.Request.Query["pageSize"].FirstOrDefault(), out var pageSize);

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var result = await _service.GetLogsAsync(page, pageSize, ct);
        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint para obter o resumo dos indicadores KPI da Engine de IA.
/// </summary>
public sealed class GetAiSummaryEndpoint : EndpointWithoutRequest<Result<AiSummaryDto>>
{
    private readonly IAiOrchestratorService _service;

    public GetAiSummaryEndpoint(IAiOrchestratorService service) => _service = service;

    public override void Configure()
    {
        Get("/api/ai/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI da Engine de IA";
            s.Description = "Retorna os contadores de execuções, taxa de sucesso/falhas, total de tokens consumidos e latência média.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(ct);
        await SendAsync(result, 200, ct);
    }
}
