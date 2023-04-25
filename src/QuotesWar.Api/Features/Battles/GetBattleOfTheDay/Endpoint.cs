using Microsoft.Extensions.Caching.Memory;

namespace QuotesWar.Api.Features.Battles.GetBattleOfTheDay;

internal static class Endpoint
{
    private const string CacheKey = "battleOfTheDay";

    internal static IEndpointRouteBuilder MapGetBattleOfTheDay(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battle", async (IMemoryCache cache, CancellationToken cancellationToken) =>
        {
            if (!cache.TryGetValue(CacheKey, out IReadOnlyCollection<BattleQuote>? quotes))
            {
                quotes = await GetDayBattle(cancellationToken);
                ;

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                cache.Set(CacheKey, quotes, cacheEntryOptions);
            }

            return quotes;
        });

        return endpoints;
    }

    private static Task<IReadOnlyCollection<BattleQuote>> GetDayBattle(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}