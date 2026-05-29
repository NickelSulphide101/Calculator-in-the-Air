using System;
using System.IO;
using System.Text.Json;

namespace CalculatorInAir
{
    public static class SettingsManager
    {
        private static readonly string FolderPath = Path.Combine(
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

            SyncLanguage(settings.LanguagePreference);
            return settings;
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
