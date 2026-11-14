using Endure.Application.Common.Contracts;
using Endure.Application.Common.Exceptions;
using Endure.Application.Modules.Users.Dtos;
using Endure.Domain.Modules.Users.Repositories;
using MediatR;

namespace Endure.Application.Modules.Users.Commands;

public sealed class LoginUserCommand : IRequest<AccessTokenDto>
{
    public LoginUserCommand(LoginUserRequest request)
    {
        Request = request;
    }

    public LoginUserRequest Request { get; }
}

internal sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AccessTokenDto>
{
    private readonly IAppUserRepository _appUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenIssuer _accessTokenIssuer;

    public LoginUserCommandHandler(
        IAppUserRepository appUserRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenIssuer accessTokenIssuer)
    {
        _appUserRepository = appUserRepository;
        _passwordHasher = passwordHasher;
        _accessTokenIssuer = accessTokenIssuer;
    }

    public async Task<AccessTokenDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var email = UserEmail.Normalize(command.Request.Email);
        var appUser = await _appUserRepository.GetByEmailAsync(email, cancellationToken);

        var passwordMatches = _passwordHasher.Verify(command.Request.Password ?? string.Empty, appUser?.PasswordHash);

        if (appUser is null || !passwordMatches)
        {
            throw new UnauthorizedException("The email or password is incorrect.");
        }

        var accessToken = _accessTokenIssuer.Issue(appUser.Id);

        var accessTokenDto = new AccessTokenDto
        {
            AccessToken = accessToken.Value,
            ExpiresAtUtc = accessToken.ExpiresAtUtc
        };

        return accessTokenDto;
    }
}
