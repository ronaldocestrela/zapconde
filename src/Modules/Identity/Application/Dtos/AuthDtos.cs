namespace Modules.Identity.Application.Dtos;

public sealed record AuthProfileDto(
    Guid MembershipId,
    int TenantId,
    int CondoId,
    string Role,
    string DisplayLabel);

public sealed record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    IReadOnlyList<AuthProfileDto> Profiles);

public sealed record AuthContextTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int TenantId,
    int CondoId,
    string UserId,
    string Role);

public sealed record ForgotPasswordResultDto(string Message);
