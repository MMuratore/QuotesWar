using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.Models.Events;

public record BattleStarted(Guid BattleId, DateTimeOffset OccurredOn, params Challenger[] Challengers) : IDomainEvent;