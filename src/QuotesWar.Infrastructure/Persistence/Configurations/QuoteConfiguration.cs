using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuotesWar.Infrastructure.Persistence.Entities;
using QuotesWar.Infrastructure.Persistence.Seeds;

namespace QuotesWar.Infrastructure.Persistence.Configurations;

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