using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Endpoints;

/// <summary>
/// Endpoint para criar um novo plano de manutenção preventiva.
/// </summary>
public sealed class CreatePlanoManutencaoEndpoint : Endpoint<CreatePlanoManutencaoRequest, Result<PlanoManutencaoDto>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public CreatePlanoManutencaoEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/maintenance");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Criar plano de manutenção preventiva";
            s.Description = "Cadastra um plano de manutenção para elevadores, bombas, para-raios ou geradores com periodicidade e prazo.";
        });
    }

    public override async Task HandleAsync(CreatePlanoManutencaoRequest req, CancellationToken ct)
    {
        var result = await _service.CriarPlanoAsync(req, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record ListPlanosManutencaoRequest(
    int CondoId = 1,
    CategoriaManutencao? Categoria = null,
    StatusManutencao? Status = null,
    PeriodicidadeManutencao? Periodicidade = null,
    DateTime? Inicio = null,
    DateTime? Fim = null);

/// <summary>
/// Endpoint para listar planos de manutenção preventiva com suporte a filtros.
/// </summary>
public sealed class ListPlanosManutencaoEndpoint : Endpoint<ListPlanosManutencaoRequest, Result<IEnumerable<PlanoManutencaoDto>>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public ListPlanosManutencaoEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/maintenance");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar planos de manutenção";
            s.Description = "Retorna lista de manutenções preventivas cadastradas com cálculo dinâmico de status de alertas.";
        });
    }

    public override async Task HandleAsync(ListPlanosManutencaoRequest req, CancellationToken ct)
    {
        var result = await _service.ListarAsync(
            req.CondoId, req.Categoria, req.Status, req.Periodicidade, req.Inicio, req.Fim, ct);
        await SendAsync(result, StatusCodes.Status200OK, ct);
    }
}

public record GetPlanoManutencaoByIdRequest(Guid Id);

/// <summary>
/// Endpoint para obter detalhes de um plano de manutenção por ID.
/// </summary>
public sealed class GetPlanoManutencaoByIdEndpoint : Endpoint<GetPlanoManutencaoByIdRequest, Result<PlanoManutencaoDto>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public GetPlanoManutencaoByIdEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/maintenance/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter plano de manutenção por ID";
            s.Description = "Retorna detalhes completos do plano de manutenção especificado.";
        });
    }

    public override async Task HandleAsync(GetPlanoManutencaoByIdRequest req, CancellationToken ct)
    {
        var result = await _service.ObterPorIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
        await SendAsync(result, statusCode, ct);
    }
}

public record UpdatePlanoManutencaoRouteRequest(
    Guid Id,
    string Titulo,
    string? Descricao,
    CategoriaManutencao Categoria,
    PeriodicidadeManutencao Periodicidade,
    DateTime DataProximaManutencao,
    string? ResponsavelTecnico = null,
    string? EmpresaContratada = null,
    decimal? CustoEstimado = null,
    string? Observacoes = null);

/// <summary>
/// Endpoint para atualizar dados de um plano de manutenção existente.
/// </summary>
public sealed class UpdatePlanoManutencaoEndpoint : Endpoint<UpdatePlanoManutencaoRouteRequest, Result<PlanoManutencaoDto>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public UpdatePlanoManutencaoEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Put("/api/operations/maintenance/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Atualizar plano de manutenção";
            s.Description = "Atualiza título, periodicidade, datas e informações do responsável ou empresa contratada.";
        });
    }

    public override async Task HandleAsync(UpdatePlanoManutencaoRouteRequest req, CancellationToken ct)
    {
        var updateRequest = new UpdatePlanoManutencaoRequest(
            req.Titulo, req.Descricao, req.Categoria, req.Periodicidade,
            req.DataProximaManutencao, req.ResponsavelTecnico, req.EmpresaContratada,
            req.CustoEstimado, req.Observacoes);

        var result = await _service.AtualizarPlanoAsync(req.Id, updateRequest, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record CompletePlanoManutencaoRouteRequest(
    Guid Id,
    DateTime DataRealizacao,
    decimal? CustoReal = null,
    string? Observacoes = null,
    bool AgendarProxima = true);

/// <summary>
/// Endpoint para dar baixa/concluir uma manutenção realizada e recalcular o próximo ciclo.
/// </summary>
public sealed class CompletePlanoManutencaoEndpoint : Endpoint<CompletePlanoManutencaoRouteRequest, Result<PlanoManutencaoDto>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public CompletePlanoManutencaoEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/maintenance/{id}/complete");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Registrar baixa/conclusão de manutenção";
            s.Description = "Registra a realização da manutenção, custos reais, observações do técnico e reagenda a próxima data conforme a periodicidade.";
        });
    }

    public override async Task HandleAsync(CompletePlanoManutencaoRouteRequest req, CancellationToken ct)
    {
        var request = new ConcluirManutencaoRequest(req.DataRealizacao, req.CustoReal, req.Observacoes, req.AgendarProxima);
        var result = await _service.ConcluirManutencaoAsync(req.Id, request, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record GetPlanoManutencaoSummaryRequest(int CondoId = 1);

/// <summary>
/// Endpoint para obter estatísticas e resumo de métricas KPI de manutenção preventiva.
/// </summary>
public sealed class GetPlanoManutencaoSummaryEndpoint : Endpoint<GetPlanoManutencaoSummaryRequest, Result<PlanoManutencaoSummaryDto>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public GetPlanoManutencaoSummaryEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/maintenance/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI de manutenção";
            s.Description = "Retorna quantidade de manutenções em dia, próximas a vencer, atrasadas e total acumulado de custos.";
        });
    }

    public override async Task HandleAsync(GetPlanoManutencaoSummaryRequest req, CancellationToken ct)
    {
        var result = await _service.ObterResumoMetricasAsync(req.CondoId, ct);
        await SendAsync(result, StatusCodes.Status200OK, ct);
    }
}

public record GetPlanoManutencaoCalendarRequest(
    int CondoId = 1,
    DateTime? Inicio = null,
    DateTime? Fim = null);

/// <summary>
/// Endpoint para obter os eventos do calendário de manutenção preventiva.
/// </summary>
public sealed class GetPlanoManutencaoCalendarEndpoint : Endpoint<GetPlanoManutencaoCalendarRequest, Result<IEnumerable<ManutencaoCalendarEventDto>>>
{
    private readonly IPlanoManutencaoApplicationService _service;

    public GetPlanoManutencaoCalendarEndpoint(IPlanoManutencaoApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/maintenance/calendar");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Calendário de manutenções";
            s.Description = "Retorna eventos formatados para visualização em grade de calendário mensal/semanal.";
        });
    }

    public override async Task HandleAsync(GetPlanoManutencaoCalendarRequest req, CancellationToken ct)
    {
        var result = await _service.ObterEventosCalendarioAsync(req.CondoId, req.Inicio, req.Fim, ct);
        await SendAsync(result, StatusCodes.Status200OK, ct);
    }
}
