using System.Runtime.InteropServices;
using static CenterWindow.Interop.NativeMethods;

namespace CenterWindow.Interop;
internal static partial class NativeMethod
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NOTIFYICONDATA lpData);
}
