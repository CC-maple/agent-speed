namespace AgentSpeed.Core.Money;

/// <summary>The rates every displayed amount uses (METRICS 9.8): the last accepted rates, else the bundled fallback.</summary>
public sealed class RatesInUse
{
    public const long StaleAfterMs = 7 * ExchangeRates.DayMs;

    private readonly IReadOnlyDictionary<string, decimal> _fallback;

    public RatesInUse(ExchangeRateSet? stored, FallbackRates fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        _fallback = fallback.Rates;
        if (stored is not null)
        {
            Rates = stored.Rates;
            DateMs = stored.DateMs;
            Provider = stored.Provider;
            FetchedMs = stored.FetchedMs;
        }
        else
        {
            Rates = fallback.Rates;
            DateMs = fallback.DateMs;
        }
    }

    public IReadOnlyDictionary<string, decimal> Rates { get; }

    /// <summary>The rates' own date (Unix ms).</summary>
    public long DateMs { get; }

    /// <summary>The provider's host; null for the bundled fallback.</summary>
    public string? Provider { get; }

    public long? FetchedMs { get; }

    public bool IsFallback => Provider is null;

    public bool NeedsAttribution => Provider == ExchangeRates.ErApiHost;

    /// <summary>Units of <paramref name="code"/> per 1 USD (USD = 1); a currency the accepted list lacks takes its bundled rate.</summary>
    public decimal Rate(string code)
    {
        if (code == Currency.UsdCode)
        {
            return 1;
        }

        return Rates.TryGetValue(code, out var rate) ? rate : _fallback.TryGetValue(code, out var bundled) ? bundled : 1;
    }

    /// <summary>Older than 7 days by the rates' date (<c>rates.stale</c>); conversion continues with them.</summary>
    public bool IsStale(long nowMs) => nowMs - DateMs > StaleAfterMs;
}
