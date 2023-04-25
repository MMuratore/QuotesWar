using System.Text.Json;
using System.Text.Json.Serialization;
using QuotesWar.Infrastructure.Models;

namespace QuotesWar.Infrastructure.Persistence;

internal static class QuoteSeeder
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = {new JsonStringEnumConverter()}
    };

    public static IEnumerable<Quote> GetQuotesFromFile(string path)
    {
        using var fileStream = File.OpenRead(path);
        var quotes = JsonSerializer.Deserialize<IEnumerable<Quote>>(fileStream, JsonSerializerOptions) ??
                     throw new InvalidOperationException();

        var quotesFromFile = quotes.ToList();

        return quotesFromFile;
    }
}