using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Klinkby.CleanApi;

internal class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context, Correlation correlation, ILogger<CleanApi> logger)
    {
        AddCorrelationId(context, correlation);
        var correlationId = correlation.Id;
        var scope = logger.BeginScope(
            "Request {Method} {Path} {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);
        var stopwatch = Stopwatch.StartNew();
        return _next(context)
            .ContinueWith(result =>
            {
                LogResult(context.Response.StatusCode, logger, result, correlationId, stopwatch.ElapsedMilliseconds);
                scope?.Dispose();
                return Task.CompletedTask;
            });
    }

    private static void AddCorrelationId(HttpContext context, Correlation correlation)
    {
        context.Response.OnStarting(() =>
        {
            correlation.TryAddCorrelationHeader(context.Response.Headers);
            return Task.CompletedTask;
        });
    }

    private static void LogResult(int statusCode, ILogger<CleanApi> logger, Task result, string correlationId,
        long elapsedMs)
    {
        const string messageFormat = "Request {CorrelationId} {Completion} in {ElapsedMs} mS status {StatusCode}";
        var messageParameters = new object[] { correlationId, result.Status, elapsedMs, statusCode };
        if (result.IsFaulted)
        {
            logger.LogCritical(result.Exception, messageFormat, messageParameters);
            return;
        }

        Action<ILogger, string?, object?[]> logSeverity = result.Status switch
        {
            TaskStatus.Canceled => LoggerExtensions.LogWarning,
            _ => statusCode switch
            {
                >= 500 => LoggerExtensions.LogError,
                >= 400 => LoggerExtensions.LogWarning,
                _ => LoggerExtensions.LogInformation
            }
        };
        logSeverity(logger, messageFormat, messageParameters);
    }
}