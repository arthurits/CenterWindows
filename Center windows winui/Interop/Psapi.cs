﻿using System.Runtime.InteropServices;
using System.Text;

namespace CenterWindow.Interop;
internal static partial class NativeMethods
{
    [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

    public static string GetModuleBaseName(IntPtr hProcess)
    {
        StringBuilder sb = new(MAX_CAPACITY);
        _ = GetModuleBaseName(hProcess, IntPtr.Zero, sb, sb.Capacity);
        return sb.ToString();
    }
}
