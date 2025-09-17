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
    private enum State { Idle, Capturing, Disposed }
    private State _state = State.Idle;
    private int _unmanagedCleanupDone;  // 0 = pending cleanup, 1 = cleanup done

    public event EventHandler<MouseMoveEventArgs>? MouseMoved;
    protected virtual void OnMouseMoved(MouseMoveEventArgs e) => MouseMoved?.Invoke(this, e);

    public MouseHookService()
    {
        // Create the hook callback delegate
        _proc = HookCallback;
    }

    public bool CaptureMouse(string cursorPath, bool changeCursor = false, bool onlyParentWnd = false, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_state != State.Idle)
        {
            throw new InvalidOperationException("Hook is already active or the service has been disposed");
        }

        // Get the service parameters
        _cursorPath = cursorPath;
        _changeCursor = changeCursor;
        _onlyParentWnd = onlyParentWnd;

        // Set the flag to 0 before starting the capture
        Interlocked.Exchange(ref _unmanagedCleanupDone, 0);

        _state = State.Capturing;

        // Set a try/catch block to handle any errors and manage the resources
        var succeeded = false;
        try
        {
            succeeded = CaptureWindowUnderCursor(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"[Warning] Mouse capture was not started: {ex.Message}");
        }
        catch (Win32Exception ex)
        {
            Debug.WriteLine($"[Error Win32] Error when creating the mouse hook or changing the image: {ex.Message}");
        }
        finally
        {
            // In case something happened, release resources and set the state to idle
            if (!succeeded)
            {
                ReleaseUnmanaged();
                _state = State.Idle;
            }
        }

        return succeeded;
    }

    public void ReleaseMouse()
    {
        ThrowIfDisposed();
        if (_state != State.Capturing)
        {
            return;
        }

        ReleaseUnmanaged();
        _state = State.Idle;
        _taskCS?.TrySetCanceled();
    }

    private bool CaptureWindowUnderCursor(CancellationToken cancellationToken = default)
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
            var arrowPath = CursorLoader.GetArrowCursorPath();
            if (!CursorLoader.TryLoadCursorFromFile(arrowPath, out _originalCursor))
            {
                Debug.WriteLine($"[Warning] Could not load the base cursor from: '{arrowPath}'");
            }

            // Switch cursor to crosshair
            //using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            var hCross = IntPtr.Zero;
            if (string.IsNullOrEmpty(_cursorPath))
            {
                hCross = Win32.LoadCursor(IntPtr.Zero, Win32.IDC_CROSS);
            }
            else if (!CursorLoader.TryLoadCursorFromFile(_cursorPath, out hCross))
            {
                Debug.WriteLine($"[Warning] Could not load the custrom cursor from: '{_cursorPath}'");
            }

            // If we get a valid cursor (hCross != 0), then we apply it.
            // If not, we skip this part and continue setting the mouse hook.
            // We need to copy the cursor to avoid destroying the shared resource.
            if (hCross != IntPtr.Zero)
            {
                _hCrossCopy = Win32.CopyIcon(hCross);
                if (_hCrossCopy != IntPtr.Zero)
                {
                    Win32.SetSystemCursor(_hCrossCopy, Win32.OCR_NORMAL);
                }
                else
                {
                    Debug.WriteLine("[Error] Win32.CopyIcon returned IntPtr.Zero and the cursor was not changed");
                }
            }
            else
            {
                Debug.WriteLine("[Warning] Cursor change was skipped because hCross == IntPtr.Zero");
            }
        }

        // Set global mouse hook
        _hookId = SetHook(_proc);

        if (_hookId == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Error when installing mouse hook");
        }

        // Cancellation in case the operation is canceled
        _ctr = cancellationToken.Register(() =>
        {
            // Release resources and reset state
            ReleaseUnmanaged();
            _state = State.Idle;
            // Notify the TaskCompletionSource
            _taskCS?.TrySetCanceled();
        });

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
            ReleaseUnmanaged();
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

        var windowText = Win32.GetWindowText(hWnd);
        var className = Win32.GetClassName(hWnd);
        _ = Win32.GetWindowThreadProcessId(hWnd, out var uHandle);
        var process = Process.GetProcessById((int)uHandle);
        var moduleName = process.MainModule?.ModuleName ?? string.Empty;
        _ = Win32.GetWindowRect(hWnd, out var rect);

        // Raise the MouseMoved event
        OnMouseMoved(new MouseMoveEventArgs(hWnd, windowText, moduleName, className, rect.Left, rect.Top, rect.Width, rect.Height));
    }

    private void ReleaseUnmanaged()
    {
        // Only the first call to this method will execute the cleanup
        if (Interlocked.Exchange(ref _unmanagedCleanupDone, 1) == 0)
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
    }

    private void RestoreSystemCursor(IntPtr hCursor)
    {
        _ = Win32.SetSystemCursor(hCursor, Win32.OCR_NORMAL);
        _ = Win32.SystemParametersInfo(Win32.SPI_SETCURSORS, 0, IntPtr.Zero, Win32.SPIF_SENDCHANGE);
    }

    private void ReleaseManaged()
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

    public void Dispose()
    {
        if (_state == State.Disposed)
        {
            return;
        }

        ReleaseUnmanaged();
        ReleaseManaged();

        _state = State.Disposed;
        GC.SuppressFinalize(this);
    }

    ~MouseHookService() => Dispose();

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_state == State.Disposed, nameof(MouseHookService));
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
    /// <param name="hCursor">Loaded cursor as a pointer to the heap memory allocating the cursor</param>
    /// <returns><see langword="true"> if the cursor was successfully loaded, <see langword="false"> otherwise</returns>
    public static bool TryLoadCursorFromFile(string path, out IntPtr hCursor)
    {
        hCursor = IntPtr.Zero;

        if (!File.Exists(path))
        {
            return false;
        }

        hCursor = Win32.LoadImage(
            IntPtr.Zero,
            path,
            Win32.IMAGE_CURSOR,
            0, 0,
            Win32.LR_LOADFROMFILE);

        return hCursor != IntPtr.Zero;
    }
}
