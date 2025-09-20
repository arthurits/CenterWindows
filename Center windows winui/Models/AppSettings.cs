using System.Text.Json.Serialization;
using Microsoft.UI.Xaml;
using Windows.Devices.Sms;

namespace CenterWindow.Models;
public partial class AppSettings
{
    /// <summary>
    /// Stores the settings file name
    /// </summary>
    [JsonIgnore]
    public string FileName { get; set; } = "appsettings.json";
    [JsonIgnore]
    public string AppDataFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CenterWindows", "ApplicationData");

    [JsonPropertyName("Select window default image path")]
    public string SelectWindowDefaultImagePath { get; set; } = "ms-appx:///Assets/Select window - 48x44 - Finder home.svg";
    [JsonPropertyName("Select window clicked image path")]
    public string SelectWindowClickedImagePath { get; set; } = "ms-appx:///Assets/Select window - 48x44 - Finder gone.svg";
    [JsonPropertyName("Select window cursor path")]
    public string SelectWindowCursorPath { get; set; } = "ms-appx:///Assets/Finder - 32x32.cur";


    /// <summary>
    /// Remember window position on start up
    /// </summary>
    [JsonPropertyName("Window position")]
    public bool WindowPosition { get; set; } = false;
    [JsonPropertyName("Window top")]
    public int WindowTop { get; set; } = 0;
    [JsonPropertyName("Window left")]
    public int WindowLeft { get; set; } = 0;
    [JsonPropertyName("Window width")]
    public int WindowWidth { get; set; } = 900;
    [JsonPropertyName("Window height")]
    public int WindowHeight { get; set; } = 760;

    /// <summary>
    /// App theme name
    /// </summary>
    [JsonPropertyName("App theme name")]
    public string ThemeName { get; set; } = ElementTheme.Default.ToString();

    /// <summary>
    /// Culture used throughout the app
    /// </summary>
    [JsonIgnore]
    public System.Globalization.CultureInfo AppCulture { get; set; } = System.Globalization.CultureInfo.CurrentCulture;
    /// <summary>
    /// Define the culture used throughout the app by asigning a culture string name
    /// </summary>
    [JsonPropertyName("Culture name")]
    public string AppCultureName
    {
        get => AppCulture.Name;
        set => AppCulture = new System.Globalization.CultureInfo(value);
    }

    /// <summary>
    /// True if open/save dialogs should remember the user's previous path
    /// </summary>
    [JsonPropertyName("Remember path in FileDialog")]
    public bool RememberFileDialogPath { get; set; } = true;
    /// <summary>
    /// Default path for saving files to disk
    /// </summary>
    [JsonPropertyName("Default save path")]
    public string DefaultSavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    /// <summary>
    /// User-defined path for saving files to disk
    /// </summary>
    [JsonPropertyName("User save path")]
    public string UserSavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    /// <summary>
    /// Default path for reading files from disk
    /// </summary>
    [JsonPropertyName("Default open path")]
    public string DefaultOpenPath { get; set; } = $"{System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)}\\examples";
    /// <summary>
    /// User-defined path for reading files from disk
    /// </summary>
    [JsonPropertyName("User open path")]
    public string UserOpenPath { get; set; } = $"{System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)}\\examples";

    /// <summary>
    /// Gets or sets a value indicating whether the application should display an icon in the system tray.
    /// </summary>
    [JsonPropertyName("Show icon in tray")]
    public bool ShowTrayIcon { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the application minimizes to the system tray instead of the system taskbar.
    /// </summary>
    [JsonPropertyName("Minimize to tray")]
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Launch the application at system startup.
    /// </summary>
    [JsonPropertyName("Launch app on startup")]
    public bool LaunchAtStartup { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the selected windows should be highlighted.
    /// </summary>
    [JsonPropertyName("Show window highlight")]
    public bool ShowHighlight { get; set; } = true;

    /// <summary>
    /// Gets or sets the color used to highlight the border, in ARGB format.
    /// </summary>
    [JsonPropertyName("Border highlight color")]
    public string BorderColor { get; set; } = "#FFFF0000"; // Default red color for the border highlight

    /// <summary>
    /// Gets or sets the width of the highlight border, in pixels.
    /// </summary>
    [JsonPropertyName("Border width")]
    public int BorderThickness{ get; set; } = 3;

    /// <summary>
    /// Gets or sets the corner radius of highlight border marquee, in pixels.
    /// </summary>
    [JsonPropertyName("Corner radius")]
    public int BorderRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets a value indicating whether child windows can be selected.
    /// </summary>
    /// [JsonPropertyName("Select child windows")]
    public bool SelectChildWindows { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the cursor should be changed during window selection.
    /// </summary>
    [JsonPropertyName("Change cursor")]
    public bool ChangeCursor { get; set; } = true;

    /// <summary>
    /// The application can only be minimized to the system tray if both <see cref="ShowTrayIcon"/> and <see cref="MinimizeToTray"/> are true.
    /// </summary>
    [JsonIgnore]
    public bool CanMinimizeToTray => ShowTrayIcon && MinimizeToTray;

    /// <summary>
    /// Gets or sets a value indicating whether the StackPanel for the cursor restoration should be shown in the SelectWindow page view.
    /// </summary>
    [JsonPropertyName("Restore cursor")]
    public bool RestoreCursor { get; set; } = true;

    /// <summary>
    /// Gets or sets the horizontal position, in pixels, of the StackPanel location showing the restore cursor option in the SelectWindow page view.
    /// </summary>
    [JsonPropertyName("Restore cursor left")]
    public double RestoreCursorLeft { get; set; } = -1;

    /// <summary>
    /// Gets or sets the vertical position, in pixels, of the StackPanel location showing the restore cursor option in the SelectWindow page view.
    /// </summary>
    [JsonPropertyName("Restore cursor top")]
    public double RestoreCursorTop { get; set; } = -1;

    /// <summary>
    /// Gets or sets the transparency level for the slider control in ListWindows page view.
    /// </summary>
    [JsonPropertyName("List windows transparency")]
    public int ListWindowsTransparency { get; set; } = 255;
    /// <summary>
    /// Gets or sets whether the center option is checked in the ListWindows page view.
    /// This property is used to determine if the center option should be enabled or disabled both in the CommandBar button and the Flyout menu.
    /// </summary>
    [JsonPropertyName("Center windows checked")]
    public bool IsCenterChecked { get; set; } = false;
    /// <summary>
    /// Gets or sets whether the alpha option is checked in the ListWindows page view.
    /// This property is used to determine if the alpha option should be enabled or disabled both in the CommandBar button and the Flyout menu.
    /// </summary>
    [JsonPropertyName("Alpha checked")]
    public bool IsAlphaChecked { get; set; } = false;
    /// <summary>
    /// Gets or sets the transparency level for the slider control in SelectWindow page view.
    /// </summary>
    [JsonPropertyName("Select window transparency")]
    public int SelectWindowTransparency { get; set; } = 255;

    [JsonIgnore]
    public string? AppPath { get; set; } = Path.GetDirectoryName(Environment.ProcessPath);
    [JsonIgnore]
    public string DocumentPath { get; set; } = string.Empty;
    [JsonIgnore]
    public string DocumentExtension { get; set; } = string.Empty;

    public AppSettings()
    {
    }


}