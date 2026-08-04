namespace Modules.Identity.Infrastructure;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string SigningKey { get; set; } = "SmartCondo-Dev-Signing-Key-Min-32-Chars-Long!";

    public string Issuer { get; set; } = "SmartCondo";

    public string Audience { get; set; } = "SmartCondo.Web";

    public int AccessTokenLifetimeMinutes { get; set; } = 60;

    public int RefreshTokenLifetimeDays { get; set; } = 7;

    public string BlazorClientId { get; set; } = "smartcondo-blazor";
}
