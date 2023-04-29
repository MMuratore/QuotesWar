using QuotesWar.Api.Features.Battles.Models;
using QuotesWar.Infrastructure.Marten;

namespace QuotesWar.Api.Features.Battles.VoteForTheQuote;

internal static class Endpoint
{
    internal static IEndpointRouteBuilder MapVoteForTheQuote(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("battle/{id:guid}/vote",
            async (HttpContext context, LinkGenerator linkGenerator, IEventStoreRepository<Battle> repository,
                Guid quoteId,
                CancellationToken cancellationToken, Guid id) =>
            {
                var battle = await repository.LoadAsync(id, cancellationToken: cancellationToken);
                battle.VoteForTheQuote(quoteId);
                await repository.StoreAsync(battle, cancellationToken);

                return TypedResults.Accepted(GetLocation(context, linkGenerator, id));
            }).WithName("VoteForTheQuote");

        return endpoints;
    }

    private static string? GetLocation(HttpContext context, LinkGenerator linkGenerator, Guid id) =>
        linkGenerator.GetUriByName(context, "GetBattleOfTheDayResults", new {id});

    internal static IEndpointRouteBuilder MapGetBattleOfTheDayResults(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battle/{id:guid}/results",
                async (Guid id, CancellationToken cancellationToken) => { throw new NotImplementedException(); })
            .WithName("GetBattleOfTheDayResults");

        return endpoints;
    }
}