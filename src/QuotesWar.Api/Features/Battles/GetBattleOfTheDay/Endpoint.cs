using Marten;
using Microsoft.Extensions.Caching.Memory;
using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Api.Features.Battles.Models.Events;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.GetBattleOfTheDay;

internal static class Endpoint
{
    private const string CacheKey = "battleOfTheDayId";

    internal static IEndpointRouteBuilder MapGetBattleOfTheDay(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battle",
            async (HttpContext context, IMemoryCache cache, LinkGenerator linkGenerator, IDocumentSession session,
                IEventStoreRepository<Battle> repository, CancellationToken cancellationToken) =>
            {
                var battleId = GetBattleOfTheDayId(cache, session);

                var battle = await repository.LoadAsync(battleId, cancellationToken: cancellationToken);
                var quotes = battle.GetBattleQuotes();
                await repository.StoreAsync(battle, cancellationToken);

                return TypedResults.Accepted(GetLocation(context, linkGenerator, battleId), quotes);
            }).WithName("GetBattleOfTheDay");

        return endpoints;
    }

    private static string? GetLocation(HttpContext context, LinkGenerator linkGenerator, Guid id) =>
        linkGenerator.GetUriByName(context, "VoteForTheQuote", new {id});

    private static Guid GetBattleOfTheDayId(IMemoryCache cache, IDocumentSession session)
    {
        if (cache.TryGetValue(CacheKey, out Guid battleId)) return battleId;

        battleId = session.Events.QueryRawEventDataOnly<BattleStarted>().OrderByDescending(x => x.OccuredAt)
            .Select(x => x.BattleId)
            .FirstOrDefault();

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
        cache.Set(CacheKey, battleId, cacheEntryOptions);

        return battleId;
    }
}