namespace CenterWindow.Contracts.Services;

/// <summary>
/// Service for managing the system tray icon.
/// </summary>
public interface ITrayIconService
{
    /// <summary>
    /// Initializes and shows the tray icon (if enabled).
    /// </summary>
    void Initialize();

    /// <summary>
    /// Hides and releases resources for the icon.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Shows the context menu for the icon.
    /// </summary>
    void ShowContextMenu();
}

