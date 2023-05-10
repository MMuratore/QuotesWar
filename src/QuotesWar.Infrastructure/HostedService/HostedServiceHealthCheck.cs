using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QuotesWar.Infrastructure.HostedService;

public class HostedServiceHealthCheck : IHealthCheck
{
    private Exception? _exception;
    private bool _isRunning = true;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isRunning
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy($"Hosted service has stopped.", _exception));
    }

    public void Run()
    {
        _isRunning = true;
        _exception = default;
    }

    public void Crash(Exception? exception = default)
    {
        _isRunning = false;
        _exception = exception;
    }
}