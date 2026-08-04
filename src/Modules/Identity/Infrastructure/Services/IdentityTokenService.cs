using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Application;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public sealed class IdentityTokenService : IIdentityTokenService
{
    private readonly IdentityDbContext _dbContext;
    private readonly AuthOptions _options;

    public IdentityTokenService(
        IdentityDbContext dbContext,
        IOptions<AuthOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> CreatePreContextTokensAsync(
        ApplicationUser user,
        IReadOnlyList<UserCondoMembership> memberships,
        CancellationToken ct = default)
    {
        var claims = BuildBaseClaims(user);
        return await CreateTokensInternalAsync(user.Id, claims, membershipId: null, ct);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> CreateContextTokensAsync(
        ApplicationUser user,
        UserCondoMembership membership,
        CancellationToken ct = default)
    {
        var claims = BuildBaseClaims(user);
        claims.Add(new Claim(SmartCondoClaimTypes.TenantId, membership.TenantId.ToString()));
        claims.Add(new Claim(SmartCondoClaimTypes.CondoId, membership.CondoId.ToString()));
        claims.Add(new Claim(SmartCondoClaimTypes.Role, membership.Role));
        claims.Add(new Claim(SmartCondoClaimTypes.MembershipId, membership.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Role, membership.Role));

        return await CreateTokensInternalAsync(user.Id, claims, membership.Id, ct);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RefreshTokensAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        var storedToken = await _dbContext.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (storedToken is null || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new SecurityTokenException("Refresh token inválido ou expirado.");
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId, ct)
            ?? throw new SecurityTokenException("Usuário não encontrado.");

        var claims = BuildBaseClaims(user);

        if (storedToken.MembershipId.HasValue)
        {
            var membership = await _dbContext.UserCondoMemberships
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == storedToken.MembershipId && m.UserId == user.Id && m.IsActive, ct);

            if (membership is not null)
            {
                claims.Add(new Claim(SmartCondoClaimTypes.TenantId, membership.TenantId.ToString()));
                claims.Add(new Claim(SmartCondoClaimTypes.CondoId, membership.CondoId.ToString()));
                claims.Add(new Claim(SmartCondoClaimTypes.Role, membership.Role));
                claims.Add(new Claim(SmartCondoClaimTypes.MembershipId, membership.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, membership.Role));
            }
        }

        _dbContext.UserRefreshTokens.Remove(storedToken);
        await _dbContext.SaveChangesAsync(ct);

        return await CreateTokensInternalAsync(user.Id, claims, storedToken.MembershipId, ct);
    }

    private async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> CreateTokensInternalAsync(
        Guid userId,
        List<Claim> claims,
        Guid? membershipId,
        CancellationToken ct)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _dbContext.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MembershipId = membershipId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays)
        });

        await _dbContext.SaveChangesAsync(ct);

        return (accessToken, refreshToken, expiresAt);
    }

    private static List<Claim> BuildBaseClaims(ApplicationUser user)
    {
        return
        [
            new Claim(SmartCondoClaimTypes.UserId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        ];
    }
}
