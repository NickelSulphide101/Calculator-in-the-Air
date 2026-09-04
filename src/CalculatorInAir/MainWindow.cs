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
using System.Windows.Threading;

// Disambiguate types between WPF and WinForms/System.Drawing namespaces
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using MessageBox = System.Windows.MessageBox;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Clipboard = System.Windows.Clipboard;
using Orientation = System.Windows.Controls.Orientation;
using Cursors = System.Windows.Input.Cursors;

namespace CalculatorInAir
{
    public class MainWindow : Window
    {
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;

        private double _heightCollapsed = 109;
        private double _heightExpanded = 166;
        private double _windowWidth = 600;
        private double _windowScale = 1.0;
        private double _targetOpacity = 1.0;

        // UI Grids for dynamic scaling
        private Grid _inputGrid = null!;
        private Grid _resultContentGrid = null!;

        // Settings & State
        private readonly AppSettings _settings;
        private bool _isSettingsWindowOpen = false;
        private bool _isShowing = false;
        private bool _isPinned = false;
        private DateTime _lastEscPressTime = DateTime.MinValue;

        // UI Controls & Transforms
        private Border _mainBorder = null!;
        private TranslateTransform _translateTransform = null!;
        private ScaleTransform _scaleTransform = null!;
        private TransformGroup _transformGroup = null!;

        private TextBox _inputTextBox = null!;
        private TextBlock _placeholderTextBlock = null!;
        private Border _resultBorder = null!;
        private TextBlock _resultTextBlock = null!;
        private TextBlock _hintTextBlock = null!;
        private TextBlock _formatTagTextBlock = null!;
        private Path _calculatorIcon = null!;
        private Border _separator = null!;
        private TextBlock _equalsLabel = null!;
        private DropShadowEffect _shadowEffect = null!;

        // Pin Button Controls
        private Button _pinButton = null!;
        private Path _pinIcon = null!;

        // Clipboard Hint Controls
        private Border _clipboardHintBorder = null!;
        private TextBlock _clipboardHintTextBlock = null!;
        private string _clipboardDetectedFormula = "";

        // Toast Feedback Controls
        private Border _toastBorder = null!;
        private TextBlock _toastTextBlock = null!;
        private DispatcherTimer? _toastTimer;

        // Format candidates
        private readonly List<string> _formatCandidates = new List<string>();
        private readonly List<string> _formatLabels = new List<string>();
        private int _selectedFormatIndex = 0;

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

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        private static bool IsWindows11OrGreater()
        {
            try
            {
                var sysVersion = Environment.OSVersion.Version;
                if (sysVersion.Major > 10 || (sysVersion.Major == 10 && sysVersion.Build >= 22000))
                {
                    return true;
                }

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

            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;

            _heightCollapsed = 109;
            _heightExpanded = 166;
            _windowWidth = 600;

            InitializeUI();
            ApplyWindowLayout(_settings.WindowWidth, _settings.WindowScale, _settings.WindowOpacity, isInitializing: true);
            Deactivated += MainWindow_Deactivated;
        }

        private void InitializeUI()
        {
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;
            Width = _windowWidth;
            Height = _heightCollapsed;
            Title = "Calculator in the Air";
            SizeToContent = SizeToContent.Manual;
            WindowStartupLocation = WindowStartupLocation.Manual;

            // 1. Root Container Border
            _mainBorder = new Border
            {
                CornerRadius = _isWin11OrGreater ? new CornerRadius(12) : new CornerRadius(16),
                BorderThickness = new Thickness(1.5),
                Margin = _isWin11OrGreater ? new Thickness(0) : new Thickness(25),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            _mainBorder.SetResourceReference(Border.BackgroundProperty, "WindowBackgroundBrush");
            _mainBorder.SetResourceReference(Border.BorderBrushProperty, "WindowBorderBrush");

            if (!_isWin11OrGreater)
            {
                _shadowEffect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.55
                };
                _mainBorder.Effect = _shadowEffect;
            }

            _mainBorder.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            // Setup combined transform for scale, slide, and shake animations
            _translateTransform = new TranslateTransform();
            _scaleTransform = new ScaleTransform(1.0, 1.0);
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
            _mainBorder.RenderTransform = _transformGroup;

            // 2. Inner Grid Layout
            var gridLayout = new Grid();
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Input row
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Result row (collapsible)

            // 2.1 Input Panel (Icon + Input text box + Pin Button)
            _inputGrid = new Grid { Height = 56 };
            _inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // Icon
            _inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Textbox
            _inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) }); // Pin Button

            // Calculator icon
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
            _inputGrid.Children.Add(_calculatorIcon);
            Grid.SetColumn(_calculatorIcon, 0);

            // Container for input box, placeholder, and clipboard hint
            var textBoxStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 10, 0) };
            var textBoxContainer = new Grid();

            _placeholderTextBlock = new TextBlock
            {
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            _placeholderTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "PlaceholderForegroundBrush");
            textBoxContainer.Children.Add(_placeholderTextBlock);

            _inputTextBox = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 18,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Normal,
                MaxLength = 500
            };
            _inputTextBox.SetResourceReference(TextBox.ForegroundProperty, "InputForegroundBrush");
            _inputTextBox.SetResourceReference(TextBox.CaretBrushProperty, "InputCaretBrush");
            _inputTextBox.SetResourceReference(TextBox.SelectionBrushProperty, "InputSelectionBrush");

            _inputTextBox.TextChanged += InputTextBox_TextChanged;
            _inputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
            textBoxContainer.Children.Add(_inputTextBox);
            textBoxStack.Children.Add(textBoxContainer);

            // Smart Clipboard Hint Bar
            _clipboardHintBorder = new Border
            {
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 2, 0, 0),
                Cursor = Cursors.Hand
            };
            _clipboardHintTextBlock = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(124, 76, 237)),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            _clipboardHintBorder.Child = _clipboardHintTextBlock;
            _clipboardHintBorder.MouseLeftButtonDown += (s, e) => ApplyClipboardHint();
            textBoxStack.Children.Add(_clipboardHintBorder);

            _inputGrid.Children.Add(textBoxStack);
            Grid.SetColumn(textBoxStack, 1);

            // Pin Button on the right
            _pinButton = new Button
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = Loc.Get("PinToolTip")
            };
            _pinIcon = new Path
            {
                Data = Geometry.Parse("M16 12V4H17V2H7V4H8V12L6 14V16H11V22H13V16H18V14L16 12Z"),
                Stretch = Stretch.Uniform,
                Width = 15,
                Height = 15,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            UpdatePinIconVisual();
            _pinButton.Content = _pinIcon;
            _pinButton.Click += (s, e) => TogglePin();
            _inputGrid.Children.Add(_pinButton);
            Grid.SetColumn(_pinButton, 2);

            Grid.SetRow(_inputGrid, 0);
            gridLayout.Children.Add(_inputGrid);

            // 2.2 Result Panel
            _resultBorder = new Border
            {
                Visibility = Visibility.Collapsed,
                Opacity = 0
            };

            var resultPanelGrid = new Grid();
            resultPanelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Separator
            resultPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) }); // Content

            _separator = new Border
            {
                Height = 1,
                Margin = new Thickness(15, 0, 15, 0)
            };
            _separator.SetResourceReference(Border.BackgroundProperty, "SeparatorBrush");
            Grid.SetRow(_separator, 0);
            resultPanelGrid.Children.Add(_separator);

            _resultContentGrid = new Grid { Height = 56 };
            _resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // "="
            _resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Result text
            _resultContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Format tag + Hint

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
            _resultContentGrid.Children.Add(_equalsLabel);

            _resultTextBlock = new TextBlock
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 10, 0)
            };
            _resultTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "ResultForegroundBrush");
            System.Windows.Documents.Typography.SetNumeralAlignment(_resultTextBlock, System.Windows.FontNumeralAlignment.Tabular);

            Grid.SetColumn(_resultTextBlock, 1);
            _resultContentGrid.Children.Add(_resultTextBlock);

            var hintPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };

            _formatTagTextBlock = new TextBlock
            {
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(124, 76, 237)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            hintPanel.Children.Add(_formatTagTextBlock);

            _hintTextBlock = new TextBlock
            {
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            _hintTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "HintForegroundBrush");
            hintPanel.Children.Add(_hintTextBlock);

            Grid.SetColumn(hintPanel, 2);
            _resultContentGrid.Children.Add(hintPanel);

            Grid.SetRow(_resultContentGrid, 1);
            resultPanelGrid.Children.Add(_resultContentGrid);

            _resultBorder.Child = resultPanelGrid;
            Grid.SetRow(_resultBorder, 1);
            gridLayout.Children.Add(_resultBorder);

            // Toast feedback popup at bottom
            _toastBorder = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromArgb(230, 20, 20, 25)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 6, 12, 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 12),
                Visibility = Visibility.Collapsed,
                Opacity = 0,
                IsHitTestVisible = false
            };
            _toastTextBlock = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Medium
            };
            _toastBorder.Child = _toastTextBlock;
            gridLayout.Children.Add(_toastBorder);
            Grid.SetRowSpan(_toastBorder, 2);

            _mainBorder.Child = gridLayout;
            Content = _mainBorder;

            ApplyFontFamily();
            ApplyLanguage();
        }

        public void ApplyFontFamily()
        {
            FontFamily font = _settings.UseMonospaceFont
                ? new FontFamily("Cascadia Code, Consolas, Segoe UI Variable Display, Courier New, monospace")
                : new FontFamily("Segoe UI Variable Text, Segoe UI, Arial");

            FontFamily = font;
            if (_inputTextBox != null) _inputTextBox.FontFamily = font;
            if (_placeholderTextBlock != null) _placeholderTextBlock.FontFamily = font;
            if (_resultTextBlock != null) _resultTextBlock.FontFamily = font;
        }

        public void ApplyLanguage()
        {
            _placeholderTextBlock.Text = Loc.Get("Placeholder");
            _hintTextBlock.Text = Loc.Get("ShortcutHint");
            _pinButton.ToolTip = Loc.Get("PinToolTip");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwnd = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            _hwndSource?.AddHook(HwndHook);

            RegisterHotkey();
        }

        public void RegisterHotkey()
        {
            if (_hwnd == IntPtr.Zero) return;

            UnregisterHotKey(_hwnd, HOTKEY_ID);

            uint modifiers = 0;
            if (_settings.Ctrl) modifiers |= 0x0002;
            if (_settings.Alt) modifiers |= 0x0001;
            if (_settings.Shift) modifiers |= 0x0004;
            if (_settings.Win) modifiers |= 0x0008;

            // Security guard: Ensure at least one modifier key is set to prevent globally hijacking a bare key
            if (modifiers == 0)
            {
                modifiers = 0x0001; // Fallback to Alt
                _settings.Alt = true;
                _settings.VirtualKey = 0x20;
                _settings.HotkeyDisplay = "Alt + Space";
            }

            bool success = RegisterHotKey(_hwnd, HOTKEY_ID, modifiers, (uint)_settings.VirtualKey);
            if (!success)
            {
                string msg = string.Format(Loc.Get("HotkeyConflict"), _settings.HotkeyDisplay);
                MessageBox.Show(msg, Loc.Get("HotkeyConflictTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hwnd, HOTKEY_ID);
            }
            base.OnClosed(e);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleWindow();
                handled = true;
            }
            else if (msg == (int)Program.WakeupMessage)
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

        private void ForceForeground()
        {
            if (_hwnd != IntPtr.Zero)
            {
                ShowWindow(_hwnd, SW_SHOW);
                SetForegroundWindow(_hwnd);
            }
        }

        public void ShowWindow()
        {
            _isShowing = true;

            this.Opacity = 0;
            _translateTransform.Y = -8;
            _scaleTransform.ScaleX = 0.96;
            _scaleTransform.ScaleY = 0.96;

            this.Show();
            ForceForeground();
            this.Activate();

            UpdatePositionToActiveMonitor();
            _historyIndex = _history.Count;

            _inputTextBox.Focus();
            _inputTextBox.SelectAll();

            CheckClipboardForFormula();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = _targetOpacity,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            fadeIn.Completed += (s, e) => { _isShowing = false; };

            var scaleXIn = new DoubleAnimation
            {
                From = 0.96,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var scaleYIn = new DoubleAnimation
            {
                From = 0.96,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var slideDown = new DoubleAnimation
            {
                From = -8,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(Window.OpacityProperty, fadeIn);
            _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXIn);
            _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYIn);
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

            var scaleXOut = new DoubleAnimation
            {
                From = _scaleTransform.ScaleX,
                To = 0.96,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var scaleYOut = new DoubleAnimation
            {
                From = _scaleTransform.ScaleY,
                To = 0.96,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, e) =>
            {
                this.Hide();
                Height = _heightCollapsed;
                HideResultBorder();
                _scaleTransform.ScaleX = 1.0;
                _scaleTransform.ScaleY = 1.0;
            };

            this.BeginAnimation(Window.OpacityProperty, fadeOut);
            _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXOut);
            _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYOut);
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

        private void TogglePin()
        {
            _isPinned = !_isPinned;
            UpdatePinIconVisual();
            ShowToast(_isPinned ? Loc.Get("PinnedToast") : Loc.Get("UnpinnedToast"));
        }

        private void UpdatePinIconVisual()
        {
            if (_pinIcon == null) return;
            if (_isPinned)
            {
                _pinIcon.SetResourceReference(Path.FillProperty, "CalculatorIconBrush");
                _pinIcon.Opacity = 1.0;
            }
            else
            {
                _pinIcon.SetResourceReference(Path.FillProperty, "PlaceholderForegroundBrush");
                _pinIcon.Opacity = 0.5;
            }
        }

        private void CheckClipboardForFormula()
        {
            if (!_settings.EnableClipboardDetection)
            {
                HideClipboardHint();
                return;
            }

            try
            {
                if (!Clipboard.ContainsText()) { HideClipboardHint(); return; }
                string text = Clipboard.GetText().Trim();
                if (string.IsNullOrEmpty(text) || text.Length < 2 || text.Length > 100) { HideClipboardHint(); return; }

                bool hasOperator = text.Contains('+') || text.Contains('-') || text.Contains('*') || text.Contains('/') || text.Contains('%') || text.Contains('^') || text.Contains('(');
                if (!hasOperator) { HideClipboardHint(); return; }

                MathParser.Evaluate(text);
                _clipboardDetectedFormula = text;
                _clipboardHintTextBlock.Text = string.Format(Loc.Get("ClipboardHint"), text);
                _clipboardHintBorder.Visibility = Visibility.Visible;
            }
            catch
            {
                HideClipboardHint();
            }
        }

        private void HideClipboardHint()
        {
            _clipboardDetectedFormula = "";
            if (_clipboardHintBorder != null)
            {
                _clipboardHintBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyClipboardHint()
        {
            if (!string.IsNullOrEmpty(_clipboardDetectedFormula))
            {
                _inputTextBox.Text = _clipboardDetectedFormula;
                _inputTextBox.CaretIndex = _inputTextBox.Text.Length;
                HideClipboardHint();
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = _inputTextBox.Text.Trim();
            _placeholderTextBlock.Visibility = string.IsNullOrEmpty(_inputTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

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
                BuildFormatCandidates(val);
                UpdateResultDisplay();
                ShowResultBorder();
            }
            catch
            {
                HideResultBorder();
            }
        }

        private void BuildFormatCandidates(double val)
        {
            _formatCandidates.Clear();
            _formatLabels.Clear();

            string std = NumberFormatter.FormatStandard(val, _settings.DecimalPlaces, _settings.UseThousandsSeparator);
            _formatCandidates.Add(std);
            _formatLabels.Add(Loc.Get("FormatStandardLabel"));

            string alt = NumberFormatter.FormatStandard(val, _settings.DecimalPlaces, !_settings.UseThousandsSeparator);
            _formatCandidates.Add(alt);
            _formatLabels.Add(Loc.Get("FormatRawLabel"));

            if (Math.Abs(val) >= 10000)
            {
                string wan = NumberFormatter.FormatTenThousand(val);
                _formatCandidates.Add(wan);
                _formatLabels.Add(Loc.Get("FormatWanLabel"));
            }

            if (val > 0 && val < 1e15)
            {
                string rmb = NumberFormatter.FormatChineseRMB(val);
                _formatCandidates.Add(rmb);
                _formatLabels.Add(Loc.Get("FormatRMBLabel"));
            }

            _selectedFormatIndex = 0;
        }

        private void UpdateResultDisplay()
        {
            if (_formatCandidates.Count == 0) return;
            if (_selectedFormatIndex < 0 || _selectedFormatIndex >= _formatCandidates.Count)
                _selectedFormatIndex = 0;

            _resultTextBlock.Text = _formatCandidates[_selectedFormatIndex];
            _formatTagTextBlock.Text = $"[{_selectedFormatIndex + 1}/{_formatCandidates.Count} {_formatLabels[_selectedFormatIndex]}]";
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
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                TogglePin();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                var span = (DateTime.Now - _lastEscPressTime).TotalMilliseconds;
                if (span <= 350 || string.IsNullOrEmpty(_inputTextBox.Text))
                {
                    HideWindow();
                }
                else
                {
                    _inputTextBox.Text = string.Empty;
                    _lastEscPressTime = DateTime.Now;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                string text = _inputTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(text) && _formatCandidates.Count > 0)
                {
                    string resText = _formatCandidates[_selectedFormatIndex];
                    bool isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
                    string copyText = isShift ? $"{text} = {resText}" : resText;

                    if (_settings.CopyOnEnter)
                    {
                        for (int attempt = 0; attempt < 5; attempt++)
                        {
                            try
                            {
                                Clipboard.SetText(copyText);
                                break;
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(10);
                            }
                        }
                    }

                    AddToHistory(_inputTextBox.Text);

                    string toastMsg = (isShift ? Loc.Get("CopiedFormula") : Loc.Get("CopiedResult")) + copyText;
                    ShowToast(toastMsg);

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        HideWindow();
                    };
                    timer.Start();
                }
                else
                {
                    HideWindow();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (_resultBorder.Visibility == Visibility.Visible && _formatCandidates.Count > 1 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    _selectedFormatIndex = (_selectedFormatIndex - 1 + _formatCandidates.Count) % _formatCandidates.Count;
                    UpdateResultDisplay();
                    e.Handled = true;
                    return;
                }

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
                if (_resultBorder.Visibility == Visibility.Visible && _formatCandidates.Count > 1 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    _selectedFormatIndex = (_selectedFormatIndex + 1) % _formatCandidates.Count;
                    UpdateResultDisplay();
                    e.Handled = true;
                    return;
                }

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

        private void ShowToast(string message)
        {
            if (_toastBorder == null || _toastTextBlock == null) return;

            _toastTextBlock.Text = message;
            _toastBorder.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = _toastBorder.Opacity,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            _toastBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            _toastTimer?.Stop();
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                var fadeOut = new DoubleAnimation
                {
                    From = _toastBorder.Opacity,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                fadeOut.Completed += (s2, e2) =>
                {
                    _toastBorder.Visibility = Visibility.Collapsed;
                };
                _toastBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };
            _toastTimer.Start();
        }

        private void ShowErrorFeedback(string message)
        {
            _resultTextBlock.Text = message;
            _resultTextBlock.Foreground = Brushes.Tomato;
            _equalsLabel.Foreground = Brushes.Tomato;

            ShowResultBorder();

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
            if (_isShowing) return;
            if (_isPinned) return; // Stay open when pinned!

            if (_settings.HideOnBlur && !_isSettingsWindowOpen)
            {
                HideWindow();
            }
        }

        public void OpenSettings()
        {
            if (_isSettingsWindowOpen) return;

            _isSettingsWindowOpen = true;
            var settingsWindow = new SettingsWindow(_settings, this, () =>
            {
                RegisterHotkey();
                ApplyLanguage();
                ApplyFontFamily();
                ApplyWindowLayout(_settings.WindowWidth, _settings.WindowScale, _settings.WindowOpacity);
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

        public void ApplyWindowLayout(double width, double scale, int opacityPercent, bool isInitializing = false)
        {
            _windowWidth = Math.Clamp(width, 420.0, 900.0);
            _windowScale = Math.Clamp(scale, 0.8, 1.6);
            _targetOpacity = Math.Clamp(opacityPercent / 100.0, 0.3, 1.0);

            double baseContentHeight = 56 * _windowScale;
            double marginHeight = _isWin11OrGreater ? 0 : 50;
            double borderPaddingHeight = 3;

            _heightCollapsed = baseContentHeight + marginHeight + borderPaddingHeight;
            _heightExpanded = (baseContentHeight * 2) + marginHeight + borderPaddingHeight;

            Width = _windowWidth;
            if (_resultBorder != null && _resultBorder.Visibility == Visibility.Visible)
            {
                Height = _heightExpanded;
            }
            else
            {
                Height = _heightCollapsed;
            }

            if (_inputGrid != null) _inputGrid.Height = baseContentHeight;
            if (_resultContentGrid != null) _resultContentGrid.Height = baseContentHeight;

            if (_inputTextBox != null) _inputTextBox.FontSize = 18 * _windowScale;
            if (_placeholderTextBlock != null) _placeholderTextBlock.FontSize = 18 * _windowScale;
            if (_resultTextBlock != null) _resultTextBlock.FontSize = 22 * _windowScale;
            if (_hintTextBlock != null) _hintTextBlock.FontSize = 11 * _windowScale;
            if (_formatTagTextBlock != null) _formatTagTextBlock.FontSize = 11 * _windowScale;
            if (_equalsLabel != null) _equalsLabel.FontSize = 22 * _windowScale;

            if (_calculatorIcon != null)
            {
                _calculatorIcon.Width = 22 * _windowScale;
                _calculatorIcon.Height = 22 * _windowScale;
            }

            if (!isInitializing && this.IsVisible && !_isShowing)
            {
                this.Opacity = _targetOpacity;
            }
        }

        public void ApplyTheme(bool isDark)
        {
            if (_shadowEffect != null && System.Windows.Application.Current?.Resources["ShadowOpacity"] is double shadowOpacity)
            {
                _shadowEffect.Opacity = shadowOpacity;
            }
        }
    }
}
