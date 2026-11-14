namespace Endure.Api.Common.Configuration;

internal static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const int AuthPermitLimit = 10;
    public static readonly TimeSpan AuthWindow = TimeSpan.FromMinutes(1);
}
