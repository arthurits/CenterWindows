using System.ComponentModel;

namespace CenterWindow.Contracts.Services;

/// <summary>
/// Defines the contract for the main window service.
/// This service is responsible for managing the main window's title and dimensions.
/// </summary>
public interface IMainWindowService : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// The full-text title of the main window.
    /// </summary>
    string Title { get; }
    /// <summary>
    /// The main title of the application, typically left-hand side of the window title.
    /// </summary>
    string TitleMain { get; set; }
    /// <summary>
    /// The file name or additional text to be displayed in the window title, typically right-hand side.
    /// </summary>
    string TitleFile { get; set; }
    /// <summary>
    /// The union string that separates the main title and file name in the window title.
    /// </summary>
    string TitleUnion { get; set; }

    int WindowLeft { get; }
    int WindowTop { get; }
    int WindowWidth { get;}
    int WindowHeight { get; }

    WindowState WindowState { get; }

    /// <summary>
    /// Computes the window text based on the main title, file name, and union string.
    /// </summary>
    /// <returns>The window text</returns>
    string WindowText();

    /// <summary>
    /// Hide the window from desktop and task bar
    /// </summary>
    void Hide();

    /// <summary>
    /// Show the window, restore its position and place it on top
    /// </summary>
    void Show();
}
