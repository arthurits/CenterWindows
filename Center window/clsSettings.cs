using System.Text.Json.Serialization;

namespace Center_window;

public class ClassSettings
{
    /// <summary>
    /// Stores the settings file name
    /// </summary>
    [JsonIgnore]
    public string FileName { get; set; } = "configuration.json";

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
        get { return AppCulture.Name; }
        set { AppCulture = new System.Globalization.CultureInfo(value); }
    }
    
    [JsonIgnore]
    public string? AppPath { get; set; } = Path.GetDirectoryName(Environment.ProcessPath);

    /// <summary>
    /// Remember window position on start up
    /// </summary>
    [JsonPropertyName("Window position")]
    public bool WindowPosition { get; set; } = true;    // Remember window position
    /// <summary>
    /// Window top-left x coordinate
    /// </summary>
    [JsonPropertyName("Window left")]
    public int WindowLeft { get; set; } = 0;
    /// <summary>
    /// Window top-left y coordinate
    /// </summary>
    [JsonPropertyName("Window top")]
    public int WindowTop { get; set; } = 0;
    /// <summary>
    /// Window width
    /// </summary>
    [JsonPropertyName("Window width")]
    public int WindowWidth { get; set; } = 828;
    /// <summary>
    /// Window height
    /// </summary>
    [JsonPropertyName("Window height")]
    public int WindowHeight { get; set; } = 782;

    /// <summary>
    /// Center window option
    /// </summary>
    [JsonPropertyName("Center window")]
    public bool CenterWindow { get; set; } = true;
    /// <summary>
    /// Transparency option
    /// </summary>
    [JsonPropertyName("Transparency")]
    public bool Transparency { get; set; } = false;
    /// <summary>
    /// Transparency value
    /// </summary>
    [JsonPropertyName("Transparency value")]
    public byte TransparencyValue { get; set; } = byte.MinValue;
    /// <summary>
    /// Parent window option
    /// </summary>
    [JsonPropertyName("Parent window")]
    public bool OnlyParentWnd { get; set; } = false;
    /// <summary>
    /// Rectangle color
    /// </summary>
    [JsonPropertyName("Rectangle color")]
    public int RectangleColor { get; set; }= Color.Red.ToArgb();
    /// <summary>
    /// Rectangle width (pixels)
    /// </summary>
    [JsonPropertyName("Rectangle width")]
    public int RectangleWidth { get; set; } = 3;

    public ClassSettings()
    {
    }

}