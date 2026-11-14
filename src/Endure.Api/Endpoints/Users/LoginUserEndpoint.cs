using Endure.Api.Common.Configuration;
using Endure.Api.Common.Contracts;
using Endure.Application.Modules.Users.Commands;
using Endure.Application.Modules.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Endure.Api.Endpoints.Users;

internal sealed class LoginUserEndpoint : IEndpoint
{
    public const string Name = nameof(LoginUserEndpoint);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Users.Login,
                async Task<Ok<AccessTokenDto>> (
                    LoginUserRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var accessToken = await sender.Send(new LoginUserCommand(request), cancellationToken);

                    return TypedResults.Ok(accessToken);
                })
            .WithName(Name)
            .WithTags(ApiEndpoints.Users.Tag)
            .WithSummary("Exchanges an email and password for a bearer token.")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .AllowAnonymous();
    }
}
