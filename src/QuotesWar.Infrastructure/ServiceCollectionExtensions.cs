using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuotesWar.Infrastructure.Marten;
using QuotesWar.Infrastructure.Persistence;

namespace QuotesWar.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<QuoteDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("QuotesWarDatabase")));

        services.AddHostedService<DatabaseHostedService<QuoteDbContext>>();

        services.AddMarten(options =>
            {
                options.Connection(configuration.GetConnectionString("QuotesWarEventDatabase") ??
                                   throw new InvalidOperationException());
                options.DatabaseSchemaName = "event_store";
            })
            .UseLightweightSessions();

        services.AddScoped(typeof(IEventStoreRepository<>), typeof(EventStoreRepository<>));

        return services;
    }
}