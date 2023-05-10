using System.Threading.Channels;

namespace QuotesWar.Infrastructure.HostedService.Channel;

public class HostedServiceRequestsChannel
{
    public readonly Channel<IHostedServiceRequest> Requests =
        System.Threading.Channels.Channel.CreateUnbounded<IHostedServiceRequest>();

    public string Name { get; set; } = string.Empty;
}