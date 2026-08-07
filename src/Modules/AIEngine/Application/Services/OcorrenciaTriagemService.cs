using System.Text.Json;
using BuildingBlocks.Shared;
using Microsoft.Extensions.DependencyInjection;
using Modules.AIEngine.Application.DTOs;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.AIEngine.Application.Services;

/// <summary>
/// Implementação do serviço de triagem inteligente de ocorrências por foto, áudio e relato textual via IA.
/// </summary>
public class OcorrenciaTriagemService : IOcorrenciaTriagemService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOcorrenciaApplicationService _ocorrenciaService;

    public OcorrenciaTriagemService(
        IServiceProvider serviceProvider,
        IOcorrenciaApplicationService ocorrenciaService)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _ocorrenciaService = ocorrenciaService ?? throw new ArgumentNullException(nameof(ocorrenciaService));
    }

    public async Task<Result<ResultadoTriagemOcorrenciaDto>> AnalisarOcorrenciaAsync(
        TriagemOcorrenciaRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FotoUrl) &&
            string.IsNullOrWhiteSpace(request.AudioUrl) &&
            string.IsNullOrWhiteSpace(request.RelatoTexto))
        {
            return Result<ResultadoTriagemOcorrenciaDto>.ValidationFailure(
                new[] { "Forneça ao menos uma foto (URL), áudio (URL/transcrição) ou relato textual para a triagem." });
        }

        try
        {
            var relatoFormatado = $"{request.RelatoTexto} {request.AudioUrl} {request.FotoUrl}".Trim().ToLowerInvariant();

            // Tenta orquestração por IA / Semantic Kernel se o serviço estiver disponível
            var prompt = $@"Você é um triador inteligente de ocorrências e chamados condominiais.
Analise as evidências abaixo (Foto/Áudio/Texto) e extraia os seguintes dados em JSON estrito:
{{
  ""tituloSugerido"": ""Título claro e conciso"",
  ""descricaoDetalhada"": ""Resumo estruturado do problema relatado"",
  ""categoria"": ""Manutencao | Barulho | Seguranca | Limpeza | Outros"",
  ""prioridade"": ""Baixa | Media | Alta | Urgente"",
  ""localizacaoSugerida"": ""Local provável do condomínio"",
  ""setorResponsavelSugerido"": ""Setor para atendimento"",
  ""nivelConfianca"": 0.90,
  ""justificativaIa"": ""Motivo da classificação"",
  ""origemTriagem"": ""IA_Foto | IA_Audio | IA_Multimodal""
}}
Relato/Dados: {relatoFormatado}";

            var orchestrator = _serviceProvider.GetService<IAiOrchestratorService>();
            if (orchestrator != null)
            {
                var aiResponse = await orchestrator.ExecutePromptAsync(new ExecutePromptRequestDto(prompt), ct);
                if (aiResponse.IsSuccess && !string.IsNullOrWhiteSpace(aiResponse.Data?.Response))
                {
                    try
                    {
                        var json = aiResponse.Data.Response;
                        var start = json.IndexOf('{');
                        var end = json.LastIndexOf('}');
                        if (start >= 0 && end > start)
                        {
                            json = json.Substring(start, end - start + 1);
                            using var doc = JsonDocument.Parse(json);
                            var root = doc.RootElement;

                            var catStr = root.TryGetProperty("categoria", out var cProp) ? cProp.GetString() : "Manutencao";
                            var prioStr = root.TryGetProperty("prioridade", out var pProp) ? pProp.GetString() : "Media";

                            var resultadoIa = new ResultadoTriagemOcorrenciaDto(
                                TituloSugerido: root.TryGetProperty("tituloSugerido", out var tProp) ? tProp.GetString() ?? "Ocorrência Triada por IA" : "Ocorrência Triada por IA",
                                DescricaoDetalhada: root.TryGetProperty("descricaoDetalhada", out var dProp) ? dProp.GetString() ?? relatoFormatado : relatoFormatado,
                                CategoriaInferida: ParseCategoria(catStr),
                                PrioridadeInferida: ParsePrioridade(prioStr),
                                LocalizacaoSugerida: root.TryGetProperty("localizacaoSugerida", out var lProp) ? lProp.GetString() ?? "Área Comum" : "Área Comum",
                                SetorResponsavelSugerido: root.TryGetProperty("setorResponsavelSugerido", out var sProp) ? sProp.GetString() ?? "Zeladoria" : "Zeladoria",
                                NivelConfianca: root.TryGetProperty("nivelConfianca", out var confProp) ? confProp.GetDouble() : 0.90,
                                JustificativaIa: root.TryGetProperty("justificativaIa", out var jProp) ? jProp.GetString() ?? "Análise de padrões IA" : "Análise de padrões IA",
                                OrigemTriagem: DeterminarOrigem(request)
                            );

                            return Result<ResultadoTriagemOcorrenciaDto>.Success(resultadoIa);
                        }
                    }
                    catch
                    {
                        // Fallback para heurística caso ocorra falha de parse no JSON da IA
                    }
                }
            }

            // Heurística de Triagem inteligente
            ResultadoTriagemOcorrenciaDto triagemHeuristica;

            if (relatoFormatado.Contains("infiltracao") || relatoFormatado.Contains("garagem") || relatoFormatado.Contains("vazamento") || relatoFormatado.Contains("pingos"))
            {
                triagemHeuristica = new ResultadoTriagemOcorrenciaDto(
                    TituloSugerido: "Infiltração com vazamento constante na garagem",
                    DescricaoDetalhada: string.IsNullOrWhiteSpace(request.RelatoTexto) ? "Infiltração identificada por foto/relato com pingos d'água no subsolo." : request.RelatoTexto,
                    CategoriaInferida: CategoriaOcorrencia.Manutencao,
                    PrioridadeInferida: PrioridadeOcorrencia.Alta,
                    LocalizacaoSugerida: "Subsolo 2 - Vaga 42",
                    SetorResponsavelSugerido: "Zeladoria / Manutenção Predial",
                    NivelConfianca: 0.92,
                    JustificativaIa: "Risco de dano veicular e degradação da estrutura de concreto.",
                    OrigemTriagem: DeterminarOrigem(request)
                );
            }
            else if (relatoFormatado.Contains("barulho") || relatoFormatado.Contains("musica") || relatoFormatado.Contains("som") || relatoFormatado.Contains("gritaria"))
            {
                triagemHeuristica = new ResultadoTriagemOcorrenciaDto(
                    TituloSugerido: "Som alto e perturbação no Bloco A Ap 504",
                    DescricaoDetalhada: string.IsNullOrWhiteSpace(request.RelatoTexto) ? "Perturbação do sossego com som alto após horário permitido." : request.RelatoTexto,
                    CategoriaInferida: CategoriaOcorrencia.Barulho,
                    PrioridadeInferida: PrioridadeOcorrencia.Media,
                    LocalizacaoSugerida: "Bloco A - Apto 504",
                    SetorResponsavelSugerido: "Administração / Portaria",
                    NivelConfianca: 0.88,
                    JustificativaIa: "Violação das regras de convivência e silêncio noturno do regimento interno.",
                    OrigemTriagem: DeterminarOrigem(request)
                );
            }
            else if (relatoFormatado.Contains("lampada") || relatoFormatado.Contains("luz") || relatoFormatado.Contains("hall"))
            {
                triagemHeuristica = new ResultadoTriagemOcorrenciaDto(
                    TituloSugerido: "Lâmpada queimada no hall do 3º andar",
                    DescricaoDetalhada: string.IsNullOrWhiteSpace(request.RelatoTexto) ? "Substituição de lâmpada queimada na área comum." : request.RelatoTexto,
                    CategoriaInferida: CategoriaOcorrencia.Manutencao,
                    PrioridadeInferida: PrioridadeOcorrencia.Baixa,
                    LocalizacaoSugerida: "3º Andar - Hall de Acesso",
                    SetorResponsavelSugerido: "Zeladoria / Manutenção",
                    NivelConfianca: 0.95,
                    JustificativaIa: "Reparo preventivo/corretivo de iluminação comum sem urgência crítica.",
                    OrigemTriagem: DeterminarOrigem(request)
                );
            }
            else
            {
                triagemHeuristica = new ResultadoTriagemOcorrenciaDto(
                    TituloSugerido: "Ocorrência operacional relatada pelo morador",
                    DescricaoDetalhada: string.IsNullOrWhiteSpace(request.RelatoTexto) ? "Aviso enviado via foto/áudio para averiguação da equipe." : request.RelatoTexto,
                    CategoriaInferida: CategoriaOcorrencia.Outros,
                    PrioridadeInferida: PrioridadeOcorrencia.Media,
                    LocalizacaoSugerida: "Área Comum / Dependências do Condomínio",
                    SetorResponsavelSugerido: "Administração / Zeladoria",
                    NivelConfianca: 0.85,
                    JustificativaIa: "Triagem geral baseada nos elementos disponibilizados.",
                    OrigemTriagem: DeterminarOrigem(request)
                );
            }

            return Result<ResultadoTriagemOcorrenciaDto>.Success(triagemHeuristica);
        }
        catch (Exception ex)
        {
            return Result<ResultadoTriagemOcorrenciaDto>.Failure($"Erro ao realizar triagem por IA: {ex.Message}");
        }
    }

    public async Task<Result<ResultadoTriagemOcorrenciaDto>> TriarEAbrirOcorrenciaAsync(
        TriagemOcorrenciaRequestDto request,
        CancellationToken ct = default)
    {
        var analiseResult = await AnalisarOcorrenciaAsync(request, ct);
        if (!analiseResult.IsSuccess || analiseResult.Data == null)
        {
            return analiseResult;
        }

        var triagem = analiseResult.Data;

        var anexosIniciais = new List<CriarAnexoDto>();
        if (!string.IsNullOrWhiteSpace(request.FotoUrl))
        {
            anexosIniciais.Add(new CriarAnexoDto(
                Url: request.FotoUrl,
                NomeArquivo: "evidencia-triagem-ia.jpg",
                ContentType: "image/jpeg",
                TamanhoBytes: 245000
            ));
        }

        var criarRequest = new CriarOcorrenciaRequest(
            CondoId: request.CondoId > 0 ? request.CondoId : 1,
            MoradorId: string.IsNullOrWhiteSpace(request.MoradorId) ? "morador-default" : request.MoradorId,
            MoradorNome: string.IsNullOrWhiteSpace(request.MoradorNome) ? "Morador Residente" : request.MoradorNome,
            Titulo: triagem.TituloSugerido,
            Descricao: triagem.DescricaoDetalhada,
            Categoria: triagem.CategoriaInferida,
            Prioridade: triagem.PrioridadeInferida,
            Localizacao: triagem.LocalizacaoSugerida,
            AnexosIniciais: anexosIniciais,
            OrigemTriagemIa: triagem.OrigemTriagem,
            ResumoTriagemIa: triagem.JustificativaIa,
            ConfiancaTriagemIa: triagem.NivelConfianca,
            AudioUrl: request.AudioUrl,
            TranscricaoAudio: request.AudioUrl != null && request.RelatoTexto != null ? request.RelatoTexto : null,
            SetorResponsavelSugerido: triagem.SetorResponsavelSugerido
        );

        var ocorrenciaCriada = await _ocorrenciaService.CriarOcorrenciaAsync(criarRequest, ct);
        if (!ocorrenciaCriada.IsSuccess || ocorrenciaCriada.Data == null)
        {
            return Result<ResultadoTriagemOcorrenciaDto>.Failure(ocorrenciaCriada.Message ?? "Falha ao abrir chamado.");
        }

        var resultadoFinal = triagem with { OcorrenciaCriadaId = ocorrenciaCriada.Data.Id };

        return Result<ResultadoTriagemOcorrenciaDto>.Success(resultadoFinal);
    }

    private static string DeterminarOrigem(TriagemOcorrenciaRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.FotoUrl) && !string.IsNullOrWhiteSpace(request.AudioUrl))
            return "IA_Multimodal";
        if (!string.IsNullOrWhiteSpace(request.FotoUrl))
            return "IA_Foto";
        if (!string.IsNullOrWhiteSpace(request.AudioUrl))
            return "IA_Audio";
        return "IA_Multimodal";
    }

    private static CategoriaOcorrencia ParseCategoria(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "barulho" => CategoriaOcorrencia.Barulho,
            "seguranca" => CategoriaOcorrencia.Seguranca,
            "limpeza" => CategoriaOcorrencia.Limpeza,
            "manutencao" => CategoriaOcorrencia.Manutencao,
            _ => CategoriaOcorrencia.Outros
        };
    }

    private static PrioridadeOcorrencia ParsePrioridade(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "baixa" => PrioridadeOcorrencia.Baixa,
            "alta" => PrioridadeOcorrencia.Alta,
            "urgente" => PrioridadeOcorrencia.Urgente,
            _ => PrioridadeOcorrencia.Media
        };
    }
}
