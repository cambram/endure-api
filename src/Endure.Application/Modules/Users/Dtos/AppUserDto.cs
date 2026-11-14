namespace Endure.Application.Modules.Users.Dtos;

public sealed record AppUserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
