using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;

public record BattleVoted(Guid BattleId, Guid QuoteId) : IDomainEvent;