using Marten;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Api.Features.Battles.Models.Events;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.GenerateBattle;

public class BattleOfTheDayHostedService : BackgroundService
{
    private readonly IAsyncGenerator<IEnumerable<Challenger>> _challengerGenerator;

    private readonly BattleOfTheDayHealthCheck _healthCheck;
    private readonly ILogger<BattleOfTheDayHostedService> _logger;
    private readonly BattleOfTheDayRequestsChannel _requestsChannel;
    private readonly IServiceProvider _serviceProvider;
    private int _runningStatus;

    public BattleOfTheDayHostedService(BattleOfTheDayHealthCheck healthCheck,
        BattleOfTheDayRequestsChannel requestsChannel, IAsyncGenerator<IEnumerable<Challenger>> challengerGenerator,
        IServiceProvider serviceProvider, ILogger<BattleOfTheDayHostedService> logger)
    {
        _logger = logger;
        _healthCheck = healthCheck;
        _requestsChannel = requestsChannel;
        _challengerGenerator = challengerGenerator;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _healthCheck.Run();
        _logger.LogInformation("Battle Of The Day Hosted Service is starting");

        await foreach (var request in _requestsChannel.Requests.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Battle Of The Day Hosted Service has received the request: '{Name}'",
                request.GetType().Name);

            var _ = request switch
            {
                StartBattle => StartGeneratorAsync(stoppingToken),
                StopBattle => StopGeneratorAsync(stoppingToken),
                _ => Task.CompletedTask
            };
        }

        _logger.LogInformation("Battle Of The Day Hosted Service is stopping");
    }

    private async Task StartGeneratorAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _runningStatus, 1) == 1)
        {
            _logger.LogInformation("Battle Of The Day Hosted Service is already running");
            return;
        }

        await foreach (var challengers in _challengerGenerator.StartAsync(cancellationToken))
        {
            var scope = _serviceProvider.CreateScope();
            await UpdateBattleStreamAsync(scope, challengers, cancellationToken);
        }
    }

    private async Task StopGeneratorAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _runningStatus, 0) == 0)
        {
            _logger.LogInformation("Battle Of The Day Hosted Service is not running");
            return;
        }

        await _challengerGenerator.StopAsync(cancellationToken);
    }

    private async Task<BattleStarted> UpdateBattleStreamAsync(IServiceScope scope, IEnumerable<Challenger> challengers,
        CancellationToken cancellationToken)
    {
        var lastBattleStarted = GetLastBattleStartedEvent(scope);

        var repository = scope.ServiceProvider.GetRequiredService<IEventStoreRepository<Battle>>();

        if (lastBattleStarted is not null) await CloseLastBattle(repository, lastBattleStarted, cancellationToken);

        return await CreateNextBattle(repository, challengers, cancellationToken);
    }

    private async Task<BattleStarted> CreateNextBattle(
        IEventStoreRepository<Battle> repository, IEnumerable<Challenger> challengers,
        CancellationToken cancellationToken)
    {
        var battle = new Battle(challengers.ToArray());
        var battleStarted = battle.DomainEvents.OfType<BattleStarted>().Single();
        await repository.StoreAsync(battle, cancellationToken);
        _logger.LogInformation("Battle Of The Day Hosted Service is opening a new battle with id: '{BattleId}'",
            battle.Id);
        return battleStarted;
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