using System;
using System.Globalization;

namespace DesktopClock;

// lock screen style: "9:41" or "21:41" per the system 12/24-hour setting, never am/pm
internal static class TimeFormat
{
    public static string Now()
    {
        string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
        string format = pattern.Contains("HH") ? "HH:mm" : pattern.Contains('H') ? "H:mm" : "h:mm";
        return DateTime.Now.ToString(format, CultureInfo.CurrentCulture);
    }
}
