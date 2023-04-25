using Marten;
using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Infrastructure.Marten;

internal sealed class EventStoreRepository<T> : IEventStoreRepository<T> where T : Entity, IAggregateRoot
{
    private readonly IDocumentStore _store;

    public EventStoreRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task StoreAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        await using var session = await _store.LightweightSerializableSessionAsync(cancellationToken);

        session.Events.Append(aggregate.Id, aggregate.Version, aggregate.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        aggregate.ClearDomainEvents();
    }

    public async Task<T> LoadAsync(Guid id, int? version = null, CancellationToken cancellationToken = default)
    {
        await using var session = await _store.LightweightSerializableSessionAsync(token: cancellationToken);
        var aggregate = await session.Events.AggregateStreamAsync<T>(id, version ?? 0, token: cancellationToken);
        return aggregate ?? throw new InvalidOperationException($"No aggregate by id {id}.");
    }
}