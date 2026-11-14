namespace Endure.Api.Common.Contracts;

internal interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
