using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AIEngine.Application.Plugins;
using Modules.AIEngine.Application.Services;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace Modules.AIEngine.Endpoints;

public record ExecuteOcorrenciaPluginRequest(
    string? FotoUrl = null,
    string? AudioUrl = null,
    string? RelatoTexto = null,
    string MoradorId = "morador-default",
    string MoradorNome = "Morador Residente",
    int CondoId = 1
);

/// <summary>
/// Endpoint HTTP para análise prévia de triagem de ocorrências via foto, áudio ou texto.
/// </summary>
public sealed class AnalisarTriagemOcorrenciaEndpoint : Endpoint<TriagemOcorrenciaRequestDto, Result<ResultadoTriagemOcorrenciaDto>>
{
    private readonly IOcorrenciaTriagemService _triagemService;

    public AnalisarTriagemOcorrenciaEndpoint(IOcorrenciaTriagemService triagemService)
    {
        _triagemService = triagemService ?? throw new ArgumentNullException(nameof(triagemService));
    }

    public override void Configure()
    {
        Post("/api/ai/triagem-ocorrencia/analisar");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Análise prévia de triagem inteligente de ocorrência (IA)";
            s.Description = "Analisa a foto, áudio ou relato textual e infere a categoria, prioridade, título e setor sem persistir o chamado.";
        });
    }

    public override async Task HandleAsync(TriagemOcorrenciaRequestDto req, CancellationToken ct)
    {
        var result = await _triagemService.AnalisarOcorrenciaAsync(req, ct);
        if (!result.IsSuccess)
        {
            await SendAsync(result, 400, ct);
            return;
        }

        await SendAsync(result, 200, ct);
    }
}

/// <summary>
/// Endpoint HTTP para triagem de ocorrência e abertura automática de chamado no módulo de Operações.
/// </summary>
public sealed class ProcessarEAbrirTriagemOcorrenciaEndpoint : Endpoint<TriagemOcorrenciaRequestDto, Result<ResultadoTriagemOcorrenciaDto>>
{
    private readonly IOcorrenciaTriagemService _triagemService;

    public ProcessarEAbrirTriagemOcorrenciaEndpoint(IOcorrenciaTriagemService triagemService)
    {
        _triagemService = triagemService ?? throw new ArgumentNullException(nameof(triagemService));
    }

    public override void Configure()
    {
        Post("/api/ai/triagem-ocorrencia/processar-e-abrir");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Triagem por IA e abertura automática de chamado de ocorrência";
            s.Description = "Analisa foto/áudio/relato, executa a triagem por IA e realiza o cadastro automático do chamado no módulo de Operações.";
        });
    }

    public override async Task HandleAsync(TriagemOcorrenciaRequestDto req, CancellationToken ct)
    {
        var result = await _triagemService.TriarEAbrirOcorrenciaAsync(req, ct);
        if (!result.IsSuccess)
        {
            await SendAsync(result, 400, ct);
            return;
        }

        await SendAsync(result, 201, ct);
    }
}

/// <summary>
/// Endpoint HTTP para simulação interativa de Function Calling da tool TriarEAbrirOcorrencia do OcorrenciaTriagemPlugin.
/// </summary>
public sealed class ExecuteOcorrenciaPluginEndpoint : Endpoint<ExecuteOcorrenciaPluginRequest, Result<ResultadoTriagemOcorrenciaDto>>
{
    private readonly OcorrenciaTriagemPlugin _plugin;

    public ExecuteOcorrenciaPluginEndpoint(OcorrenciaTriagemPlugin plugin)
    {
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
    }

    public override void Configure()
    {
        Post("/api/ai/plugins/ocorrencia/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Executar Function Calling do OcorrenciaTriagemPlugin (TriarEAbrirOcorrencia)";
            s.Description = "Simula a invocação da tool TriarEAbrirOcorrencia do Semantic Kernel para triagem e abertura de chamado.";
        });
    }

    public override async Task HandleAsync(ExecuteOcorrenciaPluginRequest req, CancellationToken ct)
    {
        var requestDto = new TriagemOcorrenciaRequestDto(
            FotoUrl: req.FotoUrl,
            AudioUrl: req.AudioUrl,
            RelatoTexto: req.RelatoTexto,
            MoradorId: req.MoradorId,
            MoradorNome: req.MoradorNome,
            CondoId: req.CondoId
        );

        var json = await _plugin.TriarEAbrirOcorrenciaAsync(
            fotoUrl: req.FotoUrl,
            audioUrl: req.AudioUrl,
            relatoTexto: req.RelatoTexto,
            moradorId: req.MoradorId,
            moradorNome: req.MoradorNome,
            condoId: req.CondoId,
            ct: ct);

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            var sucesso = root.TryGetProperty("sucesso", out var sProp) && sProp.GetBoolean();

            if (!sucesso)
            {
                var msg = root.TryGetProperty("mensagem", out var mProp) ? mProp.GetString() : "Falha na execução do plugin.";
                await SendAsync(Result<ResultadoTriagemOcorrenciaDto>.Failure(msg ?? "Erro no plugin."), 400, ct);
                return;
            }

            Guid? occId = root.TryGetProperty("ocorrenciaId", out var oProp) && oProp.ValueKind != System.Text.Json.JsonValueKind.Null
                ? Guid.Parse(oProp.GetString()!)
                : null;

            var dto = new ResultadoTriagemOcorrenciaDto(
                TituloSugerido: root.GetProperty("titulo").GetString() ?? "Ocorrência",
                DescricaoDetalhada: root.GetProperty("descricao").GetString() ?? "",
                CategoriaInferida: Enum.Parse<CategoriaOcorrencia>(root.GetProperty("categoria").GetString() ?? "Manutencao"),
                PrioridadeInferida: Enum.Parse<PrioridadeOcorrencia>(root.GetProperty("prioridade").GetString() ?? "Media"),
                LocalizacaoSugerida: root.GetProperty("localizacao").GetString() ?? "",
                SetorResponsavelSugerido: root.GetProperty("setorResponsavel").GetString() ?? "",
                NivelConfianca: root.GetProperty("nivelConfianca").GetDouble(),
                JustificativaIa: root.GetProperty("justificativaIa").GetString() ?? "",
                OrigemTriagem: root.GetProperty("origemTriagem").GetString() ?? "IA_Multimodal",
                OcorrenciaCriadaId: occId
            );

            await SendAsync(Result<ResultadoTriagemOcorrenciaDto>.Success(dto), 200, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(Result<ResultadoTriagemOcorrenciaDto>.Failure($"Erro ao interpretar resposta do plugin: {ex.Message}"), 500, ct);
        }
    }
}
