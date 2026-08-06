using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Endpoints;

/// <summary>
/// Endpoint para cadastrar uma nova área comum com regras de capacidade e custos.
/// </summary>
public sealed class CreateAreaComumEndpoint : Endpoint<CreateAreaComumRequest, Result<AreaComumDto>>
{
    private readonly IAreaComumApplicationService _service;

    public CreateAreaComumEndpoint(IAreaComumApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/common-areas");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cadastrar área comum";
            s.Description = "Cadastra uma nova área comum (Salão de Festas, Churrasqueira, etc.) com taxas, capacidade e horários.";
        });
    }

    public override async Task HandleAsync(CreateAreaComumRequest req, CancellationToken ct)
    {
        var result = await _service.CreateAsync(req, ct);
        var statusCode = result.IsSuccess ? 201 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record ListAreasComunsRequest(
    int CondoId = 1,
    StatusAreaComum? Status = null,
    TipoAreaComum? Tipo = null);

/// <summary>
/// Endpoint para listar áreas comuns do condomínio com isolamento multi-tenant.
/// </summary>
public sealed class ListAreasComunsEndpoint : Endpoint<ListAreasComunsRequest, Result<IEnumerable<AreaComumDto>>>
{
    private readonly IAreaComumApplicationService _service;

    public ListAreasComunsEndpoint(IAreaComumApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/common-areas");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar áreas comuns";
            s.Description = "Retorna todas as áreas comuns registradas para o condomínio e tenant logado.";
        });
    }

    public override async Task HandleAsync(ListAreasComunsRequest req, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(req.CondoId, req.Status, req.Tipo, ct);
        await SendAsync(result, result.IsSuccess ? 200 : 400, ct);
    }
}

public record GetAreaComumByIdRequest(int Id);

/// <summary>
/// Endpoint para obter detalhes de uma área comum pelo ID.
/// </summary>
public sealed class GetAreaComumByIdEndpoint : Endpoint<GetAreaComumByIdRequest, Result<AreaComumDto>>
{
    private readonly IAreaComumApplicationService _service;

    public GetAreaComumByIdEndpoint(IAreaComumApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/common-areas/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter área comum por ID";
            s.Description = "Retorna os detalhes de uma área comum específica.";
        });
    }

    public override async Task HandleAsync(GetAreaComumByIdRequest req, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

public record UpdateAreaComumRouteRequest(
    int Id,
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
    string RegrasUso)
{
    public UpdateAreaComumRequest ToApplicationRequest() => new(
        Nome, Descricao, Tipo, CapacidadeMaxima, TaxaReserva, TaxaLimpeza,
        HorarioInicioFuncionamento, HorarioFimFuncionamento,
        TempoAntecedenciaMinimaDias, TempoAntecedenciaMaximaDias,
        RequerAprovacaoSindico, RegrasUso);
}

/// <summary>
/// Endpoint para atualizar dados, capacidade e regras de custo de uma área comum.
/// </summary>
public sealed class UpdateAreaComumEndpoint : Endpoint<UpdateAreaComumRouteRequest, Result<AreaComumDto>>
{
    private readonly IAreaComumApplicationService _service;

    public UpdateAreaComumEndpoint(IAreaComumApplicationService service) => _service = service;

    public override void Configure()
    {
        Put("/api/operations/common-areas/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Atualizar área comum";
            s.Description = "Atualiza nome, tipo, capacidade, horários e regras de custos da área comum.";
        });
    }

    public override async Task HandleAsync(UpdateAreaComumRouteRequest req, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(req.Id, req.ToApplicationRequest(), ct);
        var statusCode = result.IsSuccess ? 200 : (result.Message.Contains("não foi encontrada") ? 404 : 400);
        await SendAsync(result, statusCode, ct);
    }
}

public record ChangeAreaComumStatusRouteRequest(
    int Id,
    StatusAreaComum NovoStatus);

/// <summary>
/// Endpoint para alterar o status operacional de uma área comum.
/// </summary>
public sealed class ChangeAreaComumStatusEndpoint : Endpoint<ChangeAreaComumStatusRouteRequest, Result<AreaComumDto>>
{
    private readonly IAreaComumApplicationService _service;

    public ChangeAreaComumStatusEndpoint(IAreaComumApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/operations/common-areas/{id}/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Alterar status da área comum";
            s.Description = "Altera o status de funcionamento da área comum (Ativa, Manutenção ou Inativa).";
        });
    }

    public override async Task HandleAsync(ChangeAreaComumStatusRouteRequest req, CancellationToken ct)
    {
        var result = await _service.ChangeStatusAsync(req.Id, new ChangeAreaComumStatusRequest(req.NovoStatus), ct);
        var statusCode = result.IsSuccess ? 200 : (result.Message.Contains("não encontrada") ? 404 : 400);
        await SendAsync(result, statusCode, ct);
    }
}

public record GetAreaComumSummaryRequest(int CondoId = 1);

/// <summary>
/// Endpoint para obter o resumo/KPIs de áreas comuns do condomínio.
/// </summary>
public sealed class GetAreaComumSummaryEndpoint : Endpoint<GetAreaComumSummaryRequest, Result<AreaComumSummaryDto>>
{
    private readonly IAreaComumApplicationService _service;

    public GetAreaComumSummaryEndpoint(IAreaComumApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/common-areas/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI de áreas comuns";
            s.Description = "Retorna o acumulado de áreas ativas, em manutenção, inativas e médias de custos.";
        });
    }

    public override async Task HandleAsync(GetAreaComumSummaryRequest req, CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(req.CondoId, ct);
        await SendAsync(result, result.IsSuccess ? 200 : 400, ct);
    }
}
