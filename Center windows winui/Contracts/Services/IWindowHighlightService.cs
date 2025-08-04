namespace CenterWindow.Contracts.Services;
public interface IWindowHighlightService : IDisposable
{
    /// <summary>
    /// Highlight the specified window with a border.
    /// </summary>
    /// <param name="hWnd">Window handle of the window to be highlighted.</param>
    /// <param name="cornerRadius">Corner radius in pixels.</param>
    /// <param name="borderColor">Border color.</param>
    /// <param name="thickness">Border width in pixels.</param>
    void HighlightWindow(
        IntPtr hWnd,
        int cornerRadius = 0,
        Color? borderColor = null,
        int thickness = 3);

    /// <summary>
    /// Remove the highlight from the currently highlighted window.
    /// </summary>
    void ClearHighlight();
}
