using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;

namespace CenterWindow.Services;

public class MouseHookService : IMouseHookService
{
    // Constantes Win32 para hooks y cursores
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const uint OCR_NORMAL = 32512;
    private static readonly IntPtr IDC_ARROW = 32512;
    private static readonly IntPtr IDC_CROSS = 32515;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static readonly LowLevelMouseProc _proc = HookCallback;
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
        var hCross = LoadCursor(IntPtr.Zero, IDC_CROSS);

        // Copy the cursor to avoid destroying the shared resource
        var hCrossCopy = CopyIcon(hCross);
        SetSystemCursor(hCrossCopy, OCR_NORMAL);

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

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WM_LBUTTONDOWN)
        {
            // Read the mouse position from the lParam
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam)!;
            var pt = hookStruct.pt;

            // Retrieve the window handle at the mouse position
            var hWnd = WindowFromPoint(pt);
            Cleanup();
            _taskCS.TrySetResult(hWnd);
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static void Cleanup()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;

            // Restore the default arrow cursor
            var hArrow = CopyIcon(LoadCursor(IntPtr.Zero, IDC_ARROW));
            SetSystemCursor(hArrow, OCR_NORMAL);
        }
    }

    #region PInvoke
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

    [DllImport("user32.dll")]
    private static extern bool SetSystemCursor(IntPtr hcur, uint id);

    [DllImport("user32.dll")]
    private static extern IntPtr CopyIcon(IntPtr hIcon);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    #endregion
}
