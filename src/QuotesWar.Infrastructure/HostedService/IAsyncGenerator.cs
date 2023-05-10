using Microsoft.Extensions.DependencyInjection;

namespace QuotesWar.Infrastructure.HostedService;

public interface IAsyncGenerator<out T>
{
    public string Name { get; }
    public IAsyncEnumerable<T> StartAsync(IServiceScope scope, CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);
}