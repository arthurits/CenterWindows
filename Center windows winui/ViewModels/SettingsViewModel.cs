using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.System.UserProfile;

using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;

namespace CenterWindow.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, IDisposable
{
    // Services
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalizationService _localizationService;
    private readonly ITrayIconService _trayIconService;
    private readonly IMainWindowService _mainWindowService;
    private readonly ILocalSettingsService<AppSettings> _settingsService;
    private AppSettings _appSettings;
    private readonly HashSet<string> _pocoSettings;

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
    public partial bool LaunchAtStartup { get; set; }
    [ObservableProperty]
    public partial bool IsLaunchAtStartupEnabled { get; set; }

    [ObservableProperty]
    public partial bool ShowHighlight { get; set; } = true;

    [ObservableProperty]
    public partial string BorderColor { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Windows.UI.Color BorderColorUI { get; set; } = Colors.Red;

    [ObservableProperty]
    public partial int BorderThickness { get; set; } = 0;

    [ObservableProperty]
    public partial int BorderRadius { get; set; } = 0;

    [ObservableProperty]
    public partial bool SelectChildWindows { get; set; } = false;

    [ObservableProperty]
    public partial bool IsResetVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool ChangeCursor { get; set; } = true;

    public string WindowSizeDescription => string.Format(StrWindowSize, _mainWindowService.WindowWidth, _mainWindowService.WindowHeight);

    public string WindowPositionDescription => string.Format(StrWindowPosition, _mainWindowService.WindowTop, _mainWindowService.WindowLeft);

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

        // Retrieve the properties from the AppSettings POCO
        _pocoSettings = typeof(AppSettings)
            .GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p => p.Name)
            .ToHashSet();

        // Initialize the ViewModel properties with the POCO values
        foreach (var propName in _pocoSettings)
        {
            var vmProp = GetType().GetProperty(propName);
            var pocoProp = _appSettings.GetType().GetProperty(propName);

            if (vmProp is null || pocoProp is null)
            {
                continue; // Skip if the property does not exist in either the ViewModel or the POCO
            }
            vmProp!.SetValue(this, pocoProp!.GetValue(_appSettings));
        }
    }

    public void Dispose()
    {
        _mainWindowService.PropertyChanged   -= MainWindow_Changed;
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _trayIconService.Dispose();
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

    /// <summary>
    /// Override OnPropertyChanged to handle custom behaviour
    /// </summary>
    /// <param name="e"></param>
    /// <see href="https://stackoverflow.com/questions/71857854/how-to-call-a-method-after-a-property-update"/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        // Call the base function
        base.OnPropertyChanged(e);

        // If the AppSettings POCO is not initialized or the property name is null, do nothing
        if (_pocoSettings is null || e.PropertyName is null)
        {
            return;
        }

        // The BorderColorUI property is a special case, as it is a Windows.UI.Color type
        // and needs to be converted to a string for the AppSettings POCO
        if (e.PropertyName == nameof(BorderColorUI))
        {   
            BorderColor = BorderColorUI.ToString() ?? string.Empty;
            return;
        }

        // We are only interested in properties that are part of the AppSettings POCO
        if (!_pocoSettings.Contains(e.PropertyName!))
        {
            return;
        }

        // Get the property value from the ViewModel
        var vmProp = GetType().GetProperty(e.PropertyName!);
        var newValue = vmProp?.GetValue(this);

        // Copy the value to the AppSettings POCO
        var pocoProp = _appSettings.GetType().GetProperty(e.PropertyName!);
        pocoProp?.SetValue(_appSettings, newValue);

        // Notify the settings service that a setting has changed
        _settingsService.NotifySettingChanged(e.PropertyName!, newValue);

        // Set the reset button visibility
        if (IsResetVisible == false)
        {
            IsResetVisible = true;
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
        IsMinimizeToTrayEnabled = value;
        IsLaunchAtStartupEnabled = value;

        if (value)
        {
            _trayIconService.Initialize();
        }
        else
        {
            _trayIconService.Dispose();
        }
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        IsLaunchAtStartupEnabled = value;
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
