using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static DesktopClock.NativeMethods;

namespace DesktopClock;

// tray icon with an about/quit menu; the only way to interact with the app
internal sealed class TrayIcon : IDisposable
{
    const int AboutId = 1;
    const int QuitId  = 2;

    readonly HwndSource _source;
    readonly string _tooltip;
    readonly uint _taskbarCreated;
    IntPtr _icon;

    public event Action? About;
    public event Action? Quit;

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
            if (mouse == WM_LBUTTONUP || mouse == WM_RBUTTONUP) ShowMenu();
        }
        else if (_taskbarCreated != 0 && msg == (int)_taskbarCreated)
        {
            Add(); // explorer restarted, the tray was wiped
        }
        return IntPtr.Zero;
    }

    void ShowMenu()
    {
        IntPtr menu = CreatePopupMenu();
        AppendMenuW(menu, MF_STRING, (IntPtr)AboutId, "about");
        AppendMenuW(menu, MF_SEPARATOR, IntPtr.Zero, null);
        AppendMenuW(menu, MF_STRING, (IntPtr)QuitId, "quit");
        GetCursorPos(out POINT pt);
        SetForegroundWindow(_source.Handle); // required so the menu dismisses on outside clicks
        int cmd = TrackPopupMenuEx(menu, TPM_RETURNCMD | TPM_RIGHTBUTTON | TPM_NONOTIFY,
                                   pt.X, pt.Y, _source.Handle, IntPtr.Zero);
        PostMessageW(_source.Handle, WM_NULL, IntPtr.Zero, IntPtr.Zero);
        DestroyMenu(menu);

        if (cmd == AboutId) About?.Invoke();
        else if (cmd == QuitId) Quit?.Invoke();
    }

    public void Dispose()
    {
        var data = Data();
        Shell_NotifyIcon(NIM_DELETE, ref data);
        if (_icon != IntPtr.Zero) { DestroyIcon(_icon); _icon = IntPtr.Zero; }
        _source.RemoveHook(WndProc);
    }
}
