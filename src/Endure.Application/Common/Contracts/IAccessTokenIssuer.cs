namespace Endure.Application.Common.Contracts;

public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);

public interface IAccessTokenIssuer
{
    AccessToken Issue(Guid appUserId);
}
