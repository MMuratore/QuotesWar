using QuotesWar.Api.Features.Battles.BattleOfTheDay;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.GenerateBattle;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.GetBattleOfTheDay;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.VoteForTheQuote;

namespace QuotesWar.Api.Features.Battles;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBattleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBattleOfTheDay(configuration);

        return services;
    }

    public static async Task<IApplicationBuilder> UseBattleModuleAsync(this IApplicationBuilder app)
    {
        await app.USeBattleOfTheDayAsync();

        return app;
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