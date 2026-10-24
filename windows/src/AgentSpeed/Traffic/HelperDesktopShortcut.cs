using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using AgentSpeed.Core.Traffic.Helper;

namespace AgentSpeed.Traffic;

/// <summary>Created by the unelevated app, on the installing user's desktop, after a successful service install.</summary>
internal static class HelperDesktopShortcut
{
    internal const string FileName = "Agent Speed Traffic Helper.lnk";
    internal static readonly Guid ShellLinkClass = new("00021401-0000-0000-C000-000000000046");
    internal static string Target => Path.Combine(Environment.SystemDirectory, "mmc.exe");
    internal static string Arguments => "\"" + Path.Combine(Environment.SystemDirectory, "services.msc") + "\"";
    internal static string Description => $"Agent Speed Traffic Helper — {HelperProtocol.ServiceName}. Local process traffic counters. Open Services to inspect, stop or manage the service.";

    public static string CreateForCurrentUser() => Create(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

    internal static string Create(string desktopDirectory)
    {
        if (!Path.IsPathFullyQualified(desktopDirectory) || !Directory.Exists(desktopDirectory))
            throw new IOException("The desktop directory is unavailable.");
        var path = Path.Combine(desktopDirectory, FileName);
        if (File.Exists(path))
        {
            // Do not replace an existing shortcut, even if it happens to have the same name.
            var existing = Read(path);
            if (!string.Equals(existing.Target, Target, StringComparison.OrdinalIgnoreCase) || existing.Arguments != Arguments)
                throw new IOException("A different file already uses the traffic helper shortcut name.");
            return path;
        }

        var instance = NewShellLink();
        try
        {
            var link = (IShellLinkW)instance;
            link.SetPath(Target);
            link.SetArguments(Arguments);
            link.SetWorkingDirectory(Environment.SystemDirectory);
            link.SetDescription(Description);
            link.SetIconLocation(Target, 0);
            link.SetShowCmd(1);
            ((IPersistFile)instance).Save(path, true);
            return path;
        }
        finally { Marshal.FinalReleaseComObject(instance); }
    }

    internal static (string Target, string Arguments, string Description) Read(string path)
    {
        var instance = NewShellLink();
        try
        {
            ((IPersistFile)instance).Load(path, 0);
            var link = (IShellLinkW)instance;
            var target = new StringBuilder(32768);
            var arguments = new StringBuilder(32768);
            var description = new StringBuilder(1024);
            link.GetPath(target, target.Capacity, 0, 4); // SLGP_RAWPATH: no target resolution or execution.
            link.GetArguments(arguments, arguments.Capacity);
            link.GetDescription(description, description.Capacity);
            return (target.ToString(), arguments.ToString(), description.ToString());
        }
        finally { Marshal.FinalReleaseComObject(instance); }
    }

    private static object NewShellLink() => Activator.CreateInstance(Type.GetTypeFromCLSID(ShellLinkClass, true)!)
        ?? throw new COMException("Shell Link could not be created.");

    [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int count, nint findData, uint flags);
        void GetIDList(out nint idList);
        void SetIDList(nint idList);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder description, int count);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string description);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder directory, int count);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder arguments, int count);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string arguments);
        void GetHotkey(out short hotkey);
        void SetHotkey(short hotkey);
        void GetShowCmd(out int show);
        void SetShowCmd(int show);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder icon, int count, out int index);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string icon, int index);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
        void Resolve(nint window, uint flags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
    }
}
