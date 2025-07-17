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

    /// <summary>
    /// Occurs when a menu item in the tray menu is clicked.
    /// </summary>
    /// <remarks>This event is triggered whenever a user selects a menu item from the tray menu. Use the <see
    /// cref="TrayMenuItemEventArgs"/> parameter to access details about the clicked menu item, such as its identifier
    /// or associated data.</remarks>
    event EventHandler<TrayMenuItemEventArgs> TrayMenuItemClicked;
}

public class TrayMenuItemEventArgs : EventArgs
{
    public int ItemId { get; }
    public TrayMenuItemEventArgs(int id)
    {
        ItemId = id;
    }
}
