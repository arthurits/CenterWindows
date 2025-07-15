using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;

public class MouseHookService : IMouseHookService
{
    private static readonly NativeMethods.LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookId = IntPtr.Zero;

    private static TaskCompletionSource<IntPtr> _taskCS;
    private static CancellationToken _token;

    public Task<IntPtr> CaptureWindowUnderCursorAsync(CancellationToken cancellationToken = default)
    {
        if (_hookId != IntPtr.Zero)
        {
            throw new InvalidOperationException("Already active hook");
        }

        _taskCS = new TaskCompletionSource<IntPtr>();
        _token = cancellationToken;

        // Switch cursor to crosshair
        var hCross = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_CROSS);

        // Copy the cursor to avoid destroying the shared resource
        var hCrossCopy = NativeMethods.CopyIcon(hCross);
        NativeMethods.SetSystemCursor(hCrossCopy, NativeMethods.OCR_NORMAL);

        // Set global mouse hook
        _hookId = SetHook(_proc);

        // Cancellation: unhook and restore
        cancellationToken.Register(() =>
        {
            Cleanup();
            _taskCS.TrySetCanceled();
        });

        return _taskCS.Task;
    }

    private static IntPtr SetHook(NativeMethods.LowLevelMouseProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == NativeMethods.WM_LBUTTONDOWN)
        {
            // Read the mouse position from the lParam
            var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam)!;
            var pt = hookStruct.pt;

            // Retrieve the window handle at the mouse position
            var hWnd = NativeMethods.WindowFromPoint(pt);
            Cleanup();
            _taskCS.TrySetResult(hWnd);
        }
        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static void Cleanup()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;

            // Restore the default arrow cursor
            var hArrow = NativeMethods.CopyIcon(NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW));
            NativeMethods.SetSystemCursor(hArrow, NativeMethods.OCR_NORMAL);
        }
    }
}
