using Endure.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Endure.Api.Common.Exceptions;

internal sealed class ExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = GetStatusCode(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Detail = GetDetail(exception)
        };

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    private static int GetStatusCode(Exception exception)
    {
        if (exception is ValidationException)
        {
            return StatusCodes.Status400BadRequest;
        }

        if (exception is NotFoundException)
        {
            return StatusCodes.Status404NotFound;
        }

        if (exception is ConflictException)
        {
            return StatusCodes.Status409Conflict;
        }

        if (exception is UnauthorizedException)
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (exception is BadHttpRequestException badHttpRequestException)
        {
            return badHttpRequestException.StatusCode;
        }

        return StatusCodes.Status500InternalServerError;
    }

    private static string? GetDetail(Exception exception)
    {
        if (exception is ValidationException or NotFoundException or ConflictException or UnauthorizedException)
        {
            return exception.Message;
        }

        return null;
    }
}
