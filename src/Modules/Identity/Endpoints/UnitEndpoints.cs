using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;
using System.Security.Claims;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Lista blocos do condomínio ativo.
/// </summary>
[Authorize]
public sealed class GetBlocksEndpoint : EndpointWithoutRequest<Result<IReadOnlyList<BlockDto>>>
{
    private readonly IUnitResidentService _service;

    public GetBlocksEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Get("/api/blocks");
        Summary(s => s.Summary = "Listar blocos do condomínio");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetBlocksAsync(ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Cria um novo bloco no condomínio ativo.
/// </summary>
[Authorize]
public sealed class CreateBlockEndpoint : Endpoint<CreateBlockRequestDto, Result<BlockDto>>
{
    private readonly IUnitResidentService _service;

    public CreateBlockEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Post("/api/blocks");
        Summary(s => s.Summary = "Criar bloco");
    }

    public override async Task HandleAsync(CreateBlockRequestDto req, CancellationToken ct)
    {
        var result = await _service.CreateBlockAsync(req, ct);
        await SendAsync(result, result.IsSuccess ? 201 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Lista unidades com filtros de bloco, status, papel e busca textual.
/// </summary>
[Authorize]
public sealed class GetUnitsEndpoint : Endpoint<UnitListQueryDto, Result<IReadOnlyList<UnitListItemDto>>>
{
    private readonly IUnitResidentService _service;

    public GetUnitsEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Get("/api/units");
        Summary(s => s.Summary = "Listar unidades com filtros");
    }

    public override async Task HandleAsync(UnitListQueryDto req, CancellationToken ct)
    {
        var result = await _service.ListUnitsAsync(req, ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Cadastra unidade com morador e vínculo de titularidade.
/// </summary>
[Authorize]
public sealed class CreateUnitEndpoint : Endpoint<CreateUnitRequestDto, Result<UnitCreatedDto>>
{
    private readonly IUnitResidentService _service;

    public CreateUnitEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Post("/api/units");
        Summary(s => s.Summary = "Cadastrar unidade e morador");
    }

    public override async Task HandleAsync(CreateUnitRequestDto req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(SmartCondoClaimTypes.UserId);
        var result = await _service.CreateUnitAsync(req, userId, ct);
        await SendAsync(result, result.IsSuccess ? 201 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Atualiza dados da unidade e morador ativo.
/// </summary>
[Authorize]
public sealed class UpdateUnitEndpoint : Endpoint<UpdateUnitRequestDto, Result<UnitListItemDto>>
{
    private readonly IUnitResidentService _service;

    public UpdateUnitEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Put("/api/units/{unitId}");
        Summary(s => s.Summary = "Atualizar unidade");
    }

    public override async Task HandleAsync(UpdateUnitRequestDto req, CancellationToken ct)
    {
        var unitId = Route<int>("unitId");
        var result = await _service.UpdateUnitAsync(unitId, req, ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Transfere titularidade encerrando vínculo ativo e criando novo registro auditável.
/// </summary>
[Authorize]
public sealed class TransferOwnershipEndpoint : Endpoint<TransferOwnershipRequestDto, Result>
{
    private readonly IUnitResidentService _service;

    public TransferOwnershipEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Post("/api/units/{unitId}/transfer");
        Summary(s => s.Summary = "Transferir titularidade");
    }

    public override async Task HandleAsync(TransferOwnershipRequestDto req, CancellationToken ct)
    {
        var unitId = Route<int>("unitId");
        var userId = User.FindFirstValue(SmartCondoClaimTypes.UserId);
        var result = await _service.TransferOwnershipAsync(unitId, req, userId, ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Retorna histórico cronológico de vínculos da unidade.
/// </summary>
[Authorize]
public sealed class GetUnitHistoryEndpoint : EndpointWithoutRequest<Result<IReadOnlyList<UnitHistoryItemDto>>>
{
    private readonly IUnitResidentService _service;

    public GetUnitHistoryEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Get("/api/units/{unitId}/history");
        Summary(s => s.Summary = "Histórico de alterações da unidade");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var unitId = Route<int>("unitId");
        var result = await _service.GetHistoryAsync(unitId, ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Download do template XLSX para importação em lote.
/// </summary>
[Authorize]
public sealed class GetUnitImportTemplateEndpoint : EndpointWithoutRequest
{
    private readonly IUnitResidentService _service;

    public GetUnitImportTemplateEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Get("/api/units/import/template");
        Summary(s => s.Summary = "Download template de importação");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bytes = await _service.GetImportTemplateAsync(ct);
        await SendBytesAsync(bytes, "zapcond-unidades-template.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", cancellation: ct);
    }
}

/// <summary>
/// Preview de importação em lote com validação linha a linha.
/// </summary>
[Authorize]
public sealed class PreviewUnitImportEndpoint : EndpointWithoutRequest<Result<ImportPreviewResultDto>>
{
    private readonly IUnitResidentService _service;

    public PreviewUnitImportEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Post("/api/units/import/preview");
        AllowFileUploads();
        Summary(s => s.Summary = "Preview de importação em lote");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (Files.Count == 0)
        {
            await SendAsync(Result<ImportPreviewResultDto>.ValidationFailure("Arquivo é obrigatório.", ["Envie um arquivo .xlsx"]), 422, ct);
            return;
        }

        await using var stream = Files[0].OpenReadStream();
        var result = await _service.PreviewImportAsync(stream, ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

/// <summary>
/// Commit de importação em lote persistindo linhas válidas.
/// </summary>
[Authorize]
public sealed class CommitUnitImportEndpoint : Endpoint<ImportCommitRequestDto, Result<ImportCommitResultDto>>
{
    private readonly IUnitResidentService _service;

    public CommitUnitImportEndpoint(IUnitResidentService service) => _service = service;

    public override void Configure()
    {
        Post("/api/units/import/commit");
        Summary(s => s.Summary = "Confirmar importação em lote");
    }

    public override async Task HandleAsync(ImportCommitRequestDto req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(SmartCondoClaimTypes.UserId);
        var result = await _service.CommitImportAsync(req, userId, ct);
        await SendAsync(result, result.IsSuccess ? 200 : UnitEndpointStatus.Map(result), ct);
    }
}

internal static class UnitEndpointStatus
{
    internal static int Map(Result result)
    {
        if (result.Errors.Any() ||
            result.Message.Contains("validação", StringComparison.OrdinalIgnoreCase) ||
            result.Message.Contains("inválido", StringComparison.OrdinalIgnoreCase))
        {
            return 422;
        }

        if (result.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ||
            result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return 404;
        }

        if (result.Message.Contains("já cadastrad", StringComparison.OrdinalIgnoreCase) ||
            result.Message.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
        {
            return 409;
        }

        return 400;
    }
}
