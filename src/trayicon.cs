using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static DesktopClock.NativeMethods;

namespace DesktopClock;

// tray icon; raises MenuRequested when clicked so the app can show its flyout
internal sealed class TrayIcon : IDisposable
{
    readonly HwndSource _source;
    readonly string _tooltip;
    readonly uint _taskbarCreated;
    IntPtr _icon;

    public event Action? MenuRequested;

    public TrayIcon(HwndSource source, string tooltip)
    {
        _source = source;
        _tooltip = tooltip;
        _taskbarCreated = RegisterWindowMessageW("TaskbarCreated");
        _source.AddHook(WndProc);
        LoadTrayIcon();
        Add();
    }

    // assets/icon.ico next to the exe wins; otherwise the stock clock icon
    void LoadTrayIcon()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "assets", "icon.ico");
        if (File.Exists(path))
            _icon = LoadImageW(IntPtr.Zero, path, IMAGE_ICON, 0, 0, LR_LOADFROMFILE | LR_DEFAULTSIZE);
        if (_icon == IntPtr.Zero)
        {
            ExtractIconExW("shell32.dll", 21, out IntPtr large, out _icon, 1);
            if (large != IntPtr.Zero) DestroyIcon(large);
        }
        if (_icon == IntPtr.Zero)
            _icon = LoadIconW(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
    }

    void Add() { var data = Data(); Shell_NotifyIcon(NIM_ADD, ref data); }

    NOTIFYICONDATA Data() => new()
    {
        cbSize           = Marshal.SizeOf<NOTIFYICONDATA>(),
        hWnd             = _source.Handle,
        uID              = 1,
        uFlags           = NIF_MESSAGE | NIF_ICON | NIF_TIP,
        uCallbackMessage = WM_TRAYICON,
        hIcon            = _icon,
        szTip            = _tooltip
    };

    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_TRAYICON)
        {
            int mouse = (int)(lParam.ToInt64() & 0xFFFF);
            if (mouse == WM_LBUTTONUP || mouse == WM_RBUTTONUP) MenuRequested?.Invoke();
        }
        else if (_taskbarCreated != 0 && msg == (int)_taskbarCreated)
        {
            Add(); // explorer restarted, the tray was wiped
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        var data = Data();
        Shell_NotifyIcon(NIM_DELETE, ref data);
        if (_icon != IntPtr.Zero) { DestroyIcon(_icon); _icon = IntPtr.Zero; }
        _source.RemoveHook(WndProc);
    }
}
