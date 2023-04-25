namespace QuotesWar.Infrastructure.Core;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}