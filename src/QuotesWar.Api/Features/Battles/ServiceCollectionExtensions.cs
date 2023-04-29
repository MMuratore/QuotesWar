using QuotesWar.Api.Features.Battles.GenerateBattle;
using QuotesWar.Api.Features.Battles.GetBattleOfTheDay;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Api.Features.Battles.VoteForTheQuote;

namespace QuotesWar.Api.Features.Battles;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBattleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<BattleOfTheDayHostedService>();
        services.AddSingleton<BattleOfTheDayRequestsChannel>();
        services.AddSingleton<BattleOfTheDayHealthCheck>();

        services.AddSingleton<IAsyncGenerator<IEnumerable<Challenger>>, ChallengersGenerator>();
        services.Configure<BattleOfTheDayOptions>(configuration.GetSection(BattleOfTheDayOptions.Section));

        return services;
    }

    public static IEndpointRouteBuilder MapBattleModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGenerateBattle();
        endpoints.MapGetBattleOfTheDay();
        endpoints.MapVoteForTheQuote();
        endpoints.MapGetBattleOfTheDayResults();

        return endpoints;
    }
}