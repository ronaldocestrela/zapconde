namespace SmartCondo.Api.Endpoints.System;

/// <summary>
/// DTO de resposta do endpoint de health check
/// </summary>
public class HealthDto
{
    /// <summary>
    /// Estado atual de saúde da API.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Data e hora UTC da verificação.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Versão atual da aplicação.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Ambiente de execução da aplicação.
    /// </summary>
    public string Environment { get; init; } = string.Empty;
}
