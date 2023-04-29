namespace QuotesWar.Api.Features.Battles.GenerateBattle;

internal static class Endpoint
{
    internal static IEndpointRouteBuilder MapGenerateBattle(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("battle/start",
            async (BattleOfTheDayRequestsChannel channel, CancellationToken cancellationToken) =>
            {
                await channel.Requests.Writer.WriteAsync(new StartBattle(), cancellationToken);
                return Results.Accepted();
            }).WithName("StartBattleOfTheDay");

        endpoints.MapPost("battle/stop",
            async (BattleOfTheDayRequestsChannel channel, CancellationToken cancellationToken) =>
            {
                await channel.Requests.Writer.WriteAsync(new StopBattle(), cancellationToken);
                return Results.Accepted();
            }).WithName("StopBattleOfTheDay");

        return endpoints;
    }
}