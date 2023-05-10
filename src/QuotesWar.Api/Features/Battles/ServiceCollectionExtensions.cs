using QuotesWar.Api.Features.Battles.BattleOfTheDay;
using QuotesWar.Api.Features.Battles.GetBattleOfTheDay;
using QuotesWar.Api.Features.Battles.VoteForTheQuote;

namespace QuotesWar.Api.Features.Battles;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBattleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBattleOfTheDay(configuration);

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