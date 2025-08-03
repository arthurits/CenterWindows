using System.Runtime.InteropServices;
using System.Text;

namespace CenterWindow.Interop;

internal static partial class NativeMethods
{
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

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        // campos posteriores ignorados para no recargar la estructura
    }

    // Structure for menu-item information
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MENUITEMINFO
    {
        public uint cbSize;
        public uint fMask;
        public uint fType;
        public uint fState;
        public uint wID;
        public IntPtr hSubMenu;
        public IntPtr hbmpChecked;
        public IntPtr hbmpUnchecked;
        public IntPtr dwItemData;
        public IntPtr dwTypeData;
        public uint cch;
        public IntPtr hbmpItem;
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cx, int cy, uint fuLoad);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetCursor(IntPtr hCursor);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetSystemCursor(IntPtr hcur, uint id);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CopyIcon(IntPtr hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo info);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int newProc);
    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, newProc);
        else
            return new IntPtr(SetWindowLong32(hWnd, nIndex, newProc.ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);
    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowModuleFileName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo( uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NOTIFYICONDATA lpData);

    // P/Invoke: create menu, add items, track and destroy
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
        _ = GetModuleBaseName(hProcess, IntPtr.Zero, sb, sb.Capacity);
        return sb.ToString();
    }

    // Associates an HBITMAP with a menu item
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetMenuItemInfo(IntPtr hMenu, uint uItem, bool fByPos, ref MENUITEMINFO lpmii);

    // GDI / user32 functions to create bitmaps and draw icons
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DrawIconEx(
        IntPtr hdc,
        int xLeft,
        int yTop,
        IntPtr hIcon,
        int cxWidth,
        int cyWidth,
        uint istepIfAniCur,
        IntPtr hbrFlickerFreeDraw,
        uint diFlags);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdiplusStartup(
        out IntPtr token,
        ref GdiplusStartupInput input,
        IntPtr output);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern void GdiplusShutdown(IntPtr token);

    [StructLayout(LayoutKind.Sequential)]
    public struct GdiplusStartupInput
    {
        public uint GdiplusVersion;
        public IntPtr DebugEventCallback;
        public bool SuppressBackgroundThread;
        public bool SuppressExternalCodecs;
    }

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int GdipCreateBitmapFromFile(
        string filename,
        out IntPtr bitmap);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipCreateHICONFromBitmap(
        IntPtr bitmap,
        out IntPtr hicon);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipDisposeImage(IntPtr image);

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


    // gdi32.dll
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateDIBSection(
        IntPtr hdc,
        ref BITMAPINFO pbmi,
        uint usage,
        out IntPtr ppvBits,
        IntPtr hSection,
        uint offset);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateIconIndirect(ref ICONINFO iconInfo);

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        public uint bmiColors;  // espacio mínimo para el color table
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public bool fIcon;
        public uint xHotspot;
        public uint yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    // Constants
    public const uint BI_RGB = 0u;
    public const uint DIB_RGB_COLORS = 0u;
}
