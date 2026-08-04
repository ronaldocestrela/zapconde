using Modules.Identity.Domain;

namespace Modules.Identity.Application;

public interface IIdentityTokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> CreatePreContextTokensAsync(
        ApplicationUser user,
        IReadOnlyList<UserCondoMembership> memberships,
        CancellationToken ct = default);

    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> CreateContextTokensAsync(
        ApplicationUser user,
        UserCondoMembership membership,
        CancellationToken ct = default);

    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RefreshTokensAsync(
        string refreshToken,
        CancellationToken ct = default);
}
