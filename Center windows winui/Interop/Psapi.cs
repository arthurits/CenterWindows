using System.Runtime.InteropServices;
using System.Text;

namespace CenterWindow.Interop;
internal static partial class NativeMethods
{
    [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);
}
