using System.Runtime.InteropServices;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace CenterWindow.Interop;

internal static class NativeMethods
{
    public const int MAX_PATH = 260;
    public const int MAX_CAPACITY = 256;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly int Width => Right - Left;
        public readonly int Height => Bottom - Top;
    }

    public struct WindowInfo
    {
        public int size;
        public Rect window;
        public Rect client;
        public int style;
        public int exStyle;
        public int windowStatus;
        public uint xWindowBorders;
        public uint yWindowBorders;
        public short atomWindowtype;
        public short creatorVersion;
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
    private static extern int GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

    [DllImport("user32.dll")]
    public static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo info);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtrW")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowModuleFileName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("User32.dll")]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    /// <summary>
	/// Returns the caption of a window Win32.GetWindowText
	/// </summary>
	/// <param name="hWnd"></param>
	/// <returns></returns>
	public static string GetWindowText(IntPtr hWnd)
    {
        var length = GetWindowTextLength(hWnd);
        if (length == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new(length + 1);
        _ = GetWindowText(hWnd, sb, sb.Capacity);

        return sb.ToString();
    }

    /// <summary>
	/// Returns the name of the window's module
	/// </summary>
	/// <param name="hWnd"></param>
	/// <returns></returns>
	public static string GetWindowModuleFileName(IntPtr hWnd)
    {
        StringBuilder sb = new(MAX_CAPACITY);
        _ = GetWindowModuleFileName(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    /// <summary>
    /// Returns the name of a window's class Win32.GetClassName
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    public static string GetClassName(IntPtr hWnd)
    {
        StringBuilder sb = new(MAX_CAPACITY);
        _ = GetClassName(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    public static string GetModuleBaseName(IntPtr hProcess)
    {
        StringBuilder sb = new(MAX_CAPACITY);
        int result = GetModuleBaseName(hProcess, IntPtr.Zero, sb, sb.Capacity);
        return sb.ToString();
    }

    public enum PROCESS_ACCESS_TYPES
    {
        PROCESS_TERMINATE = 0x00000001,
        PROCESS_CREATE_THREAD = 0x00000002,
        PROCESS_SET_SESSIONID = 0x00000004,
        PROCESS_VM_OPERATION = 0x00000008,
        PROCESS_VM_READ = 0x00000010,
        PROCESS_VM_WRITE = 0x00000020,
        PROCESS_DUP_HANDLE = 0x00000040,
        PROCESS_CREATE_PROCESS = 0x00000080,
        PROCESS_SET_QUOTA = 0x00000100,
        PROCESS_SET_INFORMATION = 0x00000200,
        PROCESS_QUERY_INFORMATION = 0x00000400,
        STANDARD_RIGHTS_REQUIRED = 0x000F0000,
        SYNCHRONIZE = 0x00100000,
        PROCESS_ALL_ACCESS = PROCESS_TERMINATE | PROCESS_CREATE_THREAD | PROCESS_SET_SESSIONID | PROCESS_VM_OPERATION |
            PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_DUP_HANDLE | PROCESS_CREATE_PROCESS | PROCESS_SET_QUOTA |
            PROCESS_SET_INFORMATION | PROCESS_QUERY_INFORMATION | STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE
    }
}
