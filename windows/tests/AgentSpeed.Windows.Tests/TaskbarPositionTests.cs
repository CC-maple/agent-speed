using System.Windows;
using System.Windows.Controls;
using AgentSpeed.Core;
using AgentSpeed.Presentation.Localization;
using AgentSpeed.Interop;
using AgentSpeed.Menus;
using AgentSpeed.Presentation.Menus;
using AgentSpeed.Presentation.Settings;
using AgentSpeed.Taskbar;

namespace AgentSpeed.Windows.Tests;

public class TaskbarPositionTests
{
    [Theory]
    [InlineData(BarPosition.Tray)]
    [InlineData(BarPosition.Left)]
    public void ChoosingAPositionShowsTheWidgetAndHideKeepsThePosition(BarPosition position)
    {
        var commands = new Commands { Settings = AppSettings.Default with { ShowOnTaskbar = false, BarMode = BarDisplayMode.ValueOnly } };
        MenuBuilder.Run(new MenuAction.SetBarPosition(position), commands);
        Assert.True(commands.Settings.ShowOnTaskbar);
        Assert.Equal(position, commands.Settings.BarPosition);
        Assert.Equal(BarDisplayMode.ValueOnly, commands.Settings.BarMode);
        MenuBuilder.Run(new MenuAction.SetShowOnTaskbar(false), commands);
        Assert.False(commands.Settings.ShowOnTaskbar);
        Assert.Equal(position, commands.Settings.BarPosition);
    }

    private sealed class Commands : IAppCommands
    {
        public AppSettings Settings { get; set; } = AppSettings.Default;
        public void UpdateSettings(Func<AppSettings, AppSettings> change) => Settings = change(Settings);
        public void ToggleDashboardFromMenu() => throw new NotSupportedException();
        public void OpenHistory(SourceKind? source = null) => throw new NotSupportedException();
        public void OpenSettings(string? page = null) => throw new NotSupportedException();
        public void Quit() => throw new NotSupportedException();
    }

    [Fact]
    public void CheckedTaskbarMenuStillHasASubmenuAndSeparateGlyphs()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var localizer = new Localizer(Localizer.LoadEmbeddedTable(), LanguageCodes.En);
                var menu = AppMenu.Build(new Commands(), AppSettings.Default, localizer);
                var resources = new ResourceDictionary { Source = new Uri("/AgentSpeed;component/Menus/MenuStyles.xaml", UriKind.Relative) };
                var parent = menu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, localizer["settings.bar.show"]));
                parent.Style = (Style)resources[MenuBuilder.ItemStyleKey];
                parent.Measure(new Size(400, 100));
                parent.Arrange(new Rect(0, 0, 400, parent.DesiredSize.Height));
                Assert.Equal(MenuItemRole.SubmenuHeader, parent.Role);
                Assert.False(parent.IsCheckable);
                Assert.Equal(4, parent.Items.Count);
                var check = Assert.IsType<TextBlock>(parent.Template.FindName("HeaderCheckGlyph", parent));
                var chevron = Assert.IsType<TextBlock>(parent.Template.FindName("Chevron", parent));
                var trailing = Assert.IsType<Grid>(parent.Template.FindName("TrailingBox", parent));
                Assert.Equal(Visibility.Visible, check.Visibility);
                Assert.Equal(HorizontalAlignment.Left, check.HorizontalAlignment);
                Assert.Equal(HorizontalAlignment.Right, chevron.HorizontalAlignment);
                Assert.Equal(36, trailing.Width);
            }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    [Fact]
    public void SwitchingPositionInvalidatesLayoutEvenWhenSizeIsUnchanged()
    {
        var state = TaskbarState.Initial with { Enabled = true };
        Assert.False(state.LayoutDiffers(state with { Value = 100 }));
        Assert.True(state.LayoutDiffers(state with { Position = BarPosition.Left }));
        Assert.Equal(BarPosition.Tray, state.Position);
    }

    [Theory]
    [InlineData(96, 4)]
    [InlineData(144, 6)]
    [InlineData(192, 8)]
    public void LocatorPassesPositionAndScaledGap(uint dpi, int gap)
    {
        var geometry = new TaskbarGeometry(0, 0, new RECT(-1920, 1032, 0, 1080),
            new RECT(-220, 1032, 0, 1080), dpi, false);
        var left = TaskbarLocator.WidgetRect(geometry, 150, BarPosition.Left)!.Value;
        Assert.Equal(-1920 + gap, left.Left);
        Assert.Equal(150, left.Width);
        var tray = TaskbarLocator.WidgetRect(geometry, 150)!.Value;
        Assert.Equal(-220 - gap, tray.Right);
    }

    [Fact]
    public void LocatorSupportsLeftPositionWithoutATrayWindow()
    {
        var geometry = new TaskbarGeometry(0, 0, new RECT(0, 1032, 1920, 1080), default, 96, false);
        var left = TaskbarLocator.WidgetRect(geometry, 150, BarPosition.Left)!.Value;
        Assert.Equal(4, left.Left);
        Assert.Equal(1032, left.Top);
        Assert.Equal(48, left.Height);
    }
}
