using System.Diagnostics;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Infrastructure.Persistence;

namespace Modules.AIEngine.Application.Services;

public class AiOrchestratorService : IAiOrchestratorService
{
    private readonly AiDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IAiKernelFactory _kernelFactory;
    private readonly Application.Plugins.BoletoPlugin? _boletoPlugin;
    private readonly Application.Plugins.ReservaPlugin? _reservaPlugin;
    private readonly Application.Plugins.PortariaPlugin? _portariaPlugin;

    public AiOrchestratorService(
        AiDbContext dbContext,
        ICurrentTenantService currentTenantService,
        IAiKernelFactory kernelFactory,
        Application.Plugins.BoletoPlugin? boletoPlugin = null,
        Application.Plugins.ReservaPlugin? reservaPlugin = null,
        Application.Plugins.PortariaPlugin? portariaPlugin = null)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _kernelFactory = kernelFactory;
        _boletoPlugin = boletoPlugin;
        _reservaPlugin = reservaPlugin;
        _portariaPlugin = portariaPlugin;
    }


    public async Task<Result<AiKernelConfigDto>> GetConfigAsync(CancellationToken ct = default)
    {
        var config = await _dbContext.KernelConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (config == null)
        {
            return Result<AiKernelConfigDto>.Failure("Nenhuma configuração do Semantic Kernel encontrada para este condomínio.");
        }

        return Result<AiKernelConfigDto>.Success(MapToDto(config));
    }

    public async Task<Result<AiKernelConfigDto>> SaveConfigAsync(SaveAiConfigCommand command, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue || tenantId.Value <= 0)
        {
            return Result<AiKernelConfigDto>.Failure("Contexto de condomínio (Tenant) não identificado.");
        }

        var condoId = _currentTenantService.CondoId ?? 1;

        var existingConfig = await _dbContext.KernelConfigs
            .FirstOrDefaultAsync(ct);

        AiKernelConfig config;

        if (existingConfig == null)
        {
            config = AiKernelConfig.Criar(
                tenantId.Value,
                condoId,
                command.Provider,
                command.ModelId,
                command.EmbeddingModelId ?? "text-embedding-3-small",
                command.ApiKey ?? string.Empty,
                command.Endpoint,
                command.OrgId,
                command.Temperature,
                command.MaxTokens);

            _dbContext.KernelConfigs.Add(config);
        }
        else
        {
            existingConfig.Atualizar(
                command.Provider,
                command.ModelId,
                command.EmbeddingModelId ?? "text-embedding-3-small",
                command.ApiKey ?? existingConfig.ApiKey,
                command.Endpoint,
                command.OrgId,
                command.Temperature,
                command.MaxTokens,
                command.IsActive);

            config = existingConfig;
        }

        await _dbContext.SaveChangesAsync(ct);

        return Result<AiKernelConfigDto>.Success(MapToDto(config), "Configuração do Semantic Kernel salva com sucesso.");
    }

    public async Task<Result<ExecutePromptResponseDto>> ExecutePromptAsync(ExecutePromptRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return Result<ExecutePromptResponseDto>.ValidationFailure(["O prompt enviado não pode ser vazio."]);
        }

        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue || tenantId.Value <= 0)
        {
            return Result<ExecutePromptResponseDto>.Failure("Contexto de condomínio (Tenant) não identificado.");
        }

        var condoId = _currentTenantService.CondoId ?? 1;

        var config = await _dbContext.KernelConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsActive, ct);

        if (config == null)
        {
            return Result<ExecutePromptResponseDto>.Failure("Nenhuma configuração ativa do Semantic Kernel encontrada para este condomínio.");
        }

        var sw = Stopwatch.StartNew();

        try
        {
            string responseText;
            int promptTokens;
            int completionTokens;

            if (config.Provider == AiProvider.MockLocal)
            {
                // Resposta simulada local para desenvolvimento e testes
                await Task.Delay(150, ct); // Simula latência de rede
                responseText = $"[MockLocal AI Engine]: Processado o prompt '{request.Prompt.Trim()}' utilizando o modelo {config.ModelId}.";
                promptTokens = Math.Max(1, request.Prompt.Length / 4);
                completionTokens = Math.Max(1, responseText.Length / 4);
            }
            else
            {
                var pluginList = new List<object>();
                if (_boletoPlugin != null) pluginList.Add(_boletoPlugin);
                if (_reservaPlugin != null) pluginList.Add(_reservaPlugin);
                if (_portariaPlugin != null) pluginList.Add(_portariaPlugin);
                var kernel = _kernelFactory.CreateKernel(config, pluginList);

                var executionSettings = new PromptExecutionSettings
                {
                    ModelId = config.ModelId,
                    ExtensionData = new Dictionary<string, object>
                    {
                        { "Temperature", request.TemperatureOverride ?? config.Temperature },
                        { "MaxTokens", request.MaxTokensOverride ?? config.MaxTokens }
                    }
                };

                var kernelResult = await kernel.InvokePromptAsync(request.Prompt, new KernelArguments(executionSettings), cancellationToken: ct);
                responseText = kernelResult.GetValue<string>() ?? string.Empty;

                promptTokens = Math.Max(1, request.Prompt.Length / 4);
                completionTokens = Math.Max(1, responseText.Length / 4);
            }

            sw.Stop();

            var log = AiExecutionLog.RegistrarSucesso(
                tenantId.Value,
                condoId,
                request.Prompt,
                responseText,
                config.ModelId,
                promptTokens,
                completionTokens,
                sw.ElapsedMilliseconds);

            _dbContext.ExecutionLogs.Add(log);
            await _dbContext.SaveChangesAsync(ct);

            var dto = new ExecutePromptResponseDto(
                responseText,
                config.ModelId,
                promptTokens,
                completionTokens,
                promptTokens + completionTokens,
                sw.ElapsedMilliseconds,
                true,
                null,
                log.ExecutedAt);

            return Result<ExecutePromptResponseDto>.Success(dto, "Prompt executado com sucesso.");
        }
        catch (Exception ex)
        {
            sw.Stop();

            var log = AiExecutionLog.RegistrarFalha(
                tenantId.Value,
                condoId,
                request.Prompt,
                ex.Message,
                config.ModelId,
                sw.ElapsedMilliseconds);

            _dbContext.ExecutionLogs.Add(log);
            await _dbContext.SaveChangesAsync(ct);

            return Result<ExecutePromptResponseDto>.Failure($"Erro na execução do Semantic Kernel: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AiExecutionLogDto>>> GetLogsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var logs = await _dbContext.ExecutionLogs
            .AsNoTracking()
            .OrderByDescending(l => l.ExecutedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AiExecutionLogDto(
                l.Id,
                l.TenantId,
                l.Prompt,
                l.Response,
                l.ModelUsed,
                l.PromptTokens,
                l.CompletionTokens,
                l.TotalTokens,
                l.DurationMs,
                l.Success,
                l.ErrorMessage,
                l.ExecutedAt))
            .ToListAsync(ct);

        return Result<IEnumerable<AiExecutionLogDto>>.Success(logs);
    }

    public async Task<Result<AiSummaryDto>> GetSummaryAsync(CancellationToken ct = default)
    {
        var config = await _dbContext.KernelConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        var logs = await _dbContext.ExecutionLogs
            .AsNoTracking()
            .ToListAsync(ct);

        var totalExecucoes = logs.Count;
        var sucessos = logs.Count(l => l.Success);
        var falhas = logs.Count(l => !l.Success);
        var totalTokens = logs.Sum(l => (long)l.TotalTokens);
        var latenciaMedia = totalExecucoes > 0 ? logs.Average(l => (double)l.DurationMs) : 0.0;

        var dto = new AiSummaryDto(
            Configurada: config != null && config.IsActive,
            Provider: config?.Provider.ToString() ?? "Não configurado",
            ModelId: config?.ModelId ?? "N/A",
            TotalExecucoes: totalExecucoes,
            ExecucoesComSucesso: sucessos,
            ExecucoesComFalha: falhas,
            TotalTokensConsumidos: totalTokens,
            LatenciaMediaMs: Math.Round(latenciaMedia, 2));

        return Result<AiSummaryDto>.Success(dto);
    }

    private static AiKernelConfigDto MapToDto(AiKernelConfig config)
    {
        var apiKey = config.ApiKey ?? string.Empty;
        var masked = apiKey.Length > 8
            ? apiKey[..4] + "..." + apiKey[^4..]
            : "********";

        return new AiKernelConfigDto(
            config.Id,
            config.TenantId,
            config.CondoId,
            config.Provider,
            config.ModelId,
            config.EmbeddingModelId,
            masked,
            config.Endpoint,
            config.OrgId,
            config.Temperature,
            config.MaxTokens,
            config.IsActive,
            config.CriadoEm,
            config.AtualizadoEm);
    }
}
