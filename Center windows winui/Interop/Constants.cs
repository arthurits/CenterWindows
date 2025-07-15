namespace CenterWindow.Interop;

internal static partial class NativeMethods
{
    // Constants Win32 for hooks and cursors
    public const int WH_MOUSE_LL = 14;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const uint OCR_NORMAL = 32512;
    public static readonly IntPtr IDC_ARROW = 32512;
    public static readonly IntPtr IDC_CROSS = 32515;

    // Constants for window and path limits
    public const int MAX_PATH = 260;
    public const int MAX_CAPACITY = 256;
}
