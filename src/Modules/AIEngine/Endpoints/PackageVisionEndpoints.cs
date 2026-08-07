using System.Text.Json;
using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.AccessControl.Application.DTOs;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Application.Plugins;
using Modules.AIEngine.Application.Services;

namespace Modules.AIEngine.Endpoints;

/// <summary>
/// Endpoint para extração de metadados de fotos de etiquetas de encomendas via Visão/OCR.
/// </summary>
public sealed class ProcessPackageLabelEndpoint : Endpoint<ProcessPackageLabelRequest, Result<PackageLabelExtractionResultDto>>
{
    private readonly IPackageVisionOcrService _visionService;

    public ProcessPackageLabelEndpoint(IPackageVisionOcrService visionService)
    {
        _visionService = visionService;
    }

    public override void Configure()
    {
        Post("/api/ai/vision/package-label/process");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Extração OCR / Visão Computacional de Etiqueta de Encomenda";
            s.Description = "Analisa a foto da etiqueta (Base64 ou URL) e extrai destinatário, unidade/bloco, código de rastreio, transportadora e confiança.";
        });
    }

    public override async Task HandleAsync(ProcessPackageLabelRequest req, CancellationToken ct)
    {
        var res = await _visionService.ProcessLabelImageAsync(req.Base64Image, req.ImageUrl, req.CondoId, ct);
        var httpStatus = res.IsSuccess ? 200 : 400;
        await SendAsync(res, httpStatus, ct);
    }
}

/// <summary>
/// Endpoint para extração, registro da encomenda na portaria e notificação ao morador.
/// </summary>
public sealed class ProcessAndRegisterPackageLabelEndpoint : Endpoint<ProcessPackageLabelRequest, Result<EncomendaDto>>
{
    private readonly IPackageVisionOcrService _visionService;

    public ProcessAndRegisterPackageLabelEndpoint(IPackageVisionOcrService visionService)
    {
        _visionService = visionService;
    }

    public override void Configure()
    {
        Post("/api/ai/vision/package-label/process-and-register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Processar Etiqueta, Registrar Encomenda e Notificar Morador";
            s.Description = "Realiza o OCR da etiqueta, salva o recebimento da encomenda na portaria e dispara a notificação no WhatsApp.";
        });
    }

    public override async Task HandleAsync(ProcessPackageLabelRequest req, CancellationToken ct)
    {
        var res = await _visionService.ProcessLabelAndRegisterAsync(
            req.Base64Image,
            req.ImageUrl,
            req.CondoId,
            req.EnviarNotificacaoMorador,
            req.RecebidoPorNome,
            ct);

        var httpStatus = res.IsSuccess ? 200 : 400;
        await SendAsync(res, httpStatus, ct);
    }
}

public record ExecutePackageVisionPluginRequest(
    string ImagemEtiqueta,
    bool EnviarNotificacao = true,
    string RecebidoPorNome = "Portaria IA (Vision OCR)",
    int CondoId = 1);

public record PackageVisionPluginExecutionResultDto(
    bool Sucesso,
    string Mensagem,
    int? EncomendaId,
    string BlocoUnidade,
    string CodigoRastreio,
    string Transportadora,
    string Status,
    bool NotificacaoMoradorEnviada,
    string MensagemFormatadaIa);

/// <summary>
/// Endpoint para simular a invocação da tool ReadPackageLabelAndNotify do PackageVisionPlugin (Semantic Kernel Function Calling).
/// </summary>
public sealed class ExecutePackageVisionPluginEndpoint : Endpoint<ExecutePackageVisionPluginRequest, Result<PackageVisionPluginExecutionResultDto>>
{
    private readonly PackageVisionPlugin _plugin;

    public ExecutePackageVisionPluginEndpoint(PackageVisionPlugin plugin)
    {
        _plugin = plugin;
    }

    public override void Configure()
    {
        Post("/api/ai/plugins/packages/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Executar Function Calling do PackageVisionPlugin (ReadPackageLabelAndNotify)";
            s.Description = "Invoca a tool ReadPackageLabelAndNotify do Semantic Kernel simulando a leitura e notificação de encomenda por IA.";
        });
    }

    public override async Task HandleAsync(ExecutePackageVisionPluginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ImagemEtiqueta))
        {
            await SendAsync(Result<PackageVisionPluginExecutionResultDto>.ValidationFailure(new[] { "Imagem da etiqueta (Base64 ou URL) é obrigatória." }), 400, ct);
            return;
        }

        var jsonResponse = await _plugin.ReadPackageLabelAndNotifyAsync(
            req.ImagemEtiqueta,
            req.EnviarNotificacao,
            req.RecebidoPorNome,
            req.CondoId,
            ct);

        using var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;
        var sucesso = root.TryGetProperty("sucesso", out var sucProp) && sucProp.GetBoolean();
        var mensagem = root.TryGetProperty("mensagem", out var msgProp) ? msgProp.GetString() ?? string.Empty : string.Empty;

        int? encomendaId = null;
        string blocoUnidade = "Bloco A - Apto 102";
        string codigoRastreio = "TRK-000000";
        string transportadora = "Mercado Livre";
        string status = "AguardandoRetirada";
        bool notificado = false;

        if (sucesso)
        {
            if (root.TryGetProperty("encomendaId", out var encProp)) encomendaId = encProp.GetInt32();
            if (root.TryGetProperty("blocoUnidade", out var buProp)) blocoUnidade = buProp.GetString() ?? blocoUnidade;
            if (root.TryGetProperty("codigoRastreio", out var codProp)) codigoRastreio = codProp.GetString() ?? codigoRastreio;
            if (root.TryGetProperty("transportadora", out var transpProp)) transportadora = transpProp.GetString() ?? transportadora;
            if (root.TryGetProperty("status", out var stProp)) status = stProp.GetString() ?? status;
            if (root.TryGetProperty("notificacaoMoradorEnviada", out var notifProp)) notificado = notifProp.GetBoolean();
        }

        var mensagemFormatadaIa = sucesso
            ? $"Olá! A foto da etiqueta foi processada com sucesso via OCR/IA. Encomenda #{encomendaId} ({transportadora} - Rastreio: {codigoRastreio}) foi registrada para a {blocoUnidade}. Status: {status}. {(notificado ? "Notificação enviada ao morador via WhatsApp!" : "")}"
            : $"Não foi possível processar a etiqueta. Motivo: {mensagem}";

        var dto = new PackageVisionPluginExecutionResultDto(
            Sucesso: sucesso,
            Mensagem: mensagem,
            EncomendaId: encomendaId,
            BlocoUnidade: blocoUnidade,
            CodigoRastreio: codigoRastreio,
            Transportadora: transportadora,
            Status: status,
            NotificacaoMoradorEnviada: notificado,
            MensagemFormatadaIa: mensagemFormatadaIa);

        var httpStatus = sucesso ? 200 : 400;
        await SendAsync(Result<PackageVisionPluginExecutionResultDto>.Success(dto, mensagem), httpStatus, ct);
    }
}
