using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

namespace Clean;

/// <summary>
/// Options for the service host middleware configuration
/// </summary>
public record ServiceHostOptions
{
    /// <summary>Determines if the service requires authorization</summary>
    public bool Authorization { get; init; } = true;

    /// <summary>Determines if the service use caching</summary>
    public bool Cache { get; init; } = true;

    /// <summary>Determines if the service has a rate-limited policy</summary>
    public TokenBucketRateLimiterOptions? RateLimiter { get; set; }

    /// <summary>Determines if the service use secure TLS transmission</summary>
    public bool Https { get; init; } = true;
    /// <summary>The path to the health check endpoint</summary>
    public string HealthPath { get; init; } = "/health";

    /// <summary>Determines if the service allow cross-origin requests</summary>
    public string[] CorsOrigins { get; init; } = new string[0];

    public OpenApiInfo OpenApi { get; init; } = new();
}