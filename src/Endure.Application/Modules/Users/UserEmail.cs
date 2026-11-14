namespace Endure.Application.Modules.Users;

internal static class UserEmail
{
    internal const int MaxLength = 320;

    internal static string Normalize(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    internal static bool IsValid(string normalizedEmail)
    {
        if (normalizedEmail.Length == 0 || normalizedEmail.Length > MaxLength)
        {
            return false;
        }

        if (normalizedEmail.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var atIndex = normalizedEmail.IndexOf('@');

        if (atIndex <= 0 || atIndex != normalizedEmail.LastIndexOf('@') || atIndex == normalizedEmail.Length - 1)
        {
            return false;
        }

        return true;
    }
}
