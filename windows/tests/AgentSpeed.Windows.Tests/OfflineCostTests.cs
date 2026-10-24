using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Threading;
using AgentSpeed.Core.Money;
using AgentSpeed.Cost;
using AgentSpeed.Presentation.Localization;
using AgentSpeed.Services;

namespace AgentSpeed.Windows.Tests;

public class OfflineCostTests
{
    [Fact]
    public void LocalRateUpdatesRequireANewServiceAndInvalidStartupDataIsRejected()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var directory = Path.Combine(Path.GetTempPath(), "agentspeed-local-rates-test-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, "rates.json");
                var document = JsonSerializer.SerializeToNode(LocalExchangeRates.Bundled(CurrencyTable.Bundled))!.AsObject();
                document["rates"]!["CNY"] = 7.0m;
                File.WriteAllText(path, document.ToJsonString());
                var dispatcher = Dispatcher.CurrentDispatcher;
                using var settings = new SettingsService(dispatcher);
                settings.MakeEphemeral();
                var localizer = new Localizer(Localizer.LoadEmbeddedTable(), "en");
                var historyPath = Path.Combine(directory, "history.db");
                var service = new CostService(settings, localizer, historyPath, path);
                Assert.Equal(7.0m, service.Display.Rates.Rate("CNY"));
                document["rates"]!["CNY"] = 7.25m;
                File.WriteAllText(path, document.ToJsonString());
                service.OnSettingsChanged(settings.Current, settings.Current with { Cost = settings.Current.Cost with { Currency = "CNY" } });
                Assert.Equal(7.0m, service.Display.Rates.Rate("CNY"));
                var restarted = new CostService(settings, localizer, historyPath, path);
                Assert.Equal(7.25m, restarted.Display.Rates.Rate("CNY"));
                File.WriteAllText(path, "{broken");
                Assert.Equal(7.25m, restarted.Display.Rates.Rate("CNY"));
                Assert.Throws<FormatException>(() => new CostService(settings, localizer, historyPath, path));
                document["rates"]!["CNY"] = 7.5m;
                File.WriteAllText(path, document.ToJsonString());
                var repaired = new CostService(settings, localizer, historyPath, path);
                Assert.Equal(7.5m, repaired.Display.Rates.Rate("CNY"));
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw new InvalidOperationException("Offline cost integration failed.", failure);
    }

    [Theory]
    [InlineData("AgentSpeed.Updates.UpdateService")]
    [InlineData("AgentSpeed.Reporting.ReportController")]
    [InlineData("AgentSpeed.Cost.PriceSyncController")]
    [InlineData("AgentSpeed.Cost.ExchangeRateController")]
    [InlineData("AgentSpeed.Settings.ReportPaneViewModel")]
    public void ApplicationContainsNoNetworkServices(string type) =>
        Assert.Null(typeof(CostService).Assembly.GetType(type));

    [Fact]
    public void ApplicationAndCoreDoNotReferenceHttpClientAssembly()
    {
        foreach (var assembly in new[] { typeof(CostService).Assembly, typeof(LocalExchangeRates).Assembly })
            Assert.DoesNotContain(assembly.GetReferencedAssemblies(), reference => reference.Name == "System.Net.Http");
    }
}
