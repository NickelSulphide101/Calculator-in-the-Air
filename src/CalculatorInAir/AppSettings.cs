using System;

namespace CalculatorInAir
{
    public class AppSettings
    {
        public bool Ctrl { get; set; } = false;
        public bool Alt { get; set; } = true;
        public bool Shift { get; set; } = false;
        public bool Win { get; set; } = false;
        public int VirtualKey { get; set; } = 0x20; // Default: Space (Virtual Key Code: 32)
        public string HotkeyDisplay { get; set; } = "Alt + Space";
        public int DecimalPlaces { get; set; } = -1; // -1 means Auto
        public bool HideOnBlur { get; set; } = true;
        public bool CopyOnEnter { get; set; } = true;
        public bool EnableClipboardDetection { get; set; } = false;
        public string LanguagePreference { get; set; } = "Auto"; // "Auto", "zh_CN", "en_GB"
        public string Theme { get; set; } = "Auto"; // "Auto", "Dark", "Light"
        public int WindowOpacity { get; set; } = 100; // 30 - 100
        public double WindowWidth { get; set; } = 600.0; // 420 - 900
        public double WindowScale { get; set; } = 1.0; // 0.8 - 1.6
        public bool UseMonospaceFont { get; set; } = false;
        public bool UseThousandsSeparator { get; set; } = false;
        public string IconColorPreset { get; set; } = "Default"; // "Default", "SunsetGlow", "OceanMist", "PineJade", "CosmicTwilight", "WarmAmber", "SmokySakura", "MutedLavender", "SlateBlue", "SageGreen", "TerracottaClay", "DustyRose", "NordicTeal", "WarmTitanium", "Graphite", "Custom"
        public string IconCustomColor1 { get; set; } = "#8E7DBE";
        public string IconCustomColor2 { get; set; } = "#5C82A6";
        public bool IconCustomIsGradient { get; set; } = true;
    }
}
