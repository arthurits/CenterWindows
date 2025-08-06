using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using Microsoft.Win32;

namespace CenterWindow.Services;

public partial class MouseHookService : IMouseHookService, IDisposable
{
    private readonly Win32.LowLevelMouseProc _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private IntPtr _originalCursor = IntPtr.Zero;
    private IntPtr _hCrossCopy = IntPtr.Zero;
    private TaskCompletionSource<IntPtr>? _taskCS;
    private CancellationTokenRegistration? _ctr;
    private CancellationToken _token;

    private string _cursorPath = string.Empty;
    private bool _changeCursor;
    private bool _onlyParentWnd;
    private bool _isDisposed;

    public event EventHandler<MouseMoveEventArgs>? MouseMoved;
    protected virtual void OnMouseMoved(MouseMoveEventArgs e) => MouseMoved?.Invoke(this, e);

    public MouseHookService()
    {
        // Create the hook callback delegate
        _proc = HookCallback;
    }

    public bool CaptureMouse(string cursorPath, bool changeCursor = false, bool onlyParentWnd = false, CancellationToken cancellationToken = default)
    {
        _cursorPath = cursorPath;
        _changeCursor = changeCursor;
        _onlyParentWnd = onlyParentWnd;
        return CaptureWindowUnderCursorAsync(cancellationToken);
    }

    public void ReleaseMouse()
    {
        Cleanup();
    }

    private bool CaptureWindowUnderCursorAsync(CancellationToken cancellationToken = default)
    {
        bool result = true;

        if (_hookId != IntPtr.Zero)
        {
            throw new InvalidOperationException("Already active hook");
        }

        _taskCS = new TaskCompletionSource<IntPtr>();
        _token = cancellationToken;

        if (_changeCursor)
        {
            // Get the original cursor to restore later
            //_originalCursor = NativeMethods.CopyIcon(NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW));
            var path = CursorLoader.GetArrowCursorPath();
            _originalCursor = CursorLoader.LoadArrowCursorFromFile(path);

            // Switch cursor to crosshair
            //using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            IntPtr hCross;
            if (_cursorPath == string.Empty)
            {
                hCross = Win32.LoadCursor(IntPtr.Zero, Win32.IDC_CROSS);
            }
            else
            {
                // Load the cursor from the specified path
                hCross = CursorLoader.LoadArrowCursorFromFile(_cursorPath);
            }

            // Copy the cursor to avoid destroying the shared resource
            _hCrossCopy = Win32.CopyIcon(hCross);
            Win32.SetSystemCursor(_hCrossCopy, Win32.OCR_NORMAL);
        }

        // Set global mouse hook
        _hookId = SetHook(_proc);

        if (_hookId == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Error when installing mouse hook");
        }

        // Cancellation: call Cleanup if the operation is canceled
        _ctr = cancellationToken.Register(Cleanup);

        return result;
    }

    private IntPtr SetHook(Win32.LowLevelMouseProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return Win32.SetWindowsHookEx(Win32.WH_MOUSE_LL, proc, Win32.GetModuleHandle(curModule.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                // Retrieve the message type from wParam
                var msg = (uint)wParam;

                switch (msg)
                {
                    case Win32.WM_MOUSEMOVE:
                        HandleMouseMove(lParam);
                        break;
                }
            }
            
        }
        catch
        {
            // Handle any exceptions that might occur during the hook callback
            Cleanup();
        }

        return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// Handles the WM_MOUSEMOVE message to retrieve the window under the cursor.
    /// and the corresponding class name, text, and rectangle information.
    /// </summary>
    /// <param name="lParam">Pointer to MSLLHOOKSTRUCT</param>
    private void HandleMouseMove(IntPtr lParam)
    {
        // Read the mouse position from lParam
        var hookStruct = Marshal.PtrToStructure<Win32.MSLLHOOKSTRUCT>(lParam)!;

        // Retrieve the window under the cursor
        var hWnd = Win32.WindowFromPoint(hookStruct.pt);

        // If no window is found, exit early
        // Since everything is a window, we should not need to check for IntPtr.Zero
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        if (_onlyParentWnd)
        {
            // GA_ROOT = 2 → obtiene la ventana toplevel
            hWnd = Win32.GetAncestor(hWnd, Win32.GA_ROOT);
        }
        
        var className = Win32.GetClassName(hWnd);
        var windowText = Win32.GetWindowText(hWnd);
        _ = Win32.GetWindowRect(hWnd, out var rect);

        // Raise the MouseMoved event
        OnMouseMoved(new MouseMoveEventArgs(hWnd, className, windowText, rect.Left, rect.Top, rect.Width, rect.Height));
    }

    private void Cleanup()
    {
        if (_hookId != IntPtr.Zero)
        {
            if (!Win32.UnhookWindowsHookEx(_hookId))
            {
                var err = Marshal.GetLastWin32Error();
                Debug.WriteLine($"Failed to unhook mouse: {err}");
            }
            _hookId = IntPtr.Zero;
        }

        if (_changeCursor)
        {
            // Restore the default arrow cursor
            RestoreSystemCursor(_originalCursor);
        }

        // Destroy the allocate cursors on the heap to free resources
        if (_originalCursor != IntPtr.Zero)
        {
            Win32.DestroyIcon(_originalCursor);
            _originalCursor = IntPtr.Zero;
        }
        if (_hCrossCopy != IntPtr.Zero)
        {
            Win32.DestroyIcon(_hCrossCopy);
            _hCrossCopy = IntPtr.Zero;
        }

        // Free cancellation token
        _ctr?.Dispose();
        _ctr = null;
    }

    private void RestoreSystemCursor(IntPtr hCursor)
    {
        _ = Win32.SetSystemCursor(hCursor, Win32.OCR_NORMAL);
        _ = Win32.SystemParametersInfo(Win32.SPI_SETCURSORS, 0, IntPtr.Zero, Win32.SPIF_SENDCHANGE);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        // Free managed resources here
        // e.g. events, injected services, etc.
        if (disposing)
        {
            // Cancel and set to null TaskCompletionSource
            if (_taskCS != null && !_taskCS.Task.IsCompleted)
            {
                _taskCS.TrySetCanceled();
            }
            _taskCS = null;

            // Free cancellation token registration
            _ctr?.Dispose();
            _ctr = default;

            // Unsubscribe all listeners from the MouseMoved event
            MouseMoved = null;
        }

        // Free unmanaged resources here
        Cleanup();

        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~MouseHookService() => Dispose(disposing: false);
}

public static class CursorLoader
{
    /// <summary>
    /// Gets the path to the system arrow cursor.
    /// </summary>
    public static string GetArrowCursorPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Cursors", false);
        var value = key?.GetValue("Arrow") as string;
        if (string.IsNullOrEmpty(value))
        {
            // Fallback al cursor por defecto en System32\Cursors
            return Path.Combine(Environment.SystemDirectory, "Cursors", "arrow.cur");
        }
        return Environment.ExpandEnvironmentVariables(value);
    }

    /// <summary>
    /// Loads a cursor from a file path.
    /// </summary>
    /// <param name="path">File path of the cursor</param>
    /// <returns>Pointer to the heap memory allocating the cursor</returns>
    /// <exception cref="Win32Exception"></exception>
    public static IntPtr LoadArrowCursorFromFile(string path)
    {
        var hCur = Win32.LoadImage(
            IntPtr.Zero,
            path,
            Win32.IMAGE_CURSOR,
            0, 0,
            Win32.LR_LOADFROMFILE);

        return hCur == IntPtr.Zero
            ? throw new Win32Exception(Marshal.GetLastWin32Error(),
                $"The cursor couldn't be loaded from '{path}'")
            : hCur;
    }
}
