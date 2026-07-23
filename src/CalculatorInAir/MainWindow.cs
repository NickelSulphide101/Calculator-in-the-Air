using System;
using System.Collections.Generic;
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

        private readonly double _heightCollapsed;
        private readonly double _heightExpanded;
        private readonly double _windowWidth;

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
        private Path _calculatorIcon = null!;
        private Border _separator = null!;
        private TextBlock _equalsLabel = null!;
        private DropShadowEffect _shadowEffect = null!;

        // Win32 Interop Variables
        private IntPtr _hwnd;
        private HwndSource? _hwndSource;
        private readonly bool _isWin11OrGreater;

        // History
        private readonly List<string> _history = new List<string>();
        private int _historyIndex = -1;
        private string _tempInput = "";

        // Win32 P/Invokes for backdrop and version
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RtlGetVersion(ref OSVERSIONINFOEX lpVersionInformation);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_TRANSLUCENTAUTHORITATIVE = 3; // Acrylic
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2; // 启用圆角

        private static bool IsWindows11OrGreater()
        {
            try
            {
                var os = new OSVERSIONINFOEX();
                os.dwOSVersionInfoSize = Marshal.SizeOf(os);
                if (RtlGetVersion(ref os) == 0)
                {
                    return os.dwBuildNumber >= 22000;
                }
            }
            catch { }
            return false;
        }

        public MainWindow(AppSettings settings)
        {
            _settings = settings;
            _isWin11OrGreater = IsWindows11OrGreater();

            if (_isWin11OrGreater)
            {
                AllowsTransparency = false;
                WindowStyle = WindowStyle.None;

                _heightCollapsed = 59;
                _heightExpanded = 116;
                _windowWidth = 550;

                // Setup WindowChrome to remove client borders while allows transparency is false
                var chrome = new System.Windows.Shell.WindowChrome
                {
                    CaptionHeight = 0,
                    ResizeBorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(0),
                    GlassFrameThickness = new Thickness(-1)
                };
                System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
            }
            else
            {
                AllowsTransparency = true;
                WindowStyle = WindowStyle.None;

                _heightCollapsed = 109;
                _heightExpanded = 166;
                _windowWidth = 600;
            }

            InitializeUI();
            Deactivated += MainWindow_Deactivated;
        }

        private void InitializeUI()
        {
            // Configure basic window parameters
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;
            Width = _windowWidth;
            Height = _heightCollapsed;
            Title = "Calculator in the Air";
            SizeToContent = SizeToContent.Manual;
            WindowStartupLocation = WindowStartupLocation.Manual;
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Arial");

            // 1. Root Container Border
            _mainBorder = new Border
            {
                CornerRadius = _isWin11OrGreater ? new CornerRadius(12) : new CornerRadius(16),
                BorderThickness = new Thickness(1.5),
                Margin = _isWin11OrGreater ? new Thickness(0) : new Thickness(25) // Leave space for the drop shadow
            };

            // Set resource references for dynamic styling (XAML-separated)
            _mainBorder.SetResourceReference(Border.BackgroundProperty, "WindowBackgroundBrush");
            _mainBorder.SetResourceReference(Border.BorderBrushProperty, "WindowBorderBrush");

            if (!_isWin11OrGreater)
            {
                // Setup modern soft drop shadow
                _shadowEffect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.55 // Default dark theme value; updated dynamically by ApplyTheme
                };
                _mainBorder.Effect = _shadowEffect;
            }

            // Allow moving window by dragging
            _mainBorder.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            // Setup transform for slide and shake animations
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
            _calculatorIcon = new Path
            {
                Data = Geometry.Parse("M4 5a3 3 0 0 1 3-3h10a3 3 0 0 1 3 3v14a3 3 0 0 1-3 3H7a3 3 0 0 1-3-3V5zm3 4h2V7H7v2zm4 0h2V7h-2v2zm4 0h2V7h-2v2zm-8 4h2v-2H7v2zm4 0h2v-2h-2v2zm4 0h2v-2h-2v2zm-8 4h2v-2H7v2zm4 4h6v-2h-6v2z"),
                Stretch = Stretch.Uniform,
                Width = 22,
                Height = 22,
                Margin = new Thickness(18, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _calculatorIcon.SetResourceReference(Path.FillProperty, "CalculatorIconBrush");
            inputGrid.Children.Add(_calculatorIcon);
            Grid.SetColumn(_calculatorIcon, 0);

            // Container for input box and placeholder overlapping
            var textBoxContainer = new Grid { Margin = new Thickness(5, 0, 20, 0) };

            // Placeholder Text
            _placeholderTextBlock = new TextBlock
            {
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false // Click-through
            };
            _placeholderTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "PlaceholderForegroundBrush");
            textBoxContainer.Children.Add(_placeholderTextBlock);

            // Active Input Textbox
            _inputTextBox = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 18,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Normal
            };
            _inputTextBox.SetResourceReference(TextBox.ForegroundProperty, "InputForegroundBrush");
            _inputTextBox.SetResourceReference(TextBox.CaretBrushProperty, "InputCaretBrush");
            _inputTextBox.SetResourceReference(TextBox.SelectionBrushProperty, "InputSelectionBrush");

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
            _separator = new Border
            {
                Height = 1,
                Margin = new Thickness(15, 0, 15, 0)
            };
            _separator.SetResourceReference(Border.BackgroundProperty, "SeparatorBrush");
            Grid.SetRow(_separator, 0);
            resultPanelGrid.Children.Add(_separator);

            // Content Grid
            var resultContentGrid = new Grid { Height = 56 };
            resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // "=" sign
            resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Result text
            resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Hint text

            // Glowing Equals Glyphs
            _equalsLabel = new TextBlock
            {
                Text = "=",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(15, 0, 5, 0)
            };
            _equalsLabel.SetResourceReference(TextBlock.ForegroundProperty, "EqualsLabelBrush");
            Grid.SetColumn(_equalsLabel, 0);
            resultContentGrid.Children.Add(_equalsLabel);

            // Main Result display TextBlock
            _resultTextBlock = new TextBlock
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI, Arial"),
                Margin = new Thickness(5, 0, 10, 0)
            };
            _resultTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "ResultForegroundBrush");
            
            // Tabular lining figures to prevent numeric width jitter
            System.Windows.Documents.Typography.SetNumeralAlignment(_resultTextBlock, System.Windows.FontNumeralAlignment.Tabular);

            Grid.SetColumn(_resultTextBlock, 1);
            resultContentGrid.Children.Add(_resultTextBlock);

            // Action hints on the right
            _hintTextBlock = new TextBlock
            {
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 20, 0)
            };
            _hintTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "HintForegroundBrush");
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

            // Enable Win11 Native Backdrop
            if (_isWin11OrGreater)
            {
                int backdropType = DWMSBT_TRANSLUCENTAUTHORITATIVE; // Acrylic
                DwmSetWindowAttribute(_hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));

                int cornerPreference = DWMWCP_ROUND;
                DwmSetWindowAttribute(_hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
            }
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
            modifiers |= 0x4000; // MOD_NOREPEAT

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
            // Pre-set invisible state to prevent visual flash before positioning
            this.Opacity = 0;
            _translateTransform.Y = -15;

            this.Show();
            this.Activate();

            // Position after Show() so PresentationSource is available for accurate DPI scaling
            UpdatePositionToActiveMonitor();

            _historyIndex = _history.Count; // Reset navigation index to the end

            _inputTextBox.Focus();
            _inputTextBox.SelectAll();

            // Slide and fade-in animation
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
            if (_isSettingsWindowOpen) return;

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
                // Reset to collapsed height for next show
                Height = _heightCollapsed;
                HideResultBorder();
            };

            this.BeginAnimation(Window.OpacityProperty, fadeOut);
            _translateTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void UpdatePositionToActiveMonitor()
        {
            var mousePos = System.Windows.Forms.Cursor.Position;
            var activeScreen = System.Windows.Forms.Screen.FromPoint(mousePos);

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
            this.Top = screenTop + (screenHeight * 0.20);
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = _inputTextBox.Text.Trim();

            _placeholderTextBlock.Visibility = string.IsNullOrEmpty(_inputTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

            // Restore error colors back to theme dynamic resources
            _resultTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "ResultForegroundBrush");
            _equalsLabel.SetResourceReference(TextBlock.ForegroundProperty, "EqualsLabelBrush");

            if (string.IsNullOrEmpty(text))
            {
                HideResultBorder();
                return;
            }

            try
            {
                double val = MathParser.Evaluate(text);
                string formatted = MathParser.FormatResult(val, _settings.DecimalPlaces);

                _resultTextBlock.Text = formatted;
                ShowResultBorder();
            }
            catch
            {
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

            // Animate Window height expansion smoothly
            var heightAnimation = new DoubleAnimation
            {
                From = Height,
                To = _heightExpanded,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(Window.HeightProperty, heightAnimation);
        }

        private void HideResultBorder()
        {
            if (_resultBorder.Visibility == Visibility.Collapsed) return;

            var fadeOut = new DoubleAnimation
            {
                From = _resultBorder.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, e) =>
            {
                _resultBorder.Visibility = Visibility.Collapsed;
            };
            _resultBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            // Animate Window height collapse smoothly
            var heightAnimation = new DoubleAnimation
            {
                From = Height,
                To = _heightCollapsed,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            BeginAnimation(Window.HeightProperty, heightAnimation);
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
                string text = _inputTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    try
                    {
                        double val = MathParser.Evaluate(text);
                        string formatted = MathParser.FormatResult(val, _settings.DecimalPlaces);

                        if (_settings.CopyOnEnter)
                        {
                            for (int attempt = 0; attempt < 5; attempt++)
                            {
                                try
                                {
                                    Clipboard.SetText(formatted);
                                    break;
                                }
                                catch
                                {
                                    System.Threading.Thread.Sleep(10);
                                }
                            }
                        }

                        AddToHistory(_inputTextBox.Text); // save the full text typed
                        HideWindow();
                    }
                    catch (Exception ex)
                    {
                        // Play shake animation and display error message on enter key failure
                        ShowErrorFeedback(ex.Message);
                    }
                }
                else
                {
                    HideWindow();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (_history.Count > 0 && _historyIndex > 0)
                {
                    if (_historyIndex == _history.Count)
                    {
                        _tempInput = _inputTextBox.Text;
                    }
                    _historyIndex--;
                    _inputTextBox.Text = _history[_historyIndex];
                    _inputTextBox.CaretIndex = _inputTextBox.Text.Length;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (_history.Count == 0)
                {
                    e.Handled = true;
                    return;
                }

                if (_historyIndex < _history.Count)
                {
                    _historyIndex++;
                    if (_historyIndex == _history.Count)
                    {
                        _inputTextBox.Text = _tempInput;
                    }
                    else
                    {
                        _inputTextBox.Text = _history[_historyIndex];
                    }
                    _inputTextBox.CaretIndex = _inputTextBox.Text.Length;
                }
                e.Handled = true;
            }
        }

        private void ShowErrorFeedback(string message)
        {
            _resultTextBlock.Text = message;
            _resultTextBlock.Foreground = Brushes.Tomato;
            _equalsLabel.Foreground = Brushes.Tomato;

            ShowResultBorder();

            // Perform shake keyframe animation
            var shakeAnimation = new DoubleAnimationUsingKeyFrames();
            shakeAnimation.Duration = TimeSpan.FromMilliseconds(400);
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50))));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-4, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250))));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(4, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(350))));

            _translateTransform.BeginAnimation(TranslateTransform.XProperty, shakeAnimation);
        }

        private void AddToHistory(string expr)
        {
            if (_history.Count == 0 || _history[_history.Count - 1] != expr)
            {
                _history.Add(expr);
                if (_history.Count > 100)
                {
                    _history.RemoveAt(0);
                }
            }
            _historyIndex = _history.Count;
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
                RegisterHotkey();
                ApplyLanguage();
                (System.Windows.Application.Current as App)?.OnSettingsSaved();
            });

            settingsWindow.Closed += (s, e) =>
            {
                _isSettingsWindowOpen = false;
                if (this.IsVisible)
                {
                    _inputTextBox.Focus();
                }
            };

            settingsWindow.ShowDialog();
        }

        public void ApplyTheme(bool isDark)
        {
            // Most styling is managed dynamically via XAML ResourceDictionaries.
            // DropShadowEffect is not a FrameworkElement so DynamicResource binding
            // does not work — manually apply the shadow opacity from theme resources.
            if (_shadowEffect != null && System.Windows.Application.Current?.Resources["ShadowOpacity"] is double shadowOpacity)
            {
                _shadowEffect.Opacity = shadowOpacity;
            }
        }
    }
}
