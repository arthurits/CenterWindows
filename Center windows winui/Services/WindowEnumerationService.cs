using System.Diagnostics;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using CenterWindow.Models;

namespace CenterWindow.Services;
internal class WindowEnumerationService : IWindowEnumerationService
{
    private readonly List<WindowModel> _windows = [];
    private readonly Win32.EnumWindowsProc _enumProc;

    public WindowEnumerationService()
    {
        _enumProc = WindowEnumCallback;
    }

    public IReadOnlyList<WindowModel> GetDesktopWindows()
    {
        _windows.Clear();

        _ = Win32.EnumWindows(_enumProc, IntPtr.Zero);

        return (IReadOnlyList<WindowModel>)_windows;
    }

    // este método tiene la misma firma que EnumWindowsProc
    private bool WindowEnumCallback(IntPtr hWnd, IntPtr lParam)
    {
        var WindowVisible = Win32.IsWindowVisible(hWnd);
        var WindowText = Win32.GetWindowText(hWnd);
        var WindowClassName = Win32.GetClassName(hWnd);
        if (WindowClassName == "Program" || WindowClassName == "Button")
        {
            WindowClassName = string.Empty;
        }

        if (!string.IsNullOrEmpty(WindowText) && WindowVisible && !string.IsNullOrEmpty(WindowClassName))
        {
            // Retrieves the window's executable name
            _ = Win32.GetWindowThreadProcessId(hWnd, out var uHandle);

            var process = Process.GetProcessById((int)uHandle);

            //// Alternative way to get the module name using Win32 API
            //var handle = Win32.OpenProcess((uint)(Win32.PROCESS_ACCESS_TYPES.PROCESS_QUERY_INFORMATION | Win32.PROCESS_ACCESS_TYPES.PROCESS_VM_READ), false, (uint)uHandle);
            //var moduleName = Win32.GetModuleBaseName(handle);
            //windowModule = Win32.GetModuleFileNameEx(handle); //Gets the full path

            var className = Win32.GetClassName(hWnd);

            // Gets additional window info: we are interested in the border width
            Win32.WindowInfo winInfo = new();
            _ = Win32.GetWindowInfo(hWnd, ref winInfo);

            //if (windowText != "Microsoft Edge" && windowText != "Program Manager")
            if (winInfo.xWindowBorders > 0 && winInfo.xWindowBorders > 0 && winInfo.window.Width > 0 && winInfo.window.Height > 0)
            {
                _windows.Add(new WindowModel (
                    hWnd,
                    WindowText,
                    process?.MainModule?.ModuleName ?? string.Empty,
                    className,
                    winInfo.window.Left + (int)winInfo.xWindowBorders,
                    winInfo.window.Top + (int)winInfo.yWindowBorders,
                    winInfo.window.Width - (int)(winInfo.xWindowBorders * 2),
                    winInfo.window.Height - (int)(winInfo.yWindowBorders * 2)
                    )
                );
            }
        }
        return true;
    }
}
