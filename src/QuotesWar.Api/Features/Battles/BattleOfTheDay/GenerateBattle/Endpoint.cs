using QuotesWar.Infrastructure.HostedService;
using QuotesWar.Infrastructure.HostedService.Channel;

namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.GenerateBattle;

internal static class Endpoint
{
    internal static IEndpointRouteBuilder MapGenerateBattle(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("battles", (IServiceProvider provider) =>
            {
                var services = provider.GetServices<IHostedService>().OfType<IGeneratorService>();

                return TypedResults.Ok(services.Select(x => new
                    {x.Name, Status = x.RunningStatus == 1 ? "Running" : "Stopped"}));
            })
            .WithName("GeneratorBattleOfTheDay")
            .WithSummary("Gets all battle generator status")
            .WithOpenApi();

        endpoints.MapPost("battles/{name}/start",
                async (IServiceProvider provider, string name, CancellationToken cancellationToken) =>
                {
                    var services = provider.GetServices<HostedServiceRequestsChannel>();
                    var channel = services.Single(x => x.Name == name);

                    await channel.Requests.Writer.WriteAsync(new StartHostedService(), cancellationToken);
                    return Results.Accepted();
                })
            .WithName("StartBattleOfTheDay")
            .WithSummary("Start battle generation")
            .WithOpenApi();

        endpoints.MapPost("battles/{name}/stop",
                async (IServiceProvider provider, string name, CancellationToken cancellationToken) =>
                {
                    var services = provider.GetServices<HostedServiceRequestsChannel>();
                    var channel = services.Single(x => x.Name == name);

                    await channel.Requests.Writer.WriteAsync(new StopHostedService(), cancellationToken);
                    return Results.Accepted();
                })
            .WithName("StopBattleOfTheDay")
            .WithSummary("Stop battle generation")
            .WithOpenApi();

        return endpoints;
    }
}