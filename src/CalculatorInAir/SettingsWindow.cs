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
using Cursors = System.Windows.Input.Cursors;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace CalculatorInAir
{
    public class SettingsWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly Action _onSaveCallback;

        private Button _recordButton = null!;
        private ComboBox _precisionComboBox = null!;
        private ComboBox _languageComboBox = null!;
        private CheckBox _hideOnBlurCheckBox = null!;
        private CheckBox _copyOnEnterCheckBox = null!;

        // Recording state
        private bool _isRecording = false;
        private bool _recordedCtrl = false;
        private bool _recordedAlt = false;
        private bool _recordedShift = false;
        private bool _recordedWin = false;
        private int _recordedVk = 0;
        private string _recordedDisplay = "";

        public SettingsWindow(AppSettings settings, Action onSaveCallback)
        {
            _settings = settings;
            _onSaveCallback = onSaveCallback;

            // Setup temporary recording states with current values
            _recordedCtrl = settings.Ctrl;
            _recordedAlt = settings.Alt;
            _recordedShift = settings.Shift;
            _recordedWin = settings.Win;
            _recordedVk = settings.VirtualKey;
            _recordedDisplay = settings.HotkeyDisplay;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Title = Loc.Get("SettingsTitle");
            Width = 460;
            Height = 440;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(24, 24, 28)); // Dark background
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Arial");

            // Main layout
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(65) }); // Actions

            // 1. Header
            var headerPanel = new StackPanel { Margin = new Thickness(20, 15, 20, 0) };
            var headerTitle = new TextBlock
            {
                Text = Loc.Get("SettingsTitle").Split(" - ")[0],
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new LinearGradientBrush(Color.FromRgb(167, 139, 250), Color.FromRgb(103, 232, 249), 0)
            };
            headerPanel.Children.Add(headerTitle);
            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);

            // 2. Content
            var contentGrid = new Grid { Margin = new Thickness(20, 5, 20, 5) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Hotkey
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Precision
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Language
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Behaviors

            // 2.1 Hotkey Row
            var hotkeyLabel = CreateLabel(Loc.Get("GlobalShortcut"));
            Grid.SetRow(hotkeyLabel, 0);
            Grid.SetColumn(hotkeyLabel, 0);
            contentGrid.Children.Add(hotkeyLabel);

            _recordButton = new Button
            {
                Content = _recordedDisplay,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 48)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 72)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };
            _recordButton.Click += (s, e) => StartRecording();
            SetupButtonHover(_recordButton, Color.FromRgb(40, 40, 48), Color.FromRgb(55, 55, 68));
            Grid.SetRow(_recordButton, 0);
            Grid.SetColumn(_recordButton, 1);
            contentGrid.Children.Add(_recordButton);

            // 2.2 Precision Row
            var precisionLabel = CreateLabel(Loc.Get("Precision"));
            Grid.SetRow(precisionLabel, 1);
            Grid.SetColumn(precisionLabel, 0);
            contentGrid.Children.Add(precisionLabel);

            _precisionComboBox = new ComboBox
            {
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 48)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 72)),
                BorderThickness = new Thickness(1)
            };
            _precisionComboBox.Items.Add(Loc.Get("PrecisionAuto"));
            for (int i = 0; i <= 10; i++)
            {
                _precisionComboBox.Items.Add(i.ToString());
            }
            // Set active index
            if (_settings.DecimalPlaces < 0)
                _precisionComboBox.SelectedIndex = 0;
            else
                _precisionComboBox.SelectedIndex = _settings.DecimalPlaces + 1;

            Grid.SetRow(_precisionComboBox, 1);
            Grid.SetColumn(_precisionComboBox, 1);
            contentGrid.Children.Add(_precisionComboBox);

            // 2.3 Language Row
            var languageLabel = CreateLabel(Loc.Get("LanguageSetting"));
            Grid.SetRow(languageLabel, 2);
            Grid.SetColumn(languageLabel, 0);
            contentGrid.Children.Add(languageLabel);

            _languageComboBox = new ComboBox
            {
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 48)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 72)),
                BorderThickness = new Thickness(1)
            };
            _languageComboBox.Items.Add(Loc.Get("LanguageAuto"));
            _languageComboBox.Items.Add("简体中文");
            _languageComboBox.Items.Add("English (UK)");

            if (_settings.LanguagePreference == "zh_CN")
                _languageComboBox.SelectedIndex = 1;
            else if (_settings.LanguagePreference == "en_GB")
                _languageComboBox.SelectedIndex = 2;
            else
                _languageComboBox.SelectedIndex = 0;

            Grid.SetRow(_languageComboBox, 2);
            Grid.SetColumn(_languageComboBox, 1);
            contentGrid.Children.Add(_languageComboBox);

            // 2.4 Behaviors Row
            var behaviorLabel = CreateLabel(Loc.Get("Behavior"));
            behaviorLabel.Margin = new Thickness(0, 10, 0, 5);
            Grid.SetRow(behaviorLabel, 3);
            Grid.SetColumn(behaviorLabel, 0);
            contentGrid.Children.Add(behaviorLabel);

            var behaviorPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            
            _hideOnBlurCheckBox = new CheckBox
            {
                Content = Loc.Get("HideOnBlur"),
                Foreground = Brushes.White,
                IsChecked = _settings.HideOnBlur,
                Margin = new Thickness(0, 5, 0, 8)
            };
            
            _copyOnEnterCheckBox = new CheckBox
            {
                Content = Loc.Get("CopyOnEnter"),
                Foreground = Brushes.White,
                IsChecked = _settings.CopyOnEnter,
                Margin = new Thickness(0, 5, 0, 5)
            };

            behaviorPanel.Children.Add(_hideOnBlurCheckBox);
            behaviorPanel.Children.Add(_copyOnEnterCheckBox);
            Grid.SetRow(behaviorPanel, 3);
            Grid.SetColumn(behaviorPanel, 1);
            contentGrid.Children.Add(behaviorPanel);

            Grid.SetRow(contentGrid, 1);
            mainGrid.Children.Add(contentGrid);

            // 3. Actions Panel
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 20, 0)
            };

            var saveButton = new Button
            {
                Content = Loc.Get("Save"),
                Width = 90,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(139, 92, 246)), // Purple accent
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.Bold
            };
            saveButton.Click += (s, e) => SaveSettings();
            SetupButtonHover(saveButton, Color.FromRgb(139, 92, 246), Color.FromRgb(124, 76, 237));
            actionsPanel.Children.Add(saveButton);

            var cancelButton = new Button
            {
                Content = Loc.Get("Cancel"),
                Width = 90,
                Height = 32,
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 48)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 72)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (s, e) => Close();
            SetupButtonHover(cancelButton, Color.FromRgb(40, 40, 48), Color.FromRgb(55, 55, 68));
            actionsPanel.Children.Add(cancelButton);

            Grid.SetRow(actionsPanel, 2);
            mainGrid.Children.Add(actionsPanel);

            Content = mainGrid;

            // Wire key events to the whole window for hotkey recording
            PreviewKeyDown += SettingsWindow_KeyDown;
        }

        private TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 185)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 10, 0)
            };
        }

        private void SetupButtonHover(Button btn, Color normalColor, Color hoverColor)
        {
            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(hoverColor);
            btn.MouseLeave += (s, e) => btn.Background = new SolidColorBrush(normalColor);
        }

        private void StartRecording()
        {
            _isRecording = true;
            _recordButton.Content = Loc.Get("RecordingPrompt");
            _recordButton.Background = new SolidColorBrush(Color.FromRgb(124, 76, 237));
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
            _recordButton.Background = new SolidColorBrush(Color.FromRgb(40, 40, 48));
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

            // 4. Update behavior checkboxes
            _settings.HideOnBlur = _hideOnBlurCheckBox.IsChecked ?? true;
            _settings.CopyOnEnter = _copyOnEnterCheckBox.IsChecked ?? true;

            // Save settings via manager
            SettingsManager.Save(_settings);

            // Execute callback to reload settings and re-register hotkeys in the app
            _onSaveCallback?.Invoke();

            Close();
        }
    }
}
