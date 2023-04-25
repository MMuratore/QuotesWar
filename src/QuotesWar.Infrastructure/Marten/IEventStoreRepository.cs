using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Infrastructure.Marten;

public interface IEventStoreRepository<T> where T : Entity, IAggregateRoot
{
    public Task StoreAsync(T entity, CancellationToken cancellationToken = default);
    public Task<T> LoadAsync(Guid id, int? version = null, CancellationToken cancellationToken = default);
}