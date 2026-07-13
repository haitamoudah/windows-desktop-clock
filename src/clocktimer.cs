using System;
using System.Windows.Threading;
using Microsoft.Win32;

namespace DesktopClock;

// fires once per minute aligned to the minute boundary,
// and immediately after resume or a system time change
internal sealed class ClockTimer : IDisposable
{
    readonly DispatcherTimer _timer = new();
    readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

    public event Action? Tick;

    public ClockTimer()
    {
        _timer.Tick += (_, _) => Fire();
        SystemEvents.TimeChanged      += OnTimeChanged;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    public void Start() => Fire();

    void Fire()
    {
        Tick?.Invoke();
        var now = DateTime.Now;
        _timer.Interval = TimeSpan.FromMilliseconds(60_000 - now.Second * 1000 - now.Millisecond + 50);
        _timer.Start();
    }

    void OnTimeChanged(object? s, EventArgs e) => _dispatcher.BeginInvoke(Fire);

    void OnPowerModeChanged(object? s, PowerModeChangedEventArgs e)
    { if (e.Mode == PowerModes.Resume) _dispatcher.BeginInvoke(Fire); }

    public void Dispose()
    {
        _timer.Stop();
        SystemEvents.TimeChanged      -= OnTimeChanged;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }
}
