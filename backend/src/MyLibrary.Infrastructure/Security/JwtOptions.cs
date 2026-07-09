namespace MyLibrary.Infrastructure.Security;

/// <summary>
/// JWT signing configuration, bound from the "Jwt" configuration section (Options Pattern).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
