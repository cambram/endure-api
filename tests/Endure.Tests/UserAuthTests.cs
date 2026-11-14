using Endure.Application.Common.Contracts;
using Endure.Application.Common.Exceptions;
using Endure.Application.Modules.Users.Commands;
using Endure.Application.Modules.Users.Dtos;
using Endure.Domain.Modules.Users.Entities;
using Endure.Domain.Modules.Users.Repositories;
using Endure.Infrastructure.Authentication;

namespace Endure.Tests;

public sealed class UserAuthTests
{
    private const string Email = "runner@example.com";
    private const string Password = "correct horse battery";

    private readonly PasswordHasher _passwordHasher = new();
    private readonly FakeAppUserRepository _appUserRepository = new();
    private readonly LoginUserCommandHandler _loginHandler;
    private readonly RegisterUserCommandHandler _registerHandler;

    public UserAuthTests()
    {
        _appUserRepository.AppUsers.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            Email = Email,
            PasswordHash = _passwordHasher.Hash(Password)
        });

        _loginHandler = new LoginUserCommandHandler(_appUserRepository, _passwordHasher, new FakeAccessTokenIssuer());
        _registerHandler = new RegisterUserCommandHandler(_appUserRepository, _passwordHasher);
    }

    [Fact]
    public async Task LoginRejectsUnknownEmailAndWrongPasswordWithSameError()
    {
        var unknownEmailError = await Assert.ThrowsAsync<UnauthorizedException>(LoginWithUnknownEmail);
        var wrongPasswordError = await Assert.ThrowsAsync<UnauthorizedException>(LoginWithWrongPassword);

        Assert.Equal(unknownEmailError.Message, wrongPasswordError.Message);
    }

    [Fact]
    public async Task LoginSucceedsWithCorrectPasswordAndUnnormalizedEmail()
    {
        var accessToken = await Login("  RUNNER@Example.com ", Password);

        Assert.False(string.IsNullOrEmpty(accessToken.AccessToken));
    }

    [Theory]
    [InlineData(null, Password)]
    [InlineData("", Password)]
    [InlineData("not-an-email", Password)]
    [InlineData("new@example.com", null)]
    [InlineData("new@example.com", "short")]
    public async Task RegisterRejectsInvalidEmailOrPassword(string? email, string? password)
    {
        Task Register()
        {
            return _registerHandler.Handle(
                new RegisterUserCommand(new RegisterUserRequest(email, password)),
                CancellationToken.None);
        }

        await Assert.ThrowsAsync<ValidationException>(Register);
        Assert.Single(_appUserRepository.AppUsers);
    }

    private Task<AccessTokenDto> LoginWithUnknownEmail()
    {
        return Login("nobody@example.com", Password);
    }

    private Task<AccessTokenDto> LoginWithWrongPassword()
    {
        return Login(Email, "wrong password");
    }

    private Task<AccessTokenDto> Login(string email, string password)
    {
        return _loginHandler.Handle(
            new LoginUserCommand(new LoginUserRequest(email, password)),
            CancellationToken.None);
    }

    private sealed class FakeAppUserRepository : IAppUserRepository
    {
        public List<AppUser> AppUsers { get; } = [];

        public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(AppUsers.FirstOrDefault(u => u.Id == id));
        }

        public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(AppUsers.FirstOrDefault(u => u.Email == email));
        }

        public Task AddAsync(AppUser appUser, CancellationToken cancellationToken)
        {
            AppUsers.Add(appUser);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
    {
        public AccessToken Issue(Guid appUserId)
        {
            return new AccessToken($"token-for-{appUserId}", DateTime.UtcNow.AddMinutes(15));
        }
    }
}
