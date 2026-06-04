using System;
using System.Globalization;
using System.Windows;

namespace CalculatorInAir
{
    public static class Loc
    {
        public enum Language
        {
            Auto,
            zh_CN,
            en_GB
        }

        public static Language CurrentLanguage { get; set; } = Language.Auto;

        public static Language GetActiveLanguage()
        {
            if (CurrentLanguage != Language.Auto)
                return CurrentLanguage;

            string name = CultureInfo.CurrentUICulture.Name;
            if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                return Language.zh_CN;

            return Language.en_GB;
        }

        public static string Get(string key)
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Resources.Contains(key))
            {
                return System.Windows.Application.Current.Resources[key] as string ?? key;
            }

            return GetFallback(key, GetActiveLanguage());
        }

        public static void LoadLanguage(Language language)
        {
            var active = language;
            if (active == Language.Auto)
            {
                string name = CultureInfo.CurrentUICulture.Name;
                if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                    active = Language.zh_CN;
                else
                    active = Language.en_GB;
            }

            string filename = active == Language.zh_CN ? "Strings.zh-CN.xaml" : "Strings.en-GB.xaml";
            var uri = new Uri($"/CalculatorInAir;component/Locales/{filename}", UriKind.Relative);

            if (System.Windows.Application.Current == null) return;

            var merged = System.Windows.Application.Current.Resources.MergedDictionaries;
            ResourceDictionary? oldDict = null;

            foreach (var d in merged)
            {
                if (d.Source != null && d.Source.OriginalString.Contains("Locales/Strings."))
                {
                    oldDict = d;
                    break;
                }
            }

            if (oldDict != null)
            {
                merged.Remove(oldDict);
            }

            try
            {
                var newDict = new ResourceDictionary { Source = uri };
                merged.Add(newDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load language resources: {ex.Message}");
            }
        }

        private static string GetFallback(string key, Language active)
        {
            bool isZh = active == Language.zh_CN;
            switch (key)
            {
                case "Placeholder":
                    return isZh ? "输入数学公式... (例如: 2pi * 5 或 sqrt(16))" : "Type a math formula... (e.g. 2pi * 5 or sqrt(16))";
                case "PressEnterToCopy":
                    return isZh ? "按回车复制" : "Press Enter to copy";
                case "Copied":
                    return isZh ? "已复制到剪贴板！" : "Copied to clipboard!";
                case "TrayShow":
                    return isZh ? "显示计算器" : "Show Calculator";
                case "TraySettings":
                    return isZh ? "设置..." : "Settings...";
                case "TrayExit":
                    return isZh ? "退出" : "Exit";
                case "SettingsTitle":
                    return isZh ? "设置 - Calculator in the Air" : "Settings - Calculator in the Air";
                case "GlobalShortcut":
                    return isZh ? "全局快捷键：" : "Global Shortcut:";
                case "RecordHotkey":
                    return isZh ? "录制快捷键" : "Record Hotkey";
                case "RecordingPrompt":
                    return isZh ? "请按下按键... (Esc 取消)" : "Press keys... (Esc to cancel)";
                case "Precision":
                    return isZh ? "计算精度 (保留小数位)：" : "Calculation Precision (decimals):";
                case "PrecisionAuto":
                    return isZh ? "自动" : "Auto";
                case "Behavior":
                    return isZh ? "行为设置：" : "Behavior Settings:";
                case "HideOnBlur":
                    return isZh ? "失去焦点时自动隐藏" : "Hide when focus is lost";
                case "CopyOnEnter":
                    return isZh ? "按回车键复制计算结果" : "Copy result on pressing Enter";
                case "LanguageSetting":
                    return isZh ? "界面语言：" : "Interface Language:";
                case "LanguageAuto":
                    return isZh ? "跟随系统 (Auto)" : "Follow System (Auto)";
                case "ThemeSetting":
                    return isZh ? "界面主题：" : "Theme:";
                case "ThemeAuto":
                    return isZh ? "跟随系统 (Auto)" : "Follow System (Auto)";
                case "ThemeDark":
                    return isZh ? "暗黑模式" : "Dark Mode";
                case "ThemeLight":
                    return isZh ? "明亮模式" : "Light Mode";
                case "Save":
                    return isZh ? "保存" : "Save";
                case "Cancel":
                    return isZh ? "取消" : "Cancel";
                case "HotkeyConflict":
                    return isZh ? "无法注册全局快捷键 '{0}'。它可能已被其他程序占用。" : "Failed to register global hotkey '{0}'. It might be already in use by another application.";
                case "HotkeyConflictTitle":
                    return isZh ? "快捷键冲突" : "Hotkey Conflict";
                case "HotkeyWarningText":
                    return isZh ? "快捷键必须包含至少一个修饰键（如 Ctrl, Alt, Shift 或 Win），以防止您的键盘按键被全局锁定！" : "The shortcut must contain at least one modifier key (Ctrl, Alt, Shift, or Win) to prevent your keyboard keys from being locked globally!";
                case "HotkeyWarningTitle":
                    return isZh ? "不安全的快捷键" : "Unsafe Shortcut";
                default:
                    return key;
            }
        }
    }
}
