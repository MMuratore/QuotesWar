namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.GenerateBattle;

public class BattleOfTheDayOptions
{
    public const string Section = "BattleOfTheDay";
    public List<BattleGeneratorOptions> Battles { get; set; } = new();
}

public class BattleGeneratorOptions
{
    public string Name { get; set; } = string.Empty;
    public string Schedule { get; set; } = "*/10 * * * *"; //	At every 10th minute.
    public int NumberOfChallenger { get; set; } = 2;
}