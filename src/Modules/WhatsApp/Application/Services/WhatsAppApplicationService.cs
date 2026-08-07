using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.WhatsApp.Application.DTOs;
using Modules.WhatsApp.Domain.Entities;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Domain.Exceptions;
using Modules.WhatsApp.Infrastructure.Persistence;

namespace Modules.WhatsApp.Application.Services;

public class WhatsAppApplicationService : IWhatsAppApplicationService
{
    private readonly WhatsAppDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IEvolutionPayloadParser _parser;
    private readonly ILogger<WhatsAppApplicationService> _logger;

    public WhatsAppApplicationService(
        WhatsAppDbContext dbContext,
        ICurrentTenantService currentTenantService,
        IEvolutionPayloadParser parser,
        ILogger<WhatsAppApplicationService> logger)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _parser = parser;
        _logger = logger;
    }

    public async Task<Result<WebhookIngestionResultDto>> IngestEvolutionWebhookAsync(
        string rawJson,
        string? headerApiKey = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return Result<WebhookIngestionResultDto>.ValidationFailure(
                "Payload inválido", new[] { "O corpo da requisição do webhook não pode ser vazio." });
        }

        var parsed = _parser.Parse(rawJson);
        if (parsed == null)
        {
            _logger.LogWarning("Falha ao efetuar parse do payload da Evolution API.");
            return Result<WebhookIngestionResultDto>.Failure("Payload malformado ou não reconhecido pela Evolution API.");
        }

        // Tentar resolver a instância cadastrada no banco sem filtro de tenant inicial para identificar o condomínio
        var instanceConfig = await _dbContext.InstanceConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.InstanceName == parsed.InstanceName, ct);

        int tenantId = _currentTenantService.TenantId ?? 1;
        int condoId = instanceConfig?.CondoId ?? _currentTenantService.CondoId ?? 1;

        if (instanceConfig != null)
        {
            tenantId = instanceConfig.TenantId;
            condoId = instanceConfig.CondoId;

            // Se a instância exige ApiKey e foi informada no header ou payload, validar
            if (!string.IsNullOrWhiteSpace(instanceConfig.ApiKey))
            {
                var keyProvided = headerApiKey ?? ExtractApiKeyFromRawJson(rawJson);
                if (!string.IsNullOrWhiteSpace(keyProvided) && keyProvided != instanceConfig.ApiKey)
                {
                    _logger.LogWarning("Rejeitando webhook para instância {Instance}: API Key incompatível.", parsed.InstanceName);
                    return Result<WebhookIngestionResultDto>.Failure("Chave de API do Webhook inválida.");
                }
            }
        }
        else
        {
            // Se tenantId for 0 ou não definido na request, usar fallback 1
            if (tenantId <= 0) tenantId = 1;
        }

        // Checar Idempotência (se a mensagem já foi gravada anteriormente para esta instância)
        var existingLog = await _dbContext.WebhookLogs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.MessageId == parsed.MessageId, ct);

        if (existingLog != null)
        {
            _logger.LogInformation("Mensagem {MessageId} ignorada por duplicidade (Idempotência).", parsed.MessageId);
            return Result<WebhookIngestionResultDto>.Success(
                new WebhookIngestionResultDto(
                    IsSuccess: true,
                    IsDuplicate: true,
                    WebhookLogId: existingLog.Id,
                    Message: "Mensagem ignorada por duplicidade (Idempotência)"
                ),
                "Payload já processado anteriormente"
            );
        }

        try
        {
            var log = WhatsAppWebhookLog.Registrar(
                tenantId: tenantId,
                condoId: condoId,
                instanceName: parsed.InstanceName,
                provider: parsed.Provider,
                messageId: parsed.MessageId,
                senderPhone: parsed.SenderPhone,
                pushName: parsed.PushName,
                messageType: parsed.MessageType,
                messageText: parsed.MessageText,
                mediaUrl: parsed.MediaUrl,
                rawPayloadJson: rawJson
            );

            _dbContext.WebhookLogs.Add(log);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Webhook registrado com sucesso! ID: {LogId}, Remetente: {Phone}, Instância: {Instance}",
                log.Id, log.SenderPhone, log.InstanceName);

            return Result<WebhookIngestionResultDto>.Success(
                new WebhookIngestionResultDto(
                    IsSuccess: true,
                    IsDuplicate: false,
                    WebhookLogId: log.Id,
                    Message: "Webhook recebido e registrado com sucesso"
                ),
                "Webhook processado com sucesso"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao registrar webhook da Evolution API.");
            return Result<WebhookIngestionResultDto>.Failure($"Erro ao persistir log de webhook: {ex.Message}");
        }
    }

    public async Task<Result<WhatsAppInstanceConfigDto>> CreateInstanceAsync(
        CreateWhatsAppInstanceCommand command,
        CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId ?? 0;
        if (tenantId <= 0)
        {
            return Result<WhatsAppInstanceConfigDto>.ValidationFailure(
                "Tenant inválido", new[] { "É necessário um contexto de tenant ativo para cadastrar instâncias." });
        }

        var exists = await _dbContext.InstanceConfigs
            .AnyAsync(i => i.InstanceName == command.InstanceName, ct);

        if (exists)
        {
            return Result<WhatsAppInstanceConfigDto>.Failure("Já existe uma instância cadastrada com este nome no condomínio.");
        }

        try
        {
            Enum.TryParse<WhatsAppProvider>(command.Provider, true, out var provider);
            var entity = WhatsAppInstanceConfig.Criar(
                tenantId: tenantId,
                condoId: command.CondoId,
                instanceName: command.InstanceName,
                provider: provider == 0 ? WhatsAppProvider.EvolutionApi : provider,
                baseUrl: command.BaseUrl,
                apiKey: command.ApiKey,
                webhookSecret: command.WebhookSecret
            );

            _dbContext.InstanceConfigs.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            var dto = MapToInstanceDto(entity);
            return Result<WhatsAppInstanceConfigDto>.Success(dto, "Instância cadastrada com sucesso");
        }
        catch (WhatsAppDomainException ex)
        {
            return Result<WhatsAppInstanceConfigDto>.ValidationFailure(ex.Message, new[] { ex.Message });
        }
    }

    public async Task<Result<IEnumerable<WhatsAppInstanceConfigDto>>> GetInstancesAsync(
        int? condoId = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.InstanceConfigs.AsNoTracking();
        if (condoId.HasValue && condoId.Value > 0)
        {
            query = query.Where(i => i.CondoId == condoId.Value);
        }

        var items = await query.OrderByDescending(i => i.CriadoEm).ToListAsync(ct);
        var dtos = items.Select(MapToInstanceDto);

        return Result<IEnumerable<WhatsAppInstanceConfigDto>>.Success(dtos);
    }

    public async Task<Result<WhatsAppInstanceConfigDto>> ToggleInstanceStatusAsync(
        int instanceId,
        CancellationToken ct = default)
    {
        var instance = await _dbContext.InstanceConfigs.FindAsync(new object[] { instanceId }, ct);
        if (instance == null)
        {
            return Result<WhatsAppInstanceConfigDto>.Failure("Instância do WhatsApp não encontrada.");
        }

        instance.AlternarAtivo();
        await _dbContext.SaveChangesAsync(ct);

        return Result<WhatsAppInstanceConfigDto>.Success(MapToInstanceDto(instance), "Status da instância atualizado com sucesso");
    }

    public async Task<Result<IEnumerable<WhatsAppWebhookLogDto>>> GetWebhookLogsAsync(
        string? instanceName = null,
        string? status = null,
        string? phone = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _dbContext.WebhookLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(instanceName))
        {
            query = query.Where(w => w.InstanceName == instanceName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WhatsAppWebhookStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(w => w.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var cleanPhone = phone.Trim();
            query = query.Where(w => w.SenderPhone.Contains(cleanPhone) || (w.PushName != null && w.PushName.Contains(cleanPhone)));
        }

        var skip = Math.Max(0, page - 1) * pageSize;
        var items = await query
            .OrderByDescending(w => w.ReceivedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToLogDto);
        return Result<IEnumerable<WhatsAppWebhookLogDto>>.Success(dtos);
    }

    public async Task<Result<WhatsAppWebhookSummaryDto>> GetSummaryAsync(
        int? condoId = null,
        CancellationToken ct = default)
    {
        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        var logsQuery = _dbContext.WebhookLogs.AsNoTracking()
            .Where(w => w.ReceivedAt >= today);

        if (condoId.HasValue && condoId.Value > 0)
        {
            logsQuery = logsQuery.Where(w => w.CondoId == condoId.Value);
        }

        var totalRecebidos = await logsQuery.CountAsync(ct);
        var processados = await logsQuery.CountAsync(w => w.Status == WhatsAppWebhookStatus.Processed || w.Status == WhatsAppWebhookStatus.Received, ct);
        var falhas = await logsQuery.CountAsync(w => w.Status == WhatsAppWebhookStatus.Failed, ct);
        var ignorados = await logsQuery.CountAsync(w => w.Status == WhatsAppWebhookStatus.Ignored, ct);

        var instanciasQuery = _dbContext.InstanceConfigs.AsNoTracking().Where(i => i.IsActive);
        if (condoId.HasValue && condoId.Value > 0)
        {
            instanciasQuery = instanciasQuery.Where(i => i.CondoId == condoId.Value);
        }

        var instanciasAtivas = await instanciasQuery.CountAsync(ct);

        var summary = new WhatsAppWebhookSummaryDto(
            TotalRecebidosHoje: totalRecebidos,
            ProcessadosComSucesso: processados,
            Falhas: falhas,
            IgnoradosIdempotencia: ignorados,
            InstanciasAtivas: instanciasAtivas
        );

        return Result<WhatsAppWebhookSummaryDto>.Success(summary);
    }

    private static string? ExtractApiKeyFromRawJson(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("apikey", out var prop))
            {
                return prop.GetString();
            }
        }
        catch { }
        return null;
    }

    private static WhatsAppInstanceConfigDto MapToInstanceDto(WhatsAppInstanceConfig entity) => new(
        Id: entity.Id,
        TenantId: entity.TenantId,
        CondoId: entity.CondoId,
        InstanceName: entity.InstanceName,
        Provider: entity.Provider.ToString(),
        BaseUrl: entity.BaseUrl,
        ApiKey: entity.ApiKey,
        WebhookSecret: entity.WebhookSecret,
        IsActive: entity.IsActive,
        Status: entity.Status,
        CriadoEm: entity.CriadoEm,
        UltimaConexaoEm: entity.UltimaConexaoEm
    );

    private static WhatsAppWebhookLogDto MapToLogDto(WhatsAppWebhookLog entity) => new(
        Id: entity.Id,
        TenantId: entity.TenantId,
        CondoId: entity.CondoId,
        InstanceName: entity.InstanceName,
        Provider: entity.Provider.ToString(),
        MessageId: entity.MessageId,
        SenderPhone: entity.SenderPhone,
        PushName: entity.PushName,
        MessageType: entity.MessageType.ToString(),
        MessageText: entity.MessageText,
        MediaUrl: entity.MediaUrl,
        Status: entity.Status.ToString(),
        ErrorMessage: entity.ErrorMessage,
        ReceivedAt: entity.ReceivedAt,
        ProcessedAt: entity.ProcessedAt,
        RawPayloadJson: entity.RawPayloadJson
    );
}
