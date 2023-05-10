using Microsoft.Extensions.DependencyInjection;

namespace QuotesWar.Infrastructure.HostedService;

public interface IHostedServiceHandler<in T>
{
    public string Name { get; }
    public Task HandleAsync(T element, IServiceScope scope, CancellationToken cancellationToken = default);
}