using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Необроблений виняток під час обробки запиту");

        var (statusCode, title, detail) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path.Value
        };

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException or ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Некоректні дані",
                exception.Message),

            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Операція неможлива",
                exception.Message),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Доступ заборонено",
                exception.Message),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Ресурс не знайдено",
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Внутрішня помилка сервера",
                "Сталася неочікувана помилка. Спробуйте пізніше.")
        };
    }
}
