using AgentSpeed.App;
using AgentSpeed.Core;
using AgentSpeed.Core.Money;
using AgentSpeed.Core.Pricing;
using AgentSpeed.Presentation.Cost;
using AgentSpeed.Presentation.Localization;
using AgentSpeed.Presentation.Settings;
using AgentSpeed.Services;

namespace AgentSpeed.Cost;

/// <summary>Loads local exchange rates once at startup. Restart after an agent updates the rates file.</summary>
internal sealed class CostService
{
    public CostService(SettingsService settings, Localizer localizer, string historyStorePath, string? ratesFile = null)
    {
        Book = new PriceBook([PriceBook.Bundled]);
        HistoryService.RestorePricing(Book, historyStorePath);
        var custom = ratesFile ?? Environment.GetEnvironmentVariable("AGENT_SPEED_RATES_FILE");
        RatesFile = custom ?? Path.Combine(AppDataDirectory, "rates", "local-rates.json");
        if (!Path.IsPathFullyQualified(RatesFile) || RatesFile.StartsWith(@"\\", StringComparison.Ordinal))
            throw new ArgumentException("AGENT_SPEED_RATES_FILE must be an absolute local file path.");
        if (custom is null && !File.Exists(RatesFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RatesFile)!);
            using var stream = new FileStream(RatesFile, FileMode.CreateNew, FileAccess.Write);
            System.Text.Json.JsonSerializer.Serialize(stream, LocalExchangeRates.Bundled(CurrencyTable.Bundled));
        }
        var json = File.ReadAllText(RatesFile);
        var rates = LocalExchangeRates.Parse(json, CurrencyTable.Bundled);
        Display = new CostDisplay(localizer, CurrencyTable.Bundled, rates.ToRatesInUse(CurrencyTable.Bundled)) { Settings = settings.Current.Cost };
        RatesDate = rates.Date;
        Display.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler? Changed;
    public static string AppDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppPaths.ProductFolder);
    public PriceBook Book { get; }
    public CostDisplay Display { get; }
    public string RatesFile { get; }
    public string RatesDate { get; }
    public void OnSettingsChanged(AppSettings previous, AppSettings current) => Display.Settings = current.Cost;
}
