using Microsoft.Extensions.Caching.Memory;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.VoteForTheQuote;

internal static class Endpoint
{
    private const string CacheKey = "battleOfTheDayId";

    internal static IEndpointRouteBuilder MapVoteForTheQuote(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("battle/vote",
            async (IMemoryCache cache, IEventStoreRepository<Battle> repository, Guid quoteId,
                CancellationToken cancellationToken) =>
            {
                if (!cache.TryGetValue(CacheKey, out Guid battleId))
                {
                    battleId = GetDayBattleId();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    cache.Set(CacheKey, battleId, cacheEntryOptions);
                }

                var battle = await repository.LoadAsync(battleId, cancellationToken: cancellationToken);
                battle.VoteForTheQuote(quoteId);
                await repository.StoreAsync(battle, cancellationToken);

                return TypedResults.Accepted($"battle/{battleId}/results");
            });

        return endpoints;
    }

    private static Guid GetDayBattleId()
    {
        throw new NotImplementedException();
    }

    internal static IEndpointRouteBuilder MapGetBattleOfTheDayResults(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battle/{id:guid}/results",
            async (Guid id, CancellationToken cancellationToken) => { throw new NotImplementedException(); });

        return endpoints;
    }
}