using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CalculatorInAir
{
    public class App : System.Windows.Application
    {
        private AppSettings _settings = null!;
        private MainWindow _mainWindow = null!;
        private NotifyIcon _notifyIcon = null!;
        
        private ToolStripMenuItem _showMenuItem = null!;
        private ToolStripMenuItem _settingsMenuItem = null!;
        private ToolStripMenuItem _exitMenuItem = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 0. Load shared styles resource dictionary
            try
            {
                var stylesDict = new ResourceDictionary { Source = new Uri("/CalculatorInAir;component/Themes/Styles.xaml", UriKind.Relative) };
                Resources.MergedDictionaries.Add(stylesDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Styles.xaml: {ex.Message}");
            }

            // 1. Load configuration settings
            _settings = SettingsManager.Load();

            // Load localized strings based on preference
            Loc.LoadLanguage(Loc.CurrentLanguage);

            // 2. Initialize main search window
            _mainWindow = new MainWindow(_settings);
            MainWindow = _mainWindow;

            // Apply theme configuration
            ApplyTheme();

            // Register global user preference changes hook
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            // 3. Setup system tray integration
            SetupTrayIcon();

            // 4. Do not shutdown when main window is hidden (we want it persistent in background)
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateDynamicIcon(),
                Visible = true,
                Text = "Calculator in the Air"
            };

            // Double click opens the calculator
            _notifyIcon.DoubleClick += (s, e) => _mainWindow.ShowWindow();

            // Create tray context menu
            var contextMenu = new ContextMenuStrip();

            _showMenuItem = new ToolStripMenuItem();
            _showMenuItem.Click += (s, e) => _mainWindow.ShowWindow();
            contextMenu.Items.Add(_showMenuItem);

            _settingsMenuItem = new ToolStripMenuItem();
            _settingsMenuItem.Click += (s, e) => _mainWindow.OpenSettings();
            contextMenu.Items.Add(_settingsMenuItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            _exitMenuItem = new ToolStripMenuItem();
            _exitMenuItem.Click += (s, e) => ExitApp();
            contextMenu.Items.Add(_exitMenuItem);

            _notifyIcon.ContextMenuStrip = contextMenu;

            // Apply text and register settings-saved callback to keep tray in sync
            UpdateTrayMenuTexts();
        }

        public void UpdateTrayMenuTexts()
        {
            _showMenuItem.Text = $"{Loc.Get("TrayShow")} ({_settings.HotkeyDisplay})";
            _settingsMenuItem.Text = Loc.Get("TraySettings");
            _exitMenuItem.Text = Loc.Get("TrayExit");
            var titleParts = Loc.Get("SettingsTitle").Split(" - ");
            _notifyIcon.Text = titleParts.Length > 1 ? titleParts[^1] : titleParts[0];
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private Icon CreateDynamicIcon()
        {
            // Retrieve system-tray small icon size dynamically to handle DPI scaling crispness
            var iconSize = System.Windows.Forms.SystemInformation.SmallIconSize;
            int width = iconSize.Width;
            int height = iconSize.Height;

            using (var bmp = new Bitmap(width, height))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw a beautiful rounded rectangle background with violet-blue gradient
                    int margin = Math.Max(2, (int)(width * 0.0625));
                    var rect = new Rectangle(margin, margin, width - 2 * margin, height - 2 * margin);
                    using (var brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(139, 92, 246), // Violet
                        Color.FromArgb(59, 130, 246),  // Blue
                        45f))
                    {
                        int radius = Math.Max(2, (int)(width * 0.22));
                        using (var path = GetRoundedRectPath(rect, radius))
                        {
                            g.FillPath(brush, path);
                        }
                    }

                    // Draw an equal sign symbol in the middle
                    float penWidth = Math.Max(1.5f, width * 0.09375f);
                    using (var pen = new Pen(Color.White, penWidth))
                    {
                        float xStart = width * 0.28125f;
                        float xEnd = width * 0.71875f;
                        float yLine1 = height * 0.375f;
                        float yLine2 = height * 0.5625f;
                        g.DrawLine(pen, xStart, yLine1, xEnd, yLine1);
                        g.DrawLine(pen, xStart, yLine2, xEnd, yLine2);
                    }
                }

                // Retrieve handle to icon and copy it to a managed Icon object
                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    using var icon = Icon.FromHandle(hIcon);
                    return (Icon)icon.Clone(); // Clone guarantees we own the resource
                }
                finally
                {
                    DestroyIcon(hIcon); // Free the native GDI icon handle
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ExitApp()
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            _notifyIcon.Visible = false;
            var currentIcon = _notifyIcon.Icon;
            _notifyIcon.Icon = null;
            currentIcon?.Dispose();
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                var currentIcon = _notifyIcon.Icon;
                _notifyIcon.Icon = null;
                currentIcon?.Dispose();
                _notifyIcon.Dispose();
            }
            base.OnExit(e);
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                Dispatcher.BeginInvoke(new Action(() => ApplyTheme()));
            }
        }

        public void ApplyTheme()
        {
            bool isDark = true;
            if (_settings.Theme == "Light")
            {
                isDark = false;
            }
            else if (_settings.Theme == "Dark")
            {
                isDark = true;
            }
            else // "Auto"
            {
                isDark = ThemeDetector.IsSystemDarkTheme();
            }

            LoadThemeResource(isDark);

            _mainWindow?.ApplyTheme(isDark);

            foreach (Window window in Windows)
            {
                if (window is SettingsWindow settingsWindow)
                {
                    settingsWindow.ApplyTheme(isDark);
                }
            }
        }

        private void LoadThemeResource(bool isDark)
        {
            string filename = isDark ? "DarkTheme.xaml" : "LightTheme.xaml";
            var uri = new Uri($"/CalculatorInAir;component/Themes/{filename}", UriKind.Relative);
            
            var merged = Resources.MergedDictionaries;
            ResourceDictionary? oldDict = null;
            foreach (var d in merged)
            {
                if (d.Source != null && (d.Source.OriginalString.Contains("DarkTheme.xaml") || d.Source.OriginalString.Contains("LightTheme.xaml")))
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
                System.Diagnostics.Debug.WriteLine($"Failed to load theme resource: {ex.Message}");
            }
        }

        public static class ThemeDetector
        {
            public static bool IsSystemDarkTheme()
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    {
                        if (key != null)
                        {
                            var value = key.GetValue("AppsUseLightTheme");
                            if (value is int i)
                            {
                                return i == 0;
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback to dark theme on read error
                }
                return true;
            }
        }

        // Helper trigger method called when MainWindow saves settings
        public void OnSettingsSaved()
        {
            Loc.LoadLanguage(Loc.CurrentLanguage);
            UpdateTrayMenuTexts();
            var oldIcon = _notifyIcon.Icon;
            _notifyIcon.Icon = CreateDynamicIcon(); // Redraw icon in case we add customization hooks later
            oldIcon?.Dispose();
            ApplyTheme(); // Ensure theme updates immediately if settings saved
        }
    }
}
