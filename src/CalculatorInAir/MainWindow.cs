using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

// Disambiguate types between WPF and WinForms/System.Drawing namespaces
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using MessageBox = System.Windows.MessageBox;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Clipboard = System.Windows.Clipboard;

namespace CalculatorInAir
{
    public class MainWindow : Window
    {
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;
        private const int WM_USER_WAKEUP = 0x0400 + 101;

        // Settings & State
        private readonly AppSettings _settings;
        private bool _isSettingsWindowOpen = false;

        // UI Controls
        private Border _mainBorder = null!;
        private TranslateTransform _translateTransform = null!;
        private TextBox _inputTextBox = null!;
        private TextBlock _placeholderTextBlock = null!;
        private Border _resultBorder = null!;
        private TextBlock _resultTextBlock = null!;
        private TextBlock _hintTextBlock = null!;

        // Win32 Interop Variables
        private IntPtr _hwnd;
        private HwndSource? _hwndSource;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow(AppSettings settings)
        {
            _settings = settings;

            InitializeUI();
            
            // Focus events
            Deactivated += MainWindow_Deactivated;
        }

        private void InitializeUI()
        {
            // Configure basic window parameters
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;
            Width = 600;
            Title = "Calculator in the Air";
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.Manual;
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Arial");

            // 1. Root Container Border
            _mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(242, 20, 20, 25)), // Translucent dark background
                CornerRadius = new CornerRadius(16),
                BorderThickness = new Thickness(1.5),
                Margin = new Thickness(25) // Leave space for the drop shadow
            };

            // Setup border gradient outline (violet to cyan glow edge)
            var borderGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            borderGradient.GradientStops.Add(new GradientStop(Color.FromArgb(77, 167, 139, 250), 0.0)); // Violet
            borderGradient.GradientStops.Add(new GradientStop(Color.FromArgb(77, 103, 232, 249), 0.5)); // Cyan
            borderGradient.GradientStops.Add(new GradientStop(Color.FromArgb(26, 255, 255, 255), 1.0));  // Subtle white
            _mainBorder.BorderBrush = borderGradient;

            // Setup modern soft drop shadow
            var shadowEffect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 25,
                ShadowDepth = 0,
                Opacity = 0.55
            };
            _mainBorder.Effect = shadowEffect;

            // Allow moving window by dragging
            _mainBorder.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            // Setup transform for slide animations
            _translateTransform = new TranslateTransform();
            _mainBorder.RenderTransform = _translateTransform;

            // 2. Inner Grid Layout
            var gridLayout = new Grid();
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Input row
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Result row (collapsible)

            // 2.1 Input Panel (Icon + Input text box + Placeholder)
            var inputGrid = new Grid { Height = 56 };
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // Icon
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Textbox

            // Vector calculator icon on the left
            var calculatorIcon = new Path
            {
                Data = Geometry.Parse("M4 5a3 3 0 0 1 3-3h10a3 3 0 0 1 3 3v14a3 3 0 0 1-3 3H7a3 3 0 0 1-3-3V5zm3 4h2V7H7v2zm4 0h2V7h-2v2zm4 0h2V7h-2v2zm-8 4h2v-2H7v2zm4 0h2v-2h-2v2zm4 0h2v-2h-2v2zm-8 4h2v-2H7v2zm4 4h6v-2h-6v2z"),
                Stretch = Stretch.Uniform,
                Width = 22,
                Height = 22,
                Margin = new Thickness(18, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            // Violet to Cyan gradient fill for the icon
            var iconGradient = new LinearGradientBrush(Color.FromRgb(167, 139, 250), Color.FromRgb(103, 232, 249), 45);
            calculatorIcon.Fill = iconGradient;
            inputGrid.Children.Add(calculatorIcon);
            Grid.SetColumn(calculatorIcon, 0);

            // Container for input box and placeholder overlapping
            var textBoxContainer = new Grid { Margin = new Thickness(5, 0, 20, 0) };

            // Placeholder Text
            _placeholderTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(110, 115, 125)),
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false // Click-through
            };
            textBoxContainer.Children.Add(_placeholderTextBlock);

            // Active Input Textbox
            _inputTextBox = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                CaretBrush = new SolidColorBrush(Color.FromRgb(167, 139, 250)), // Violet caret
                FontSize = 18,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Normal,
                SelectionBrush = new SolidColorBrush(Color.FromArgb(100, 139, 92, 246))
            };
            _inputTextBox.TextChanged += InputTextBox_TextChanged;
            _inputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
            textBoxContainer.Children.Add(_inputTextBox);

            inputGrid.Children.Add(textBoxContainer);
            Grid.SetColumn(textBoxContainer, 1);

            Grid.SetRow(inputGrid, 0);
            gridLayout.Children.Add(inputGrid);

            // 2.2 Result Panel (Separator line + equals glyph + Result value + tooltip)
            _resultBorder = new Border
            {
                Visibility = Visibility.Collapsed,
                Opacity = 0
            };

            var resultPanelGrid = new Grid();
            resultPanelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Separator line
            resultPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) }); // Content

            // Separator Line
            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                Margin = new Thickness(15, 0, 15, 0)
            };
            Grid.SetRow(separator, 0);
            resultPanelGrid.Children.Add(separator);

            // Content Grid
            var resultContentGrid = new Grid { Height = 56 };
            resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // "=" sign
            resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Result text
            resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Hint text

            // Glowing Equals Glyphs
            var equalsLabel = new TextBlock
            {
                Text = "=",
                Foreground = new SolidColorBrush(Color.FromRgb(167, 139, 250)),
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(15, 0, 5, 0)
            };
            Grid.SetColumn(equalsLabel, 0);
            resultContentGrid.Children.Add(equalsLabel);

            // Main Result display TextBlock (with gorgeous Gradient)
            _resultTextBlock = new TextBlock
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI, Arial"),
                Margin = new Thickness(5, 0, 10, 0)
            };
            var resultGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            resultGradient.GradientStops.Add(new GradientStop(Color.FromRgb(167, 139, 250), 0.0)); // Violet
            resultGradient.GradientStops.Add(new GradientStop(Color.FromRgb(103, 232, 249), 1.0)); // Cyan
            _resultTextBlock.Foreground = resultGradient;
            Grid.SetColumn(_resultTextBlock, 1);
            resultContentGrid.Children.Add(_resultTextBlock);

            // Action hints on the right
            _hintTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(110, 115, 125)),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 20, 0)
            };
            Grid.SetColumn(_hintTextBlock, 2);
            resultContentGrid.Children.Add(_hintTextBlock);

            Grid.SetRow(resultContentGrid, 1);
            resultPanelGrid.Children.Add(resultContentGrid);

            _resultBorder.Child = resultPanelGrid;
            Grid.SetRow(_resultBorder, 1);
            gridLayout.Children.Add(_resultBorder);

            _mainBorder.Child = gridLayout;
            Content = _mainBorder;

            ApplyLanguage();
        }

        public void ApplyLanguage()
        {
            _placeholderTextBlock.Text = Loc.Get("Placeholder");
            _hintTextBlock.Text = Loc.Get("PressEnterToCopy");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Win32 Interop Setup
            var helper = new WindowInteropHelper(this);
            _hwnd = helper.Handle;
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            _hwndSource.AddHook(HwndHook);

            RegisterHotkey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _hwndSource?.RemoveHook(HwndHook);
            UnregisterHotKey(_hwnd, HOTKEY_ID);
            base.OnClosed(e);
        }

        public void RegisterHotkey()
        {
            if (_hwnd == IntPtr.Zero) return;

            // Clear previous hotkey first
            UnregisterHotKey(_hwnd, HOTKEY_ID);

            // Construct modifier mask
            uint modifiers = 0;
            if (_settings.Alt) modifiers |= 0x0001;
            if (_settings.Ctrl) modifiers |= 0x0002;
            if (_settings.Shift) modifiers |= 0x0004;
            if (_settings.Win) modifiers |= 0x0008;
            modifiers |= 0x4000; // MOD_NOREPEAT - prevents holding down hotkey spamming the hook

            uint vk = (uint)_settings.VirtualKey;

            bool ok = RegisterHotKey(_hwnd, HOTKEY_ID, modifiers, vk);
            if (!ok)
            {
                MessageBox.Show(
                    string.Format(Loc.Get("HotkeyConflict"), _settings.HotkeyDisplay),
                    Loc.Get("HotkeyConflictTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleWindow();
                handled = true;
            }
            else if (msg == WM_USER_WAKEUP)
            {
                ShowWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void ToggleWindow()
        {
            if (this.IsVisible && this.Opacity > 0.1)
            {
                HideWindow();
            }
            else
            {
                ShowWindow();
            }
        }

        public void ShowWindow()
        {
            UpdatePositionToActiveMonitor();

            this.Show();
            this.Activate();

            _inputTextBox.Focus();
            _inputTextBox.SelectAll();

            // Slide and fade-in animation
            this.Opacity = 0;
            _translateTransform.Y = -15; // Offset upwards by 15px

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideDown = new DoubleAnimation
            {
                From = -15,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(Window.OpacityProperty, fadeIn);
            _translateTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);
        }

        public void HideWindow()
        {
            // Close settings if it happens to be open to prevent layout glitches
            if (_isSettingsWindowOpen) return;

            // Slide and fade-out animation
            var fadeOut = new DoubleAnimation
            {
                From = this.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var slideUp = new DoubleAnimation
            {
                From = _translateTransform.Y,
                To = -10,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, e) =>
            {
                this.Hide();
            };

            this.BeginAnimation(Window.OpacityProperty, fadeOut);
            _translateTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void UpdatePositionToActiveMonitor()
        {
            // Find monitor where cursor is
            var mousePos = System.Windows.Forms.Cursor.Position;
            var activeScreen = System.Windows.Forms.Screen.FromPoint(mousePos);

            // Compute DPI factor to convert pixel bounds to WPF Device Independent Units (DIPs)
            double dpiX = 1.0;
            double dpiY = 1.0;
            
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            double screenWidth = activeScreen.WorkingArea.Width / dpiX;
            double screenHeight = activeScreen.WorkingArea.Height / dpiY;
            double screenLeft = activeScreen.WorkingArea.Left / dpiX;
            double screenTop = activeScreen.WorkingArea.Top / dpiY;

            this.Left = screenLeft + (screenWidth - this.Width) / 2;
            this.Top = screenTop + (screenHeight * 0.20); // 20% down from top
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = _inputTextBox.Text.Trim();

            // Toggle placeholder
            _placeholderTextBlock.Visibility = string.IsNullOrEmpty(_inputTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrEmpty(text))
            {
                HideResultBorder();
                return;
            }

            try
            {
                // Try evaluating math expression
                double val = MathParser.Evaluate(text);
                string formatted = MathParser.FormatResult(val, _settings.DecimalPlaces);

                _resultTextBlock.Text = formatted;
                ShowResultBorder();
            }
            catch
            {
                // Silently hide result if it is invalid / incomplete
                HideResultBorder();
            }
        }

        private void ShowResultBorder()
        {
            if (_resultBorder.Visibility == Visibility.Visible && _resultBorder.Opacity > 0.9) return;

            _resultBorder.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = _resultBorder.Opacity,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            _resultBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void HideResultBorder()
        {
            if (_resultBorder.Visibility == Visibility.Collapsed) return;

            _resultBorder.Visibility = Visibility.Collapsed;
            _resultBorder.Opacity = 0;
            _resultBorder.BeginAnimation(UIElement.OpacityProperty, null); // Stop animations
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideWindow();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (_resultBorder.Visibility == Visibility.Visible && !string.IsNullOrEmpty(_resultTextBlock.Text))
                {
                    if (_settings.CopyOnEnter)
                    {
                        try
                        {
                            Clipboard.SetText(_resultTextBlock.Text);
                        }
                        catch
                        {
                            // In case of clipboard locking issues
                        }
                    }
                }
                HideWindow();
                e.Handled = true;
            }
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            if (_settings.HideOnBlur && !_isSettingsWindowOpen)
            {
                HideWindow();
            }
        }

        public void OpenSettings()
        {
            if (_isSettingsWindowOpen) return;

            _isSettingsWindowOpen = true;
            var settingsWindow = new SettingsWindow(_settings, () =>
            {
                // Settings saved callback: re-register hotkey, re-apply UI language text
                RegisterHotkey();
                ApplyLanguage();
                (System.Windows.Application.Current as App)?.OnSettingsSaved();
            });

            // Re-enable deactivation hide when the settings window closes
            settingsWindow.Closed += (s, e) =>
            {
                _isSettingsWindowOpen = false;
                // Refocus input text box on main window
                if (this.IsVisible)
                {
                    _inputTextBox.Focus();
                }
            };

            settingsWindow.ShowDialog();
        }
    }
}
