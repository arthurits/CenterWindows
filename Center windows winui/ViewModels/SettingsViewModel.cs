using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;
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
    private readonly ILocalSettingsService<AppSettings> _settingsService;
    private AppSettings _appSettings;

    public ObservableCollection<CultureOption> AvailableLanguages { get; set; } = [];

    [ObservableProperty]
    public partial int SelectedLanguageIndex { get; set; } = -1;

    [ObservableProperty]
    public partial int Theme { get; set; } = 0;
    public ObservableCollection<ComboBoxData> ColorModes { get; set; } = [];

    //[ObservableProperty]
    //private ElementTheme _elementTheme;

    //[ObservableProperty]
    //private string _versionDescription;

    [ObservableProperty]
    public partial bool WindowPosition { get; set; }
    [ObservableProperty]
    public partial int WindowTop { get; set; }
    
    [ObservableProperty]
    public partial int WindowLeft { get; set; }
    [ObservableProperty]
    public partial int WindowWidth { get; set; }
    [ObservableProperty]
    public partial int WindowHeight { get; set; }


    [ObservableProperty]
    public partial bool ShowInTray { get; set; }

    [ObservableProperty]
    public partial bool IsResetVisible { get; set; } = false;

    //public string WindowSizeDescription => string.Format(StrWindowSize, WindowWidth, WindowHeight);

    //public string WindowPositionDescription => string.Format(StrWindowPosition, WindowTop, WindowLeft);


    public ICommand SwitchThemeCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService<AppSettings> settings, ILocalizationService localizationService, ITrayIconService trayIconService)
    {
        // Tray icon service
        _trayIconService = trayIconService;

        // Settings service
        _settingsService = settings;
        _appSettings = settings.GetValues;

        // Get settings and update the observable properties
        WindowPosition = _appSettings.WindowPosition;

        // Theme service
        _themeSelectorService = themeSelectorService;
        Theme = (int)Enum.Parse<ElementTheme>(_appSettings.ThemeName);
        //_elementTheme = _themeSelectorService.Theme;
        //_versionDescription = GetVersionDescription();

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

        SelectedLanguageIndex = selectedCultureIndex;

        //SwitchThemeCommand = new RelayCommand<ElementTheme>(
        //    async (param) =>
        //    {
        //        if (ElementTheme != param)
        //        {
        //            ElementTheme = param;
        //            await _themeSelectorService.SetThemeAsync(param);
        //        }
        //    });

        // Populate the settings dictionary for synchronization
        _syncActions[nameof(WindowPosition)] = () => _appSettings.WindowPosition = WindowPosition;
        _syncActions[nameof(WindowTop)] = () => _appSettings.WindowTop = WindowTop;
        _syncActions[nameof(WindowLeft)] = () => _appSettings.WindowLeft = WindowLeft;
        _syncActions[nameof(WindowWidth)] = () => _appSettings.WindowWidth = WindowWidth;
        _syncActions[nameof(WindowHeight)] = () => _appSettings.WindowHeight = WindowHeight;

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

    partial void OnWindowLeftChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(WindowPositionDescription));
    }
    partial void OnWindowTopChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(WindowPositionDescription));
    }
    partial void OnWindowWidthChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(WindowSizeDescription);
    }
    partial void OnWindowHeightChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(WindowSizeDescription);
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

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    partial void OnShowInTrayChanged(bool value)
    {
        if (value)
        {
            _trayIconService.Initialize();
        }
        else
        {
            _trayIconService.Dispose();
        }

        //settingsService.ShowInTray = value;
    }
}
