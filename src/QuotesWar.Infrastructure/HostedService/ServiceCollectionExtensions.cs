using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuotesWar.Infrastructure.HostedService.Channel;

namespace QuotesWar.Infrastructure.HostedService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratorService<TGenerator, THandler, TElement>(
        this IServiceCollection services, string name)
        where TGenerator : class, IAsyncGenerator<TElement>
        where THandler : class, IHostedServiceHandler<TElement>
    {
        services.AddSingleton<TGenerator>();
        services.AddSingleton<THandler>();

        services.AddSingleton(new HostedServiceRequestsChannel {Name = name});

        services.AddHostedService<HostedService<TGenerator, THandler, TElement>>(provider =>
        {
            var channels = provider.GetServices<HostedServiceRequestsChannel>();
            var handler = provider.GetRequiredService<THandler>();
            var generator = provider.GetRequiredService<TGenerator>();
            var logger = provider.GetRequiredService<ILogger<HostedService<TGenerator, THandler, TElement>>>();

            return new HostedService<TGenerator, THandler, TElement>(name, channels.Single(x => x.Name == name),
                generator, handler, provider, logger);
        });

        return services;
    }

    public static IServiceCollection AddGeneratorService<TGenerator, THandler, TElement>(
        this IServiceCollection services, string name,
        Func<IServiceProvider, TGenerator> generatorImplementationFactory,
        Func<IServiceProvider, THandler> handlerImplementationFactory)
        where TGenerator : class, IAsyncGenerator<TElement>
        where THandler : class, IHostedServiceHandler<TElement>
    {
        services.AddSingleton(new HostedServiceRequestsChannel {Name = name});
        services.AddSingleton(generatorImplementationFactory.Invoke);
        services.AddSingleton(handlerImplementationFactory.Invoke);


        services.AddHostedService<HostedService<TGenerator, THandler, TElement>>(provider =>
        {
            var channel = provider.GetServices<HostedServiceRequestsChannel>().Single(x => x.Name == name);
            var handler = provider.GetServices<THandler>().Single(x => x.Name == name);
            var generator = provider.GetServices<TGenerator>().Single(x => x.Name == name);
            ;
            var logger = provider.GetRequiredService<ILogger<HostedService<TGenerator, THandler, TElement>>>();

            return new HostedService<TGenerator, THandler, TElement>(name, channel, generator, handler, provider,
                logger);
        });

        return services;
    }
}