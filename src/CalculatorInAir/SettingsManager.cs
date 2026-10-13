using System;
using System.IO;
using System.Text.Json;

namespace CalculatorInAir
{
    public static class SettingsManager
    {
        public static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CalculatorInAir"
        );
        private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

        public static AppSettings Load()
        {
            AppSettings settings;
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    settings = new AppSettings();
                }
            }
            catch
            {
                settings = new AppSettings();
            }

            // Sanitize settings to prevent crashes or dangerous configurations
            settings.DecimalPlaces = Math.Clamp(settings.DecimalPlaces, -1, 15);
            settings.WindowOpacity = Math.Clamp(settings.WindowOpacity, 30, 100);

            // Guard against NaN or Infinity values which cause WPF layout crashes
            if (double.IsNaN(settings.WindowWidth) || double.IsInfinity(settings.WindowWidth))
                settings.WindowWidth = 600.0;
            else
                settings.WindowWidth = Math.Clamp(settings.WindowWidth, 420.0, 900.0);

            if (double.IsNaN(settings.WindowScale) || double.IsInfinity(settings.WindowScale))
                settings.WindowScale = 1.0;
            else
                settings.WindowScale = Math.Clamp(settings.WindowScale, 0.8, 1.6);

            // Validate VirtualKey code (Win32 virtual-key range 0x08 - 0xFE, excluding mouse buttons)
            if (settings.VirtualKey <= 0x07 || settings.VirtualKey > 0xFE)
            {
                settings.VirtualKey = 0x20; // Default: Space
                settings.HotkeyDisplay = "Alt + Space";
            }

            // Constrain HotkeyDisplay string length to prevent memory or UI buffer issues
            if (string.IsNullOrWhiteSpace(settings.HotkeyDisplay) || settings.HotkeyDisplay.Length > 50)
            {
                settings.HotkeyDisplay = "Alt + Space";
            }

            // Ensure at least one modifier key is set to prevent locking user's bare keys globally
            if (!settings.Ctrl && !settings.Alt && !settings.Shift && !settings.Win)
            {
                settings.Alt = true;
                settings.VirtualKey = 0x20;
                settings.HotkeyDisplay = "Alt + Space";
            }

            // Validate theme preference
            if (settings.Theme != "Dark" && settings.Theme != "Light" && settings.Theme != "Auto")
            {
                settings.Theme = "Auto";
            }

            // Validate icon color settings
            if (string.IsNullOrWhiteSpace(settings.IconColorPreset) || settings.IconColorPreset.Length > 50)
            {
                settings.IconColorPreset = "Default";
            }
            if (string.IsNullOrWhiteSpace(settings.IconCustomColor1) || settings.IconCustomColor1.Length > 20)
            {
                settings.IconCustomColor1 = "#8E7DBE";
            }
            if (string.IsNullOrWhiteSpace(settings.IconCustomColor2) || settings.IconCustomColor2.Length > 20)
            {
                settings.IconCustomColor2 = "#5C82A6";
            }

            SyncLanguage(settings.LanguagePreference);
            return settings;
        }

        public static void LogException(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
                string logFile = Path.Combine(FolderPath, "crash.log");

                // Enforce maximum log size (512 KB) to prevent disk space exhaustion
                var logFileInfo = new FileInfo(logFile);
                if (logFileInfo.Exists && logFileInfo.Length > 512 * 1024)
                {
                    string oldLogFile = Path.Combine(FolderPath, "crash.old.log");
                    try
                    {
                        File.Move(logFile, oldLogFile, overwrite: true);
                    }
                    catch
                    {
                        File.Delete(logFile);
                    }
                }

                // Sanitize exception message to prevent CRLF log injection
                string safeMessage = ex.Message.Replace("\r", " ").Replace("\n", " ");
                string logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}] {ex.GetType().FullName}: {safeMessage}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}";
                File.AppendAllText(logFile, logEntry);
            }
            catch { }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

                // Atomic write using temporary file to prevent file corruption on power loss or termination
                string tempFilePath = Path.Combine(FolderPath, $"settings.{Guid.NewGuid():N}.tmp");
                File.WriteAllText(tempFilePath, json);
                File.Move(tempFilePath, FilePath, overwrite: true);
            }
            catch { }

            SyncLanguage(settings.LanguagePreference);
        }

        private static void SyncLanguage(string preference)
        {
            if (Enum.TryParse<Loc.Language>(preference, out var lang))
            {
                Loc.CurrentLanguage = lang;
            }
            else
            {
                Loc.CurrentLanguage = Loc.Language.Auto;
            }
        }
    }
}
