﻿using System.Runtime.InteropServices;

namespace CenterWindow.Interop;
internal static partial class Win32
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NOTIFYICONDATA lpData);
}
