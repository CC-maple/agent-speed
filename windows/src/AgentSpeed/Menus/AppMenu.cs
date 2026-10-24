using System.Windows.Controls;
using AgentSpeed.Presentation.Localization;
using AgentSpeed.Presentation.Menus;
using AgentSpeed.Presentation.Settings;

namespace AgentSpeed.Menus;

/// <summary>The quick menu of the tray icon and the taskbar widget (DESIGN 2b, Master switch; <see cref="QuickMenuModel"/>).</summary>
internal static class AppMenu
{
    /// <param name="sources">Source availability for the orbs' colour dots; null treats every source as available.</param>
    /// <param name="sourceTraffic">Per-process traffic can be measured now (DESIGN 10).</param>
    public static ContextMenu Build(IAppCommands commands, AppSettings settings, Localizer loc,
        SourceAvailability? sources = null, bool sourceTraffic = true) =>
        MenuBuilder.Build(QuickMenuModel.Build(settings, sources ?? SourceAvailability.All, loc, sourceTraffic), commands, loc);
}
