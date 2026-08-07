using BuildingBlocks.Shared;
using Modules.AccessControl.Application.DTOs;
using Modules.AIEngine.Application.DTOs;

namespace Modules.AIEngine.Application.Services;

/// <summary>
/// Contrato do serviço de visão computacional multimodal e OCR para etiquetas de encomendas.
/// </summary>
public interface IPackageVisionOcrService
{
    /// <summary>
    /// Extrai metadados estruturados de uma imagem de etiqueta de encomenda.
    /// </summary>
    Task<Result<PackageLabelExtractionResultDto>> ProcessLabelImageAsync(
        string? base64Image,
        string? imageUrl,
        int condoId,
        CancellationToken ct = default);

    /// <summary>
    /// Extrai dados da etiqueta, registra a encomenda na portaria e envia notificação ao morador.
    /// </summary>
    Task<Result<EncomendaDto>> ProcessLabelAndRegisterAsync(
        string? base64Image,
        string? imageUrl,
        int condoId,
        bool enviarNotificacao,
        string recebidoPorNome,
        CancellationToken ct = default);
}
