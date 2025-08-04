using System.Diagnostics;
using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;
public partial class WindowHighlightService : IWindowHighlightService, IDisposable
{
    private const string OverlayClassName = "HighlightOverlay";
    private IntPtr _overlayHwnd = IntPtr.Zero;
    private IntPtr _borderBrush = IntPtr.Zero;
    private IntPtr _lastTargetHwnd = IntPtr.Zero;
    private int _lastWidth;
    private int _lastHeight;
    private bool _isDisposed;

    private readonly Win32.WndProc _wndProcDelegate;

    public WindowHighlightService()
    {
        _wndProcDelegate = OverlayWndProc;
        RegisterOverlayClass();
    }

    public void HighlightWindow(IntPtr targetHwnd, int cornerRadius = 0, uint borderColor = 0xFFFF0000, int thickness = 3)
    {
        if (!Win32.GetWindowRect(targetHwnd, out var rect))
        {
            return;
        }

        int width = rect.Right  - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        // If the overlay is already showing for the same window and dimensions, skip
        if (_overlayHwnd != IntPtr.Zero
            && targetHwnd == _lastTargetHwnd
            && width == _lastWidth
            && height == _lastHeight)
        {
            return;
        }

        // Otherwise, update the last target and dimensions
        _lastTargetHwnd = targetHwnd;
        _lastWidth = width;
        _lastHeight = height;

        // Create the overlay window if it doesn't exist
        if (_overlayHwnd == IntPtr.Zero)
        {
            CreateOverlayWindow();
        }

        // Position the overlay window
        Win32.SetWindowPos(
            _overlayHwnd,
            Win32.HWND_TOPMOST,
            rect.Left,
            rect.Top,
            width,
            height,
            Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
        
        Win32.ShowWindow(_overlayHwnd, Win32.SW_SHOWNA);

        // Create a new border brush if needed
        if (_borderBrush != IntPtr.Zero)
        {
            Win32.DeleteObject(_borderBrush);
        }
        _borderBrush = Win32.CreateSolidBrush((int)borderColor);

        // Create rounded rectangle regions for the border
        IntPtr rgnOuter = Win32.CreateRoundRectRgn(
            0, 0, width, height,
            cornerRadius * 2,
            cornerRadius * 2);

        int innerRadius = Math.Max(0, cornerRadius - thickness);
        IntPtr rgnInner = Win32.CreateRoundRectRgn(
            thickness, thickness,
            width - thickness, height - thickness,
            innerRadius * 2,
            innerRadius * 2);

        IntPtr rgnFrame = Win32.CreateRectRgn(0, 0, 0, 0);
        _ = Win32.CombineRgn(rgnFrame, rgnOuter, rgnInner, Win32.RGN_DIFF);

        // The region used is rgnFrame, so we can delete the inner and outer regions
        Win32.DeleteObject(rgnOuter);
        Win32.DeleteObject(rgnInner);

        // Apply the region to the overlay window
        _ = Win32.SetWindowRgn(_overlayHwnd, rgnFrame, true);

        // Force a repaint of the overlay window
        Win32.InvalidateRect(_overlayHwnd, IntPtr.Zero, true);
    }

    public void ClearHighlight()
    {
        if (_overlayHwnd != IntPtr.Zero)
        {
            //Win32.DestroyWindow(_overlayHwnd);
            //_overlayHwnd = IntPtr.Zero;
            Win32.ShowWindow(_overlayHwnd, Win32.SW_HIDE);
        }

        if (_borderBrush != IntPtr.Zero)
        {
            Win32.DeleteObject(_borderBrush);
            _borderBrush = IntPtr.Zero;
        }

        // Reset the last target and dimensions
        _lastTargetHwnd = IntPtr.Zero;
        _lastWidth = _lastHeight = 0;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        ClearHighlight();
        
        Win32.DestroyWindow(_overlayHwnd);
        _overlayHwnd = IntPtr.Zero;
        _isDisposed = true;
        
        GC.SuppressFinalize(this);
    }

    ~WindowHighlightService()
    {
        Dispose();
    }

    #region Window & Class Setup

    private void RegisterOverlayClass()
    {
        var wc = new Win32.WNDCLASSEX
        {
            cbSize        = Marshal.SizeOf<Win32.WNDCLASSEX>(),
            style         = 0,
            lpfnWndProc   = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            cbClsExtra    = 0,
            cbWndExtra    = 0,
            hInstance     = Win32.GetModuleHandle(null),
            hIcon         = IntPtr.Zero,
            hCursor       = IntPtr.Zero,
            hbrBackground = IntPtr.Zero, // pintamos manualmente en WM_PAINT
            lpszMenuName  = null,
            lpszClassName = OverlayClassName,
            hIconSm       = IntPtr.Zero
        };

        Win32.RegisterClassEx(ref wc);
    }

    private void CreateOverlayWindow()
    {
        _overlayHwnd = Win32.CreateWindowEx(
            Win32.WS_EX_LAYERED
          | Win32.WS_EX_TRANSPARENT
          | Win32.WS_EX_TOOLWINDOW,   // no aparece en el ALT+TAB
            OverlayClassName,
            string.Empty,
            Win32.WS_POPUP,
            0, 0, 0, 0,
            IntPtr.Zero,
            IntPtr.Zero,
            Win32.GetModuleHandle(null),
            IntPtr.Zero);

        // Hacer que la ventana sea click‐through
        Win32.SetLayeredWindowAttributes(
            _overlayHwnd,
            0,
            255,
            Win32.LWA_ALPHA);
    }

    private IntPtr OverlayWndProc(
        IntPtr hwnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam)
    {
        switch (msg)
        {
            case Win32.WM_PAINT:
                _ = Win32.BeginPaint(hwnd, out var ps);
                _ = Win32.FillRect(ps.hdc, ref ps.rcPaint, _borderBrush);
                Win32.EndPaint(hwnd, ref ps);
                return IntPtr.Zero;

            case Win32.WM_ERASEBKGND:
                return new IntPtr(1);
        }

        return Win32.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    #endregion
}
