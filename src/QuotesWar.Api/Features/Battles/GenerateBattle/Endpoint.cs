namespace QuotesWar.Api.Features.Battles.GenerateBattle;

internal static class Endpoint
{
    internal static IEndpointRouteBuilder MapGenerateBattle(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("battle/start",
            async (BattleOfTheDayHostedService hostedService, CancellationToken cancellationToken) =>
            {
                await hostedService.StartAsync(cancellationToken);

                return Results.Accepted();
            }).WithName("StartBattleOfTheDay");

        endpoints.MapPost("battle/stop",
            async (BattleOfTheDayHostedService hostedService, CancellationToken cancellationToken) =>
            {
                await hostedService.StopAsync(cancellationToken);

                return Results.Accepted();
            }).WithName("StopBattleOfTheDay");

        return endpoints;
    }
}