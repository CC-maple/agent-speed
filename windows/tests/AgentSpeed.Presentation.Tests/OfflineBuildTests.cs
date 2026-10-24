using System.Text.Json;
using AgentSpeed.Core.Money;
using AgentSpeed.Presentation.Localization;
using AgentSpeed.Presentation.Menus;
using AgentSpeed.Presentation.Settings;

namespace AgentSpeed.Presentation.Tests;

public class OfflineBuildTests
{
    [Fact]
    public void LegacyNetworkSettingsAreIgnoredAndNotWrittenBack()
    {
        var settings = SettingsSerializer.Deserialize("""
            {"network":{"enabled":true},"report":{"enabled":true},"updates":{"autoCheck":true},"cost":{"syncPrices":true,"currency":"CNY"}}
            """);
        using var json = JsonDocument.Parse(SettingsSerializer.Serialize(settings));
        Assert.False(json.RootElement.TryGetProperty("network", out _));
        Assert.False(json.RootElement.TryGetProperty("report", out _));
        Assert.False(json.RootElement.TryGetProperty("updates", out _));
        Assert.False(json.RootElement.GetProperty("cost").TryGetProperty("syncPrices", out _));
        Assert.Equal("CNY", settings.Cost.Currency);
    }

    [Fact]
    public void QuickMenuContainsNoOnlineActions()
    {
        var loc = new Localizer(Localizer.LoadEmbeddedTable(), "en");
        var menu = QuickMenuModel.Build(AppSettings.Default, SourceAvailability.All, loc);
        Assert.DoesNotContain(menu.Rows, row => row.Action is MenuAction.CheckForUpdates or MenuAction.OpenUpdate
            or MenuAction.PauseReporting or MenuAction.ResumeReporting or MenuAction.OpenLeaderboard);
    }

    [Theory]
    [InlineData("AgentSpeed.Core.Network.HttpClientTransport")]
    [InlineData("AgentSpeed.Core.Network.PriceTableSync")]
    [InlineData("AgentSpeed.Core.Network.ExchangeRateSync")]
    [InlineData("AgentSpeed.Core.Reporting.HttpReportTransport")]
    [InlineData("AgentSpeed.Core.Reporting.ReportApi")]
    public void NetworkImplementationsAreAbsentFromTheCoreAssembly(string type) =>
        Assert.Null(typeof(LocalExchangeRates).Assembly.GetType(type));
}
