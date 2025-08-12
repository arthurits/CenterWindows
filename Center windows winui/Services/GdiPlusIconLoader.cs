using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;

public partial class GdiPlusIconLoader : IIconLoader, IDisposable
{
    private static readonly Lock _sync = new();
    private IntPtr _gdiplusToken;
    private static int _refCount;

    // Control the lifecycle of the library:
    // _started indicates whether GDI+ has been initialized
    // _disposed indicates whether the object has been disposed
    private bool _started;
    private bool _disposed;

    public GdiPlusIconLoader()
    {
        Start();
    }

    /// <summary>
    /// Initializes the GDI+ library for the current application, ensuring it is ready for use.
    /// </summary>
    /// <remarks>This method is thread-safe and can be called multiple times. The GDI+ library is initialized
    /// only once, regardless of the number of calls, and subsequent calls will increment an internal reference count.
    /// If the library has already been started, this method will return immediately.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if the GDI+ library fails to initialize.</exception>
    public void Start()
    {
        lock (_sync)
        {
            if (_started)
            {
                return;
            }

            if (_refCount++ == 0)
            {
                var input = new Win32.GdiplusStartupInput
                {
                    GdiplusVersion           = 1u,
                    SuppressBackgroundThread = false,
                    SuppressExternalCodecs   = false
                };

                var status = Win32.GdiplusStartup(
                    out _gdiplusToken,
                    ref input,
                    IntPtr.Zero);

                if (status != Win32.GpStatus.Ok)
                {
                    _refCount--;
                    throw new InvalidOperationException($"GDI+ startup failed: {status}");
                }
            }

            _started = true;
        }
    }

    /// <summary>
    /// Asynchronously loads an icon from the specified file path and returns a handle to the icon (HICON).
    /// </summary>
    /// <remarks>This method uses GDI+ to create a bitmap from the specified file and converts it to an HICON.
    /// Ensure that the file path points to a valid image file supported by GDI+. The returned HICON must be destroyed
    /// using <see cref="Win32.DestroyIcon"/> or equivalent system calls to avoid resource leaks.</remarks>
    /// <param name="path">The file path of the icon to load. The path must point to a valid image file.</param>
    /// <param name="size">The size of the icon to load, in pixels. Default is 16 pixels.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a handle to the loaded icon (HICON).
    /// The caller is responsible for releasing the HICON using appropriate system calls when it is no longer needed.</returns>
    public IntPtr LoadIcon(string path, uint size = 16)
    {
        // Check if the object has been disposed
        ObjectDisposedException.ThrowIf(_disposed, nameof(GdiPlusIconLoader));

        // Load the GDI+ bitmap from the file path
        var status = Win32.GdipCreateBitmapFromFile(path, out var srcGdiBmp);
        if (status != Win32.GpStatus.Ok || srcGdiBmp == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        // Create an empty destination Gdi+ bitmap (ARGB format)
        status = Win32.GdipCreateBitmapFromScan0(
            (int)size, (int)size,
            (int)(size * 4),
            Win32.PixelFormat.Format32bppPARGB,
            IntPtr.Zero,
            out var dstGdiBmp);
        
        if (status != Win32.GpStatus.Ok || dstGdiBmp == 0)
        {
            ThrowAndCleanup("GdipCreateBitmapFromScan0", (int)status, srcGdiBmp);
        }

        // Create a graphics object from the destination bitmap
        status = Win32.GdipCreateFromImage(dstGdiBmp, out var graphics);
        if (status != Win32.GpStatus.Ok || graphics == 0)
        {
            ThrowAndCleanup("GdipCreateFromImage", (int)status, srcGdiBmp, dstGdiBmp);
        }

        // Modify the graphics object to use high-quality interpolation
        Win32.GdipSetInterpolationMode(graphics, Win32.InterpolationMode.HighQualityBicubic);

        // Draw the source bitmap onto the graphics object
        status = Win32.GdipDrawImageRectI(graphics, srcGdiBmp, 0, 0, (int)size, (int)size);
        Win32.GdipDeleteGraphics(graphics);
        if (status != Win32.GpStatus.Ok)
        {
            ThrowAndCleanup("GdipDrawImageRectI", (int)status, srcGdiBmp, dstGdiBmp);
        }

        // // Convert the GDI+ bitmap to an HICON
        status = Win32.GdipCreateHICONFromBitmap(dstGdiBmp, out var hIcon);
        _ = Win32.GdipDisposeImage(srcGdiBmp);
        _ = Win32.GdipDisposeImage(dstGdiBmp);
        if (status != Win32.GpStatus.Ok)
        {
            return IntPtr.Zero;
        }

        return hIcon;

        static void ThrowAndCleanup(string call, int code, params IntPtr[] images)
        {
            foreach (var img in images)
            {
                _ = Win32.GdipDisposeImage(img);
            }

            throw new IOException($"{call} function failed with code ({code})");
        }
    }

    public IntPtr LoadIcon(string path)
    {
        // Check if the object has been disposed
        ObjectDisposedException.ThrowIf(_disposed, nameof(GdiPlusIconLoader));

        // Create a GDI+ bitmap from the file path
        var status = Win32.GdipCreateBitmapFromFile(path, out var gdiBitmap);
        if (status != Win32.GpStatus.Ok || gdiBitmap == 0)
        {
            return IntPtr.Zero;
        }

        // Convert the GDI+ bitmap to an HICON
        status = Win32.GdipCreateHICONFromBitmap(gdiBitmap, out var hIcon);

        // Free the GDI+ bitmap
        _ = Win32.GdipDisposeImage(gdiBitmap);

        if (status != Win32.GpStatus.Ok || hIcon == 0)
        {
            return IntPtr.Zero;
        }

        // Return the HICON. The caller is responsible for destroying the HICON when done.
        return hIcon;
    }

    public Task<IntPtr> LoadIconAsync(string path, uint size = 16) => Task.Run(() => LoadIcon(path, size));

    public Task<IntPtr> LoadIconAsync(string path) => Task.Run(() => LoadIcon(path));   

    public IntPtr LoadHBitmap(string path, uint size = 16)
    {
        // Check if the object has been disposed
        ObjectDisposedException.ThrowIf(_disposed, nameof(GdiPlusIconLoader));

        // Load the GDI+ bitmap from the file path
        Win32.GpStatus status = Win32.GdipCreateBitmapFromFile(path, out var srcGdiBmp);
        if (status != Win32.GpStatus.Ok || srcGdiBmp == 0)
        {
            throw new IOException($"GdipCreateBitmapFromFile failed ({status})");
        }

        // Create an empty destination Gdi+ bitmap (ARGB format)
        status = Win32.GdipCreateBitmapFromScan0(
            (int)size, (int)size,
            (int)(size * 4),                    // stride = width * bytesByPixel
            Win32.PixelFormat.Format32bppPARGB,       // formato premult alfa
            IntPtr.Zero,                        // so that GDI+ reserves the buffer memory
            out var dstGdiBmp
        );
        if (status != Win32.GpStatus.Ok || dstGdiBmp == 0)
        {
            _ = Win32.GdipDisposeImage(srcGdiBmp);
            throw new IOException($"GdipCreateBitmapFromScan0 failed ({status})");
        }

        // Create a graphics object from the destination bitmap
        status = Win32.GdipCreateFromImage(dstGdiBmp, out var graphics);
        if (status != Win32.GpStatus.Ok)
        {
            throw CreateAndCleanup("GdipCreateFromImage", (int)status, srcGdiBmp, dstGdiBmp);
        }

        // Modify the graphics object to use high-quality interpolation
        Win32.GdipSetInterpolationMode(graphics, Win32.InterpolationMode.HighQualityBicubic);

        // Draw the source bitmap onto the graphics object
        status = Win32.GdipDrawImageRectI(
            graphics, srcGdiBmp,
            0, 0,                 // origin coordinates
            (int)size, (int)size  // destination rectangle size
        );
        Win32.GdipDeleteGraphics(graphics);
        if (status != Win32.GpStatus.Ok)
        {
            throw CreateAndCleanup("GdipDrawImageRectI", (int)status, srcGdiBmp, dstGdiBmp);
        }

        // Convert the GDI+ bitmap to an HBITMAP
        status = Win32.GdipCreateHBITMAPFromBitmap(dstGdiBmp, out var hBmp, 0);
        _ = Win32.GdipDisposeImage(srcGdiBmp);
        _ = Win32.GdipDisposeImage(dstGdiBmp);
        if (status != Win32.GpStatus.Ok || hBmp == 0)
        {
            throw new IOException($"GdipCreateHBITMAPFromBitmap failed ({status})");
        }

        // Return the HBITMAP. The caller is responsible for destroying the HBITMAP when done
        return hBmp;

        static IOException CreateAndCleanup(string call, int code, params IntPtr[] images)
        {
            foreach (var img in images)
            {
                _ = Win32.GdipDisposeImage(img);
            }

            throw new IOException($"{call} function failed with code ({code})");
        }
    }

    public IntPtr LoadHBitmap(string path)
    {
        // Load the GDI+ bitmap from the file path
        var status = Win32.GdipCreateBitmapFromFile(path, out var gdiBitmap);
        if (status != Win32.GpStatus.Ok || gdiBitmap == 0)
        {
            return IntPtr.Zero;
        }

        // Convert the GDI+ bitmap to an HBITMAP
        status = Win32.GdipCreateHBITMAPFromBitmap(gdiBitmap, out var hBmp, 0);

        // Free the GDI+ bitmap
        _ = Win32.GdipDisposeImage(gdiBitmap);

        if (status != Win32.GpStatus.Ok)
        {
            return IntPtr.Zero;
        }

        // Return the HBITMAP. The caller is responsible for destroying the HBITMAP when done
        return hBmp;
    }

    public Task<IntPtr> LoadHBitmapAsync(string path, uint size = 16) => Task.Run(() => LoadHBitmap(path, size));

    public Task<IntPtr> LoadHBitmapAsync(string path) => Task.Run(() => LoadHBitmap(path));

    /// <summary>
    /// Shuts down the current instance, releasing any associated resources.
    /// </summary>
    /// <remarks>This method decrements the internal reference count and, if it reaches zero, releases
    /// resources associated with the instance. It is thread-safe and ensures  that shutdown operations are performed
    /// only once when no references remain.</remarks>
    public void Shutdown()
    {
        lock (_sync)
        {
            if (!_started)
            {
                return;
            }

            if (--_refCount == 0 && _gdiplusToken != IntPtr.Zero)
            {
                Win32.GdiplusShutdown(_gdiplusToken);
                _gdiplusToken = IntPtr.Zero;
            }

            _started = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Shutdown();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    // In case the Dispose method is not called, we ensure that GDI+ is shut down when the object is finalized.
    ~GdiPlusIconLoader() => Dispose();
}
