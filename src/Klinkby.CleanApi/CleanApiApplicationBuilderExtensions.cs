using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Klinkby.CleanApi;
using Serialization;

internal static class CleanApiApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use((context, next) =>
        {
            var res = context.Response;
            res.OnStarting(
                _ =>
                {
                    var headers = res.Headers;
                    headers.Append(HeaderNames.ContentSecurityPolicy, SecurityHeaderValue.ContentSecurityPolicy);
                    headers.Append(HeaderNames.XContentTypeOptions, SecurityHeaderValue.XContentTypeOptions);
                    return Task.CompletedTask;
                },
                context);
            return next();
        });
    }

    public static IApplicationBuilder UseJsonSerializedHealthChecks(this IApplicationBuilder app, string healthPath)
    {
        return app.UseHealthChecks(
            healthPath,
            new HealthCheckOptions
            {
                ResponseWriter = WriteJsonResponseAsync
            });
    }

    private static Task WriteJsonResponseAsync(HttpContext context, HealthReport report)
    {
        var status = new HealthReportResponse(
            report.Status.ToString(),
            report.Entries.Select(entry => new HealthReportEntryResponse(
                entry.Key,
                entry.Value.Duration,
                entry.Value.Status.ToString()
            )).ToArray(),
            report.TotalDuration);
        return HealthReportResponseSerializerContext
            .Default
            .WriteJsonAsync(status, context.Response);
    }
}

internal record HealthReportResponse(string Status, HealthReportEntryResponse[] Components,
    TimeSpan Duration)
{
}

internal record HealthReportEntryResponse(string Name, TimeSpan Duration, /*object Data,*/ string Status)
{
}
