namespace CenterWindow.Contracts.Services;
public interface IWindowHighlightService : IDisposable
{
    /// <summary>
    /// Highlight the specified window with a border.
    /// </summary>
    /// <param name="hWnd">Window handle of the window to be highlighted.</param>
    /// <param name="cornerRadius">Corner radius in pixels.</param>
    /// <param name="borderColor">Border color in 0xAARRGGBB format.</param>
    /// <param name="thickness">Border width in pixels.</param>
    void HighlightWindow(
        IntPtr hWnd,
        int cornerRadius = 0,
        uint borderColor = 0xFFFF0000,
        int thickness = 3);

    /// <summary>
    /// Remove the highlight from the currently highlighted window.
    /// </summary>
    void ClearHighlight();

    void HideHighlight();
}
