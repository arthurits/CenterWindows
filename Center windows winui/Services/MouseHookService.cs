using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using Microsoft.Win32;

namespace CenterWindow.Services;

public partial class MouseHookService : IMouseHookService, IDisposable
{
    private readonly NativeMethods.LowLevelMouseProc _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private IntPtr _originalCursor = IntPtr.Zero;
    private IntPtr _hCrossCopy = IntPtr.Zero;
    private TaskCompletionSource<IntPtr>? _taskCS;
    private CancellationTokenRegistration? _ctr;
    private CancellationToken _token;

    public event EventHandler<MouseMoveEventArgs>? MouseMoved;
    protected virtual void OnMouseMoved(MouseMoveEventArgs e) => MouseMoved?.Invoke(this, e);

    public MouseHookService()
    {
        // Create the hook callback delegate
        _proc = HookCallback;
    }

    public Task<IntPtr> CaptureMouse(bool capture, CancellationToken cancellationToken = default)
    {
        if (capture)
        {
            return CaptureWindowUnderCursorAsync(cancellationToken);
        }
        else
        {
            Cleanup();
            return Task.FromResult(IntPtr.Zero);
        }
    }

    public Task<IntPtr> CaptureWindowUnderCursorAsync(CancellationToken cancellationToken = default)
    {
        if (_hookId != IntPtr.Zero)
        {
            throw new InvalidOperationException("Already active hook");
        }

        _taskCS = new TaskCompletionSource<IntPtr>();
        _token = cancellationToken;

        // Get the original cursor to restore later
        //_originalCursor = NativeMethods.CopyIcon(NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW));
        _originalCursor = CursorLoader.LoadArrowCursorFromFile();

        // Switch cursor to crosshair
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        var hCross = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_CROSS); // Or LoadImage your custom .cur

        // Copy the cursor to avoid destroying the shared resource
        _hCrossCopy = NativeMethods.CopyIcon(hCross);
        NativeMethods.SetSystemCursor(_hCrossCopy, NativeMethods.OCR_NORMAL);

        // Set global mouse hook
        _hookId = SetHook(_proc);

        if (_hookId == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Error when installing mouse hook");
        }

        // Cancellation: unhook and restore
        _ctr = cancellationToken.Register(() =>
        {
            Cleanup();
            _taskCS?.TrySetCanceled();
        });

        return _taskCS.Task;
    }

    private IntPtr SetHook(NativeMethods.LowLevelMouseProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                // Read the mouse position from lParam
                var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam)!;

                // Retrieve the message type from wParam
                var msg = (uint)wParam;

                switch (msg)
                {
                    case NativeMethods.WM_MOUSEMOVE:
                        // Rise the MouseMoved event with the current mouse position
                        var dto = new MousePoint { X = hookStruct.pt.x, Y = hookStruct.pt.y };
                        OnMouseMoved(new MouseMoveEventArgs(dto));
                        break;

                    case NativeMethods.WM_LBUTTONDOWN:

                        // Retrieve the window handle at the mouse position
                        var hWnd = NativeMethods.WindowFromPoint(hookStruct.pt);

                        Cleanup();
                        _taskCS?.TrySetResult(hWnd);
                        break;
                }
            }
            
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during the hook callback
            Cleanup();
            _taskCS?.TrySetException(ex);
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void Cleanup()
    {
        if (_hookId == IntPtr.Zero)
        {
            return;
        }

        if (!NativeMethods.UnhookWindowsHookEx(_hookId))
        {
            var err = Marshal.GetLastWin32Error();
            Debug.WriteLine($"Failed to unhook mouse: {err}");
        }
        _hookId = IntPtr.Zero;

        // Restore the default arrow cursor
        RestoreSystemCursor(_originalCursor);

        // Destroy the copied cursor to free resources
        if (_originalCursor != IntPtr.Zero)
        {
            NativeMethods.DestroyIcon(_originalCursor);
            _originalCursor = IntPtr.Zero;
        }
        if (_hCrossCopy != IntPtr.Zero)
        {
            NativeMethods.DestroyIcon(_hCrossCopy);
            _hCrossCopy = IntPtr.Zero;
        }

        // Free cancellation token
        _ctr?.Dispose();
        _ctr = null;
    }

    private void RestoreSystemCursor(IntPtr hCursor)
    {
        NativeMethods.SetSystemCursor(hCursor, NativeMethods.OCR_NORMAL);
        NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETCURSORS, 0, IntPtr.Zero, NativeMethods.SPIF_SENDCHANGE);
    }

    public void Dispose()
    {
        Cleanup();
        GC.SuppressFinalize(this);
    }

    ~MouseHookService() => Cleanup();
}

public static class CursorLoader
{
    /// <summary>
    /// Devuelve la ruta real del cursor Arrow que el usuario tenga configurado.
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
    /// Carga el HCURSOR desde fichero .cur sin duplicar el global.
    /// </summary>
    public static IntPtr LoadArrowCursorFromFile()
    {
        var path = GetArrowCursorPath();
        var hCur = NativeMethods.LoadImage(
            IntPtr.Zero,
            path,
            NativeMethods.IMAGE_CURSOR,
            0, 0,
            NativeMethods.LR_LOADFROMFILE);

        return hCur == IntPtr.Zero
            ? throw new Win32Exception(Marshal.GetLastWin32Error(),
                $"The cursor couldn't be loaded from '{path}'")
            : hCur;
    }
}
