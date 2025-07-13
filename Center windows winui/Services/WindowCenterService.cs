using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;

namespace CenterWindow.Services;
public class WindowCenterService : IWindowCenterService
{
    // Ya tienes definido RECT en el Core; lo reutilizamos.
    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtrW")]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    const int GWL_EXSTYLE = -20;
    const int WS_EX_LAYERED = 0x00080000;
    const uint LWA_ALPHA = 0x02;
    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;

    public void CenterWindow(IntPtr hWnd, byte alpha)
    {
        if (!GetWindowRect(hWnd, out var rect)) return;

        int width = rect.Right  - rect.Left;
        int height = rect.Bottom - rect.Top;

        int screenW = GetSystemMetrics(SM_CXSCREEN);
        int screenH = GetSystemMetrics(SM_CYSCREEN);

        int x = (screenW - width)  / 2;
        int y = (screenH - height) / 2;

        MoveWindow(hWnd, x, y, width, height, true);

        // Habilitar estilo Layered y aplicar alpha
        SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(WS_EX_LAYERED));
        SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
    }
}
