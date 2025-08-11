using CenterWindow.Helpers;
using CenterWindow.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class SettingsViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string StrAppSettings { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrThemeHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrThemeDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrThemeOptions { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowSize { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowPosition { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowPositionHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowPositionDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrFilepathHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrFilepathDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrLanguageHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrLanguageDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrShowTrayIconHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrShowTrayIconDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrMinimizeToTrayHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrMinimizeToTrayDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrLaunchAtStartupHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrLaunchAtStartupDescription { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string StrWindowSelection { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrShowHighlightHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrShowHighlightDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrBorderColorHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrBorderColorDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrBorderThicknessHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrBorderThicknessDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrBorderRadiusHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrBorderRadiusDescription { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrSelectChildHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrSelectChildDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrResetButton { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrActivate { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrDeactivate { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrChangeCursorHeader { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrChangeCursorDescription { get; set; } = string.Empty;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Subtitles / section headers
        StrAppSettings = "StrAppSettings".GetLocalized("Settings");

        // Theme settings card
        StrThemeHeader = "StrThemeHeader".GetLocalized("Settings");
        StrThemeDescription = "StrThemeDescription".GetLocalized("Settings");
        StrThemeOptions = "StrThemeOptions".GetLocalized("Settings");

        // Populate the theme options in the ComboBox
        var theme = Theme;  // Store the current theme to re-assign after populating the ComboBox
        var comboItems = new List<ComboBoxData>();
        var themeNames = StrThemeOptions.Split(',');
        for (var i = 0; i < themeNames.Length; i++)
        {
            comboItems.Add(new ComboBoxData { DisplayName = themeNames[i], Value = i });
        }
        ColorModes?.Clear();
        foreach (var item in comboItems)
        {
            ColorModes?.Add(item);
        }
        Theme = theme; // Re-assign to trigger property change

        // Window size and position settings card
        StrWindowPosition = "StrWindowPosition".GetLocalized("Settings");
        StrWindowPositionHeader = "StrWindowPositionHeader".GetLocalized("Settings");
        StrWindowPositionDescription = "StrWindowPositionDescription".GetLocalized("Settings");
        StrWindowSize = "StrWindowSize".GetLocalized("Settings");

        // Filepath settings card
        StrFilepathHeader = "StrFilepathHeader".GetLocalized("Settings");
        StrFilepathDescription = "StrFilepathDescription".GetLocalized("Settings");

        // Language settings card
        StrLanguageHeader = "StrLanguageHeader".GetLocalized("Settings");
        StrLanguageDescription = "StrLanguageDescription".GetLocalized("Settings");

        // Tray icon
        StrShowTrayIconHeader = "StrShowTrayIconHeader".GetLocalized("Settings");
        StrShowTrayIconDescription = "StrShowTrayIconDescription".GetLocalized("Settings");

        // Minimize to tray
        StrMinimizeToTrayHeader = "StrMinimizeToTrayHeader".GetLocalized("Settings");
        StrMinimizeToTrayDescription = "StrMinimizeToTrayDescription".GetLocalized("Settings");

        // Launch at startup
        StrLaunchAtStartupHeader = "StrLaunchAtStartupHeader".GetLocalized("Settings");
        StrLaunchAtStartupDescription = "StrLaunchAtStartupDescription".GetLocalized("Settings");

        // Window selection section
        StrWindowSelection = "StrWindowSelection".GetLocalized("Settings");

        // Highlight settings card
        StrShowHighlightHeader = "StrShowHighlightHeader".GetLocalized("Settings");
        StrShowHighlightDescription = "StrShowHighlightDescription".GetLocalized("Settings");
        StrBorderColorHeader = "StrBorderColorHeader".GetLocalized("Settings");
        StrBorderColorDescription = "StrBorderColorDescription".GetLocalized("Settings");
        StrBorderThicknessHeader = "StrBorderThicknessHeader".GetLocalized("Settings");
        StrBorderThicknessDescription = "StrBorderThicknessDescription".GetLocalized("Settings");
        StrBorderRadiusHeader = "StrBorderRadiusHeader".GetLocalized("Settings");
        StrBorderRadiusDescription = "StrBorderRadiusDescription".GetLocalized("Settings");

        // SelectChildWindows
        StrSelectChildHeader = "StrSelectChildHeader".GetLocalized("Settings");
        StrSelectChildDescription = "StrSelectChildDescription".GetLocalized("Settings");

        // Change cursor
        StrChangeCursorHeader = "StrChangeCursorHeader".GetLocalized("Settings");
        StrChangeCursorDescription = "StrChangeCursorDescription".GetLocalized("Settings");

        // Reset button
        StrResetButton = "StrResetButton".GetLocalized("Settings");

        // Activate/Deactivate buttons
        StrActivate = "StrActivate".GetLocalized("Settings");
        StrDeactivate = "StrDeactivate".GetLocalized("Settings");
    }
}
