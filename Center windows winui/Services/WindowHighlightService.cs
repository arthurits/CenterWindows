using System.ComponentModel;
using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;

public partial class WindowHighlightService : IWindowHighlightService, IDisposable
{
    private const string OverlayClassName = "HighlightOverlay";
    private readonly IntPtr _hInst = Win32.GetModuleHandle(null!);
    private IntPtr _overlayHwnd = IntPtr.Zero;
    private IntPtr _borderBrush = IntPtr.Zero;
    private IntPtr _lastTargetHwnd = IntPtr.Zero;
    private int _lastWidth;
    private int _lastHeight;
    private bool _isDisposed;
    private readonly Win32.WndProc _wndProcDelegate;
    
    public WindowHighlightService()
    {
        _hInst = Win32.GetModuleHandle(null!);
        if (_hInst == IntPtr.Zero)
        {
            // Throw the error from GetLastError()
            throw new Win32Exception(Marshal.GetLastWin32Error(), "GetModuleHandle failed.");
        }
        _wndProcDelegate = OverlayWndProc;
        RegisterOverlayClass();
    }

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
    public void HighlightWindow(IntPtr targetHwnd, int cornerRadius = 0, uint borderColor = 0xFFFF0000, int thickness = 3)
    {
        if (targetHwnd == IntPtr.Zero || !Win32.GetWindowRect(targetHwnd, out var rect))
        {
            return;
        }
        
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        // If the overlay is already showing for the same window and dimensions,
        // or the overlay is the same as the target window, skip it
        if (_overlayHwnd != IntPtr.Zero
            && targetHwnd == _lastTargetHwnd
            && width == _lastWidth
            && height == _lastHeight)
        {
            return;
        }
        //Debug.WriteLine($"Highlighting window {targetHwnd} at {rect.Left}, {rect.Top} with size {width}x{height} - _overlayHwnd: {_overlayHwnd}");
        // Otherwise, update the last target and dimensions
        _lastTargetHwnd = targetHwnd;
        _lastWidth = width;
        _lastHeight = height;
        _isDisposed = false;    // Since we have a valid _overlayHwnd, we signal te possibility to dispose it later

        // Create the overlay window if it doesn't exist
        if (_overlayHwnd == IntPtr.Zero)
        {
            CreateOverlayWindow();
        }
        
        // Position the overlay window
        Win32.SetWindowPos( _overlayHwnd,
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
        _borderBrush = Win32.CreateSolidBrush(borderColor);

        // Create rounded rectangle regions for the border
        IntPtr rgnOuter = Win32.CreateRoundRectRgn(
            0, 0, width, height,
            cornerRadius * 2, cornerRadius * 2);

        int innerRadius = Math.Max(0, cornerRadius - thickness);
        IntPtr rgnInner = Win32.CreateRoundRectRgn(
            thickness, thickness,
            width - thickness, height - thickness,
            innerRadius * 2, innerRadius * 2);

        IntPtr rgnFrame = Win32.CreateRectRgn(0, 0, 0, 0);
        _ = Win32.CombineRgn(rgnFrame, rgnOuter, rgnInner, Win32.RGN_DIFF);

        // The region used is rgnFrame, so we can delete the inner and outer regions
        Win32.DeleteObject(rgnOuter);
        Win32.DeleteObject(rgnInner);

        // Apply the region to the overlay window
        _ = Win32.SetWindowRgn(_overlayHwnd, rgnFrame, true);

        // Force a repaint of the overlay window
        Win32.InvalidateRect(_overlayHwnd, IntPtr.Zero, true);
        Win32.UpdateWindow(_overlayHwnd);
    }

    /// <summary>
    /// Hides the highlight overlay windows without clearing it.
    /// <remarks>This should be called when the highlight is no
    /// longer needed but you may want to show it again later.</remarks>
    /// </summary>
    public void HideHighlight()
    {
        if (_overlayHwnd != IntPtr.Zero)
        {
            Win32.ShowWindow(_overlayHwnd, Win32.SW_HIDE);
        }
    }

    /// <summary>
    /// Release any active highlight overlay and resets the associated resources and state.
    /// </summary>
    /// <remarks>This method releases the highlight overlay window, as well as any allocated resources such as the
    /// border brush. It also resets the last target and dimensions to their default values. After calling this method,
    /// resources will need to be allocated by calling <see cref="HighlightWindow"/>.</remarks>
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

        // Reset the last target and dimensions
        _lastTargetHwnd = IntPtr.Zero;
        _lastWidth = _lastHeight = 0;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="WindowHighlightService"/>.
    /// Internally, it first calls <see cref="ClearHighlight"/>, which destroys the overlay window
    /// and the border brush, and afterwards the OverlayClass is unregistered and the instance is marked as disposed.
    /// </summary>
    /// <remarks>This method should be called when the <see cref="WindowHighlightService"/> is no longer
    /// needed to ensure that all unmanaged resources are properly released. After calling this method, the instance is
    /// considered disposed and reset, so that <see cref="HighlightWindow"/> can be used again.</remarks>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        // Clear the highlight if it exists
        ClearHighlight();

        // Unregister the overlay class
        var result = Win32.UnregisterClass(OverlayClassName, _hInst);
        if (!result)
        {
            var error = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine($"UnregisterClass failed: code {error}");
        }

        // Since we are disposing, tell the garbage collector not to call
        // the finalizer (destructor) of "this" object
        GC.SuppressFinalize(this);

        // Signal that the service is disposed
        _isDisposed = true;
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
            cbSize = Marshal.SizeOf<Win32.WNDCLASSEX>(),
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = _hInst,
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hbrBackground = IntPtr.Zero, // pintamos manualmente en WM_PAINT
            lpszMenuName = null,
            lpszClassName = OverlayClassName,
            hIconSm = IntPtr.Zero
        };

        var atom = Win32.RegisterClassEx(ref wc);
        if (atom == 0)
        {
            var err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err, $"RegisterClassEx failed (error {err}).");
        }
    }

    private void CreateOverlayWindow()
    {
        var exStyles = Win32.WS_EX_TRANSPARENT
                        | Win32.WS_EX_TOOLWINDOW    // needed to avoid showing it in ALT+TAB
                        | Win32.WS_EX_LAYERED;      // needed for transparency using color key

        exStyles = Win32.WS_EX_TRANSPARENT
                        | Win32.WS_EX_TOOLWINDOW;    // needed to avoid showing it in ALT+TAB

        _overlayHwnd = Win32.CreateWindowEx(
            exStyles,
            OverlayClassName,
            string.Empty,
            Win32.WS_POPUP,
            0, 0, 0, 0,
            IntPtr.Zero,
            IntPtr.Zero,
            _hInst,
            IntPtr.Zero);

        if (_overlayHwnd == IntPtr.Zero)
        {
            var err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err, $"CreateWindowEx failed (error {err}).");
        }

        //// Set the window to be layered for transparency
        //var ok = Win32.SetLayeredWindowAttributes(
        //    _overlayHwnd,
        //    0x000000,   // color key to be used for transparency
        //    0,          // alpha is ignored with LWA_COLORKEY
        //    Win32.LWA_COLORKEY);

        //if (!ok)
        //{
        //    var err = Marshal.GetLastWin32Error();
        //    throw new Win32Exception(err, $"SetLayeredWindowAttributes failed: {err}");
        //}
    }
    
    private IntPtr OverlayWndProc( IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case Win32.WM_PAINT:
                _ = Win32.BeginPaint(hwnd, out var ps);
                _ = Win32.FillRect(ps.hdc, ref ps.rcPaint, _borderBrush);
                
                //// Black background, which will be transparent
                //IntPtr hBlack = Win32.GetStockObject(Win32.BLACK_BRUSH);
                //_ = Win32.FillRect(ps.hdc, ref ps.rcPaint, hBlack);

                //// Draw the border using the brush
                ////    for widths > 1px repeat FrameRect 'thickness' times
                //_ = Win32.FrameRect(ps.hdc, ref ps.rcPaint, _borderBrush);

                Win32.EndPaint(hwnd, ref ps);
                return IntPtr.Zero;

            case Win32.WM_ERASEBKGND:
                return new IntPtr(1);

            case Win32.WM_NCHITTEST:
                return new IntPtr(Win32.HTTRANSPARENT);
        }
        return Win32.DefWindowProc(hwnd, msg, wParam, lParam);
    }
    #endregion
}