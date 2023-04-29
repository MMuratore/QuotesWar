namespace QuotesWar.Api.Features.Battles.GenerateBattle;

public class BattleOfTheDayOptions
{
    public const string Section = "BattleOfTheDay";
    public string Schedule { get; set; } = "*/10 * * * *"; //	At every 10th minute.
    public int NumberOfChallenger { get; set; } = 2;
}