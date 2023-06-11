using System.Runtime.InteropServices;

namespace Microsoft.Win32;

/// <summary>
/// Summary description for WindowHighlighter.
/// </summary>
public class WindowHighlighter
{

    [DllImport("dwmapi.dll", PreserveSig = false)]
    private static extern int DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    [DllImport("dwmapi.dll", PreserveSig = false)]
    private static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    [DllImport("dwmapi.dll", PreserveSig = false)]
    private static extern bool DwmIsCompositionEnabled();

    private enum DWMWINDOWATTRIBUTE
    {
        DWMWA_NCRENDERING_ENABLED = 1,
        DWMWA_NCRENDERING_POLICY,
        DWMWA_TRANSITIONS_FORCEDISABLED,
        DWMWA_ALLOW_NCPAINT,
        DWMWA_CAPTION_BUTTON_BOUNDS,
        DWMWA_NONCLIENT_RTL_LAYOUT,
        DWMWA_FORCE_ICONIC_REPRESENTATION,
        DWMWA_FLIP3D_POLICY,
        DWMWA_EXTENDED_FRAME_BOUNDS,
        DWMWA_LAST,
    }

    private enum DWMNCRENDERINGPOLICY
    {
        DWMNCRP_USEWINDOWSTYLE,
        DWMNCRP_DISABLED,
        DWMNCRP_ENABLED,
        DWMNCRP_LAST,
    }

    /// <summary>
    /// Highlights the specified window just like Spy++
    /// </summary>
    /// <param name="hWnd"></param>
    public static void Highlight(IntPtr hWnd, Color cColor, Int32 nWidth)
    {
        Win32.Rect rc = new();
        Win32.GetWindowRect(hWnd, ref rc);

        //System.Windows.Forms.ControlPaint.DrawReversibleFrame(new System.Drawing.Rectangle(rc.left, rc.top, rc.Width, rc.Height),
        //    Color.Red,
        //    System.Windows.Forms.FrameStyle.Thick);


        IntPtr hDC = Win32.GetWindowDC(hWnd);

        if (hDC != IntPtr.Zero)
        {
            /*using (Pen pen = new Pen(cColor, nWidth))
            {
                using (Graphics g = Graphics.FromHdc(hDC))
                {                        
                g.DrawRectangle(pen, 0, 0, rc.right - rc.left - (int)nWidth, rc.bottom - rc.top - (int)nWidth);
                }
            }*/

            IntPtr hParent = Win32.GetParent(hWnd);
            if (hParent == IntPtr.Zero && Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
            {
                //Int32 atribute = (Int32)DWMWINDOWATTRIBUTE.DWMWA_ALLOW_NCPAINT;
                //Boolean valor = true;
                int atribute = (int)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY;
                int policy = (int)DWMNCRENDERINGPOLICY.DWMNCRP_DISABLED;

                _ = DwmSetWindowAttribute(hWnd, atribute, ref policy, Marshal.SizeOf(policy));
            }

            using Pen pen = new(cColor, nWidth);
            using Graphics g = Graphics.FromHdc(hDC);

            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddRectangle(new Rectangle(0, 0, rc.right - rc.left - nWidth, rc.bottom - rc.top - nWidth));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawPath(pen, path);


        }

        _ = Win32.ReleaseDC(hWnd, hDC);

    }

    /// <summary>
    /// Forces a window to refresh, to eliminate our funky highlighted border
    /// </summary>
    /// <param name="hWnd"></param>
    public static void Refresh(IntPtr hWnd)
    {

        IntPtr hParent = Win32.GetParent(hWnd);
        if (hParent == IntPtr.Zero && Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
        {
            int atribute = (int)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY;
            int policy = (int)DWMNCRENDERINGPOLICY.DWMNCRP_USEWINDOWSTYLE;

            _ = DwmSetWindowAttribute(hWnd, atribute, ref policy, Marshal.SizeOf(policy));
        }

        _ = Win32.InvalidateRect(hWnd, IntPtr.Zero, 1 /* TRUE */);
        _ = Win32.UpdateWindow(hWnd);
        _ = Win32.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, Win32.RDW_FRAME | Win32.RDW_INVALIDATE | Win32.RDW_UPDATENOW | Win32.RDW_ALLCHILDREN);
        //Win32.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_DRAWFRAME | Win32.SWP_NOSIZE | Win32.SWP_NOZORDER | Win32.SWP_NOMOVE | Win32.SWP_NOACTIVATE);

    }
}