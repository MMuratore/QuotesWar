using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;

public record BattleQuotesGet(Guid BattleId) : IDomainEvent;