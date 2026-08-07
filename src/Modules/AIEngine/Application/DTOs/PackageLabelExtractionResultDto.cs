using Modules.AccessControl.Domain.Enums;

namespace Modules.AIEngine.Application.DTOs;

/// <summary>
/// DTO com resultado do processamento multimodal de visão / OCR de etiquetas de encomenda.
/// </summary>
public record PackageLabelExtractionResultDto(
    bool Sucesso,
    string Mensagem,
    string? NomeDestinatario,
    string? BlocoUnidade,
    string? CodigoRastreio,
    string? Transportadora,
    string? Remetente,
    TipoEncomenda TipoSugerido,
    double ConfiancaPercentual,
    int? UnidadeIdIdentificada,
    string? MoradorIdentificadoNome,
    string? FotoEtiquetaUrl,
    bool NotificacaoEnviada,
    string? CamposDetectadosJson);

public record ProcessPackageLabelRequest(
    string? Base64Image,
    string? ImageUrl,
    int CondoId = 1,
    bool EnviarNotificacaoMorador = true,
    string RecebidoPorNome = "Portaria IA (Vision OCR)");
