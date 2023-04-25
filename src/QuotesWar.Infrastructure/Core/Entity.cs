using System.Text.Json.Serialization;

namespace QuotesWar.Infrastructure.Core;

public abstract class Entity : Entity<Guid>
{
    protected Entity() : base(Guid.NewGuid())
    {
    }
}

public abstract class Entity<T>
{
    [JsonIgnore] private readonly List<IDomainEvent> _domainEvents = new();

    protected Entity(T id)
    {
        Id = id;
    }

    public T Id { get; protected set; }
    public long Version { get; protected set; }

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}