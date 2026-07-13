using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DesktopClock;

// draws the time as a raw glyph run for pixel-exact control, like the lock screen:
// every digit sits in a uniform cell so the layout never jitters,
// and the colon is lifted onto the digit centerline
internal sealed class TimeText : FrameworkElement
{
    const double ColonRaiseEm = 0.104;

    GlyphTypeface? _glyphs;
    double _size = 120;
    string _text = "";

    public void SetFont(FontFamily family, FontWeight weight, double size)
    {
        var typeface = new Typeface(family, FontStyles.Normal, weight, FontStretches.Normal);
        if (!typeface.TryGetGlyphTypeface(out _glyphs))
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, weight, FontStretches.Normal)
                .TryGetGlyphTypeface(out _glyphs);
        _size = size;
        InvalidateMeasure();
        InvalidateVisual();
    }

    public string Text
    {
        get => _text;
        set { if (_text == value) return; _text = value; InvalidateMeasure(); InvalidateVisual(); }
    }

    // distance from the top of this element to the first ink pixel of a "1"
    public double InkTop()
    {
        var run = BuildRun("1");
        return run?.BuildGeometry().Bounds.Y ?? 0;
    }

    double DigitCell() => _glyphs!.AdvanceWidths[_glyphs.CharacterToGlyphMap['0']] * _size;

    GlyphRun? BuildRun(string text)
    {
        if (_glyphs is null || text.Length == 0) return null;

        var indices  = new List<ushort>();
        var advances = new List<double>();
        var offsets  = new List<Point>();
        double cell = DigitCell();

        foreach (char c in text)
        {
            if (!_glyphs.CharacterToGlyphMap.TryGetValue(c, out ushort glyph)) continue;
            double advance = _glyphs.AdvanceWidths[glyph] * _size;
            if (char.IsDigit(c))
            {
                indices.Add(glyph);
                advances.Add(cell);
                offsets.Add(new Point((cell - advance) / 2, 0)); // center the digit in its cell
            }
            else
            {
                indices.Add(glyph);
                advances.Add(advance);
                offsets.Add(new Point(0, c == ':' ? _size * ColonRaiseEm : 0));
            }
        }
        if (indices.Count == 0) return null;

        return new GlyphRun(_glyphs, 0, false, _size,
            (float)VisualTreeHelper.GetDpi(this).PixelsPerDip,
            indices, new Point(0, _glyphs.Baseline * _size), advances, offsets,
            null, null, null, null, null);
    }

    protected override Size MeasureOverride(Size available)
    {
        if (_glyphs is null || _text.Length == 0) return new Size(0, 0);
        double width = 0, cell = DigitCell();
        foreach (char c in _text)
        {
            if (!_glyphs.CharacterToGlyphMap.TryGetValue(c, out ushort glyph)) continue;
            width += char.IsDigit(c) ? cell : _glyphs.AdvanceWidths[glyph] * _size;
        }
        return new Size(width, _glyphs.Height * _size);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var run = BuildRun(_text);
        if (run != null) dc.DrawGlyphRun(Brushes.White, run);
    }
}
