using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Klinkby.CleanApi;

/// <summary>
/// A health check that reports the current memory usage.
/// </summary>
internal class MemoryHealthCheck : IHealthCheck
{
    const int MegaBitShift = 20; // >>20 == /1,048,576
    const float Percent100 = 100.0f;
    const float Percent90 = 90.0f;
    const float Percent80 = 80.0f;

    /// <summary>
    /// Report the service process memory consumption.
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        long allocatedMB = Process.GetCurrentProcess().PrivateMemorySize64 >> MegaBitShift;
        long totalMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes >> MegaBitShift;
        if (0 == totalMB)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Total memory is zero"));
        }
        float pctAllocated = (allocatedMB * Percent100) / totalMB;
        var status = pctAllocated switch
        {
            >= Percent90 => HealthStatus.Unhealthy,
            >= Percent80 => HealthStatus.Degraded,
            _ => HealthStatus.Healthy
        };
        var result = new HealthCheckResult(
             status,
             data: new SortedList<string, object>(3)
             {
                 { nameof(allocatedMB), allocatedMB },
                 { nameof(pctAllocated), pctAllocated },
                 { nameof(totalMB), totalMB }
             });
        return Task.FromResult(result);
    }
}