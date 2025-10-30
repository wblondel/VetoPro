using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics; // Required for IExceptionHandler

namespace VetoPro.Api.Middleware;

/// <summary>
/// Middleware to handle exceptions globally, log them,
/// and return a standardized JSON error response.
/// Implements IExceptionHandler for .NET 8+ structured error handling.
/// </summary>
public class ErrorHandlingMiddleware : IExceptionHandler
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the detailed exception
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // Prepare the response
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Default to 500

        // Create a user-friendly response object
        var response = new
        {
            StatusCode = httpContext.Response.StatusCode,
            Message = "An unexpected error occurred. Please try again later.",
            // Include details ONLY in development environment for security
            Details = _env.IsDevelopment() ? exception.StackTrace?.ToString() : null
        };

        // Write the JSON response
        var jsonResponse = JsonSerializer.Serialize(response);
        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

        // Return true to indicate the exception has been handled
        return true;
    }
}