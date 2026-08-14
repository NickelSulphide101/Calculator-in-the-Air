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
            var uri = new Uri($"pack://application:,,,/Locales/{filename}", UriKind.Absolute);

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
                    return isZh ? "输入数学公式... (例如: 2 + 3 * 4 或 (5 + 6) ^ 2)" : "Type a math formula... (e.g. 2 + 3 * 4 or (5 + 6) ^ 2)";
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
                case "WindowOpacitySetting":
                    return isZh ? "窗口透明度：" : "Window Opacity:";
                case "WindowSizeSetting":
                    return isZh ? "窗口尺寸与字号：" : "Window Size & Font:";
                case "WidthSetting":
                    return isZh ? "窗口宽度：" : "Window Width:";
                case "ScaleSetting":
                    return isZh ? "字号与缩放：" : "Font & Scaling:";
                case "PresetOpaque":
                    return isZh ? "不透明" : "Opaque";
                case "PresetRecommended":
                    return isZh ? "推荐" : "Balanced";
                case "PresetLight":
                    return isZh ? "轻盈" : "Light";
                case "PresetTransparent":
                    return isZh ? "高透" : "Transparent";
                case "SizeCompact":
                    return isZh ? "紧凑 (480px)" : "Compact (480px)";
                case "SizeStandard":
                    return isZh ? "标准 (600px)" : "Standard (600px)";
                case "SizeWide":
                    return isZh ? "宽屏 (750px)" : "Wide (750px)";
                case "SizeLarge":
                    return isZh ? "大屏 (900px)" : "Large (900px)";
                case "UseMonospaceFont":
                    return isZh ? "使用数字等宽字体 (Monospace)" : "Use Monospace Font";
                case "UseThousandsSeparator":
                    return isZh ? "使用千位分隔符 (1,234,567.89)" : "Use Thousands Separator (1,234,567.89)";
                case "HotkeySystemConflict":
                    return isZh ? "⚠️ 该快捷键为 Windows 系统常用快捷键，可能会产生冲突" : "⚠️ This shortcut is a common Windows shortcut and may conflict.";
                case "CopiedResult":
                    return isZh ? "已复制结果：" : "Copied result: ";
                case "CopiedFormula":
                    return isZh ? "已复制算式与结果：" : "Copied formula & result: ";
                case "PinToolTip":
                    return isZh ? "置顶固定 (离焦不隐藏, Ctrl+P)" : "Pin window (Keep open on focus loss, Ctrl+P)";
                case "PinnedToast":
                    return isZh ? "📌 已开启窗口置顶固定" : "📌 Pinned window to top";
                case "UnpinnedToast":
                    return isZh ? "📌 已取消窗口置顶" : "📌 Unpinned window";
                case "ClipboardHint":
                    return isZh ? "📋 检测到剪贴板算式：{0} (按 Ctrl+V 或点击粘贴)" : "📋 Clipboard formula detected: {0} (Press Ctrl+V or click to paste)";
                case "ShortcutHint":
                    return isZh ? "⏎ 复制结果  |  Shift+⏎ 复制算式  |  ↑↓ 切换格式" : "⏎ Copy result  |  Shift+⏎ Copy formula  |  ↑↓ Switch format";
                case "FormatStandardLabel":
                    return isZh ? "标准" : "Standard";
                case "FormatRawLabel":
                    return isZh ? "纯数字" : "Raw";
                case "FormatWanLabel":
                    return isZh ? "万元" : "10k";
                case "FormatRMBLabel":
                    return isZh ? "大写RMB" : "RMB";
                case "IconColorSetting":
                    return isZh ? "图标与主题配色：" : "Icon & Theme Color:";
                case "IconColorDefault":
                    return isZh ? "极光紫青 (默认)" : "Aurora (Default)";
                case "IconColorSunset":
                    return isZh ? "落日余晖" : "Sunset Glow";
                case "IconColorOcean":
                    return isZh ? "海雾琉璃" : "Ocean Mist";
                case "IconColorPine":
                    return isZh ? "松石苍翠" : "Pine Jade";
                case "IconColorCosmic":
                    return isZh ? "星云暮夜" : "Cosmic Twilight";
                case "IconColorAmber":
                    return isZh ? "暖茶琥珀" : "Warm Amber";
                case "IconColorSakura":
                    return isZh ? "烟粉初樱" : "Smoky Sakura";
                case "IconColorLavender":
                    return isZh ? "鸢尾灰紫" : "Muted Lavender";
                case "IconColorSlate":
                    return isZh ? "雾霾静蓝" : "Slate Blue";
                case "IconColorSage":
                    return isZh ? "鼠尾草绿" : "Sage Green";
                case "IconColorTerracotta":
                    return isZh ? "复古陶土" : "Terracotta Clay";
                case "IconColorDustyRose":
                    return isZh ? "干枯粉黛" : "Dusty Rose";
                case "IconColorTeal":
                    return isZh ? "冷杉墨青" : "Nordic Teal";
                case "IconColorTitanium":
                    return isZh ? "钛金暖灰" : "Warm Titanium";
                case "IconColorGraphite":
                    return isZh ? "石墨冷灰" : "Graphite";
                case "IconColorCustom":
                    return isZh ? "自定义..." : "Custom...";
                case "CustomColorStart":
                    return isZh ? "起始色 (Hex)" : "Start Color";
                case "CustomColorEnd":
                    return isZh ? "结束色 (Hex)" : "End Color";
                case "CustomColorGradientMode":
                    return isZh ? "启用双色渐变" : "Enable Gradient";
                case "CustomColorQuickPalette":
                    return isZh ? "雅致调色板：" : "Elegant Palette:";
                case "GradientSection":
                    return isZh ? "柔和渐变" : "Gradients";
                case "SolidSection":
                    return isZh ? "雅致纯色" : "Muted Solids";
                default:
                    return key;
            }
        }
    }
}
