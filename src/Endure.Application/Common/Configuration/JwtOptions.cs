using System.Text;

namespace Endure.Application.Common.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public const int MinimumKeyBytes = 32;

    public static readonly string InvalidKeyMessage =
        $"'Jwt:Key' must be at least {MinimumKeyBytes} bytes. Set it with: dotnet user-secrets set \"Jwt:Key\" \"<random value>\" --project src/Endure.Api";

    public const string InvalidLifetimeMessage = "'Jwt:AccessTokenLifetimeMinutes' must be greater than 0.";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public byte[] GetSigningKeyBytes()
    {
        return Encoding.UTF8.GetBytes(Key);
    }

    public bool HasUsableKey()
    {
        return GetSigningKeyBytes().Length >= MinimumKeyBytes;
    }

    public bool HasUsableLifetime()
    {
        return AccessTokenLifetimeMinutes > 0;
    }
}
