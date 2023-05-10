using QuotesWar.Api.Features.Battles.BattleOfTheDay.Models;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.VoteForTheQuote;

internal static class Endpoint
{
    internal static IEndpointRouteBuilder MapVoteForTheQuote(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("battles/{id:guid}/vote",
                async (HttpContext context, LinkGenerator linkGenerator, IEventStoreRepository<Battle> repository,
                    Guid quoteId,
                    CancellationToken cancellationToken, Guid id) =>
                {
                    var battle = await repository.LoadAsync(id, cancellationToken: cancellationToken);
                    battle.VoteForTheQuote(quoteId);
                    await repository.StoreAsync(battle, cancellationToken);

                    return TypedResults.Accepted(GetLocation(context, linkGenerator, id));
                })
            .WithName("VoteForTheQuote")
            .WithSummary("Vote for your prefer quote of the battle")
            .WithOpenApi();

        return endpoints;
    }

    private static string? GetLocation(HttpContext context, LinkGenerator linkGenerator, Guid id) =>
        linkGenerator.GetUriByName(context, "GetBattleOfTheDayResults", new {id});

    internal static IEndpointRouteBuilder MapGetBattleOfTheDayResults(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battles/{id:guid}/results",
                async (Guid id, IEventStoreRepository<Battle> repository, CancellationToken cancellationToken) =>
                {
                    var battle = await repository.LoadAsync(id, cancellationToken: cancellationToken);

                    return TypedResults.Ok(battle.Challengers.Select(x => new {x.Quote, x.Score}));
                })
            .WithName("GetBattleOfTheDayResults")
            .WithSummary("Gets battle vote results for every quote")
            .WithOpenApi();

        return endpoints;
    }
}