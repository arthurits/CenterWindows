using System.Text;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using CenterWindow.Models;
using Microsoft.UI.Xaml.Controls;
using static CenterWindow.Interop.NativeMethods;

namespace CenterWindow.Services;
public class WindowEnumerationService : IWindowEnumerationService
{
    private List<WindowModel> _windows;
    private readonly EnumWindowsProc _enumProc;

    public WindowEnumerationService()
    {
        _enumProc = WindowEnumCallback;
    }

    public IEnumerable<WindowModel> GetDesktopWindows()
    {
        _windows = [];

        NativeMethods.EnumWindows(_enumProc, IntPtr.Zero);

        return _windows;
    }

    // este método tiene la misma firma que EnumWindowsProc
    private bool WindowEnumCallback(IntPtr hWnd, IntPtr lParam)
    {
        var WindowVisible = NativeMethods.IsWindowVisible(hWnd);
        var WindowText = NativeMethods.GetWindowText(hWnd);
        var WindowClassName = NativeMethods.GetClassName(hWnd);
        if (WindowClassName == "Program" || WindowClassName == "Button")
        {
            WindowClassName = string.Empty;
        }

        if (!string.IsNullOrEmpty(WindowText) && WindowVisible && !string.IsNullOrEmpty(WindowClassName))
        {
            // Retrieves the name of the windows's executable
            string strWindowModule;
            IntPtr handle;
            _ = NativeMethods.GetWindowThreadProcessId(hWnd, out var uHandle);
            handle = NativeMethods.OpenProcess((uint)(NativeMethods.PROCESS_ACCESS_TYPES.PROCESS_QUERY_INFORMATION | NativeMethods.PROCESS_ACCESS_TYPES.PROCESS_VM_READ), false, (uint)uHandle);
            strWindowModule = NativeMethods.GetModuleBaseName(handle);
            //windowModule = Win32.GetModuleFileNameEx(handle); //Gets the full path
            /* Gets the same results but using the .NET framework
            Process p = Process.GetProcessById((int)uHandle);
            windowModule = p.MainModule.ModuleName.ToString();
            */

            // Gets additional window info: we are interested in the border width
            NativeMethods.WindowInfo winInfo = new();
            NativeMethods.GetWindowInfo(hWnd, ref winInfo);

            //if (windowText != "Microsoft Edge" && windowText != "Program Manager")
            if (winInfo.xWindowBorders > 0 && winInfo.xWindowBorders > 0 && winInfo.window.Width > 0 && winInfo.window.Height > 0)
            {
                _windows.Add(new WindowModel (
                    hWnd,
                    WindowText,
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
