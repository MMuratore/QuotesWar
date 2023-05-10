using Marten;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;
using QuotesWar.Infrastructure.HostedService;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.GenerateBattle;

public class BattleHandler : IHostedServiceHandler<IEnumerable<Challenger>>
{
    private readonly ILogger<BattleHandler> _logger;

    public BattleHandler(string name, ILogger<BattleHandler> logger)
    {
        _logger = logger;
        Name = name;
    }

    public string Name { get; }

    public async Task HandleAsync(IEnumerable<Challenger> element, IServiceScope scope,
        CancellationToken cancellationToken = default)
    {
        var lastBattleStarted = GetLastBattleStartedEvent(scope);

        var repository = scope.ServiceProvider.GetRequiredService<IEventStoreRepository<Battle>>();

        if (lastBattleStarted is not null) await CloseLastBattle(repository, lastBattleStarted, cancellationToken);

        await CreateNextBattle(repository, element, cancellationToken);
    }

    private async Task CreateNextBattle(IEventStoreRepository<Battle> repository, IEnumerable<Challenger> challengers,
        CancellationToken cancellationToken)
    {
        var battle = new Battle(Name, challengers.ToArray());
        await repository.StoreAsync(battle, cancellationToken);
        _logger.LogInformation("Battle Of The Day Hosted Service is opening a new battle with id: '{BattleId}'",
            battle.Id);
    }

    private async Task CloseLastBattle(IEventStoreRepository<Battle> repository,
        BattleStarted lastBattleStarted, CancellationToken cancellationToken)
    {
        var lastBattle =
            await repository.LoadAsync(lastBattleStarted.BattleId, cancellationToken: cancellationToken);

        if (lastBattle.Status is BattleStatus.Close) return;

        lastBattle.CloseBattle();
        await repository.StoreAsync(lastBattle, cancellationToken);
        _logger.LogInformation("Battle Of The Day Hosted Service is closing battle with id: '{BattleId}'",
            lastBattleStarted.BattleId);
    }

    private BattleStarted? GetLastBattleStartedEvent(IServiceScope scope)
    {
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        return session.Events.QueryRawEventDataOnly<BattleStarted>().OrderByDescending(x => x.OccuredAt)
            .FirstOrDefault(x => x.Name == Name);
    }
}