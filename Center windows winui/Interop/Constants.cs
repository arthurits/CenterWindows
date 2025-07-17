namespace CenterWindow.Interop;

internal static partial class NativeMethods
{
    // Constants Win32 for hooks and cursors
    public const int WH_MOUSE_LL = 14;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const uint OCR_NORMAL = 32512;
    public const IntPtr IDC_ARROW = 32512;
    public const IntPtr IDC_CROSS = 32515;
    public const uint SPI_SETCURSORS = 0x0057;
    public const uint SPIF_SENDCHANGE = 0x02;
    public const uint IMAGE_ICON = 1;
    public const uint IMAGE_CURSOR = 2;
    public const uint LR_LOADFROMFILE = 0x00000010;

    // Constants for window and path limits
    public const int MAX_PATH = 260;
    public const int MAX_CAPACITY = 256;

    // Tray icon constants
    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;
    public const uint WM_USER = 0x0400;
    public const uint WM_TRAYICON = WM_USER + 1;
    public const int GWL_WNDPROC = -4;
}
