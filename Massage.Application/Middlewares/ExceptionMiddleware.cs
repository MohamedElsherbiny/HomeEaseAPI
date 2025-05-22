using Massage.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Middlewares;

public class ExceptionMiddleware(RequestDelegate _next, ILogger<ExceptionMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            BusinessException => new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                exception.Message
            },
            UnauthorizedAccessException => new
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized access."
            },
            _ => new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = exception.InnerException?.Message ?? exception.Message
            }
        };

        context.Response.StatusCode = response.StatusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}
