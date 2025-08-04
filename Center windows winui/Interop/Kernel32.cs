using System.Runtime.InteropServices;

namespace CenterWindow.Interop;
internal static partial class NativeMethods
{
    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
}
