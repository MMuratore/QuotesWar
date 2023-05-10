using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;

public record BattleStarted
    (Guid BattleId, string Name, DateTimeOffset OccuredAt, params Challenger[] Challengers) : IDomainEvent;