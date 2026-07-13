using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DesktopClock;

// custom about card, styled after windows 11 dialogs
internal sealed class AboutWindow : Window
{
    static AboutWindow? _open;

    public static void Open(string version, string repoUrl)
    {
        if (_open != null) { _open.Activate(); return; }
        _open = new AboutWindow(version, repoUrl);
        _open.Closed += (_, _) => _open = null;
        _open.Show();
        _open.Activate();
    }

    AboutWindow(string version, string repoUrl)
    {
        WindowStyle           = WindowStyle.None;
        AllowsTransparency    = true;
        Background            = Brushes.Transparent;
        ResizeMode            = ResizeMode.NoResize;
        ShowInTaskbar         = false;
        Topmost               = true;
        SizeToContent         = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var body = new FontFamily("Segoe UI Variable Text, Segoe UI");
        var panel = new StackPanel { Margin = new Thickness(24), MaxWidth = 360 };

        panel.Children.Add(new TextBlock
        {
            Text = "Windows Desktop Clock",
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI"),
            FontSize = 18, FontWeight = FontWeights.SemiBold
        });
        panel.Children.Add(new TextBlock
        {
            Text = $"Version {version}",
            Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
            FontFamily = body, FontSize = 12, Margin = new Thickness(0, 2, 0, 12)
        });
        panel.Children.Add(new TextBlock
        {
            Text = "A clean replica of the Windows 11 lock screen clock, living on your desktop. "
                 + "It sits behind every window, survives Win+D, lets every click pass through, "
                 + "and follows your system font and time format.",
            Foreground = new SolidColorBrush(Color.FromRgb(0xD6, 0xD6, 0xD6)),
            FontFamily = body, FontSize = 13, TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var link = new Hyperlink(new Run(repoUrl.Replace("https://", "")))
        { Foreground = new SolidColorBrush(Color.FromRgb(0x60, 0xA8, 0xF0)), TextDecorations = null };
        link.Click += (_, _) => Process.Start(new ProcessStartInfo(repoUrl) { UseShellExecute = true });
        panel.Children.Add(new TextBlock(link) { FontFamily = body, FontSize = 13 });

        panel.Children.Add(new TextBlock
        {
            Text = "MIT License",
            Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
            FontFamily = body, FontSize = 12, Margin = new Thickness(0, 4, 0, 16)
        });

        var ok = new Button
        {
            Content = "OK", Width = 88, Height = 32,
            HorizontalAlignment = HorizontalAlignment.Right,
            Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x3B, 0x3B)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
            FontFamily = body, FontSize = 13
        };
        ok.Click += (_, _) => Close();
        panel.Children.Add(ok);

        var shadow = new DropShadowEffect
        { BlurRadius = 24, ShadowDepth = 4, Opacity = 0.5, Color = Colors.Black };
        shadow.Freeze();

        Content = new Border
        {
            Background      = new SolidColorBrush(Color.FromRgb(0x24, 0x24, 0x24)),
            BorderBrush     = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(8),
            Margin          = new Thickness(16), // room for the shadow
            Child           = panel,
            Effect          = shadow
        };

        MouseLeftButtonDown += (_, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }
}
