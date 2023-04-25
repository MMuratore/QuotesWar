using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.Models.Events;

public record BattleQuotesGet(Guid BattleId) : IDomainEvent;