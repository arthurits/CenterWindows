using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using Microsoft.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;

namespace CenterWindow.Services;
public class Win2DIconLoader : IIconLoader, IDisposable
{
    private readonly CanvasDevice _device;
    private bool _disposed;

    public Win2DIconLoader()
    {
        _device = CanvasDevice.GetSharedDevice();
    }

    public async Task<IntPtr> LoadIconAsync(string path)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(Win2DIconLoader));

        // Load the image using Win2D
        using var bitmap = await CanvasBitmap.LoadAsync(_device, path);

        // Sets the CanvasRenderTarget to the desired size
        var size = new Size(16, 16);
        using var rt = new CanvasRenderTarget(
            _device,
            (float)size.Width,
            (float)size.Height,
            bitmap.Dpi);

        // Render the bitmap onto the CanvasRenderTarget
        using (var ds = rt.CreateDrawingSession())
        {
            ds.Clear(Colors.Transparent);
            ds.DrawImage(bitmap, new Rect(0, 0, size.Width, size.Height));
        }

        // Gets the ARGB pixel data from the CanvasRenderTarget
        var pixels = rt.GetPixelBytes();

        // Create a DIBSection to hold the pixel data
        IntPtr screenDC = NativeMethods.GetDC(IntPtr.Zero);
        var header = new NativeMethods.BITMAPINFOHEADER
        {
            biSize          = (uint)Marshal.SizeOf<NativeMethods.BITMAPINFOHEADER>(),
            biWidth         = (int)size.Width,
            biHeight        = -(int)size.Height, // top-down
            biPlanes        = 1,
            biBitCount      = 32,
            biCompression   = NativeMethods.BI_RGB,
            biSizeImage     = 0,
            biXPelsPerMeter = 0,
            biYPelsPerMeter = 0,
            biClrUsed       = 0,
            biClrImportant  = 0
        };
        var bmi = new NativeMethods.BITMAPINFO { bmiHeader = header };

        IntPtr ppvBits;
        var hBitmap = NativeMethods.CreateDIBSection(
            screenDC,
            ref bmi,
            NativeMethods.DIB_RGB_COLORS,
            out ppvBits,
            IntPtr.Zero,
            0);

        // Copy the pixel data to the native bitmap
        Marshal.Copy(pixels, 0, ppvBits, pixels.Length);

        NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);

        // Packs the pixel data into an ICONINFO structure and creates an HICON
        var iconInfo = new NativeMethods.ICONINFO
        {
            fIcon     = true,
            xHotspot  = 0,
            yHotspot  = 0,
            hbmMask   = IntPtr.Zero,
            hbmColor  = hBitmap
        };
        IntPtr hIcon = NativeMethods.CreateIconIndirect(ref iconInfo);

        // Delete the intermediate HBITMAP object to free resources
        NativeMethods.DeleteObject(hBitmap);

        return hIcon;
    }

    // Dispose pattern completo
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        // Here we would release any managed resources if needed
        _disposed = true;
    }

    ~Win2DIconLoader()
    {
        Dispose(false);
    }
}
