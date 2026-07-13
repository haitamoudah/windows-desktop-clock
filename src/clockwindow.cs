using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;

namespace DesktopClock;

// the clock itself: a borderless transparent window holding one text block
public sealed class ClockWindow : Window
{
    const double TimeSize = 115;            // lock screen time size at 1080p
    const double TopRatio = 163.0 / 1080.0; // glyph top sits 163 px from the top on a 1080p screen

    readonly TextBlock _time = new();

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

        _time.Foreground = Brushes.White;
        _time.FontWeight = FontWeights.SemiBold;
        _time.FontSize   = TimeSize;
        _time.Effect     = shadow;
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
        _time.FontFamily = system.Source == "Segoe UI"
            ? new FontFamily("Segoe UI Variable Display, Segoe UI")
            : system;
    }

    void Reposition()
    {
        Left = Math.Round((SystemParameters.PrimaryScreenWidth - ActualWidth) / 2);
        // -1: the first antialiased row renders one pixel below the ideal glyph top
        Top  = Math.Round(SystemParameters.PrimaryScreenHeight * TopRatio - GlyphTop()) - 1;
    }

    // distance from the window top to the first drawn pixel of a "1"
    double GlyphTop()
    {
        var ft = new FormattedText("1", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(_time.FontFamily, _time.FontStyle, _time.FontWeight, _time.FontStretch),
            _time.FontSize, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        return ft.Height + ft.OverhangAfter - ft.Extent;
    }

    void OnDisplayChanged(object? s, EventArgs e) => Dispatcher.BeginInvoke(Reposition);

    void OnPreferenceChanged(object? s, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.Window) return;
        Dispatcher.BeginInvoke(() => { ApplyFont(); Reposition(); });
    }
}
