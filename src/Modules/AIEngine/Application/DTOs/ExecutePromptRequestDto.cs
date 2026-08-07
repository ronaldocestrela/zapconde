namespace Modules.AIEngine.Application.DTOs;

public record ExecutePromptRequestDto(
    string Prompt,
    double? TemperatureOverride = null,
    int? MaxTokensOverride = null);

public record ExecutePromptResponseDto(
    string Response,
    string ModelUsed,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    long DurationMs,
    bool Success,
    string? ErrorMessage,
    DateTimeOffset ExecutedAt);
