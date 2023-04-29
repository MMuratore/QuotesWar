using QuotesWar.Api.Features.Battles.GenerateBattle;
using QuotesWar.Api.Features.Battles.GetBattleOfTheDay;
using QuotesWar.Api.Features.Battles.VoteForTheQuote;

namespace QuotesWar.Api.Features.Battles;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBattleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BattleOfTheDayOptions>(configuration.GetSection(BattleOfTheDayOptions.Section));
        services.AddHostedService<BattleOfTheDayHostedService>();
        services.AddSingleton<BattleOfTheDayHostedService>();
        services.AddSingleton<BattleOfTheDayHealthCheck>();

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