using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;
public class WindowHighlightService : IWindowHighlightService, IDisposable
{
    private const string OverlayClassName = "HighlightOverlay";
    private IntPtr _overlayHwnd = IntPtr.Zero;
    private IntPtr _borderBrush = IntPtr.Zero;
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

        int w = rect.Right  - rect.Left;
        int h = rect.Bottom - rect.Top;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        if (_overlayHwnd == IntPtr.Zero)
        {
            CreateOverlayWindow();
        }

        // 1) Posicionar y mostrar
        Win32.SetWindowPos(
            _overlayHwnd,
            Win32.HWND_TOPMOST,
            rect.Left,
            rect.Top,
            w,
            h,
            Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);

        // 2) Crear y asignar nueva brocha (elimina la anterior si existe)
        if (_borderBrush != IntPtr.Zero)
        {
            Win32.DeleteObject(_borderBrush);
        }
        _borderBrush = Win32.CreateSolidBrush((int)borderColor);

        // 3) Construir regiones
        IntPtr rgnOuter = Win32.CreateRoundRectRgn(
            0, 0, w, h,
            cornerRadius * 2,
            cornerRadius * 2);

        int innerRadius = Math.Max(0, cornerRadius - thickness);
        IntPtr rgnInner = Win32.CreateRoundRectRgn(
            thickness, thickness,
            w - thickness, h - thickness,
            innerRadius * 2,
            innerRadius * 2);

        IntPtr rgnFrame = Win32.CreateRectRgn(0, 0, 0, 0);
        _ = Win32.CombineRgn(rgnFrame, rgnOuter, rgnInner, Win32.RGN_DIFF);

        // El sistema toma control de rgnFrame; liberamos las otras
        Win32.DeleteObject(rgnOuter);
        Win32.DeleteObject(rgnInner);

        // 4) Aplicar la región al overlay
        _ = Win32.SetWindowRgn(_overlayHwnd, rgnFrame, true);

        // 5) Forzar repintado
        Win32.InvalidateRect(_overlayHwnd, IntPtr.Zero, true);
    }

    public void ClearHighlight()
    {
        if (_overlayHwnd != IntPtr.Zero)
        {
            Win32.DestroyWindow(_overlayHwnd);
            _overlayHwnd = IntPtr.Zero;
        }

        if (_borderBrush != IntPtr.Zero)
        {
            Win32.DeleteObject(_borderBrush);
            _borderBrush = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        ClearHighlight();
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
