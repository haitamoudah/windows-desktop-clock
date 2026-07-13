using System;
using System.Windows;
using System.Windows.Interop;

namespace DesktopClock;

public static class Program
{
    const string RepoUrl = "https://github.com/haitamoudah/windows-desktop-clock";

    [STAThread]
    public static void Main()
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
        var window = new ClockWindow();
        using var timer = new ClockTimer();
        TrayIcon? tray = null;

        timer.Tick += () => window.SetTime(TimeFormat.Now());

        window.SourceInitialized += (_, _) =>
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle)!;
            DesktopPinner.Pin(source);
            tray = new TrayIcon(source, "Windows Desktop Clock");
            tray.About += ShowAbout;
            tray.Quit += window.Close;
        };
        window.Closed += (_, _) => tray?.Dispose();

        timer.Start();
        app.Run(window);
    }

    static void ShowAbout()
    {
        string version = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "?";
        MessageBox.Show(
            $"""
            Windows Desktop Clock {version}

            A clean replica of the Windows 11 lock screen clock, living on your desktop.

            • Sits behind every window, survives Win+D and Show Desktop
            • Click-through: your mouse goes right past it
            • Follows your system font and 12/24-hour time format

            {RepoUrl}
            MIT License
            """,
            "About Windows Desktop Clock",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
