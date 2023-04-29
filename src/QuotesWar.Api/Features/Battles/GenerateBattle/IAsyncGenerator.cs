namespace QuotesWar.Api.Features.Battles.GenerateBattle;

public interface IAsyncGenerator<out T>
{
    public IAsyncEnumerable<T> StartAsync(CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);
}