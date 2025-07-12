using System.Text;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;

namespace CenterWindow.Services;
public class WindowEnumerationService : IWindowEnumerationService
{
    public IEnumerable<WindowModel> GetDesktopWindows()
    {
        var result = new List<WindowModel>();

        EnumWindows((hwnd, _) =>
        {
            if (IsWindowVisible(hwnd))
            {
                // Obtener título
                int length = GetWindowTextLength(hwnd);
                var sb = new StringBuilder(length + 1);
                GetWindowText(hwnd, sb, sb.Capacity);

                // Obtener posición
                var rect = new RECT();
                GetWindowRect(hwnd, ref rect);
                var bounds = new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                result.Add(new WindowModel(hwnd, sb.ToString(), bounds));
            }
            return true;
        }, IntPtr.Zero);

        return result;
    }

    // Aquí van las declaraciones de PInvoke: EnumWindows, IsWindowVisible, GetWindowText, GetWindowRect, etc.
}
