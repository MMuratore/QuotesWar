using System.Runtime.CompilerServices;
using Marten;
using Microsoft.EntityFrameworkCore;
using NCrontab;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;
using QuotesWar.Infrastructure.HostedService;
using QuotesWar.Infrastructure.Persistence;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.GenerateBattle;

public sealed class ChallengersGenerator : IAsyncGenerator<IEnumerable<Challenger>>
{
    private readonly ILogger<ChallengersGenerator> _logger;
    private readonly BattleGeneratorOptions _options;
    private readonly CrontabSchedule _schedule;
    private CancellationTokenSource _cancellationTokenSource;
    private DateTimeOffset _nextRun;

    public ChallengersGenerator(string name, BattleGeneratorOptions options, ILogger<ChallengersGenerator> logger)
    {
        _logger = logger;
        Name = name;
        _options = options;
        _schedule = CrontabSchedule.Parse(options.Schedule);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public string Name { get; }

    public async IAsyncEnumerable<IEnumerable<Challenger>> StartAsync(IServiceScope scope,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken(), cancellationToken);
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var context = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();

        SetInitialNextRun(session);

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            if (now > _nextRun)
            {
                var newChallengers = Enumerable.Empty<Challenger>();
                try
                {
                    newChallengers = await GetNewChallengers(context, _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException e)
                {
                }

                var challengers = newChallengers.ToList();
                if (challengers.Any())
                {
                    yield return challengers;
                    _logger.LogInformation("new challengers generated");
                    _nextRun = _schedule.GetNextOccurrence(now.DateTime);
                }
            }

            try
            {
                await Task.Delay(_nextRun - now, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException e)
            {
            }
        }

        _logger.LogInformation("challengers generation stopped");
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private void SetInitialNextRun(IDocumentSession session)
    {
        var battleStartedTime = GetLastBattleStartedEvent(session)?.OccuredAt.DateTime;
        _nextRun = battleStartedTime is not null
            ? _schedule.GetNextOccurrence(battleStartedTime.Value)
            : DateTimeOffset.MinValue;
    }

    private static BattleStarted? GetLastBattleStartedEvent(IDocumentSession session)
    {
        return session.Events.QueryRawEventDataOnly<BattleStarted>().OrderByDescending(x => x.OccuredAt)
            .FirstOrDefault();
    }

    private async Task<IEnumerable<Challenger>> GetNewChallengers(QuoteDbContext context,
        CancellationToken cancellationToken = default)
    {
        var challengers = await context.Quotes.AsNoTracking()
            .OrderBy(r => Guid.NewGuid())
            .Take(_options.NumberOfChallenger)
            .Select(x => new Challenger {Id = x.Id, Quote = x.Value})
            .ToArrayAsync(cancellationToken);

        return challengers;
    }
}