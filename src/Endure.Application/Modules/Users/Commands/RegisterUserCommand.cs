using Endure.Application.Common.Contracts;
using Endure.Application.Common.Exceptions;
using Endure.Application.Modules.Users.Dtos;
using Endure.Domain.Modules.Users.Entities;
using Endure.Domain.Modules.Users.Repositories;
using MediatR;

namespace Endure.Application.Modules.Users.Commands;

public sealed class RegisterUserCommand : IRequest<AppUserDto>
{
    public RegisterUserCommand(RegisterUserRequest request)
    {
        Request = request;
    }

    public RegisterUserRequest Request { get; }
}

internal sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AppUserDto>
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;

    private readonly IAppUserRepository _appUserRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IAppUserRepository appUserRepository, IPasswordHasher passwordHasher)
    {
        _appUserRepository = appUserRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AppUserDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var email = UserEmail.Normalize(command.Request.Email);
        var password = command.Request.Password ?? string.Empty;

        if (!UserEmail.IsValid(email))
        {
            throw new ValidationException("The email is not valid.");
        }

        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
        {
            throw new ValidationException($"The password must be between {MinPasswordLength} and {MaxPasswordLength} characters.");
        }

        if (await _appUserRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var appUser = new AppUser
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(password)
        };

        await _appUserRepository.AddAsync(appUser, cancellationToken);
        await _appUserRepository.SaveChangesAsync(cancellationToken);

        var appUserDto = new AppUserDto
        {
            Id = appUser.Id,
            Email = appUser.Email,
            CreatedAtUtc = appUser.CreatedAtUtc
        };

        return appUserDto;
    }
}
