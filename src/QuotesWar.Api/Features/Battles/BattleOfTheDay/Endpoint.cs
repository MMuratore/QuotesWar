using QuotesWar.Infrastructure.HostedService.Channel;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay;

internal static class Endpoint
{
    internal static IEndpointRouteBuilder MapGenerateBattle(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battle/generator", (IServiceProvider provider) =>
        {
            var services = provider.GetServices<HostedServiceRequestsChannel>();
            return TypedResults.Ok(services.Select(x => x.Name));
        }).WithName("GeneratorBattleOfTheDay");

        endpoints.MapPost("battle/generator/{name}/start",
            async (IServiceProvider provider, string name, CancellationToken cancellationToken) =>
            {
                var services = provider.GetServices<HostedServiceRequestsChannel>();
                var channel = services.Single(x => x.Name == name);

                await channel.Requests.Writer.WriteAsync(new StartHostedService(), cancellationToken);
                return Results.Accepted();
            }).WithName("StartBattleOfTheDay");

        endpoints.MapPost("battle/generator/{name}/stop",
            async (IServiceProvider provider, string name, CancellationToken cancellationToken) =>
            {
                var services = provider.GetServices<HostedServiceRequestsChannel>();
                var channel = services.Single(x => x.Name == name);

                await channel.Requests.Writer.WriteAsync(new StopHostedService(), cancellationToken);
                return Results.Accepted();
            }).WithName("StopBattleOfTheDay");

        return endpoints;
    }
}