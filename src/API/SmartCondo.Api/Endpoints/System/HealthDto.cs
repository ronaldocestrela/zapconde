namespace SmartCondo.Api.Endpoints.System;

/// <summary>
/// DTO de resposta do endpoint de health check
/// </summary>
public class HealthDto
{
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string Version { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
}
