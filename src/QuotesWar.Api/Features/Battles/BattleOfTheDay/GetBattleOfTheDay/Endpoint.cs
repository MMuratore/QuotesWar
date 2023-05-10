using Marten;
using Microsoft.Extensions.Caching.Memory;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models;
using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models.Events;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.GetBattleOfTheDay;

internal static class Endpoint
{
    private const string CacheKey = "battleOfTheDayId";

    internal static IEndpointRouteBuilder MapGetBattleOfTheDay(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battle/{name}",
            async (HttpContext context, IMemoryCache cache, LinkGenerator linkGenerator, IDocumentSession session,
                IEventStoreRepository<Battle> repository, CancellationToken cancellationToken, string name) =>
            {
                var battleId = GetBattleOfTheDayId(cache, session, name);

                if (battleId is null) return Results.NoContent();

                var battle = await repository.LoadAsync(battleId.Value, cancellationToken: cancellationToken);
                var quotes = battle.GetBattleQuotes();
                await repository.StoreAsync(battle, cancellationToken);

                return TypedResults.Accepted(GetLocation(context, linkGenerator, battleId.Value), quotes);
            }).WithName("GetBattleOfTheDay");

        return endpoints;
    }

    private static string? GetLocation(HttpContext context, LinkGenerator linkGenerator, Guid id) =>
        linkGenerator.GetUriByName(context, "VoteForTheQuote", new {id});

    private static Guid? GetBattleOfTheDayId(IMemoryCache cache, IDocumentSession session, string name)
    {
        if (cache.TryGetValue($"{CacheKey}-{name}", out Guid battleId)) return battleId;

        var id = session.Events.QueryRawEventDataOnly<BattleStarted>().OrderByDescending(x => x.OccuredAt)
            .FirstOrDefault(x => x.Name == name)?.BattleId;

        if (id is null) return null;

        battleId = id.Value;

        var closedBattleId = session.Events.QueryRawEventDataOnly<BattleClosed>()
            .FirstOrDefault(x => x.BattleId == battleId)?.BattleId;

        if (closedBattleId is not null) return null;

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
        cache.Set($"{CacheKey}-{name}", battleId, cacheEntryOptions);

        return battleId;
    }
}