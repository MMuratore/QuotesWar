using System.Threading.Channels;

namespace QuotesWar.Api.Features.Battles.GenerateBattle;

public class BattleOfTheDayRequestsChannel
{
    public readonly Channel<IBattleRequest> Requests = Channel.CreateUnbounded<IBattleRequest>();
}