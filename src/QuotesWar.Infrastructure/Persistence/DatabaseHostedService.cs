using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuotesWar.Infrastructure.Persistence;

internal sealed class DatabaseHostedService<TDbContext> : IHostedService where TDbContext : DbContext
{
    private readonly ILogger<DatabaseHostedService<TDbContext>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private TDbContext? _context;

    public DatabaseHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseHostedService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying migrations for {DbContext}", typeof(TDbContext).ShortDisplayName());

        var scope = _scopeFactory.CreateScope();

        try
        {
            _context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await _context.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Migrations completed for {DbContext}", typeof(TDbContext).ShortDisplayName());
        }
        finally
        {
            if (scope is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                scope.Dispose();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}