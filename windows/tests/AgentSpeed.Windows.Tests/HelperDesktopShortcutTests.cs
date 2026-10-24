using System.Buffers.Binary;
using System.IO;
using AgentSpeed.Traffic;

namespace AgentSpeed.Windows.Tests;

public class HelperDesktopShortcutTests
{
    [Fact]
    public void ShortcutOpensSystemServiceManagerWithoutElevationAndCanBeReused()
    {
        OnStaThread(() =>
        {
            var directory = NewDirectory();
            var path = HelperDesktopShortcut.Create(directory);
            var link = HelperDesktopShortcut.Read(path);
            Assert.Equal(Path.Combine(Environment.SystemDirectory, "mmc.exe"), link.Target, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("\"" + Path.Combine(Environment.SystemDirectory, "services.msc") + "\"", link.Arguments);
            Assert.Contains("AgentSpeedTraffic", link.Description);
            var data = File.ReadAllBytes(path);
            Assert.Equal(0x4cU, BinaryPrimitives.ReadUInt32LittleEndian(data));
            var flags = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(20));
            Assert.Equal(0U, flags & 0x2000U); // RunAsUser must remain unset.
            Assert.Equal(path, HelperDesktopShortcut.Create(directory));
            Assert.Equal(data, File.ReadAllBytes(path));
        });
    }

    [Fact]
    public void ExistingUnrelatedFileIsNotOverwritten()
    {
        OnStaThread(() =>
        {
            var directory = NewDirectory();
            var path = Path.Combine(directory, HelperDesktopShortcut.FileName);
            File.WriteAllText(path, "unrelated existing desktop file");
            var before = File.ReadAllBytes(path);
            Assert.ThrowsAny<Exception>(() => HelperDesktopShortcut.Create(directory));
            Assert.Equal(before, File.ReadAllBytes(path));
        });
    }

    private static string NewDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "agentspeed-shortcut-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void OnStaThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw new InvalidOperationException("Desktop shortcut test failed.", failure);
    }
}
