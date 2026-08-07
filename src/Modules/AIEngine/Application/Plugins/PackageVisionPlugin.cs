using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Modules.AIEngine.Application.Services;

namespace Modules.AIEngine.Application.Plugins;

/// <summary>
/// Plugin do Semantic Kernel para Function Calling de Visão Computacional / OCR de etiquetas de encomenda.
/// </summary>
public class PackageVisionPlugin
{
    private readonly IPackageVisionOcrService _visionOcrService;

    public PackageVisionPlugin(IPackageVisionOcrService visionOcrService)
    {
        _visionOcrService = visionOcrService ?? throw new ArgumentNullException(nameof(visionOcrService));
    }

    [KernelFunction("ReadPackageLabel")]
    [Description("Lê e analisa a foto de uma etiqueta de pacote/encomenda capturada na portaria, extraindo morador destinatário, unidade/bloco, código de rastreio e transportadora.")]
    public async Task<string> ReadPackageLabelAsync(
        [Description("Conteúdo da imagem da etiqueta em formato Base64 ou URL da imagem")] string imagemEtiqueta,
        [Description("ID do condomínio (Padrão: 1)")] int condoId = 1,
        CancellationToken ct = default)
    {
        var isUrl = imagemEtiqueta.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || imagemEtiqueta.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        var base64 = isUrl ? null : imagemEtiqueta;
        var url = isUrl ? imagemEtiqueta : null;

        var result = await _visionOcrService.ProcessLabelImageAsync(base64, url, condoId, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? "Falha na leitura da etiqueta da encomenda."
            });
        }

        var d = result.Data;
        return JsonSerializer.Serialize(new
        {
            sucesso = true,
            mensagem = d.Mensagem,
            destinatario = d.NomeDestinatario,
            blocoUnidade = d.BlocoUnidade,
            codigoRastreio = d.CodigoRastreio,
            transportadora = d.Transportadora,
            remetente = d.Remetente,
            tipoSugerido = d.TipoSugerido.ToString(),
            confiancaPercentual = d.ConfiancaPercentual,
            unidadeId = d.UnidadeIdIdentificada
        });
    }

    [KernelFunction("ReadPackageLabelAndNotify")]
    [Description("Lê a foto de uma etiqueta de encomenda, realiza o registro no sistema da portaria e notifica o morador via WhatsApp automaticamente.")]
    public async Task<string> ReadPackageLabelAndNotifyAsync(
        [Description("Foto da etiqueta em Base64 ou URL")] string imagemEtiqueta,
        [Description("Indica se deve disparar a notificação por WhatsApp imediatamente (Padrão: true)")] bool enviarNotificacao = true,
        [Description("Nome do operador ou portaria responsável")] string recebidoPorNome = "Portaria IA (Vision OCR)",
        [Description("ID do condomínio")] int condoId = 1,
        CancellationToken ct = default)
    {
        var isUrl = imagemEtiqueta.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || imagemEtiqueta.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        var base64 = isUrl ? null : imagemEtiqueta;
        var url = isUrl ? imagemEtiqueta : null;

        var result = await _visionOcrService.ProcessLabelAndRegisterAsync(base64, url, condoId, enviarNotificacao, recebidoPorNome, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? "Não foi possível registrar a encomenda via visão computacional."
            });
        }

        var enc = result.Data;
        return JsonSerializer.Serialize(new
        {
            sucesso = true,
            mensagem = $"Encomenda registrada com sucesso! ID: {enc.Id}, Unidade: {enc.BlocoUnidade}, Rastreio: {enc.CodigoRastreio}.",
            encomendaId = enc.Id,
            blocoUnidade = enc.BlocoUnidade,
            codigoRastreio = enc.CodigoRastreio,
            transportadora = enc.Transportadora,
            status = enc.StatusDescricao,
            notificadoEm = enc.NotificadoEm,
            notificacaoMoradorEnviada = enc.NotificadoEm.HasValue
        });
    }
}
