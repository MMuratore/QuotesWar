using Microsoft.Extensions.Options;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.GenerateBattle;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models;
using QuotesWar.Infrastructure.HostedService;
using QuotesWar.Infrastructure.HostedService.Channel;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBattleOfTheDay(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new BattleOfTheDayOptions();

        var section = configuration.GetSection(BattleOfTheDayOptions.Section);
        section.Bind(options);
        services.Configure<BattleOfTheDayOptions>(section);

        foreach (var battle in options.Battles)
            services.AddHostedGeneratorService<ChallengersGenerator, BattleHandler, IEnumerable<Challenger>>(
                battle.Name, provider => new ChallengersGenerator(battle.Name, battle,
                    provider.GetRequiredService<ILogger<ChallengersGenerator>>()), provider =>
                    new BattleHandler(battle.Name,
                        provider.GetRequiredService<ILogger<BattleHandler>>()));

        return services;
    }

    public static async Task<IApplicationBuilder> USeBattleOfTheDayAsync(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<BattleOfTheDayOptions>>().Value;
        var channels = app.ApplicationServices.GetServices<HostedServiceRequestsChannel>().ToList();

        foreach (var channel in from battle in options.Battles
                 where battle.AutoStart
                 select channels.Single(x => x.Name == battle.Name))
        {
            await channel.Requests.Writer.WriteAsync(new StartHostedService());
        }

        return app;
    }
}