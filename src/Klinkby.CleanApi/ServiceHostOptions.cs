using System;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;

namespace Klinkby.CleanApi;

/// <summary>
///     Options for the service host middleware configuration
/// </summary>
public record ServiceHostOptions
{
    /// <summary>Determines if the service requires authorization</summary>
    public bool Authorization { get; set; } = false;

    /// <summary>Determines if the service use caching</summary>
    public bool Cache { get; set; } = true;

    /// <summary>Determines if the service has a rate-limited policy</summary>
    public TokenBucketRateLimiterOptions? RateLimiter { get; set; }

    /// <summary>Determines if the service use secure TLS transmission</summary>
    public bool Https { get; set; } = false;

    /// <summary>The path to the health check endpoint</summary>
    public string HealthPath { get; set; } = "/health";

    /// <summary>Determines if the service allow cross-origin requests</summary>
    public string[] CorsOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The OpenAPI/Swagger configuration
    /// </summary>
    public OpenApiInfo OpenApi { get; set; } = new();
        
    /// <summary>
    ///     Service identification for observability
    /// </summary>
    public string ServiceName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name ?? nameof(CleanApi);

    /// <summary>
    ///     Service identification for observability
    /// </summary>
    public string ServiceVersion { get; set; } = "v1";

    //= Assembly.GetEntryAssembly()?.GetName().Version. ?? nameof(CleanApi);
}