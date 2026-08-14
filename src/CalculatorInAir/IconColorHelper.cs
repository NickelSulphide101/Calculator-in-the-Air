using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

namespace CalculatorInAir
{
    public class ColorPresetInfo
    {
        public string Id { get; set; } = "";
        public string NameKey { get; set; } = "";
        public bool IsGradient { get; set; }
        public string LightStartHex { get; set; } = "";
        public string LightEndHex { get; set; } = "";
        public string DarkStartHex { get; set; } = "";
        public string DarkEndHex { get; set; } = "";

        public ColorPresetInfo(string id, string nameKey, bool isGradient, string lightStart, string lightEnd, string darkStart, string darkEnd)
        {
            Id = id;
            NameKey = nameKey;
            IsGradient = isGradient;
            LightStartHex = lightStart;
            LightEndHex = lightEnd;
            DarkStartHex = darkStart;
            DarkEndHex = darkEnd;
        }

        public ColorPresetInfo(string id, string nameKey, string lightHex, string darkHex)
            : this(id, nameKey, false, lightHex, lightHex, darkHex, darkHex)
        {
        }
    }

    public static class IconColorHelper
    {
        public static readonly List<ColorPresetInfo> GradientPresets = new List<ColorPresetInfo>
        {
            new ColorPresetInfo("Default", "IconColorDefault", true, "#7C3AED", "#0D9488", "#A78BFA", "#67E8F9"),
            new ColorPresetInfo("SunsetGlow", "IconColorSunset", true, "#C4566F", "#D98A62", "#D6758A", "#E59E7C"),
            new ColorPresetInfo("OceanMist", "IconColorOcean", true, "#4F7FA8", "#5AA9A0", "#6F9EC3", "#79C2BA"),
            new ColorPresetInfo("PineJade", "IconColorPine", true, "#3E8368", "#5FA99B", "#5DA087", "#7CC2B4"),
            new ColorPresetInfo("CosmicTwilight", "IconColorCosmic", true, "#63589F", "#8B6AA6", "#8075B8", "#A688C0"),
            new ColorPresetInfo("WarmAmber", "IconColorAmber", true, "#A86A46", "#C99450", "#BE8260", "#DCA96C"),
            new ColorPresetInfo("SmokySakura", "IconColorSakura", true, "#A8627A", "#C88399", "#BF7C92", "#D99DB0"),
        };

        public static readonly List<ColorPresetInfo> SolidPresets = new List<ColorPresetInfo>
        {
            new ColorPresetInfo("MutedLavender", "IconColorLavender", "#8E7DBE", "#A594D1"),
            new ColorPresetInfo("SlateBlue", "IconColorSlate", "#5C82A6", "#7B9EB8"),
            new ColorPresetInfo("SageGreen", "IconColorSage", "#5A8F76", "#7AA992"),
            new ColorPresetInfo("TerracottaClay", "IconColorTerracotta", "#B8735C", "#D48D76"),
            new ColorPresetInfo("DustyRose", "IconColorDustyRose", "#A85D6F", "#C47D8F"),
            new ColorPresetInfo("NordicTeal", "IconColorTeal", "#4A7C82", "#6B9DA3"),
            new ColorPresetInfo("WarmTitanium", "IconColorTitanium", "#787680", "#9E9CA6"),
            new ColorPresetInfo("Graphite", "IconColorGraphite", "#525866", "#8F96A3"),
        };

        public static readonly string[] QuickSwatches = new string[]
        {
            "#8E7DBE", "#A594D1",
            "#5C82A6", "#7B9EB8",
            "#5A8F76", "#7AA992",
            "#B8735C", "#D48D76",
            "#A85D6F", "#C47D8F",
            "#4A7C82", "#6B9DA3",
            "#787680", "#9E9CA6",
            "#525866", "#8F96A3"
        };

        public static ColorPresetInfo? FindPreset(string id)
        {
            foreach (var p in GradientPresets)
            {
                if (string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)) return p;
            }
            foreach (var p in SolidPresets)
            {
                if (string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)) return p;
            }
            return null;
        }

        public static Brush CreateBrush(AppSettings settings, bool isDark, bool isLinearHorizontal = false)
        {
            GetWpfColors(settings, isDark, out var c1, out var c2, out bool isGradient);

            if (isGradient)
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new System.Windows.Point(0, 0),
                    EndPoint = isLinearHorizontal ? new System.Windows.Point(1, 0) : new System.Windows.Point(1, 1)
                };
                brush.GradientStops.Add(new GradientStop(c1, 0.0));
                brush.GradientStops.Add(new GradientStop(c2, 1.0));
                brush.Freeze();
                return brush;
            }
            else
            {
                var brush = new SolidColorBrush(c1);
                brush.Freeze();
                return brush;
            }
        }

        public static void GetWpfColors(AppSettings settings, bool isDark, out Color c1, out Color c2, out bool isGradient)
        {
            string presetId = settings.IconColorPreset ?? "Default";

            if (string.Equals(presetId, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                isGradient = settings.IconCustomIsGradient;
                c1 = ParseHexColor(settings.IconCustomColor1, Color.FromRgb(142, 125, 190));
                c2 = isGradient ? ParseHexColor(settings.IconCustomColor2, Color.FromRgb(92, 130, 166)) : c1;
                return;
            }

            var preset = FindPreset(presetId) ?? GradientPresets[0];
            isGradient = preset.IsGradient;

            string startHex = isDark ? preset.DarkStartHex : preset.LightStartHex;
            string endHex = isDark ? preset.DarkEndHex : preset.LightEndHex;

            c1 = ParseHexColor(startHex, Color.FromRgb(124, 58, 237));
            c2 = isGradient ? ParseHexColor(endHex, Color.FromRgb(13, 148, 136)) : c1;
        }

        public static void GetGdiColors(AppSettings settings, bool isDark, out System.Drawing.Color c1, out System.Drawing.Color c2)
        {
            GetWpfColors(settings, isDark, out var wpf1, out var wpf2, out bool isGradient);
            c1 = System.Drawing.Color.FromArgb(wpf1.A, wpf1.R, wpf1.G, wpf1.B);
            c2 = isGradient ? System.Drawing.Color.FromArgb(wpf2.A, wpf2.R, wpf2.G, wpf2.B) : c1;
        }

        public static Color ParseHexColor(string hex, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            hex = hex.Trim().TrimStart('#');

            try
            {
                if (hex.Length == 6)
                {
                    byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    byte a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                    byte r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                    return Color.FromArgb(a, r, g, b);
                }
                else if (hex.Length == 3)
                {
                    char rChar = hex[0];
                    char gChar = hex[1];
                    char bChar = hex[2];
                    byte r = byte.Parse($"{rChar}{rChar}", NumberStyles.HexNumber);
                    byte g = byte.Parse($"{gChar}{gChar}", NumberStyles.HexNumber);
                    byte b = byte.Parse($"{bChar}{bChar}", NumberStyles.HexNumber);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
                // Return fallback on parse error
            }

            return fallback;
        }

        public static string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
