using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;

public record BattleClosed(Guid BattleId, DateTimeOffset OccuredAt) : IDomainEvent;