using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;
public class WindowCenterService : IWindowCenterService
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const uint LWA_ALPHA = 0x02;
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    /// <summary>
    /// Centers the specified window on the screen and optionally applies alpha transparency.
    /// </summary>
    /// <remarks>This method calculates the dimensions of the specified window and positions it at the center
    /// of the screen. The method does not perform any action if the window's dimensions cannot be retrieved.</remarks>
    /// <param name="hWnd">A handle to the window to be centered. This must be a valid window handle.</param>
    public void CenterWindow(IntPtr hWnd)
    {
        if (!NativeMethods.GetWindowRect(hWnd, out var rect))
        {
            return;
        }

        var width = rect.Right  - rect.Left;
        var height = rect.Bottom - rect.Top;

        var screenW = NativeMethods.GetSystemMetrics(SM_CXSCREEN);
        var screenH = NativeMethods.GetSystemMetrics(SM_CYSCREEN);

        var x = (screenW - width)  / 2;
        var y = (screenH - height) / 2;

        NativeMethods.MoveWindow(hWnd, x, y, width, height, true);

        // Enable layered style and apply alpha blending
        //NativeMethods.SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(WS_EX_LAYERED));
        //NativeMethods.SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
    }

    /// <summary>
    /// Sets the transparency level of a specified window.
    /// </summary>
    /// <remarks>This method enables the layered window style for the specified window if it is not already
    /// set. The transparency level is controlled by the <paramref name="alpha"/> parameter, which determines  the
    /// opacity of the window.</remarks>
    /// <param name="hWnd">A handle to the window whose transparency level is to be set.</param>
    /// <param name="alpha">The alpha transparency value to apply to the window. Valid values range from 0 (completely transparent)  to 255
    /// (completely opaque).</param>
    public void SetWindowTransparency(IntPtr hWnd, byte alpha)
    {
        // Enable layered style if not already set
        var exStyle = NativeMethods.GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt32();
        if ((exStyle & WS_EX_LAYERED) == 0)
        {
            NativeMethods.SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle | WS_EX_LAYERED));
        }
        // Set the alpha value for the window
        NativeMethods.SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
    }
}
