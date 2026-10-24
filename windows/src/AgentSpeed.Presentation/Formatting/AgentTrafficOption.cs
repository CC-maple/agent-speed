using AgentSpeed.Core.Traffic;

namespace AgentSpeed.Presentation.Formatting;

/// <summary>Startup-only agent traffic preference, separate from the machine counters.</summary>
public static class AgentTrafficOption
{
    public static string StatusKey(bool requested, bool active, bool installed, ProcessTrafficKind state)
    {
        if (requested != active) return "win.traffic.agents.restart";
        if (!active) return "win.traffic.agents.off";
        if (!installed) return "win.traffic.agents.install";
        return state switch
        {
            ProcessTrafficKind.Running or ProcessTrafficKind.Standby => "win.traffic.agents.on",
            ProcessTrafficKind.Starting => "win.traffic.agents.starting",
            _ => "win.traffic.agents.error",
        };
    }
}
