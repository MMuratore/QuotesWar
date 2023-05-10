using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuotesWar.Infrastructure.HostedService.Channel;

namespace QuotesWar.Infrastructure.HostedService;

public class HostedService<TGenerator, THandler, TElement> : BackgroundService
    where TGenerator : IAsyncGenerator<TElement>
    where THandler : IHostedServiceHandler<TElement>
{
    private readonly HostedServiceRequestsChannel _channel;
    private readonly TGenerator _generator;
    private readonly IHostedServiceHandler<TElement> _handler;
    private readonly ILogger<HostedService<TGenerator, THandler, TElement>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private int _runningStatus;

    public HostedService(string name, HostedServiceRequestsChannel channel, TGenerator generator, THandler handler,
        IServiceProvider serviceProvider, ILogger<HostedService<TGenerator, THandler, TElement>> logger)
    {
        Name = name;
        _channel = channel;
        _generator = generator;
        _handler = handler;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string Name { get; set; } = string.Empty;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hosted Service is starting");

        await foreach (var request in _channel.Requests.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Hosted Service has received the request: '{Name}'",
                request.GetType().Name);

            var _ = request switch
            {
                StartHostedService => StartGeneratorAsync(stoppingToken),
                StopHostedService => StopGeneratorAsync(stoppingToken),
                _ => Task.CompletedTask
            };
        }

        _logger.LogInformation("Hosted Service is stopping");
    }

    private async Task StartGeneratorAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _runningStatus, 1) == 1)
        {
            _logger.LogInformation("Hosted Service is already running");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();

            await foreach (var element in _generator.StartAsync(scope, cancellationToken))
            {
                await _handler.HandleAsync(element, scope, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _runningStatus, 0);
            _logger.LogCritical(ex, "Generator failed");
        }
    }

    private async Task StopGeneratorAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _runningStatus, 0) == 0)
        {
            _logger.LogInformation("Hosted Service is not running");
            return;
        }

        try
        {
            await _generator.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _runningStatus, 0);
            _logger.LogCritical(ex, "Generator failed");
        }
    }
}