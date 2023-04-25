using Marten;
using Microsoft.EntityFrameworkCore;
using NCrontab;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Api.Features.Battles.Models.Events;
using QuotesWar.Infrastructure.Marten;
using QuotesWar.Infrastructure.Persistence;

namespace QuotesWar.Api.Features.Battles;

public class BattleOfTheDayBackgroundService : BackgroundService
{
    private const string Schedule = "0 6 * * *"; //	every day 6am
    private const int NumberOfChallenger = 2;
    private readonly ILogger<BattleOfTheDayBackgroundService> _logger;
    private readonly CrontabSchedule _schedule;
    private readonly IServiceProvider _serviceProvider;
    private DateTimeOffset _nextRun;

    public BattleOfTheDayBackgroundService(IServiceProvider serviceProvider,
        ILogger<BattleOfTheDayBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _schedule = CrontabSchedule.Parse(Schedule);
        var battleStartedTime = GetLastBattleStarted(serviceProvider)?.OccurredOn.DateTime;
        _nextRun = battleStartedTime is not null
            ? _schedule.GetNextOccurrence(battleStartedTime.Value)
            : DateTimeOffset.MinValue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            if (now > _nextRun)
            {
                var battleStarted = await UpdateBattleStreamAsync(stoppingToken);
                _nextRun = _schedule.GetNextOccurrence(battleStarted.OccurredOn.DateTime);
            }

            await Task.Delay(_nextRun - now, stoppingToken);
        }
    }

    private async Task<BattleStarted> UpdateBattleStreamAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var lastBattleStarted = GetLastBattleStarted(_serviceProvider);

        var repository = scope.ServiceProvider.GetRequiredService<IEventStoreRepository<Battle>>();

        if (lastBattleStarted is not null)
        {
            var lastBattle =
                await repository.LoadAsync(lastBattleStarted.BattleId, cancellationToken: cancellationToken);
            lastBattle.CloseBattle();
            await repository.StoreAsync(lastBattle, cancellationToken);
            _logger.LogInformation("Battle Of The Day Hosted Service is closing battle with id: '{BattleId}'",
                lastBattleStarted.BattleId);
        }

        var context = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
        var challengers = await context.Quotes.AsNoTracking()
            .OrderBy(r => Guid.NewGuid())
            .Take(NumberOfChallenger)
            .Select(x => new Challenger {Id = x.Id, Quote = x.Value})
            .ToArrayAsync(cancellationToken: cancellationToken);

        var battle = new Battle(challengers);
        var battleStarted = battle.DomainEvents.OfType<BattleStarted>().Single();
        await repository.StoreAsync(battle, cancellationToken);
        _logger.LogInformation("Battle Of The Day Hosted Service is opening a new battle with id: '{BattleId}'",
            battle.Id);

        return battleStarted;
    }

    private static BattleStarted? GetLastBattleStarted(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        return session.Events.QueryRawEventDataOnly<BattleStarted>().OrderByDescending(x => x.OccurredOn)
            .FirstOrDefault();
    }
}