using System.Text.Json;
using System.Text.Json.Nodes;
using AgentSpeed.Core.Money;

namespace AgentSpeed.Core.Tests;

public class LocalExchangeRatesTests
{
    private static readonly CurrencyTable Currencies = CurrencyTable.Bundled;
    private static JsonObject Document() => JsonSerializer.SerializeToNode(LocalExchangeRates.Bundled(Currencies))!.AsObject();

    [Fact]
    public void UpdatedLocalRateIsUsedForConversion()
    {
        var json = Document();
        json["date"] = "2026-10-24";
        json["rates"]!["CNY"] = 7.25m;
        var local = LocalExchangeRates.Parse(json.ToJsonString(), Currencies);
        Assert.Equal(7.25m, local.ToRatesInUse(Currencies).Rate("CNY"));
        Assert.Equal(1m, local.ToRatesInUse(Currencies).Rate("USD"));
        Assert.Equal("2026-10-24", local.Date);
    }

    [Theory]
    [InlineData("base", "EUR")]
    [InlineData("date", "2026-02-30")]
    [InlineData("date", "24/10/2026")]
    public void InvalidMetadataIsRejected(string key, string value)
    {
        var json = Document();
        json[key] = value;
        Assert.Throws<FormatException>(() => LocalExchangeRates.Parse(json.ToJsonString(), Currencies));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000000001)]
    public void InvalidRatesAreRejected(long value)
    {
        var json = Document();
        json["rates"]!["CNY"] = value;
        Assert.Throws<FormatException>(() => LocalExchangeRates.Parse(json.ToJsonString(), Currencies));
    }

    [Fact]
    public void MissingCurrenciesAndNonUnitDollarAreRejected()
    {
        var json = Document();
        json["rates"]!.AsObject().Remove("CNY");
        Assert.Throws<FormatException>(() => LocalExchangeRates.Parse(json.ToJsonString(), Currencies));
        json = Document();
        json["rates"]!["USD"] = 2;
        Assert.Throws<FormatException>(() => LocalExchangeRates.Parse(json.ToJsonString(), Currencies));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{\"base\":\"USD\",\"base\":\"USD\"}")]
    [InlineData("{\"base\":7}")]
    public void MalformedOrDuplicateFieldsAreRejected(string json) =>
        Assert.Throws<FormatException>(() => LocalExchangeRates.Parse(json, Currencies));
}
