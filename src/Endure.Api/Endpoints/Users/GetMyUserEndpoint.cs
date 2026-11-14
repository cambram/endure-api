using Endure.Api.Common.Contracts;
using Endure.Application.Common.Exceptions;
using Endure.Application.Common.Services;
using Endure.Application.Modules.Users.Dtos;
using Endure.Application.Modules.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Endure.Api.Endpoints.Users;

internal sealed class GetMyUserEndpoint : IEndpoint
{
    public const string Name = nameof(GetMyUserEndpoint);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Users.GetMyUser,
                async Task<Ok<AppUserDto>> (
                    [FromServices] ISender sender,
                    [FromServices] ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (currentUser.AppUserId is not { } appUserId)
                    {
                        throw new UnauthorizedException("The request does not contain a valid access token.");
                    }

                    var appUser = await sender.Send(new GetCurrentAppUserQuery(appUserId), cancellationToken);

                    return TypedResults.Ok(appUser);
                })
            .WithName(Name)
            .WithTags(ApiEndpoints.Users.Tag)
            .WithSummary("Returns the logged-in user.")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
}
