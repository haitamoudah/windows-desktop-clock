using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DesktopClock;

// small dark flyout shown from the tray icon, styled after windows 11 menus
internal sealed class TrayMenuWindow : Window
{
    bool _closing;

    TrayMenuWindow(Action about, Action quit)
    {
        WindowStyle           = WindowStyle.None;
        AllowsTransparency    = true;
        Background            = Brushes.Transparent;
        ResizeMode            = ResizeMode.NoResize;
        ShowInTaskbar         = false;
        Topmost               = true;
        SizeToContent         = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        var items = new StackPanel { Margin = new Thickness(4) };
        items.Children.Add(Item("About", () => Choose(about)));
        items.Children.Add(new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromArgb(0x28, 0xFF, 0xFF, 0xFF)),
            Margin = new Thickness(8, 3, 8, 3)
        });
        items.Children.Add(Item("Quit", () => Choose(quit)));

        var shadow = new DropShadowEffect
        { BlurRadius = 16, ShadowDepth = 2, Opacity = 0.5, Color = Colors.Black };
        shadow.Freeze();

        Content = new Border
        {
            Background      = new SolidColorBrush(Color.FromRgb(0x2C, 0x2C, 0x2C)),
            BorderBrush     = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(8),
            Margin          = new Thickness(12), // room for the shadow
            Child           = items,
            Effect          = shadow
        };

        Deactivated += (_, _) => CloseOnce();
    }

    // Close is not reentrant: picking an item closes the menu, which deactivates it,
    // and the Deactivated handler must not call Close again while it is closing
    void CloseOnce()
    {
        if (_closing) return;
        _closing = true;
        Close();
    }

    void Choose(Action action)
    {
        CloseOnce();
        Dispatcher.BeginInvoke(action); // run after the close finishes
    }

    static UIElement Item(string label, Action click)
    {
        var item = new Border
        {
            CornerRadius = new CornerRadius(5),
            Padding      = new Thickness(12, 8, 48, 8),
            Background   = Brushes.Transparent,
            Child = new TextBlock
            {
                Text       = label,
                Foreground = Brushes.White,
                FontSize   = 14,
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI")
            }
        };
        item.MouseEnter += (_, _) => item.Background = new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF));
        item.MouseLeave += (_, _) => item.Background = Brushes.Transparent;
        item.MouseLeftButtonUp += (_, _) => click();
        return item;
    }

    // opens above and to the left of the cursor, next to the tray
    public static void ShowAt(Point cursor, Action about, Action quit)
    {
        var menu = new TrayMenuWindow(about, quit);
        menu.Show();
        menu.UpdateLayout();
        menu.Left = cursor.X - menu.ActualWidth + 16;
        menu.Top  = cursor.Y - menu.ActualHeight + 8;
        menu.Activate();
    }
}
