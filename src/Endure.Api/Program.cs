using Endure.Api.Common.Configuration.Extensions;
using Endure.Api.Common.DependencyInjection;
using Endure.Application.Common.DependencyInjection;
using Endure.Infrastructure.Common.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiDependencies();
builder.AddApplicationDependencies();
builder.AddInfrastructureDependencies();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(ConfigureSwaggerUI);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapApiEndpoints();

app.Run();

static void ConfigureSwaggerUI(SwaggerUIOptions options)
{
    options.SwaggerEndpoint("/openapi/v1.json", "Endure API v1");
}
