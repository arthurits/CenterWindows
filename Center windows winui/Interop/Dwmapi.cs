using System.Runtime.InteropServices;

namespace CenterWindow.Interop;
internal static partial class Win32
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmEnableBlurBehindWindow(
        IntPtr hwnd,
        ref DWM_BLURBEHIND pBlurBehind);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmExtendFrameIntoClientArea(
        IntPtr hwnd,
        ref MARGINS pMarInset);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        DWMWINDOWATTRIBUTE attribute,
        ref uint pvAttribute,
        int cbAttribute);
}
