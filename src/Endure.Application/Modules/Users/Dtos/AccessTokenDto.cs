namespace Endure.Application.Modules.Users.Dtos;

public sealed record AccessTokenDto
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
