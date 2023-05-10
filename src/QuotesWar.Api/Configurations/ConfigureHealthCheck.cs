using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuotesWar.Infrastructure.HostedService;

namespace QuotesWar.Api.Configurations;

public static class ConfigureHealthCheck
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder
            .AddSqlServer(
                configuration.GetConnectionString("QuotesWarDatabase") ?? throw new InvalidOperationException(),
                name: "sqlserver-check",
                tags: new[] {"sqlserver"});

        hcBuilder.AddNpgSql(
            configuration.GetConnectionString("QuotesWarEventDatabase") ?? throw new InvalidOperationException(),
            name: "postgres-check",
            tags: new[] {"postgres"});

        hcBuilder
            .AddCheck<HostedServiceHealthCheck>(
                "hosted-service-check",
                tags: new[] {"hosted"});

        return services;
    }

    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        return endpoints;
    }
}