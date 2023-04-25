namespace QuotesWar.Infrastructure.Persistence.Entities;

public class Quote
{
    public Guid Id { get; set; }
    public required Faction Faction { get; set; }
    public required string Unit { get; set; }
    public required Action Action { get; set; }
    public required string Value { get; set; }
}