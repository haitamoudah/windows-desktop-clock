# Windows Desktop Clock

A clean, lightweight replica of the Windows 11 lock screen clock that lives on your desktop.

![Clock on the Windows 11 light wallpaper](assets/screenshot.png)

![Clock on the Windows 11 dark wallpaper](assets/screenshot-dark.png)

## What It Does

- Shows the time exactly like the Windows 11 lock screen: same font, same size, same position
- Sits on the desktop itself, behind every window. It never covers your apps, and Win+D / Show Desktop can't hide it
- Click-through: your mouse goes right past it, icons and wallpaper stay fully usable
- Follows your system font and your 12/24-hour time format
- Tiny footprint: wakes once per minute

## Download

1. Grab the latest zip from [releases](https://github.com/haitamoudah/windows-desktop-clock/releases)
2. Extract it anywhere
3. Run `windows-desktop-clock.exe`

Requires the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0). Windows offers to install it automatically if it's missing.

## Usage

- The clock appears centered on your desktop, exactly where the lock screen puts it
- Right-click the tray icon for `About` / `Quit`
- To start it with Windows: `Win + R`, type `shell:startup`, and drop a shortcut to the exe there

## Build From Source

```
git clone https://github.com/haitamoudah/windows-desktop-clock.git
cd windows-desktop-clock
dotnet run
```

## License

[MIT](license)
