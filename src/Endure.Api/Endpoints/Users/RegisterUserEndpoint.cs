using Endure.Api.Common.Configuration;
using Endure.Api.Common.Contracts;
using Endure.Application.Modules.Users.Commands;
using Endure.Application.Modules.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Endure.Api.Endpoints.Users;

internal sealed class RegisterUserEndpoint : IEndpoint
{
    public const string Name = nameof(RegisterUserEndpoint);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Users.Register,
                async Task<Created<AppUserDto>> (
                    RegisterUserRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var appUser = await sender.Send(new RegisterUserCommand(request), cancellationToken);

                    return TypedResults.Created($"/{ApiEndpoints.Users.GetMyUser}", appUser);
                })
            .WithName(Name)
            .WithTags(ApiEndpoints.Users.Tag)
            .WithSummary("Creates an account from an email and password.")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .AllowAnonymous();
    }
}
