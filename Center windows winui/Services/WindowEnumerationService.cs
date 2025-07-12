using System.Text;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using CenterWindow.Models;

namespace CenterWindow.Services;
public class WindowEnumerationService : IWindowEnumerationService
{
    public IEnumerable<WindowModel> GetDesktopWindows()
    {
        var windows = new List<WindowModel>();

        _ = NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (!NativeMethods.IsWindowVisible(hwnd))
            {
                return true;
            }

            // Título
            var length = NativeMethods.GetWindowTextLength(hwnd);
            var sb = new StringBuilder(length + 1);
            _ = NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
            var title = sb.ToString();

            // Rectángulo
            NativeMethods.RECT rc;
            NativeMethods.GetWindowRect(hwnd, out rc);
            var bounds = new WindowRect(
                rc.Left,
                rc.Top,
                rc.Right - rc.Left,
                rc.Bottom - rc.Top);

            windows.Add(new WindowModel(hwnd, title, bounds));
            return true;
        }, IntPtr.Zero);

        return windows;
    }

}
