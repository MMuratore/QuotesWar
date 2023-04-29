using System.Runtime.CompilerServices;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NCrontab;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Api.Features.Battles.Models.Events;
using QuotesWar.Infrastructure.Persistence;

namespace QuotesWar.Api.Features.Battles.GenerateBattle;

internal sealed class ChallengersGenerator : IAsyncGenerator<IEnumerable<Challenger>>
{
    private readonly ILogger<ChallengersGenerator> _logger;
    private readonly BattleOfTheDayOptions _options;
    private readonly CrontabSchedule _schedule;
    private readonly IServiceProvider _serviceProvider;
    private bool _isRunning;

    private DateTimeOffset _nextRun;

    public ChallengersGenerator(IOptions<BattleOfTheDayOptions> options, IServiceProvider serviceProvider,
        ILogger<ChallengersGenerator> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _schedule = CrontabSchedule.Parse(options.Value.Schedule);
        _options = options.Value;
    }

    public async IAsyncEnumerable<IEnumerable<Challenger>> StartAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _isRunning = true;

        using var scope = _serviceProvider.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var context = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();

        SetInitialNextRun(session);

        while (_isRunning)
        {
            if (!_isRunning) yield break;

            var now = DateTimeOffset.Now;
            if (now > _nextRun)
            {
                var challengers = await GetNewChallengers(context, cancellationToken);
                yield return challengers;
                _logger.LogInformation("new challengers generated");
                _nextRun = _schedule.GetNextOccurrence(now.DateTime);
            }

            await Task.Delay(_nextRun - now, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = false;
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