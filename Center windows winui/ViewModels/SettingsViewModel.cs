using System.Collections.ObjectModel;
using System.ComponentModel;

using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System.UserProfile;

namespace CenterWindow.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    // Settings synchronization dictionary
    private readonly Dictionary<string, Action> _syncActions = [];

    // Services
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalizationService _localizationService;
    private readonly ITrayIconService _trayIconService;
    private readonly IMainWindowService _mainWindowService;
    private readonly ILocalSettingsService<AppSettings> _settingsService;
    private AppSettings _appSettings;

    public ObservableCollection<CultureOption> AvailableLanguages { get; set; } = [];

    [ObservableProperty]
    public partial int SelectedLanguageIndex { get; set; } = -1;

    [ObservableProperty]
    public partial int Theme { get; set; } = 0;
    public ObservableCollection<ComboBoxData> ColorModes { get; set; } = [];

    [ObservableProperty]
    public partial bool WindowPosition { get; set; }
    
    [ObservableProperty]
    public partial bool RememberFileDialogPath { get; set; }

    [ObservableProperty]
    public partial bool ShowTrayIcon { get; set; }
    [ObservableProperty]
    public partial bool MinimizeToTray { get; set; }

    [ObservableProperty]
    public partial bool IsMinimizeToTrayEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsResetVisible { get; set; } = false;

    public string WindowSizeDescription => string.Format(StrWindowSize, WindowWidth, WindowHeight);

    public string WindowPositionDescription => string.Format(StrWindowPosition, WindowTop, WindowLeft);

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService<AppSettings> settings,
        ILocalizationService localizationService,
        ITrayIconService trayIconService,
        IMainWindowService mainWindowService)
    {
        // Tray icon service
        _trayIconService = trayIconService;

        // Settings service
        _settingsService = settings;
        _appSettings = settings.GetValues;

        // Get settings and update the observable properties
        WindowPosition = _appSettings.WindowPosition;
        RememberFileDialogPath = _appSettings.RememberFileDialogPath;
        ShowTrayIcon = _appSettings.ShowTrayIcon;
        MinimizeToTray = _appSettings.MinimizeToTray;

        // Theme service
        _themeSelectorService = themeSelectorService;
        Theme = (int)Enum.Parse<ElementTheme>(_appSettings.ThemeName);
        //_elementTheme = _themeSelectorService.Theme;
        //_versionDescription = GetVersionDescription();

        // Main window service and susbcription to changed event
        _mainWindowService = mainWindowService;
        _mainWindowService.PropertyChanged += MainWindow_Changed;

        // Subscribe to localization service events
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;

        // Populate the available languages
        var cultures = _localizationService.GetAvailableLanguages();
        var cultureList = cultures.ToList();
        AvailableLanguages = new ObservableCollection<CultureOption>(cultureList);

        var language = Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
        var currentLang = !string.IsNullOrEmpty(language) ? language : GlobalizationPreferences.Languages[0];

        // Look for an exact match first, then check for prefixes.
        var selectedCultureIndex = cultureList.FindIndex(lang =>
                 lang.LanguageTag == currentLang ||
                 currentLang.StartsWith(lang.LanguageTag + "-") ||
                 lang.LanguageTag.StartsWith(currentLang + "-"));

        SelectedLanguageIndex = cultureList.Count > 0 ? Math.Max(0, selectedCultureIndex) : -1;
        //SelectedLanguageIndex = selectedCultureIndex;

        // Populate the settings dictionary for synchronization
        _syncActions[nameof(WindowPosition)] = () => _appSettings.WindowPosition = WindowPosition;
        _syncActions[nameof(WindowTop)] = () => _appSettings.WindowTop = WindowTop;
        _syncActions[nameof(WindowLeft)] = () => _appSettings.WindowLeft = WindowLeft;
        _syncActions[nameof(WindowWidth)] = () => _appSettings.WindowWidth = WindowWidth;
        _syncActions[nameof(WindowHeight)] = () => _appSettings.WindowHeight = WindowHeight;
        _syncActions[nameof(RememberFileDialogPath)] = () => _appSettings.RememberFileDialogPath = RememberFileDialogPath;
        _syncActions[nameof(ShowTrayIcon)] = () => _appSettings.ShowTrayIcon = ShowTrayIcon;
        _syncActions[nameof(MinimizeToTray)] = () => _appSettings.MinimizeToTray = MinimizeToTray;
    }

    private void MainWindow_Changed(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(_mainWindowService.WindowWidth) or nameof(_mainWindowService.WindowHeight))
        {
            OnPropertyChanged(nameof(WindowSizeDescription));
        }
        if (e.PropertyName is nameof(_mainWindowService.WindowLeft) or nameof(_mainWindowService.WindowTop))
        {
            OnPropertyChanged(nameof(WindowPositionDescription));
        }
    }


    public void Dispose()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    partial void OnSelectedLanguageIndexChanged(int oldValue, int newValue)
    {
        if (newValue >= 0 && newValue < AvailableLanguages.Count)
        {
            var selected = AvailableLanguages[newValue];
            _localizationService.SetAppLanguage(selected.LanguageTag);

            // Refresh list and maintain selection
            var updated = _localizationService.GetAvailableLanguages();
            AvailableLanguages?.Clear();
            foreach (var language in updated)
            {
                AvailableLanguages?.Add(language);
            }
            //AvailableLanguages = new ObservableCollection<CultureOption>(updated.ToList());
            //SelectedLanguageIndex = updated.ToList().FindIndex(c => c.LanguageTag == selected.LanguageTag);
            //SelectedLanguageIndex = newValue; // Re-assign to trigger property change
        }
    }

    private void ThemeSelectorChanged(string? themeName)
    {
        if (Enum.TryParse(themeName, out ElementTheme theme) is true)
        {
            _themeSelectorService.SetThemeAsync(theme);
        }
    }

    private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_appSettings.WindowLeft) || e.PropertyName == nameof(_appSettings.WindowTop))
        {
            OnPropertyChanged(nameof(WindowPositionDescription));
        }
        else if (e.PropertyName == nameof(_appSettings.WindowWidth) || e.PropertyName == nameof(_appSettings.WindowHeight))
        {
            OnPropertyChanged(nameof(WindowSizeDescription));
        }
        else
        {
            if (IsResetVisible == false)
            {
                IsResetVisible = true;
            }
        }
    }

    /// <summary>
    /// Override OnPropertyChanged to handle custom behaviour
    /// </summary>
    /// <param name="e"></param>
    /// <see href="https://stackoverflow.com/questions/71857854/how-to-call-a-method-after-a-property-update"/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        // Call the base function
        base.OnPropertyChanged(e);

        // If the property name starts with "Str" then it's a localization variable and we are not concerned with them
        if (!_syncActions.ContainsKey(e.PropertyName ?? string.Empty))
        {
            return;
        }

        // Update POCO: the AppSettings properties based on the ViewModel properties
        if (_syncActions.TryGetValue(e.PropertyName!, out var action))
        {
            action();
        }

        // Set the reset button visibility
        if (e.PropertyName != nameof(WindowLeft) &&
            e.PropertyName != nameof(WindowTop) &&
            e.PropertyName != nameof(WindowWidth) &&
            e.PropertyName != nameof(WindowHeight))
        {
            if (IsResetVisible == false)
            {
                IsResetVisible = true;
            }
        }

    }

    partial void OnThemeChanged(int value)
    {
        // Update the theme in the settings
        ThemeSelectorChanged(((ElementTheme)Theme).ToString());
        _appSettings.ThemeName = ((ElementTheme)Theme).ToString();
    }

    partial void OnShowTrayIconChanged(bool value)
    {
        if (value)
        {
            _trayIconService.Initialize();
            IsMinimizeToTrayEnabled = true;
        }
        else
        {
            _trayIconService.Dispose();
            IsMinimizeToTrayEnabled = false;
        }
    }

    [RelayCommand]
    private async Task ResetSettings()
    {
        var result = await MessageBox.Show(
            messageBoxText: "MsgBoxResetSettingsContent".GetLocalized("MessageBox"),
            caption: "MsgBoxResetSettingsTitle".GetLocalized("MessageBox"),
            primaryButtonText: "MsgBoxResetSettingsPrimary".GetLocalized("MessageBox"),
            closeButtonText: "MsgBoxResetSettingsClose".GetLocalized("MessageBox"),
            defaultButton: MessageBox.MessageBoxButtonDefault.CloseButton,
            icon: MessageBox.MessageBoxImage.Question);

        if (result == ContentDialogResult.Primary)
        {
            _appSettings = new AppSettings();
            Theme = (int)Enum.Parse<ElementTheme>(_appSettings.ThemeName);

            // Hide reset button until a setting has changed
            IsResetVisible = false;
        }
    }
}
