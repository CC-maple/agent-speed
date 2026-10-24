using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentSpeed.Core.Money;

/// <summary>Complete, USD-based exchange rates supplied through a local JSON file.</summary>
public sealed record LocalExchangeRates(
    [property: JsonPropertyName("base")] string Base,
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("rates")] IReadOnlyDictionary<string, decimal> Rates)
{
    public static LocalExchangeRates Bundled(CurrencyTable currencies) => new(
        "USD", DateTimeOffset.FromUnixTimeMilliseconds(currencies.FallbackRates.DateMs).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        currencies.FallbackRates.Rates);

    /// <summary>Rejects incomplete, duplicated or invalid data rather than guessing missing rates.</summary>
    public static LocalExchangeRates Parse(string json, CurrencyTable currencies)
    {
        if (json.Length > 256 * 1024) throw new FormatException("Exchange rate file exceeds 256 KiB.");
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new FormatException("Expected a JSON object.");
            var members = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in root.EnumerateObject())
                if (!members.Add(member.Name) || member.Name is not ("base" or "date" or "rates"))
                    throw new FormatException("Duplicate or unknown exchange rate field: " + member.Name);
            if (!root.TryGetProperty("base", out var basis) || basis.GetString() != "USD")
                throw new FormatException("Exchange rate base must be USD.");
            if (!root.TryGetProperty("date", out var date) || !DateOnly.TryParseExact(date.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new FormatException("Exchange rate date must be yyyy-MM-dd.");
            if (!root.TryGetProperty("rates", out var data) || data.ValueKind != JsonValueKind.Object)
                throw new FormatException("Expected a rates object.");
            var rates = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var item in data.EnumerateObject())
            {
                if (currencies.Find(item.Name) is null || !item.Value.TryGetDecimal(out var value) || value <= 0 || value > 1_000_000_000_000m || !rates.TryAdd(item.Name, value))
                    throw new FormatException("Invalid or duplicate exchange rate: " + item.Name);
            }
            if (!rates.TryGetValue("USD", out var usd) || usd != 1m)
                throw new FormatException("USD exchange rate must equal 1.");
            foreach (var currency in currencies.Currencies)
                if (!rates.ContainsKey(currency.Code)) throw new FormatException("Missing exchange rate: " + currency.Code);
            return new LocalExchangeRates("USD", date.GetString()!, rates);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new FormatException("Invalid exchange rate JSON.", ex);
        }
    }

    public RatesInUse ToRatesInUse(CurrencyTable currencies)
    {
        var date = DateTimeOffset.ParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        return new RatesInUse(new ExchangeRateSet("local", string.Empty, date.ToUnixTimeMilliseconds(),
            date.ToUnixTimeMilliseconds(), Rates), currencies.FallbackRates);
    }
}
