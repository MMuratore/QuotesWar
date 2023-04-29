using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NCrontab;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Api.Features.Battles.Models.Events;
using QuotesWar.Infrastructure.Marten;
using QuotesWar.Infrastructure.Persistence;

namespace QuotesWar.Api.Features.Battles.GenerateBattle;

public class BattleOfTheDayHostedService : BackgroundService
{
    private readonly BattleOfTheDayHealthCheck _healthCheck;
    private readonly ILogger<BattleOfTheDayHostedService> _logger;
    private readonly BattleOfTheDayOptions _options;
    private readonly CrontabSchedule _schedule;
    private readonly IServiceProvider _serviceProvider;
    private DateTimeOffset _nextRun;

    public BattleOfTheDayHostedService(IOptions<BattleOfTheDayOptions> options, IServiceProvider serviceProvider,
        ILogger<BattleOfTheDayHostedService> logger, BattleOfTheDayHealthCheck healthCheck)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _healthCheck = healthCheck;
        _options = options.Value;
        _schedule = CrontabSchedule.Parse(options.Value.Schedule);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Battle Of The Day Hosted Service is starting");
        using var scope = _serviceProvider.CreateScope();

        try
        {
            SetInitialNextRun(scope);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                if (now > _nextRun)
                {
                    var battleStarted = await UpdateBattleStreamAsync(scope, stoppingToken);
                    _nextRun = _schedule.GetNextOccurrence(battleStarted.OccuredAt.DateTime);
                }

                await Task.Delay(_nextRun - now, stoppingToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Battle Of The Day Hosted Service failed");
            _healthCheck.Crash(e);
        }

        _logger.LogInformation("Battle Of The Day Hosted Service is stopping");
    }

    private void SetInitialNextRun(IServiceScope scope)
    {
        var battleStartedTime = GetLastBattleStartedEvent(scope)?.OccuredAt.DateTime;
        _nextRun = battleStartedTime is not null
            ? _schedule.GetNextOccurrence(battleStartedTime.Value)
            : DateTimeOffset.MinValue;
    }

    private async Task<BattleStarted> UpdateBattleStreamAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var lastBattleStarted = GetLastBattleStartedEvent(scope);

        var repository = scope.ServiceProvider.GetRequiredService<IEventStoreRepository<Battle>>();

        if (lastBattleStarted is not null) await CloseLastBattle(repository, lastBattleStarted, cancellationToken);

        var challengers = await GetNewChallengers(scope, cancellationToken);

        return await CreateNextBattle(repository, challengers, cancellationToken);
    }

    private async Task<BattleStarted> CreateNextBattle(
        IEventStoreRepository<Battle> repository, Challenger[] challengers, CancellationToken cancellationToken)
    {
        var battle = new Battle(challengers);
        var battleStarted = battle.DomainEvents.OfType<BattleStarted>().Single();
        await repository.StoreAsync(battle, cancellationToken);
        _logger.LogInformation("Battle Of The Day Hosted Service is opening a new battle with id: '{BattleId}'",
            battle.Id);
        return battleStarted;
    }

    private async Task<Challenger[]> GetNewChallengers(IServiceScope scope, CancellationToken cancellationToken)
    {
        var context = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
        var challengers = await context.Quotes.AsNoTracking()
            .OrderBy(r => Guid.NewGuid())
            .Take(_options.NumberOfChallenger)
            .Select(x => new Challenger {Id = x.Id, Quote = x.Value})
            .ToArrayAsync(cancellationToken);
        return challengers;
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

    private static BattleStarted? GetLastBattleStartedEvent(IServiceScope scope)
    {
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        return session.Events.QueryRawEventDataOnly<BattleStarted>().OrderByDescending(x => x.OccuredAt)
            .FirstOrDefault();
    }
}