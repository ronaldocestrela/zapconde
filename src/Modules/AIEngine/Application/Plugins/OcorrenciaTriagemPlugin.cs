using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Modules.AIEngine.Application.Services;
using Modules.Operations.Application.DTOs;

namespace Modules.AIEngine.Application.Plugins;

/// <summary>
/// Plugin do Semantic Kernel para Function Calling de Triagem Inteligente de Ocorrências (Foto/Áudio/Multimodal).
/// </summary>
public class OcorrenciaTriagemPlugin
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private readonly IOcorrenciaTriagemService _triagemService;

    public OcorrenciaTriagemPlugin(IOcorrenciaTriagemService triagemService)
    {
        _triagemService = triagemService ?? throw new ArgumentNullException(nameof(triagemService));
    }

    [KernelFunction("TriarEAbrirOcorrencia")]
    [Description("Realiza a triagem inteligente de ocorrência condominial enviada por foto, áudio ou relato, categoriza o problema, calcula a prioridade, sugere o setor responsável e realiza a abertura automática do chamado no módulo de Operações.")]
    public async Task<string> TriarEAbrirOcorrenciaAsync(
        [Description("URL ou Base64 da imagem/foto da evidência do problema (opcional)")] string? fotoUrl = null,
        [Description("URL do arquivo de áudio ou transcrição (opcional)")] string? audioUrl = null,
        [Description("Relato livre em texto sobre o problema no condomínio")] string? relatoTexto = null,
        [Description("ID do morador que está reportando")] string moradorId = "morador-default",
        [Description("Nome do morador que está reportando")] string moradorNome = "Morador Residente",
        [Description("ID do condomínio (Padrão: 1)")] int condoId = 1,
        CancellationToken ct = default)
    {
        var request = new TriagemOcorrenciaRequestDto(
            FotoUrl: fotoUrl,
            AudioUrl: audioUrl,
            RelatoTexto: relatoTexto,
            MoradorId: moradorId,
            MoradorNome: moradorNome,
            CondoId: condoId
        );

        var result = await _triagemService.TriarEAbrirOcorrenciaAsync(request, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? "Falha ao realizar triagem e abertura da ocorrência por IA."
            }, JsonOptions);
        }

        var d = result.Data;
        return JsonSerializer.Serialize(new
        {
            sucesso = true,
            mensagem = "Ocorrência triada pela IA e chamado aberto com sucesso!",
            ocorrenciaId = d.OcorrenciaCriadaId,
            titulo = d.TituloSugerido,
            descricao = d.DescricaoDetalhada,
            categoria = d.CategoriaInferida.ToString(),
            prioridade = d.PrioridadeInferida.ToString(),
            localizacao = d.LocalizacaoSugerida,
            setorResponsavel = d.SetorResponsavelSugerido,
            nivelConfianca = d.NivelConfianca,
            justificativaIa = d.JustificativaIa,
            origemTriagem = d.OrigemTriagem
        }, JsonOptions);
    }

    [KernelFunction("AnalisarOcorrenciaMultimodal")]
    [Description("Analisador prévio multimodal de foto/áudio/relato de ocorrência condominial. Retorna a classificação inferida sem salvar no banco de dados.")]
    public async Task<string> AnalisarOcorrenciaMultimodalAsync(
        [Description("URL ou Base64 da imagem/foto da evidência do problema")] string? fotoUrl = null,
        [Description("URL do arquivo de áudio ou transcrição")] string? audioUrl = null,
        [Description("Relato livre em texto sobre o problema")] string? relatoTexto = null,
        [Description("ID do condomínio (Padrão: 1)")] int condoId = 1,
        CancellationToken ct = default)
    {
        var request = new TriagemOcorrenciaRequestDto(
            FotoUrl: fotoUrl,
            AudioUrl: audioUrl,
            RelatoTexto: relatoTexto,
            CondoId: condoId
        );

        var result = await _triagemService.AnalisarOcorrenciaAsync(request, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? "Falha na análise prévia de triagem."
            }, JsonOptions);
        }

        var d = result.Data;
        return JsonSerializer.Serialize(new
        {
            sucesso = true,
            mensagem = "Análise prévia de triagem concluída.",
            tituloSugerido = d.TituloSugerido,
            descricaoDetalhada = d.DescricaoDetalhada,
            categoriaInferida = d.CategoriaInferida.ToString(),
            prioridadeInferida = d.PrioridadeInferida.ToString(),
            localizacaoSugerida = d.LocalizacaoSugerida,
            setorResponsavelSugerido = d.SetorResponsavelSugerido,
            nivelConfianca = d.NivelConfianca,
            justificativaIa = d.JustificativaIa,
            origemTriagem = d.OrigemTriagem
        }, JsonOptions);
    }
}
