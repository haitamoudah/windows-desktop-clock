using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;

namespace DesktopClock;

// the clock itself: a borderless transparent window holding one glyph-run element
public sealed class ClockWindow : Window
{
    const double TimeSize = 135;            // lock screen time size at 1080p
    const double TopRatio = 164.0 / 1080.0; // glyph top sits 164 px from the top on a 1080p screen

    readonly TimeText _time = new();

    public ClockWindow()
    {
        WindowStyle           = WindowStyle.None;
        AllowsTransparency    = true;
        Background            = Brushes.Transparent;
        ResizeMode            = ResizeMode.NoResize;
        ShowInTaskbar         = false;
        ShowActivated         = false;
        Focusable             = false;
        IsHitTestVisible      = false;
        SizeToContent         = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        var shadow = new DropShadowEffect
        { BlurRadius = 24, ShadowDepth = 0, Opacity = 0.35, Color = Colors.Black };
        shadow.Freeze();
        _time.Effect = shadow;
        ApplyFont();
        Content = _time;

        SizeChanged += (_, _) => Reposition();
        SystemEvents.DisplaySettingsChanged += OnDisplayChanged;
        SystemEvents.UserPreferenceChanged  += OnPreferenceChanged;
        Closed += (_, _) =>
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplayChanged;
            SystemEvents.UserPreferenceChanged  -= OnPreferenceChanged;
        };
    }

    public void SetTime(string text) => _time.Text = text;

    // lock screen font by default; follows the system font if the user replaced it
    void ApplyFont()
    {
        var system = SystemFonts.MessageFontFamily;
        var family = system.Source == "Segoe UI"
            ? new FontFamily("Segoe UI Variable Display, Segoe UI")
            : system;
        _time.SetFont(family, FontWeights.SemiBold, TimeSize);
    }

    void Reposition()
    {
        Left = Math.Round((SystemParameters.PrimaryScreenWidth - ActualWidth) / 2);
        Top  = Math.Round(SystemParameters.PrimaryScreenHeight * TopRatio - _time.InkTop());
    }

    void OnDisplayChanged(object? s, EventArgs e) => Dispatcher.BeginInvoke(Reposition);

    void OnPreferenceChanged(object? s, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.Window) return;
        Dispatcher.BeginInvoke(() => { ApplyFont(); Reposition(); });
    }
}
