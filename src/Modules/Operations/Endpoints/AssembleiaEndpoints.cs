using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Endpoints;

public sealed class CreateAssembleiaEndpoint : Endpoint<CreateAssembleiaRequest, Result<AssembleiaDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public CreateAssembleiaEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/assemblies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Criar assembleia virtual";
            s.Description = "Cadastra uma assembleia virtual ordinária ou extraordinária com pautas iniciais de votação.";
        });
    }

    public override async Task HandleAsync(CreateAssembleiaRequest req, CancellationToken ct)
    {
        var result = await _service.CriarAssembleiaAsync(req, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record ListAssembleiasRequest(
    int CondoId = 1,
    StatusAssembleia? Status = null,
    TipoAssembleia? Tipo = null);

public sealed class ListAssembleiasEndpoint : Endpoint<ListAssembleiasRequest, Result<IEnumerable<AssembleiaDto>>>
{
    private readonly IAssembleiaApplicationService _service;

    public ListAssembleiasEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/assemblies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar assembleias virtuais";
            s.Description = "Retorna assembleias do condomínio com filtros por status e tipo.";
        });
    }

    public override async Task HandleAsync(ListAssembleiasRequest req, CancellationToken ct)
    {
        var result = await _service.ListarAsync(req.CondoId, req.Status, req.Tipo, ct);
        await SendAsync(result, StatusCodes.Status200OK, ct);
    }
}

public record GetAssembleiaSummaryRequest(int CondoId = 1);

public sealed class GetAssembleiaSummaryEndpoint : Endpoint<GetAssembleiaSummaryRequest, Result<AssembleiaSummaryDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public GetAssembleiaSummaryEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/assemblies/summary");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resumo KPI de assembleias virtuais";
            s.Description = "Retorna quantidade de assembleias ativas, agendadas, encerradas e total de votos registrados.";
        });
    }

    public override async Task HandleAsync(GetAssembleiaSummaryRequest req, CancellationToken ct)
    {
        var result = await _service.ObterResumoKpiAsync(req.CondoId, ct);
        await SendAsync(result, StatusCodes.Status200OK, ct);
    }
}

public record GetAssembleiaByIdRequest(Guid Id);

public sealed class GetAssembleiaByIdEndpoint : Endpoint<GetAssembleiaByIdRequest, Result<AssembleiaDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public GetAssembleiaByIdEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/assemblies/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter assembleia por ID";
            s.Description = "Retorna detalhes da assembleia virtual, pautas e resultados.";
        });
    }

    public override async Task HandleAsync(GetAssembleiaByIdRequest req, CancellationToken ct)
    {
        var result = await _service.ObterPorIdAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
        await SendAsync(result, statusCode, ct);
    }
}

public record UpdateAssembleiaStatusApiRequest(Guid Id, StatusAssembleia NovoStatus);

public sealed class UpdateAssembleiaStatusEndpoint : Endpoint<UpdateAssembleiaStatusApiRequest, Result<AssembleiaDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public UpdateAssembleiaStatusEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Patch("/api/operations/assemblies/{Id}/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Alterar status da assembleia";
            s.Description = "Permite iniciar, cancelar ou encerrar uma assembleia virtual.";
        });
    }

    public override async Task HandleAsync(UpdateAssembleiaStatusApiRequest req, CancellationToken ct)
    {
        var result = await _service.AtualizarStatusAsync(req.Id, req.NovoStatus, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record AddPautaApiRequest(
    Guid AssembleiaId,
    string Titulo,
    TipoVotacao TipoVotacao,
    string? Descricao = null,
    List<string>? OpcoesDisponiveis = null);

public sealed class AddPautaEndpoint : Endpoint<AddPautaApiRequest, Result<AssembleiaDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public AddPautaEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/assemblies/{AssembleiaId}/pautas");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Adicionar pauta à assembleia";
            s.Description = "Inclui uma nova pauta para deliberação e votação na assembleia.";
        });
    }

    public override async Task HandleAsync(AddPautaApiRequest req, CancellationToken ct)
    {
        var input = new CreatePautaInput(req.Titulo, req.TipoVotacao, req.Descricao, req.OpcoesDisponiveis);
        var result = await _service.AdicionarPautaAsync(req.AssembleiaId, input, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record RegistrarVotoApiRequest(
    Guid AssembleiaId,
    Guid PautaId,
    string MoradorUserId,
    string UnidadeId,
    string OpcaoEscolhida,
    double PesoVoto = 1.0);

public sealed class RegistrarVotoEndpoint : Endpoint<RegistrarVotoApiRequest, Result<AssembleiaDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public RegistrarVotoEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/assemblies/{AssembleiaId}/pautas/{PautaId}/vote");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Registrar voto de morador em pauta";
            s.Description = "Computa o voto da unidade habitacional na pauta da assembleia com garantia de unicidade.";
        });
    }

    public override async Task HandleAsync(RegistrarVotoApiRequest req, CancellationToken ct)
    {
        var request = new RegistrarVotoRequest(req.MoradorUserId, req.UnidadeId, req.OpcaoEscolhida, req.PesoVoto);
        var result = await _service.RegistrarVotoAsync(req.AssembleiaId, req.PautaId, request, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record FinalizeAssembleiaRequest(Guid Id);

public sealed class FinalizeAssembleiaEndpoint : Endpoint<FinalizeAssembleiaRequest, Result<AssembleiaDto>>
{
    private readonly IAssembleiaApplicationService _service;

    public FinalizeAssembleiaEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Post("/api/operations/assemblies/{Id}/finalize");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Encerrar assembleia e gerar ata oficial";
            s.Description = "Finaliza as votações das pautas e gera o documento da Ata Oficial da Assembleia.";
        });
    }

    public override async Task HandleAsync(FinalizeAssembleiaRequest req, CancellationToken ct)
    {
        var result = await _service.EncerrarEGerarAtaAsync(req.Id, ct);
        var statusCode = result.IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        await SendAsync(result, statusCode, ct);
    }
}

public record GetAssembleiaAtaRequest(Guid Id);

public sealed class GetAssembleiaAtaEndpoint : Endpoint<GetAssembleiaAtaRequest, Result<string>>
{
    private readonly IAssembleiaApplicationService _service;

    public GetAssembleiaAtaEndpoint(IAssembleiaApplicationService service) => _service = service;

    public override void Configure()
    {
        Get("/api/operations/assemblies/{Id}/ata");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obter Ata Oficial da Assembleia";
            s.Description = "Retorna o documento de texto da Ata Oficial gerada para a assembleia encerrada.";
        });
    }

    public override async Task HandleAsync(GetAssembleiaAtaRequest req, CancellationToken ct)
    {
        var assembleiaResult = await _service.ObterPorIdAsync(req.Id, ct);
        if (!assembleiaResult.IsSuccess || assembleiaResult.Data == null)
        {
            await SendAsync(Result<string>.Failure("Assembleia não encontrada."), StatusCodes.Status404NotFound, ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(assembleiaResult.Data.AtaTexto))
        {
            await SendAsync(Result<string>.Failure("Ata ainda não foi gerada para esta assembleia."), StatusCodes.Status400BadRequest, ct);
            return;
        }

        await SendAsync(Result<string>.Success(assembleiaResult.Data.AtaTexto), StatusCodes.Status200OK, ct);
    }
}
