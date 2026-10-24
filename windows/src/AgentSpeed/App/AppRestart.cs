using System.Diagnostics;

namespace AgentSpeed.App;

internal static class AppRestart
{
    public static void Start()
    {
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("Executable path is unavailable.");
        using var process = Process.Start(CreateStartInfo(executable));
        if (process is null)
        {
            throw new InvalidOperationException("The restarted process could not be created.");
        }
    }

    // Restart a normal monitoring session; do not replay developer/demo or installation arguments.
    internal static ProcessStartInfo CreateStartInfo(string executable) => new(executable)
    {
        UseShellExecute = false,
        WorkingDirectory = AppContext.BaseDirectory,
        CreateNoWindow = true,
    };
}
