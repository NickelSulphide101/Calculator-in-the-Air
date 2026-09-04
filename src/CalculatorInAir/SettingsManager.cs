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
            settings.WindowWidth = Math.Clamp(settings.WindowWidth, 420.0, 900.0);
            settings.WindowScale = Math.Clamp(settings.WindowScale, 0.8, 1.6);

            // Ensure at least one modifier key is set to prevent locking user's bare keys globally
            if (!settings.Ctrl && !settings.Alt && !settings.Shift && !settings.Win)
            {
                settings.Alt = true;
                settings.VirtualKey = 0x20;
                settings.HotkeyDisplay = "Alt + Space";
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
                string logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}] {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}";
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
                File.WriteAllText(FilePath, json);
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
