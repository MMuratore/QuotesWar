﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuotesWar.Infrastructure.Persistence;

internal sealed class DatabaseHostedService<TDbContext> : BackgroundService where TDbContext : DbContext
{
    private readonly ILogger<DatabaseHostedService<TDbContext>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private TDbContext? _context;

    public DatabaseHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseHostedService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Applying migrations for {DbContext}", typeof(TDbContext).ShortDisplayName());

        using var scope = _scopeFactory.CreateScope();

        try
        {
            _context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await _context.Database.MigrateAsync(stoppingToken);

            _logger.LogInformation("Migrations completed for {DbContext}", typeof(TDbContext).ShortDisplayName());
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Migrations failed for {DbContext}", typeof(TDbContext).ShortDisplayName());
        }
    }
}