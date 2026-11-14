namespace Endure.Application.Modules.Users.Dtos;

public sealed record LoginUserRequest(string? Email, string? Password);
