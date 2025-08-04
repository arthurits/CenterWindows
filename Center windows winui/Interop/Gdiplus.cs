using System.Runtime.InteropServices;
using static CenterWindow.Interop.NativeMethods;

namespace CenterWindow.Interop;
internal static partial class NativeMethod
{
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int GdipCreateBitmapFromFile(string filename, out IntPtr bitmap);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipCreateHICONFromBitmap(IntPtr bitmap, out IntPtr hicon);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipDisposeImage(IntPtr image);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern void GdiplusShutdown(IntPtr token);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdiplusStartup(out IntPtr token, ref GdiplusStartupInput input, IntPtr output);
}
