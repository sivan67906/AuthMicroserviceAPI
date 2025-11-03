using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AuthMicroservice.Infrastructure.Health;

public class MemoryHealthCheckOptions
{
    public long ThresholdBytes { get; set; } = 1024L * 1024L * 1024L; // 1 GB default
}

public class MemoryHealthCheck : IHealthCheck
{
    private readonly IOptionsMonitor<MemoryHealthCheckOptions> _options;

    public MemoryHealthCheck(IOptionsMonitor<MemoryHealthCheckOptions> options)
    {
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var data = new Dictionary<string, object>
        {
            { "AllocatedBytes", allocated },
            { "Gen0Collections", GC.CollectionCount(0) },
            { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) }
        };

        var threshold = _options.CurrentValue.ThresholdBytes;
        var status = allocated < threshold ? HealthStatus.Healthy : HealthStatus.Degraded;

        return Task.FromResult(new HealthCheckResult(
            status,
            description: $"Reports degraded status if allocated memory >= {threshold} bytes",
            data: data));
    }
}
