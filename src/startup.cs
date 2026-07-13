using System;
using Microsoft.Win32;

namespace DesktopClock;

// registers the exe in the current user's run key so the clock starts with windows
internal static class Startup
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string Name   = "windows-desktop-clock";

    static string Command => $"\"{Environment.ProcessPath}\"";

    public static bool Enabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return string.Equals(key?.GetValue(Name) as string, Command, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void Toggle()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (Enabled) key.DeleteValue(Name, false);
        else key.SetValue(Name, Command);
    }
}
