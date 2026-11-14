using Endure.Application.Common.Exceptions;
using Endure.Application.Modules.Users.Dtos;
using Endure.Domain.Modules.Users.Repositories;
using MediatR;

namespace Endure.Application.Modules.Users.Queries;

public sealed class GetCurrentAppUserQuery : IRequest<AppUserDto>
{
    public GetCurrentAppUserQuery(Guid appUserId)
    {
        AppUserId = appUserId;
    }

    public Guid AppUserId { get; }
}

internal sealed class GetCurrentAppUserQueryHandler : IRequestHandler<GetCurrentAppUserQuery, AppUserDto>
{
    private readonly IAppUserRepository _appUserRepository;

    public GetCurrentAppUserQueryHandler(IAppUserRepository appUserRepository)
    {
        _appUserRepository = appUserRepository;
    }

    public async Task<AppUserDto> Handle(GetCurrentAppUserQuery query, CancellationToken cancellationToken)
    {
        var appUser = await _appUserRepository.GetByIdAsync(query.AppUserId, cancellationToken);

        if (appUser is null)
        {
            throw new NotFoundException("The user does not exist.");
        }

        var appUserDto = new AppUserDto
        {
            Id = appUser.Id,
            Email = appUser.Email,
            CreatedAtUtc = appUser.CreatedAtUtc
        };

        return appUserDto;
    }
}
