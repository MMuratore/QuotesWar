using QuotesWar.Api.Features.Battles.Models.Events;
using QuotesWar.Infrastructure.Core;

namespace QuotesWar.Api.Features.Battles.Models;

public sealed class Battle : Entity, IAggregateRoot
{
    public Battle(params Challenger[] challengers)
    {
        if (challengers.Length < 2)
            throw new ArgumentException("There must have 2 challengers minimum in the battle", nameof(challengers));

        var @event = new BattleStarted(Id, DateTimeOffset.Now, challengers);

        Apply(@event);
        AddDomainEvent(@event);
    }

    private Battle()
    {
    }

    public List<Challenger> Challengers { get; private set; } = new();
    public BattleStatus Status { get; private set; }
    public DateOnly Day { get; private set; }

    public void VoteForTheQuote(Guid quoteId)
    {
        if (!Challengers.Exists(x => x.Id == quoteId))
            throw new ArgumentException("Quote is not in the battle", nameof(quoteId));
        if (Status == BattleStatus.Close) throw new Exception("Battle is closed");
        ;

        var @event = new BattleVoted(Id, quoteId, DateTimeOffset.Now);

        Apply(@event);
        AddDomainEvent(@event);
    }

    public void CloseBattle()
    {
        if (Status == BattleStatus.Close) return;

        var @event = new BattleClosed(Id, DateTimeOffset.Now);

        Apply(@event);
        AddDomainEvent(@event);
    }

    private void Apply(BattleStarted @event)
    {
        Challengers.AddRange(@event.Challengers);
        Day = DateOnly.FromDateTime(@event.OccurredOn.Date);
        Status = BattleStatus.Open;
        Version++;
    }

    private void Apply(BattleVoted @event)
    {
        Challengers.Single(x => x.Id == @event.QuoteId).Score++;
        Version++;
    }

    private void Apply(BattleClosed @event)
    {
        Status = BattleStatus.Close;
        Version++;
    }
}