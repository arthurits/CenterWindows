namespace CenterWindow.Contracts.Services;
public interface IWindowHighlightService : IDisposable
{
    /// <summary>
    /// Highlights a specified window by overlaying a customizable border around it.
    /// </summary>
    /// <remarks>This method creates an overlay window to visually highlight the specified target window.  If
    /// the target window's dimensions or handle have not changed since the last call, the method  skips re-creating the
    /// overlay to optimize performance. The overlay is always displayed as a  topmost window and does not interfere
    /// with the target window's functionality.  The method ensures that the overlay respects the specified corner
    /// radius, border color, and  thickness. If the target window is invalid or has zero dimensions, the method exits
    /// without  performing any action.</remarks>
    /// <param name="targetHwnd">The handle of the window to highlight.</param>
    /// <param name="cornerRadius">The radius of the corners of the border, in pixels. A value of 0 creates square corners.</param>
    /// <param name="borderColor">The color of the border, specified as a 32-bit ARGB value. The default is opaque red (0xFFFF0000).</param>
    /// <param name="thickness">The thickness of the border, in pixels. The default is 3.</param>
    void HighlightWindow(
        IntPtr hWnd,
        int cornerRadius = 0,
        uint borderColor = 0xFFFF0000,
        int thickness = 3);

    /// <summary>
    /// Release any active highlight overlay and resets the associated resources and state.
    /// </summary>
    /// <remarks>This method releases the highlight overlay window, as well as any allocated resources such as the
    /// border brush. It also resets the last target and dimensions to their default values. After calling this method,
    /// resources will need to be allocated by calling <see cref="HighlightWindow"/>.</remarks>
    void ClearHighlight();

    /// <summary>
    /// Hides the highlight overlay windows without clearing it.
    /// <remarks>This should be called when the highlight is no
    /// longer needed but you may want to show it again later.</remarks>
    /// </summary>
    void HideHighlight();
}
