using System.Reflection;
using System.Windows.Input;

using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;

namespace CenterWindow.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly Dictionary<string, Action> _syncActions = [];

    // Services
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalizationService _localizationService;
    private readonly ITrayIconService _trayIconService;
    private AppSettings _appSettings;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    
    [ObservableProperty]
    public partial bool ShowInTray { get; set; }

    public ICommand SwitchThemeCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ITrayIconService trayIconService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        _trayIconService = trayIconService;
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
