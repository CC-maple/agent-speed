using System.Windows.Threading;
using AgentSpeed.Presentation.Updates;

namespace AgentSpeed.App;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var exitCode = Run(args, out var restartRequested);
        // Run has disposed the controller and released the single-instance mutex before relaunching.
        if (restartRequested)
        {
            try
            {
                AppRestart.Start();
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
            {
                AppLog.Error("Restart failed.", ex);
                var loc = new AgentSpeed.Presentation.Localization.Localizer(
                    AgentSpeed.Presentation.Localization.Localizer.LoadEmbeddedTable(),
                    AgentSpeed.Presentation.Localization.LanguageResolver.Resolve(
                        AgentSpeed.Presentation.Localization.AppLanguage.System, System.Globalization.CultureInfo.CurrentUICulture));
                System.Windows.MessageBox.Show(loc["win.app.restartFailed"], loc["app.name"],
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return 1;
            }
        }

        return exitCode;
    }

    private static int Run(string[] args, out bool restartRequested)
    {
        restartRequested = false;
        if (Traffic.HelperExtraction.TryRun(args) is { } extracted)
        {
            return extracted;
        }

        StartupPhases.MarkMain();
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            AppLog.Error($"Unhandled exception (terminating: {e.IsTerminating}).", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppLog.Error("Unobserved task exception.", e.Exception);
            e.SetObserved();
        };

        var launch = LaunchArguments.Parse(args);
        if (Environment.GetEnvironmentVariable("AGENT_SPEED_ORB_FRAMES") is { Length: > 0 } frames)
        {
            return Orb.Gpu.OrbFrameHarness.Run(frames);
        }

        using var instance = SingleInstance.TryAcquire();
        if (instance is null)
        {
            return 0;
        }

        AppLog.Info($"Agent Speed {AppPaths.Version} starting on Windows {Environment.OSVersion.Version}.");

        var app = new App();
        var guard = new CrashGuard();
        app.DispatcherUnhandledException += (_, e) =>
        {
            AppLog.Error("Unhandled UI exception.", e.Exception);
            e.Handled = guard.ShouldContinue();
            if (!e.Handled)
            {
                AppLog.Error("Too many errors in a short time; shutting down.");
            }
        };

        using var controller = new AppController(app, instance, launch);
        app.Startup += (_, _) =>
        {
            try
            {
                controller.Start();
            }
            catch (Exception ex)
            {
                AppLog.Error("Startup failed.", ex);
                app.Shutdown(1);
            }
        };
        var exitCode = app.Run();
        restartRequested = controller.RestartRequested;
        AppLog.Info("Agent Speed exited.");
        return exitCode;
    }

    /// <summary>Keeps running after isolated UI errors, but stops a crash loop.</summary>
    private sealed class CrashGuard
    {
        private readonly Queue<long> _recent = new();

        public bool ShouldContinue()
        {
            var now = Environment.TickCount64;
            _recent.Enqueue(now);
            while (_recent.Count > 0 && now - _recent.Peek() > 10_000)
            {
                _recent.Dequeue();
            }

            return _recent.Count <= 20;
        }
    }
}

internal sealed class App : System.Windows.Application
{
    public App()
    {
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
    }

    public static new App Current => (App)System.Windows.Application.Current;

    public static Dispatcher UiDispatcher => Current.Dispatcher;
}
