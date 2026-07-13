# windows-desktop-clock

a clean, lightweight replica of the windows 11 lock screen clock that lives on your desktop.

![clock on the windows 11 light wallpaper](assets/screenshot.png)

![clock on the windows 11 dark wallpaper](assets/screenshot-dark.png)

## what it does

- shows the time exactly like the windows 11 lock screen — same font, same size, same position
- sits on the desktop itself, behind every window: it never covers your apps, and `win + d` / show desktop can't hide it
- click-through: your mouse goes right past it, icons and wallpaper stay fully usable
- follows your system font and your 12/24-hour format
- tiny footprint: no ui framework bloat beyond wpf, wakes once per minute

## download

1. grab the latest zip from [releases](https://github.com/haitamoudah/windows-desktop-clock/releases)
2. extract it anywhere
3. run `windows-desktop-clock.exe`

requires the [.net 10 desktop runtime](https://dotnet.microsoft.com/download/dotnet/10.0) — windows offers to install it automatically if it's missing.

## usage

- the clock appears centered on your desktop, exactly where the lock screen puts it
- right-click the tray icon for `about` / `quit`
- to start it with windows: `win + r` → `shell:startup` → drop a shortcut to the exe there

## build from source

```
git clone https://github.com/haitamoudah/windows-desktop-clock.git
cd windows-desktop-clock
dotnet run
```

## license

[mit](license)
