using System;
using System.Globalization;

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
            var active = GetActiveLanguage();
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
                default:
                    return key;
            }
        }
    }
}
