using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Disambiguate types between WPF and WinForms/System.Drawing namespaces
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace CalculatorInAir
{
    public class SettingsWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly MainWindow? _mainWindow;
        private readonly Action _onSaveCallback;

        private Button _recordButton = null!;
        private ComboBox _precisionComboBox = null!;
        private ComboBox _languageComboBox = null!;
        private CheckBox _hideOnBlurCheckBox = null!;
        private CheckBox _copyOnEnterCheckBox = null!;
        private ComboBox _themeComboBox = null!;

        // Opacity Controls
        private Slider _opacitySlider = null!;
        private TextBlock _opacityValueText = null!;
        private readonly List<Button> _opacityPillButtons = new List<Button>();

        // Size & Font Scaling Controls
        private Slider _widthSlider = null!;
        private TextBlock _widthValueText = null!;
        private Slider _scaleSlider = null!;
        private TextBlock _scaleValueText = null!;
        private readonly List<Button> _sizePillButtons = new List<Button>();

        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private TextBlock _headerTitle = null!;
        private List<TextBlock> _labels = new List<TextBlock>();
        private bool _isDarkTheme = true;
        
        // Original states for cancel/revert
        private string _originalThemeSetting = "Auto";
        private int _originalOpacitySetting = 100;
        private double _originalWidthSetting = 600.0;
        private double _originalScaleSetting = 1.0;

        private bool _isInitializing = true;
        private bool _isSaved = false;

        // Current temporary values during editing
        private int _currentOpacity = 100;
        private double _currentWidth = 600.0;
        private double _currentScale = 1.0;

        // Recording state
        private bool _isRecording = false;
        private bool _recordedCtrl = false;
        private bool _recordedAlt = false;
        private bool _recordedShift = false;
        private bool _recordedWin = false;
        private int _recordedVk = 0;
        private string _recordedDisplay = "";

        public SettingsWindow(AppSettings settings, Action onSaveCallback)
            : this(settings, null, onSaveCallback)
        {
        }

        public SettingsWindow(AppSettings settings, MainWindow? mainWindow, Action onSaveCallback)
        {
            _settings = settings;
            _mainWindow = mainWindow;
            _onSaveCallback = onSaveCallback;

            _originalThemeSetting = settings.Theme;
            _originalOpacitySetting = settings.WindowOpacity;
            _originalWidthSetting = settings.WindowWidth;
            _originalScaleSetting = settings.WindowScale;

            _currentOpacity = settings.WindowOpacity;
            _currentWidth = settings.WindowWidth;
            _currentScale = settings.WindowScale;

            // Determine active theme
            bool isDark = true;
            if (settings.Theme == "Light")
                isDark = false;
            else if (settings.Theme == "Dark")
                isDark = true;
            else
                isDark = App.ThemeDetector.IsSystemDarkTheme();
            _isDarkTheme = isDark;

            // Setup temporary recording states with current values
            _recordedCtrl = settings.Ctrl;
            _recordedAlt = settings.Alt;
            _recordedShift = settings.Shift;
            _recordedWin = settings.Win;
            _recordedVk = settings.VirtualKey;
            _recordedDisplay = settings.HotkeyDisplay;

            InitializeUI();

            _isInitializing = false;
        }

        private void InitializeUI()
        {
            Title = Loc.Get("SettingsTitle");
            Width = 500;
            Height = 650;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Arial");

            // Setup DynamicResource brushes
            this.SetResourceReference(Window.BackgroundProperty, "SettingsBackgroundBrush");
            this.SetResourceReference(Window.ForegroundProperty, "SettingsForegroundBrush");

            // Main layout
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(55) }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) }); // Actions

            // 1. Header
            var headerPanel = new StackPanel { Margin = new Thickness(20, 15, 20, 0) };
            _headerTitle = new TextBlock
            {
                Text = Loc.Get("SettingsTitle").Split(" - ")[0],
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            _headerTitle.SetResourceReference(TextBlock.ForegroundProperty, "SettingsHeaderBrush");
            headerPanel.Children.Add(_headerTitle);
            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);

            // 2. Content with ScrollViewer for clean scrolling
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(20, 5, 20, 5)
            };

            var contentStack = new StackPanel();

            // 2.1 Hotkey Row
            contentStack.Children.Add(CreateSettingRow(Loc.Get("GlobalShortcut"), CreateHotkeyControl()));

            // 2.2 Precision Row
            contentStack.Children.Add(CreateSettingRow(Loc.Get("Precision"), CreatePrecisionControl()));

            // 2.3 Language Row
            contentStack.Children.Add(CreateSettingRow(Loc.Get("LanguageSetting"), CreateLanguageControl()));

            // 2.4 Theme Row
            contentStack.Children.Add(CreateSettingRow(Loc.Get("ThemeSetting"), CreateThemeControl()));

            // 2.5 Opacity Row (Scheme A - Preset Pills + Fluid Slider + Translucent Preview)
            contentStack.Children.Add(CreateSettingRow(Loc.Get("WindowOpacitySetting"), CreateOpacityControl(), isTopAligned: true));

            // 2.6 Window Size & Font Scaling Row
            contentStack.Children.Add(CreateSettingRow(Loc.Get("WindowSizeSetting"), CreateSizeAndScaleControl(), isTopAligned: true));

            // 2.7 Behaviors Row
            contentStack.Children.Add(CreateSettingRow(Loc.Get("Behavior"), CreateBehaviorControl(), isTopAligned: true));

            scrollViewer.Content = contentStack;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // 3. Actions Panel
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 20, 15)
            };

            _saveButton = new Button
            {
                Content = Loc.Get("Save"),
                Width = 90,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Style = (Style)FindResource("AccentButtonStyle")
            };
            _saveButton.Click += (s, e) => SaveSettings();
            actionsPanel.Children.Add(_saveButton);

            _cancelButton = new Button
            {
                Content = Loc.Get("Cancel"),
                Width = 90,
                Height = 32,
                Style = (Style)FindResource("StandardButtonStyle")
            };
            _cancelButton.Click += (s, e) => RevertAndClose();
            actionsPanel.Children.Add(_cancelButton);

            Grid.SetRow(actionsPanel, 2);
            mainGrid.Children.Add(actionsPanel);

            Content = mainGrid;

            // Wire key events to the whole window for hotkey recording
            PreviewKeyDown += SettingsWindow_KeyDown;
        }

        private Grid CreateSettingRow(string labelText, FrameworkElement controlElement, bool isTopAligned = false)
        {
            var grid = new Grid { Margin = new Thickness(0, 6, 0, 6) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = CreateLabel(labelText);
            if (isTopAligned)
            {
                label.VerticalAlignment = VerticalAlignment.Top;
                label.Margin = new Thickness(0, 6, 10, 0);
            }
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            Grid.SetColumn(controlElement, 1);
            grid.Children.Add(controlElement);

            return grid;
        }

        private TextBlock CreateLabel(string text)
        {
            var lbl = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 10, 0)
            };
            lbl.SetResourceReference(TextBlock.ForegroundProperty, "SettingsLabelForegroundBrush");
            _labels.Add(lbl);
            return lbl;
        }

        #region Control Creators

        private FrameworkElement CreateHotkeyControl()
        {
            _recordButton = new Button
            {
                Content = _recordedDisplay,
                Height = 30,
                FontWeight = FontWeights.SemiBold,
                Style = (Style)FindResource("StandardButtonStyle")
            };
            _recordButton.Click += (s, e) => StartRecording();
            return _recordButton;
        }

        private FrameworkElement CreatePrecisionControl()
        {
            _precisionComboBox = new ComboBox { Height = 30 };
            _precisionComboBox.SetResourceReference(ComboBox.BackgroundProperty, "ComboBoxBackgroundBrush");
            _precisionComboBox.SetResourceReference(ComboBox.ForegroundProperty, "ComboBoxForegroundBrush");
            _precisionComboBox.SetResourceReference(ComboBox.BorderBrushProperty, "ComboBoxBorderBrush");

            _precisionComboBox.Items.Add(Loc.Get("PrecisionAuto"));
            for (int i = 0; i <= 10; i++)
            {
                _precisionComboBox.Items.Add(i.ToString());
            }
            if (_settings.DecimalPlaces < 0)
                _precisionComboBox.SelectedIndex = 0;
            else
                _precisionComboBox.SelectedIndex = _settings.DecimalPlaces + 1;

            return _precisionComboBox;
        }

        private FrameworkElement CreateLanguageControl()
        {
            _languageComboBox = new ComboBox { Height = 30 };
            _languageComboBox.SetResourceReference(ComboBox.BackgroundProperty, "ComboBoxBackgroundBrush");
            _languageComboBox.SetResourceReference(ComboBox.ForegroundProperty, "ComboBoxForegroundBrush");
            _languageComboBox.SetResourceReference(ComboBox.BorderBrushProperty, "ComboBoxBorderBrush");

            _languageComboBox.Items.Add(Loc.Get("LanguageAuto"));
            _languageComboBox.Items.Add("简体中文");
            _languageComboBox.Items.Add("English (UK)");

            if (_settings.LanguagePreference == "zh_CN")
                _languageComboBox.SelectedIndex = 1;
            else if (_settings.LanguagePreference == "en_GB")
                _languageComboBox.SelectedIndex = 2;
            else
                _languageComboBox.SelectedIndex = 0;

            return _languageComboBox;
        }

        private FrameworkElement CreateThemeControl()
        {
            _themeComboBox = new ComboBox { Height = 30 };
            _themeComboBox.SetResourceReference(ComboBox.BackgroundProperty, "ComboBoxBackgroundBrush");
            _themeComboBox.SetResourceReference(ComboBox.ForegroundProperty, "ComboBoxForegroundBrush");
            _themeComboBox.SetResourceReference(ComboBox.BorderBrushProperty, "ComboBoxBorderBrush");

            _themeComboBox.Items.Add(Loc.Get("ThemeAuto"));
            _themeComboBox.Items.Add(Loc.Get("ThemeDark"));
            _themeComboBox.Items.Add(Loc.Get("ThemeLight"));

            if (_settings.Theme == "Dark")
                _themeComboBox.SelectedIndex = 1;
            else if (_settings.Theme == "Light")
                _themeComboBox.SelectedIndex = 2;
            else
                _themeComboBox.SelectedIndex = 0;

            _themeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
            return _themeComboBox;
        }

        private FrameworkElement CreateOpacityControl()
        {
            var panel = new StackPanel();

            // 1. Preset Pills Panel
            var pillsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var presets = new (string text, int val)[]
            {
                (Loc.Get("PresetOpaque"), 100),
                (Loc.Get("PresetRecommended"), 85),
                (Loc.Get("PresetLight"), 70),
                (Loc.Get("PresetTransparent"), 50)
            };

            foreach (var (text, val) in presets)
            {
                var pill = CreatePillButton($"{text} ({val}%)", val == _currentOpacity);
                int targetVal = val;
                pill.Click += (s, e) =>
                {
                    _currentOpacity = targetVal;
                    _opacitySlider.Value = targetVal;
                    UpdateOpacityPillsHighlight();
                    ApplyLivePreview();
                };
                _opacityPillButtons.Add(pill);
                pillsPanel.Children.Add(pill);
            }
            panel.Children.Add(pillsPanel);

            // 2. Slider & Percentage Display
            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            _opacitySlider = new Slider
            {
                Minimum = 30,
                Maximum = 100,
                Value = _currentOpacity,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                VerticalAlignment = VerticalAlignment.Center
            };

            _opacityValueText = new TextBlock
            {
                Text = $"{_currentOpacity}%",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(5, 0, 0, 0)
            };
            _opacityValueText.SetResourceReference(TextBlock.ForegroundProperty, "SettingsForegroundBrush");

            // Live preview & Translucent settings window while dragging
            _opacitySlider.ValueChanged += (s, e) =>
            {
                if (_isInitializing) return;
                _currentOpacity = (int)e.NewValue;
                _opacityValueText.Text = $"{_currentOpacity}%";
                UpdateOpacityPillsHighlight();
                ApplyLivePreview();
            };

            // Enable Obstruction-Free Preview during dragging
            _opacitySlider.PreviewMouseDown += (s, e) => { this.Opacity = 0.35; };
            _opacitySlider.PreviewMouseUp += (s, e) => { this.Opacity = 1.0; };
            _opacitySlider.LostFocus += (s, e) => { this.Opacity = 1.0; };

            Grid.SetColumn(_opacitySlider, 0);
            Grid.SetColumn(_opacityValueText, 1);
            sliderGrid.Children.Add(_opacitySlider);
            sliderGrid.Children.Add(_opacityValueText);

            panel.Children.Add(sliderGrid);
            return panel;
        }

        private FrameworkElement CreateSizeAndScaleControl()
        {
            var panel = new StackPanel();

            // 1. Preset Size Pills Panel
            var pillsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var sizePresets = new (string key, double w, double scale)[]
            {
                ("SizeCompact", 480, 0.9),
                ("SizeStandard", 600, 1.0),
                ("SizeWide", 750, 1.2),
                ("SizeLarge", 900, 1.4)
            };

            foreach (var (key, w, scale) in sizePresets)
            {
                bool isSel = Math.Abs(_currentWidth - w) < 5 && Math.Abs(_currentScale - scale) < 0.05;
                var pill = CreatePillButton(Loc.Get(key), isSel);
                double targetW = w;
                double targetScale = scale;

                pill.Click += (s, e) =>
                {
                    _currentWidth = targetW;
                    _currentScale = targetScale;
                    _widthSlider.Value = targetW;
                    _scaleSlider.Value = targetScale * 100;
                    UpdateSizePillsHighlight();
                    ApplyLivePreview();
                };
                _sizePillButtons.Add(pill);
                pillsPanel.Children.Add(pill);
            }
            panel.Children.Add(pillsPanel);

            // 2. Width Slider
            var widthGrid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            widthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            widthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            widthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });

            var widthLabel = new TextBlock
            {
                Text = Loc.Get("WidthSetting"),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            widthLabel.SetResourceReference(TextBlock.ForegroundProperty, "SettingsLabelForegroundBrush");

            _widthSlider = new Slider
            {
                Minimum = 420,
                Maximum = 900,
                Value = _currentWidth,
                TickFrequency = 10,
                IsSnapToTickEnabled = true,
                VerticalAlignment = VerticalAlignment.Center
            };

            _widthValueText = new TextBlock
            {
                Text = $"{(int)_currentWidth} px",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            _widthValueText.SetResourceReference(TextBlock.ForegroundProperty, "SettingsForegroundBrush");

            _widthSlider.ValueChanged += (s, e) =>
            {
                if (_isInitializing) return;
                _currentWidth = e.NewValue;
                _widthValueText.Text = $"{(int)_currentWidth} px";
                UpdateSizePillsHighlight();
                ApplyLivePreview();
            };

            _widthSlider.PreviewMouseDown += (s, e) => { this.Opacity = 0.35; };
            _widthSlider.PreviewMouseUp += (s, e) => { this.Opacity = 1.0; };

            Grid.SetColumn(widthLabel, 0);
            Grid.SetColumn(_widthSlider, 1);
            Grid.SetColumn(_widthValueText, 2);
            widthGrid.Children.Add(widthLabel);
            widthGrid.Children.Add(_widthSlider);
            widthGrid.Children.Add(_widthValueText);
            panel.Children.Add(widthGrid);

            // 3. Scale Slider
            var scaleGrid = new Grid();
            scaleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            scaleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            scaleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });

            var scaleLabel = new TextBlock
            {
                Text = Loc.Get("ScaleSetting"),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            scaleLabel.SetResourceReference(TextBlock.ForegroundProperty, "SettingsLabelForegroundBrush");

            _scaleSlider = new Slider
            {
                Minimum = 80,
                Maximum = 160,
                Value = _currentScale * 100,
                TickFrequency = 5,
                IsSnapToTickEnabled = true,
                VerticalAlignment = VerticalAlignment.Center
            };

            _scaleValueText = new TextBlock
            {
                Text = $"{(int)(_currentScale * 100)} %",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            _scaleValueText.SetResourceReference(TextBlock.ForegroundProperty, "SettingsForegroundBrush");

            _scaleSlider.ValueChanged += (s, e) =>
            {
                if (_isInitializing) return;
                _currentScale = e.NewValue / 100.0;
                _scaleValueText.Text = $"{(int)e.NewValue} %";
                UpdateSizePillsHighlight();
                ApplyLivePreview();
            };

            _scaleSlider.PreviewMouseDown += (s, e) => { this.Opacity = 0.35; };
            _scaleSlider.PreviewMouseUp += (s, e) => { this.Opacity = 1.0; };

            Grid.SetColumn(scaleLabel, 0);
            Grid.SetColumn(_scaleSlider, 1);
            Grid.SetColumn(_scaleValueText, 2);
            scaleGrid.Children.Add(scaleLabel);
            scaleGrid.Children.Add(_scaleSlider);
            scaleGrid.Children.Add(_scaleValueText);
            panel.Children.Add(scaleGrid);

            return panel;
        }

        private FrameworkElement CreateBehaviorControl()
        {
            var behaviorPanel = new StackPanel();

            _hideOnBlurCheckBox = new CheckBox
            {
                Content = Loc.Get("HideOnBlur"),
                IsChecked = _settings.HideOnBlur,
                Margin = new Thickness(0, 2, 0, 6)
            };
            _hideOnBlurCheckBox.SetResourceReference(CheckBox.ForegroundProperty, "SettingsForegroundBrush");

            _copyOnEnterCheckBox = new CheckBox
            {
                Content = Loc.Get("CopyOnEnter"),
                IsChecked = _settings.CopyOnEnter,
                Margin = new Thickness(0, 2, 0, 2)
            };
            _copyOnEnterCheckBox.SetResourceReference(CheckBox.ForegroundProperty, "SettingsForegroundBrush");

            behaviorPanel.Children.Add(_hideOnBlurCheckBox);
            behaviorPanel.Children.Add(_copyOnEnterCheckBox);
            return behaviorPanel;
        }

        private Button CreatePillButton(string text, bool isSelected)
        {
            var btn = new Button
            {
                Content = text,
                Height = 26,
                Padding = new Thickness(8, 0, 8, 0),
                Margin = new Thickness(0, 0, 6, 0),
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Cursor = Cursors.Hand,
                Style = (Style)FindResource(isSelected ? "AccentButtonStyle" : "StandardButtonStyle")
            };
            return btn;
        }

        private void UpdateOpacityPillsHighlight()
        {
            int[] vals = { 100, 85, 70, 50 };
            for (int i = 0; i < _opacityPillButtons.Count && i < vals.Length; i++)
            {
                bool isMatch = _currentOpacity == vals[i];
                _opacityPillButtons[i].Style = (Style)FindResource(isMatch ? "AccentButtonStyle" : "StandardButtonStyle");
            }
        }

        private void UpdateSizePillsHighlight()
        {
            var presets = new (double w, double scale)[]
            {
                (480, 0.9), (600, 1.0), (750, 1.2), (900, 1.4)
            };

            for (int i = 0; i < _sizePillButtons.Count && i < presets.Length; i++)
            {
                bool isMatch = Math.Abs(_currentWidth - presets[i].w) < 5 && Math.Abs(_currentScale - presets[i].scale) < 0.05;
                _sizePillButtons[i].Style = (Style)FindResource(isMatch ? "AccentButtonStyle" : "StandardButtonStyle");
            }
        }

        private void ApplyLivePreview()
        {
            _mainWindow?.ApplyWindowLayout(_currentWidth, _currentScale, _currentOpacity);
        }

        #endregion

        private void StartRecording()
        {
            _isRecording = true;
            _recordButton.Content = Loc.Get("RecordingPrompt");
            _recordButton.Background = new SolidColorBrush(Color.FromRgb(124, 76, 237));
            _recordButton.Foreground = Brushes.White;
        }

        private void SettingsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isRecording) return;

            e.Handled = true;

            // Esc cancels recording
            if (e.Key == Key.Escape)
            {
                _isRecording = false;
                UpdateRecordButtonText();
                return;
            }

            var modifiers = Keyboard.Modifiers;
            bool ctrl = (modifiers & ModifierKeys.Control) != 0;
            bool alt = (modifiers & ModifierKeys.Alt) != 0;
            bool shift = (modifiers & ModifierKeys.Shift) != 0;
            bool win = (modifiers & ModifierKeys.Windows) != 0;

            Key key = e.Key;
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // If it is just a modifier key, update button text and wait
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                var tempParts = new List<string>();
                if (ctrl || key == Key.LeftCtrl || key == Key.RightCtrl) tempParts.Add("Ctrl");
                if (alt || key == Key.LeftAlt || key == Key.RightAlt) tempParts.Add("Alt");
                if (shift || key == Key.LeftShift || key == Key.RightShift) tempParts.Add("Shift");
                if (win || key == Key.LWin || key == Key.RWin) tempParts.Add("Win");

                if (tempParts.Count > 0)
                    _recordButton.Content = string.Join(" + ", tempParts) + " + ...";
                else
                    _recordButton.Content = Loc.Get("RecordingPrompt");

                return;
            }

            // Real key pressed! Record combination
            _recordedCtrl = ctrl;
            _recordedAlt = alt;
            _recordedShift = shift;
            _recordedWin = win;
            _recordedVk = KeyInterop.VirtualKeyFromKey(key);

            var displayParts = new List<string>();
            if (_recordedCtrl) displayParts.Add("Ctrl");
            if (_recordedAlt) displayParts.Add("Alt");
            if (_recordedShift) displayParts.Add("Shift");
            if (_recordedWin) displayParts.Add("Win");
            displayParts.Add(GetKeyFriendlyName(key));

            _recordedDisplay = string.Join(" + ", displayParts);
            _isRecording = false;
            UpdateRecordButtonText();
        }

        private void UpdateRecordButtonText()
        {
            _recordButton.Content = _recordedDisplay;
            _recordButton.ClearValue(Button.BackgroundProperty);
            _recordButton.ClearValue(Button.ForegroundProperty);
        }

        private string GetKeyFriendlyName(Key key)
        {
            switch (key)
            {
                case Key.Space: return "Space";
                case Key.Return: return "Enter";
                case Key.Tab: return "Tab";
                case Key.Back: return "Backspace";
                case Key.Escape: return "Esc";
                default: return key.ToString();
            }
        }

        private void SaveSettings()
        {
            if (_isRecording) return; // Wait until recording stops

            // Validate that hotkey contains at least one modifier key
            if (!_recordedCtrl && !_recordedAlt && !_recordedShift && !_recordedWin)
            {
                System.Windows.MessageBox.Show(
                    Loc.Get("HotkeyWarningText"),
                    Loc.Get("HotkeyWarningTitle"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            // 1. Update hotkey config
            _settings.Ctrl = _recordedCtrl;
            _settings.Alt = _recordedAlt;
            _settings.Shift = _recordedShift;
            _settings.Win = _recordedWin;
            _settings.VirtualKey = _recordedVk;
            _settings.HotkeyDisplay = _recordedDisplay;

            // 2. Update precision
            int selectedPrecIndex = _precisionComboBox.SelectedIndex;
            if (selectedPrecIndex <= 0)
                _settings.DecimalPlaces = -1;
            else
                _settings.DecimalPlaces = selectedPrecIndex - 1;

            // 3. Update language
            int selectedLangIndex = _languageComboBox.SelectedIndex;
            if (selectedLangIndex == 1)
                _settings.LanguagePreference = "zh_CN";
            else if (selectedLangIndex == 2)
                _settings.LanguagePreference = "en_GB";
            else
                _settings.LanguagePreference = "Auto";

            // 4. Update Theme Setting
            string selectedTheme = "Auto";
            if (_themeComboBox.SelectedIndex == 1)
                selectedTheme = "Dark";
            else if (_themeComboBox.SelectedIndex == 2)
                selectedTheme = "Light";
            _settings.Theme = selectedTheme;

            // 5. Update Opacity, Width & Scale
            _settings.WindowOpacity = _currentOpacity;
            _settings.WindowWidth = _currentWidth;
            _settings.WindowScale = _currentScale;

            // 6. Update behavior checkboxes
            _settings.HideOnBlur = _hideOnBlurCheckBox.IsChecked ?? true;
            _settings.CopyOnEnter = _copyOnEnterCheckBox.IsChecked ?? true;

            // Save settings via manager
            SettingsManager.Save(_settings);

            _isSaved = true;

            // Execute callback to reload settings and re-register hotkeys in the app
            _onSaveCallback?.Invoke();

            Close();
        }

        private void RevertAndClose()
        {
            _settings.Theme = _originalThemeSetting;
            _settings.WindowOpacity = _originalOpacitySetting;
            _settings.WindowWidth = _originalWidthSetting;
            _settings.WindowScale = _originalScaleSetting;

            _mainWindow?.ApplyWindowLayout(_originalWidthSetting, _originalScaleSetting, _originalOpacitySetting);
            (System.Windows.Application.Current as App)?.ApplyTheme();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_isSaved)
            {
                RevertAndClose();
            }
            base.OnClosed(e);
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string selectedTheme = "Auto";
            if (_themeComboBox.SelectedIndex == 1)
                selectedTheme = "Dark";
            else if (_themeComboBox.SelectedIndex == 2)
                selectedTheme = "Light";

            _settings.Theme = selectedTheme;
            (System.Windows.Application.Current as App)?.ApplyTheme();
        }
    }
}
