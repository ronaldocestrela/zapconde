using BuildingBlocks.Shared;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Application;

public interface IAuthService
{
    Task<Result<AuthTokenDto>> LoginAsync(string email, string password, CancellationToken ct = default);

    Task<Result<AuthContextTokenDto>> SelectProfileAsync(Guid userId, Guid membershipId, CancellationToken ct = default);

    Task<Result<ForgotPasswordResultDto>> ForgotPasswordAsync(string email, CancellationToken ct = default);

    Task<Result<AuthTokenDto>> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
