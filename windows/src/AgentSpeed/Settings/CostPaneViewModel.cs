using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AgentSpeed.App;
using AgentSpeed.Controls;
using AgentSpeed.Core;
using AgentSpeed.Core.Pricing;
using AgentSpeed.Cost;
using AgentSpeed.Presentation.Colors;
using AgentSpeed.Presentation.Cost;
using AgentSpeed.Presentation.Formatting;
using AgentSpeed.Presentation.Layout;
using AgentSpeed.Presentation.Settings;
using AgentSpeed.Services;
using AgentSpeed.Settings.Cost;
using AgentSpeed.Theme;

namespace AgentSpeed.Settings;

/// <summary>A detected source in the Subscriptions group (DESIGN 9): colour dot, name and the subscription switch.</summary>
internal sealed class SubscriptionRowViewModel(SourceKind kind, Action<SourceKind, bool> apply) : ObservableObject
{
    private bool _isSubscription;
    private bool _syncing;
    private string _name = string.Empty;
    private string _accessibleName = string.Empty;

    public SourceKind Kind { get; } = kind;

    public Brush Dot { get; } = MediaColors.Frozen(MediaColors.ToColor(HeatPalette.SourceColor(kind)));

    public string Name { get => _name; private set => Set(ref _name, value); }

    /// <summary><c>Codex, 訂閱</c>: the switch has no visible label of its own.</summary>
    public string AccessibleName { get => _accessibleName; private set => Set(ref _accessibleName, value); }

    public bool IsSubscription
    {
        get => _isSubscription;
        set
        {
            if (Set(ref _isSubscription, value) && !_syncing)
            {
                apply(Kind, value);
            }
        }
    }

    public void Refresh(bool marked, string name, string switchLabel)
    {
        _syncing = true;
        try
        {
            IsSubscription = marked;
        }
        finally
        {
            _syncing = false;
        }

        Name = name;
        AccessibleName = $"{name}, {switchLabel}";
    }
}

/// <summary>
/// Settings → 費用 (DESIGN 9, Settings → Cost): currency and rate status, the live value unit, subscription marks and
/// the price table. Also the live value control of the taskbar page. Every change writes through
/// <see cref="SettingsService"/>; the app re-renders every surface from <see cref="CostService.Changed"/>.
/// </summary>
internal sealed class CostPaneViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly CostService _cost;
    private SpeedSnapshot _snapshot = SpeedSnapshot.Empty;
    private string _currencyCode = string.Empty;
    private string _currencyName = string.Empty;
    private string _currencyAccessibleName = string.Empty;
    private string _liveUnitCostLabel = string.Empty;
    private string _unknownToday = string.Empty;
    private bool _hasSubscriptionRows;

    public CostPaneViewModel(SettingsService settings, CostService cost)
    {
        _settings = settings;
        _cost = cost;
        Picker = new CurrencyPickerViewModel(cost.Display, SelectCurrency);
        settings.Changed += (_, _) => Refresh();
        cost.Changed += (_, _) => Refresh();
        Refresh();
    }

    public CurrencyPickerViewModel Picker { get; }
    public string RatesFile => _cost.RatesFile;
    public string LocalRatesStatus => Display.Localizer.Format("win.rates.localDate", _cost.RatesDate);
    public string BundledPricesStatus => Display.Localizer["win.prices.local"];

    private CostDisplay Display => _cost.Display;

    private AppSettings S => _settings.Current;

    // Currency

    public string CurrencyCode { get => _currencyCode; private set => Set(ref _currencyCode, value); }

    public string CurrencyName { get => _currencyName; private set => Set(ref _currencyName, value); }

    /// <summary><c>貨幣, TWD, 新臺幣</c>.</summary>
    public string CurrencyAccessibleName { get => _currencyAccessibleName; private set => Set(ref _currencyAccessibleName, value); }

    // Live value

    public bool LiveUnitIsTokens
    {
        get => S.Cost.LiveUnit == LiveUnit.Tokens;
        set
        {
            if (value)
            {
                SetLiveUnit(LiveUnit.Tokens);
            }
        }
    }

    public bool LiveUnitIsCost
    {
        get => S.Cost.LiveUnit == LiveUnit.Cost;
        set
        {
            if (value)
            {
                SetLiveUnit(LiveUnit.Cost);
            }
        }
    }

    /// <summary><c>Cost ($/s)</c>.</summary>
    public string LiveUnitCostLabel { get => _liveUnitCostLabel; private set => Set(ref _liveUnitCostLabel, value); }

    // The taskbar's live value (DESIGN 3, 9): tok/s, money, or tok/s with the money rate after it (stored as liveUnit
    // tokens with barSecondary; choosing money clears barSecondary).

    public bool BarValueTokens
    {
        get => S.Cost.LiveUnit == LiveUnit.Tokens && !S.BarSecondary;
        set => SetBarValue(value, LiveUnit.Tokens, secondary: false);
    }

    public bool BarValueCost
    {
        get => S.Cost.LiveUnit == LiveUnit.Cost;
        set => SetBarValue(value, LiveUnit.Cost, secondary: false);
    }

    public bool BarValueBoth
    {
        get => S.Cost.LiveUnit == LiveUnit.Tokens && S.BarSecondary;
        set => SetBarValue(value, LiveUnit.Tokens, secondary: true);
    }

    private void SetBarValue(bool selected, LiveUnit unit, bool secondary)
    {
        if (selected)
        {
            _settings.Update(s => s with { BarSecondary = secondary, Cost = s.Cost with { LiveUnit = unit } });
        }
    }

    // Subscriptions

    /// <summary>One row per detected source, in source order.</summary>
    public ObservableCollection<SubscriptionRowViewModel> Subscriptions { get; } = new();

    public bool HasSubscriptionRows { get => _hasSubscriptionRows; private set => Set(ref _hasSubscriptionRows, value); }

    public bool IncludeSubscriptions
    {
        get => S.Cost.IncludeSubscriptions;
        set => _settings.Update(s => s with { Cost = s.Cost with { IncludeSubscriptions = value } });
    }

    // Price table

    /// <summary><c>prices.unknownToday</c>; empty when every model of today has a price.</summary>
    public string UnknownToday { get => _unknownToday; private set => Set(ref _unknownToday, value); }

    public void UpdateLive(SpeedSnapshot snapshot)
    {
        _snapshot = snapshot;
        RefreshSubscriptions();
        UnknownToday = CostPaneText.UnknownToday(CostPaneText.UnpricedModels(snapshot.Sources), Display.Localizer);
    }

    public void Relocalize() => Refresh();

    private void Refresh()
    {
        var display = Display;
        CurrencyCode = display.Currency.Code;
        CurrencyName = display.CurrencyName;
        CurrencyAccessibleName = CostPaneText.CurrencyButtonName(display);
        LiveUnitCostLabel = CostPaneText.LiveUnitCost(display);
        RefreshSubscriptions();
        UnknownToday = CostPaneText.UnknownToday(CostPaneText.UnpricedModels(_snapshot.Sources), display.Localizer);
        foreach (var name in new[]
                 {
                     nameof(LiveUnitIsTokens), nameof(LiveUnitIsCost), nameof(IncludeSubscriptions),
                     nameof(BarValueTokens), nameof(BarValueCost), nameof(BarValueBoth),
                 })
        {
            OnPropertyChanged(name);
        }

        OnPropertyChanged(nameof(LocalRatesStatus));
        OnPropertyChanged(nameof(RatesFile));
        CommandManager.InvalidateRequerySuggested();
    }

    private void RefreshSubscriptions()
    {
        var loc = Display.Localizer;
        var detected = Enum.GetValues<SourceKind>()
            .Where(kind => _snapshot.Sources.FirstOrDefault(s => s.Kind == kind) is { } status && SourceDetection.IsDetected(status))
            .ToList();
        if (!detected.SequenceEqual(Subscriptions.Select(row => row.Kind)))
        {
            Subscriptions.Clear();
            foreach (var kind in detected)
            {
                Subscriptions.Add(new SubscriptionRowViewModel(kind, SetSubscription));
            }
        }

        var label = loc["settings.cost.subscription"];
        foreach (var row in Subscriptions)
        {
            row.Refresh(S.Cost.IsSubscription(row.Kind), loc[DisplayText.SourceKey(row.Kind)], label);
        }

        HasSubscriptionRows = Subscriptions.Count > 0;
    }

    private void SelectCurrency(string code)
    {
        if (CurrencyCodeIsKnown(code))
        {
            _settings.Update(s => s with { Cost = s.Cost with { Currency = code } });
        }
    }

    private bool CurrencyCodeIsKnown(string code) => Display.Currencies.Find(code) is not null;

    private void SetLiveUnit(LiveUnit unit) => _settings.Update(s => s with { Cost = s.Cost with { LiveUnit = unit } });

    private void SetSubscription(SourceKind kind, bool marked) =>
        _settings.Update(s => s with { Cost = s.Cost.WithSubscription(kind, marked) });

}
