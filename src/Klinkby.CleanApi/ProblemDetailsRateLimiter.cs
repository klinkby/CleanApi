using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace Klinkby.CleanApi;

/// <summary>
/// A token bucket rate limiter is partitioned by the user's identity (if authenticated) or IP address. 
/// If limit is exceeded, a <see cref="ProblemDetails"/> response is returned.
/// </summary>
internal class ProblemDetailsRateLimiter
{
    private const string Rfc6585Url = "https://www.rfc-editor.org/rfc/rfc6585#section-4";
    private const string TooManyRequests = "Too Many Requests";
    private readonly TokenBucketRateLimiterOptions bucketOptions;

    public ProblemDetailsRateLimiter(TokenBucketRateLimiterOptions bucketOptions)
    {
        this.bucketOptions = bucketOptions;
    }

    /// <summary>
    /// Configures the rate limiter with partitioning strategy and problem details response.
    /// </summary>
    public void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = HandleRejectedContext;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(GetRateLimitPartition);
    }

    /// <summary>
    /// Gets the rate limit partition for the current request.
    /// </summary>
    private RateLimitPartition<string> GetRateLimitPartition(HttpContext context)
    {
        string? partitionKey = GetPartitionKey(context);
        return partitionKey is not null
            ? RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => bucketOptions)
            : RateLimitPartition.GetNoLimiter(string.Empty);
    }

    /// <summary>
    /// Gets the partition key for the current request.
    /// </summary>
    private static string? GetPartitionKey(HttpContext context) =>
        (IPAddress.IsLoopback(context.Connection.RemoteIpAddress ?? IPAddress.None), context.User.Identity?.Name) switch
        {
            (true, _) => null,                                           // don't rate limit loopback
            (_, string name) => name,                                    // rate limit authenticated users by name
            (_, null) => context.Connection.RemoteIpAddress?.ToString(), // rate limit unauthenticated users by IP (if available)
        };

    /// <summary>
    /// Write the <see cref="ProblemDetails"/> rejection response.
    /// </summary>
    private static ValueTask HandleRejectedContext(OnRejectedContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }
        var task = response.WriteAsJsonAsync(new ProblemDetails
        {
            Type = Rfc6585Url,
            Title = TooManyRequests,
            Status = StatusCodes.Status429TooManyRequests
        }, cancellation);
        return new ValueTask(task);
    }
}