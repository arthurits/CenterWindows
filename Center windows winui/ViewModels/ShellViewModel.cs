using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace CenterWindow.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    // Services
    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial string StrAppDisplayName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrAppDisplayName_Base { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StrAppDisplayName_File { get; set; } = string.Empty;

    public readonly string StrTitleUnion;

    [ObservableProperty]
    public partial string StrAboutItem { get; set; } = "Acerca de";
    [ObservableProperty]
    public partial string StrAboutToolTip { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrListWindowsItem { get; set; } = "Ventanas";
    [ObservableProperty]
    public partial string StrListWindowsToolTip { get; set; } = "Ventanas";
    [ObservableProperty]
    public partial string StrSelectWindowItem { get; set; } = "Ventanas";
    [ObservableProperty]
    public partial string StrSelectWindowToolTip { get; set; } = "Ventanas";
    [ObservableProperty]
    public partial string StrSettingsItem { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrSettingsToolTip { get; set; } = string.Empty;

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, ILocalizationService localizationService)
    {
        // Retrieve the navigation service and navigation view service
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        // Set the title union character
        StrTitleUnion = "StrTitleUnion".GetLocalized("Shell");


        // Subscribe to localization service events
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public void Dispose()
    {
        NavigationService.Navigated -= OnNavigated;
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Update the display name and tooltips based on the current language
        StrAppDisplayName = "StrAppDisplayName".GetLocalized("Shell");
        //StrAppDisplayName_Base = "StrAppDisplayName".GetLocalized("Shell");
        StrAboutItem = "AboutItem".GetLocalized("Shell");
        StrAboutToolTip = "AboutToolTip".GetLocalized("Shell");
        StrListWindowsItem = "ListWindowsItem".GetLocalized("Shell");
        StrListWindowsToolTip = "ListWindowsToolTip".GetLocalized("Shell");
        StrSelectWindowItem = "SelectWindowItem".GetLocalized("Shell");
        StrSelectWindowToolTip = "SelectWindowToolTip".GetLocalized("Shell");
        StrSettingsItem = "SettingsItem".GetLocalized("Shell");
        StrSettingsToolTip = "SettingsToolTip".GetLocalized("Shell");

    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    partial void OnStrAppDisplayName_FileChanged(string oldValue, string newValue)
    {
        // Update the display name when StrAppDisplayName_File changes
        //StrAppDisplayName = WindowTitle.SetWindowTitle(StrAppDisplayName_Base, newValue, StrTitleUnion);
    }

    partial void OnStrAppDisplayName_BaseChanged(string oldValue, string newValue)
    {
        // Update the display name when StrAppDisplayName_Base changes
        //StrAppDisplayName = WindowTitle.SetWindowTitle(newValue, StrAppDisplayName_File, StrTitleUnion);
    }
}
