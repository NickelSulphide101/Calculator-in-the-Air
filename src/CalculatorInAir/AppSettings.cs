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
        public string LanguagePreference { get; set; } = "Auto"; // "Auto", "zh_CN", "en_GB"
        public string Theme { get; set; } = "Auto"; // "Auto", "Dark", "Light"
    }
}
