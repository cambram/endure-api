using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Endure.Application.Common.DependencyInjection;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationDependencies(this IHostApplicationBuilder builder)
    {
        AddMediatR(builder.Services);

        return builder;
    }

    private static void AddMediatR(IServiceCollection services)
    {
        services.AddMediatR(ConfigureMediatR);
    }

    private static void ConfigureMediatR(MediatRServiceConfiguration config)
    {
        config.RegisterServicesFromAssembly(typeof(IApplicationAssemblyMarker).Assembly);
    }
}
