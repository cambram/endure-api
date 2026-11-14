using System.Reflection;
using Endure.Api.Common.Contracts;

namespace Endure.Api.Common.Configuration.Extensions;

internal static class EndpointRouteBuilderExtensions
{
    internal static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                && type.IsAssignableTo(typeof(IEndpoint)));

        foreach (var endpointType in endpointTypes)
        {
            var mapMethod = endpointType.GetMethod(
                nameof(IEndpoint.MapEndpoint),
                BindingFlags.Public | BindingFlags.Static);

            if (mapMethod is null)
            {
                throw new InvalidOperationException(
                    $"{endpointType.Name} must implement {nameof(IEndpoint.MapEndpoint)} as a public static method.");
            }

            mapMethod.Invoke(null, [app]);
        }

        return app;
    }
}
