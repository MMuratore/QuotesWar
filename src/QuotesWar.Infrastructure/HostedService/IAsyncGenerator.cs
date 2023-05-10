using Microsoft.Extensions.DependencyInjection;

namespace QuotesWar.Infrastructure.HostedService;

public interface IAsyncGenerator<out T>
{
    public IAsyncEnumerable<T> StartAsync(IServiceScope scope, CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);
}