using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static DesktopClock.NativeMethods;

namespace DesktopClock;

// glues the window to the desktop: always at the bottom of the z-order,
// owned by the desktop's icon layer so win+d / show desktop leaves it alone
internal static class DesktopPinner
{
    public static void Pin(HwndSource source)
    {
        IntPtr hwnd = source.Handle;

        long ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        ex |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(ex));

        IntPtr progman = GetShellWindow();
        IntPtr icons = FindWindowExW(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
        if (icons == IntPtr.Zero)
        {
            // older windows builds keep the icons under a separate workerw window
            IntPtr worker = IntPtr.Zero;
            while ((worker = FindWindowExW(IntPtr.Zero, worker, "WorkerW", null)) != IntPtr.Zero)
            {
                icons = FindWindowExW(worker, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (icons != IntPtr.Zero) break;
            }
        }
        SetWindowLongPtr(hwnd, GWLP_HWNDPARENT, icons != IntPtr.Zero ? icons : progman);

        SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        source.AddHook(KeepOnDesktop);
    }

    // hold the window at the bottom and swallow the minimize that show desktop sends
    static IntPtr KeepOnDesktop(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_WINDOWPOSCHANGING)
        {
            var wp = Marshal.PtrToStructure<WINDOWPOS>(lParam);
            wp.hwndInsertAfter = HWND_BOTTOM;
            wp.flags &= ~SWP_NOZORDER;
            if (wp.x == -32000 || wp.y == -32000) wp.flags |= SWP_NOMOVE | SWP_NOSIZE;
            Marshal.StructureToPtr(wp, lParam, false);
        }
        return IntPtr.Zero;
    }
}
