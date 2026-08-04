using Modules.Identity.Domain;

namespace Modules.Identity.Application.Dtos;

public sealed class RequestPhoneVerificationDto
{
    public int MoradorId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
}

public sealed class VerifyPhoneDto
{
    public int MoradorId { get; set; }

    public string Code { get; set; } = string.Empty;
}

public sealed class PhoneVerificationStatusDto
{
    public int MoradorId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MaskedPhoneNumber { get; set; }

    public PhoneVerificationStatus Status { get; set; }

    public DateTime? RequestedAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public int? ResendAvailableInSeconds { get; set; }

    public string? DebugCode { get; set; }
}
