using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Infrastructure.HostedService;

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
        {
            services.AddGeneratorService<ChallengersGenerator, BattleHandler, IEnumerable<Challenger>>(
                battle.Name,
                new ChallengersGenerator(battle,));
        }

        return services;
    }
}