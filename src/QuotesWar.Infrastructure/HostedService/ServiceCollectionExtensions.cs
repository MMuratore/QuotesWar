using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuotesWar.Infrastructure.HostedService.Channel;

namespace QuotesWar.Infrastructure.HostedService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostedGeneratorService<TGenerator, THandler, TElement>(
        this IServiceCollection services, string name,
        Func<IServiceProvider, TGenerator> generatorImplementationFactory,
        Func<IServiceProvider, THandler> handlerImplementationFactory)
        where TGenerator : class, IAsyncGenerator<TElement>
        where THandler : class, IHostedServiceHandler<TElement>
    {
        services.AddSingleton(new HostedServiceRequestsChannel {Name = name});
        services.AddSingleton(generatorImplementationFactory.Invoke);
        services.AddSingleton(handlerImplementationFactory.Invoke);

        services.Add(ServiceDescriptor.Singleton<IHostedService>(provider =>
        {
            var channel = provider.GetServices<HostedServiceRequestsChannel>().Single(x => x.Name == name);
            var handler = provider.GetServices<THandler>().Single(x => x.Name == name);
            var generator = provider.GetServices<TGenerator>().Single(x => x.Name == name);
            var logger = provider.GetRequiredService<ILogger<HostedService<TGenerator, THandler, TElement>>>();

            return new HostedService<TGenerator, THandler, TElement>(name, channel, generator, handler, provider,
                logger);
        }));

        return services;
    }
}