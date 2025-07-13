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

    public void CenterWindow(IntPtr hWnd, byte alpha)
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
        NativeMethods.SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(WS_EX_LAYERED));
        NativeMethods.SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
    }
}
