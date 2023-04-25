using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Clean;

internal static class IApplicationBuilderExtensionss
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use((context, next) =>
        {
            var res = context.Response;
            res.OnStarting(
                _ =>
                {
                    res.Headers.Add(HeaderNames.ContentSecurityPolicy, SecurityHeaderValue.ContentSecurityPolicy);
                    res.Headers.Add(HeaderNames.XContentTypeOptions, SecurityHeaderValue.XContentTypeOptions);
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
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var status = new
        {
            Status = report.Status.ToString(),
            Components = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                entry.Value.Duration,
                entry.Value.Data,
                Status = entry.Value.Status.ToString()
            }),
            Duration = report.TotalDuration
        };
        return JsonSerializer.SerializeAsync(
            context.Response.Body,
            status,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
    }
}