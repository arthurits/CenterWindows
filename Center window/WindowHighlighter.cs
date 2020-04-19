using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Center_window
{
	/// <summary>
	/// Summary description for WindowHighlighter.
	/// </summary>
	public class WindowHighlighter
	{

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute, IntPtr pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, IntPtr pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

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
			Win32.Rect rc = new Win32.Rect();
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
                    Int32 atribute = (Int32)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY;
                    Int32 valor = (Int32)DWMNCRENDERINGPOLICY.DWMNCRP_DISABLED;
                    
                    // Unnecessary but just for the fun of it
                    unsafe
                    {
                        void* puntero = &valor;
                        DwmSetWindowAttribute(hWnd, atribute, (IntPtr)puntero, sizeof(Int32));
                    }
                }

                using (Pen pen = new Pen(cColor, nWidth))
                {
                    using (Graphics g = Graphics.FromHdc(hDC))
                    {
                        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                        path.AddRectangle(new Rectangle(0, 0, rc.right - rc.left - (int)nWidth, rc.bottom - rc.top - (int)nWidth));
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.DrawPath(pen, path);
                    }
                }


			}

            Win32.ReleaseDC(hWnd, hDC);

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
                Int32 atribute = (Int32)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY;
                Int32 valor = (Int32)DWMNCRENDERINGPOLICY.DWMNCRP_USEWINDOWSTYLE;

                unsafe
                {
                    void* puntero = &valor;
                    DwmSetWindowAttribute(hWnd, atribute, (IntPtr)puntero, sizeof(Int32));
                }
            }

			Win32.InvalidateRect(hWnd, IntPtr.Zero, 1 /* TRUE */);
			Win32.UpdateWindow(hWnd);
            Win32.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, Win32.RDW_FRAME | Win32.RDW_INVALIDATE | Win32.RDW_UPDATENOW | Win32.RDW_ALLCHILDREN);
            //Win32.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_DRAWFRAME | Win32.SWP_NOSIZE | Win32.SWP_NOZORDER | Win32.SWP_NOMOVE | Win32.SWP_NOACTIVATE);

		}
	}
}
