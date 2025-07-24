namespace CenterWindow.Interop;

internal static partial class NativeMethods
{
    // Constants Win32 for hooks and cursors
    public const int WH_MOUSE_LL = 14;
    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_LBUTTONDBLCLK = 0x0203;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_RBUTTONUP = 0x0205;
    public const int WM_RBUTTONDBLCLK = 0x0206;
    public const int WM_MBUTTONDOWN = 0x0207;
    public const int WM_MBUTTONUP = 0x0208;
    public const int WM_MBUTTONDBLCLK = 0x0209;
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

    // Constans for AppendMenu
    public const uint MF_STRING = 0x00000000;
    public const uint MF_POPUP = 0x00000010;
    public const uint MF_SEPARATOR = 0x00000800;
    public const uint MF_UNCHECKED = 0x00000000;
    public const uint MF_CHECKED = 0x00000008;
    public const uint MF_DISABLED = 0x00000002;
    public const uint MF_ENABLED = 0x00000000;
    public const uint MF_GRAYED = 0x00000001;

    // Flags for MENUITEMINFO.fMask
    public const uint MIIM_BITMAP = 0x00000080;

    // Flags for DrawIconEx
    public const uint DI_NORMAL = 0x0003;

    // Flags for TrackPopupMenu
    public const uint TPM_LEFTALIGN = 0x0000;
    public const uint TPM_RETURNCMD = 0x0100;
    public const uint TPM_RIGHTBUTTON = 0x0002;

    // Win32 messages
    public const uint WM_COMMAND = 0x0111;
}
