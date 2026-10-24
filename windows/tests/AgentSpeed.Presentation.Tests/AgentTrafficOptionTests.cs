using AgentSpeed.Core.Traffic;
using AgentSpeed.Presentation.Formatting;
using AgentSpeed.Presentation.Settings;

namespace AgentSpeed.Presentation.Tests;

public class AgentTrafficOptionTests
{
    [Theory]
    [InlineData(false, false, false, ProcessTrafficKind.Off, "off")]
    [InlineData(true, false, false, ProcessTrafficKind.Off, "restart")]
    [InlineData(false, true, true, ProcessTrafficKind.Running, "restart")]
    [InlineData(true, true, false, ProcessTrafficKind.Unavailable, "install")]
    [InlineData(true, true, true, ProcessTrafficKind.Starting, "starting")]
    [InlineData(true, true, true, ProcessTrafficKind.Running, "on")]
    [InlineData(true, true, true, ProcessTrafficKind.Standby, "on")]
    [InlineData(true, true, true, ProcessTrafficKind.Unavailable, "error")]
    public void StatusDistinguishesSavedPreferenceFromActualConnection(bool requested, bool active, bool installed,
        ProcessTrafficKind state, string expected) =>
        Assert.Equal("win.traffic.agents." + expected, AgentTrafficOption.StatusKey(requested, active, installed, state));

    [Fact]
    public void AgentTrafficOptInRoundTripsWithoutDisablingMachineCounters()
    {
        Assert.False(SettingsSerializer.Deserialize("{}").Traffic.AgentTrafficEnabled);
        var settings = AppSettings.Default with { Traffic = TrafficSettings.Default with { AgentTrafficEnabled = true } };
        var restored = SettingsSerializer.Deserialize(SettingsSerializer.Serialize(settings));
        Assert.True(restored.Traffic.Enabled);
        Assert.True(restored.Traffic.AgentTrafficEnabled);
    }
}
