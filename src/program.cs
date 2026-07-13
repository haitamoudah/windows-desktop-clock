using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

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
            tray.MenuRequested += () => TrayMenuWindow.ShowAt(CursorPosition(window), ShowAbout, window.Close);
        };
        window.Closed += (_, _) => tray?.Dispose();

        timer.Start();
        app.Run(window);
    }

    static Point CursorPosition(Window reference)
    {
        NativeMethods.GetCursorPos(out var pt);
        var dpi = VisualTreeHelper.GetDpi(reference);
        return new Point(pt.X / dpi.DpiScaleX, pt.Y / dpi.DpiScaleY);
    }

    static void ShowAbout()
    {
        string version = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "?";
        AboutWindow.Open(version, RepoUrl);
    }
}
