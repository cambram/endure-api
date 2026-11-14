using System.Threading.RateLimiting;
using Endure.Api.Common.Configuration;
using Endure.Api.Common.Exceptions;
using Endure.Api.Common.Schemas;
using Endure.Api.Common.Services;
using Endure.Application.Common.Configuration;
using Endure.Application.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Endure.Api.Common.DependencyInjection;

internal static class DependencyInjection
{
    internal static IHostApplicationBuilder AddApiDependencies(this IHostApplicationBuilder builder)
    {
        AddAuthentication(builder.Services);
        AddCurrentUser(builder.Services);
        AddSwagger(builder.Services);
        AddExceptionHandling(builder.Services);
        AddRateLimiting(builder.Services);

        return builder;
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(ConfigureRateLimiter);
    }

    private static void ConfigureRateLimiter(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(RateLimitPolicies.Auth, GetAuthPartition);
    }

    private static RateLimitPartition<string> GetAuthPartition(HttpContext httpContext)
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, CreateAuthLimiterOptions);
    }

    private static FixedWindowRateLimiterOptions CreateAuthLimiterOptions(string clientIp)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = RateLimitPolicies.AuthPermitLimit,
            Window = RateLimitPolicies.AuthWindow,
            QueueLimit = 0
        };
    }

    private static void AddExceptionHandling(IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionHandler>();
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(ConfigureJwtBearer);

        services.AddAuthorization();
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, IOptions<JwtOptions> jwtOptions)
    {
        var jwt = jwtOptions.Value;

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwt.GetSigningKeyBytes()),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    private static void AddCurrentUser(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddOpenApi(ConfigureOpenApi);
    }

    private static void ConfigureOpenApi(OpenApiOptions options)
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        options.AddOperationTransformer<AuthorizationRequirementOperationTransformer>();
    }
}
