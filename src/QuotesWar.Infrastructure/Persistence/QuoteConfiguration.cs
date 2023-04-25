using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuotesWar.Infrastructure.Models;

namespace QuotesWar.Infrastructure.Persistence;

internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    private const string DataPath = "..\\..\\data";

    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.Property(q => q.Action).HasConversion<string>();
        builder.Property(q => q.Faction).HasConversion<string>();

        builder.HasData(QuoteSeeder.GetQuotesFromFile($"{DataPath}\\quotes.json"));
    }
}