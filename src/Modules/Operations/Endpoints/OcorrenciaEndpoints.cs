using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Endpoints;

/// <summary>
/// Endpoint para criar um novo chamado / ocorrência.
/// </summary>
public sealed class CreateTicketEndpoint : Endpoint<CriarOcorrenciaRequest, Result<OcorrenciaDto>>
{
    private readonly IOcorrenciaApplicationService _service;

    public CreateTicketEndpoint(IOcorrenciaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/tickets");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Criar chamado / ocorrência";
            s.Description = "Registra um novo chamado ou ocorrência com categoria, prioridade, localização e fotos anexas.";
        });
    }

    public override async Task HandleAsync(CriarOcorrenciaRequest req, CancellationToken ct)
    {
        var result = await _service.CriarOcorrenciaAsync(req, ct);
        var statusCode = result.IsSuccess ? 201 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record ListTicketsRequest(
    int CondoId = 1,
    StatusOcorrencia? Status = null,
    CategoriaOcorrencia? Categoria = null,
    PrioridadeOcorrencia? Prioridade = null,
    string? MoradorId = null);

/// <summary>
/// Endpoint para listar chamados / ocorrências filtrados por status, categoria e morador.
/// </summary>
public sealed class ListTicketsEndpoint : Endpoint<ListTicketsRequest, Result<IEnumerable<OcorrenciaDto>>>
{
    private readonly IOcorrenciaApplicationService _service;

    public ListTicketsEndpoint(IOcorrenciaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/tickets");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar chamados / ocorrências";
            s.Description = "Retorna chamados operacionais do condomínio com suporte a filtros e multi-tenancy.";
        });
    }

    public override async Task HandleAsync(ListTicketsRequest req, CancellationToken ct)
    {
        var result = await _service.ListarAsync(req.CondoId, req.Status, req.Categoria, req.Prioridade, req.MoradorId, ct);
        await SendAsync(result, 200, ct);
    }
}

public record GetTicketByIdRequest(Guid Id);

/// <summary>
/// Endpoint para obter detalhes completos de uma ocorrência por ID.
/// </summary>
public sealed class GetTicketByIdEndpoint : Endpoint<GetTicketByIdRequest, Result<OcorrenciaDto>>
{
    private readonly IOcorrenciaApplicationService _service;

    public GetTicketByIdEndpoint(IOcorrenciaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/tickets/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter ocorrência por ID";
            s.Description = "Retorna detalhes completos do chamado, incluindo anexos e linha do tempo de histórico.";
        });
    }

    public override async Task HandleAsync(GetTicketByIdRequest req, CancellationToken ct)
    {
        var result = await _service.ObterPorIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? 200 : 404;
        await SendAsync(result, statusCode, ct);
    }
}

public record UpdateTicketStatusRouteRequest(Guid Id, StatusOcorrencia NovoStatus, string Comentario, string UsuarioId, string UsuarioNome, string? ObservacaoResolucao = null);

/// <summary>
/// Endpoint para transicionar o status de um chamado com registro no histórico de auditoria.
/// </summary>
public sealed class UpdateTicketStatusEndpoint : Endpoint<UpdateTicketStatusRouteRequest, Result<OcorrenciaDto>>
{
    private readonly IOcorrenciaApplicationService _service;

    public UpdateTicketStatusEndpoint(IOcorrenciaApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/operations/tickets/{id}/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Atualizar status do chamado";
            s.Description = "Transiciona o status do chamado (Aberta, EmAndamento, AguardandoPeca, Resolvida, Cancelada) registrando o motivo no histórico.";
        });
    }

    public override async Task HandleAsync(UpdateTicketStatusRouteRequest req, CancellationToken ct)
    {
        var requestPayload = new AtualizarStatusOcorrenciaRequest(req.NovoStatus, req.Comentario, req.UsuarioId, req.UsuarioNome, req.ObservacaoResolucao);
        var result = await _service.AtualizarStatusAsync(req.Id, requestPayload, ct);
        var statusCode = result.IsSuccess ? 200 : (result.Message.Contains("não foi encontrada") ? 404 : 400);
        await SendAsync(result, statusCode, ct);
    }
}

public record AddTicketAttachmentRouteRequest(Guid Id, string Url, string NomeArquivo, string ContentType, long TamanhoBytes, string UploadPorUserId);

/// <summary>
/// Endpoint para adicionar um anexo de foto/documento a um chamado existente.
/// </summary>
public sealed class AddTicketAttachmentEndpoint : Endpoint<AddTicketAttachmentRouteRequest, Result<AnexoOcorrenciaDto>>
{
    private readonly IOcorrenciaApplicationService _service;

    public AddTicketAttachmentEndpoint(IOcorrenciaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/tickets/{id}/attachments");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Adicionar anexo à ocorrência";
            s.Description = "Vincula um novo anexo de imagem ou documento ao chamado existente.";
        });
    }

    public override async Task HandleAsync(AddTicketAttachmentRouteRequest req, CancellationToken ct)
    {
        var payload = new AdicionarAnexoOcorrenciaRequest(req.Url, req.NomeArquivo, req.ContentType, req.TamanhoBytes, req.UploadPorUserId);
        var result = await _service.AdicionarAnexoAsync(req.Id, payload, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}

public record GetTicketSummaryRequest(int CondoId = 1);

/// <summary>
/// Endpoint para obter o resumo consolidado de métricas (KPIs) de chamados do condomínio.
/// </summary>
public sealed class GetTicketSummaryEndpoint : Endpoint<GetTicketSummaryRequest, Result<OcorrenciaSummaryDto>>
{
    private readonly IOcorrenciaApplicationService _service;

    public GetTicketSummaryEndpoint(IOcorrenciaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/tickets/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter resumo de métricas dos chamados";
            s.Description = "Retorna contadores dos chamados (Total, Abertas, Em Andamento, Resolvidas, Urgentes).";
        });
    }

    public override async Task HandleAsync(GetTicketSummaryRequest req, CancellationToken ct)
    {
        var result = await _service.ObterResumoMetricasAsync(req.CondoId, ct);
        await SendAsync(result, 200, ct);
    }
}
