using BuildingBlocks.Shared;
using Modules.AIEngine.Application.DTOs;

namespace Modules.AIEngine.Application.Services;

public interface IAiOrchestratorService
{
    Task<Result<AiKernelConfigDto>> GetConfigAsync(CancellationToken ct = default);
    Task<Result<AiKernelConfigDto>> SaveConfigAsync(SaveAiConfigCommand command, CancellationToken ct = default);
    Task<Result<ExecutePromptResponseDto>> ExecutePromptAsync(ExecutePromptRequestDto request, CancellationToken ct = default);
    Task<Result<IEnumerable<AiExecutionLogDto>>> GetLogsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Result<AiSummaryDto>> GetSummaryAsync(CancellationToken ct = default);
}
