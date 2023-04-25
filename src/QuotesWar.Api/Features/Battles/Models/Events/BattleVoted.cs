using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.Models.Events;

public record BattleVoted(Guid BattleId, Guid QuoteId, DateTimeOffset OccurredOn) : IDomainEvent;