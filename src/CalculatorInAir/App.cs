using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Forms;

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

            // 1. Load configuration settings
            _settings = SettingsManager.Load();

            // 2. Initialize main search window
            _mainWindow = new MainWindow(_settings);

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
            _notifyIcon.Text = Loc.Get("SettingsTitle").Split(" - ")[0]; // "Calculator in the Air" in localized form if desired, or keep default
        }

        private Icon CreateDynamicIcon()
        {
            // Draw a beautiful 32x32 icon in memory to avoid ship-along assets
            using (var bmp = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw a beautiful rounded rectangle background with violet-blue gradient
                    var rect = new Rectangle(2, 2, 28, 28);
                    using (var brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(139, 92, 246), // Violet
                        Color.FromArgb(59, 130, 246),  // Blue
                        45f))
                    {
                        using (var path = GetRoundedRectPath(rect, 7))
                        {
                            g.FillPath(brush, path);
                        }
                    }

                    // Draw an equal sign symbol in the middle
                    using (var pen = new Pen(Color.White, 3))
                    {
                        // Parallel lines for the '=' sign
                        g.DrawLine(pen, 9, 12, 23, 12);
                        g.DrawLine(pen, 9, 18, 23, 18);
                    }
                }

                // Retrieve handle to icon and copy it to a managed Icon object
                IntPtr hIcon = bmp.GetHicon();
                var icon = Icon.FromHandle(hIcon);
                return (Icon)icon.Clone(); // Clone guarantees we own the resource
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
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnExit(e);
        }

        // Helper trigger method called when MainWindow saves settings
        public void OnSettingsSaved()
        {
            UpdateTrayMenuTexts();
            _notifyIcon.Icon = CreateDynamicIcon(); // Redraw icon in case we add customization hooks later
        }
    }
}
