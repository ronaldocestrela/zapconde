namespace Modules.Identity.Domain;

public class UserRefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? MembershipId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public ApplicationUser? User { get; set; }
}
