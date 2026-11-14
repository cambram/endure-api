using Endure.Application.Common.Configuration;
using Endure.Application.Common.Contracts;
using Endure.Domain.Modules.Users.Repositories;
using Endure.Infrastructure.Authentication;
using Endure.Infrastructure.Persistence;
using Endure.Infrastructure.Repositories.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Endure.Infrastructure.Common.DependencyInjection;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructureDependencies(this IHostApplicationBuilder builder)
    {
        AddDatabase(builder.Services, builder.Configuration);
        AddRepositories(builder.Services);
        AddAuthentication(builder.Services, builder.Configuration);

        return builder;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddNpgsql<ApplicationDbContext>(connectionString);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IAppUserRepository, AppUserRepository>();
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(HasUsableKey, JwtOptions.InvalidKeyMessage)
            .Validate(HasUsableLifetime, JwtOptions.InvalidLifetimeMessage)
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
    }

    private static bool HasUsableKey(JwtOptions options)
    {
        return options.HasUsableKey();
    }

    private static bool HasUsableLifetime(JwtOptions options)
    {
        return options.HasUsableLifetime();
    }
}
