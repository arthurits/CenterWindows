using CenterWindow.Contracts.Services;
using CenterWindow.Interop;

namespace CenterWindow.Services;

public partial class GdiPlusIconLoader : IIconLoader, IDisposable
{
    private readonly IntPtr _gdiplusToken;
    private bool _disposed;

  
    public GdiPlusIconLoader()
    {
        // Start up GDI+
        var input = new Win32.GdiplusStartupInput
        {
            GdiplusVersion = 1u,
            SuppressBackgroundThread = false,
            SuppressExternalCodecs = false
        };

        var status = Win32.GdiplusStartup(out _gdiplusToken, ref input, IntPtr.Zero);
        if (status != 0)
        {
            throw new InvalidOperationException($"GdiplusStartup did not start up: {status}");
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
    public Task<IntPtr> LoadIconAsync(string path, uint size = 16)
    {
        // Check if the object has been disposed
        ObjectDisposedException.ThrowIf(_disposed, nameof(GdiPlusIconLoader));

        // Create a GDI+ bitmap from the file path
        Win32.GdipCreateBitmapFromFile(path, out var bitmap);

        // Convert the GDI+ bitmap to an HICON
        Win32.GdipCreateHICONFromBitmap(bitmap, out var hIcon);

        // Free the GDI+ bitmap
        Win32.GdipDisposeImage(bitmap);

        // Return the HICON. The caller is responsible for destroying the HICON when done.
        return Task.FromResult(hIcon);
    }

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

        // We only shut down GDI+ once if we are disposing.
        Win32.GdiplusShutdown(_gdiplusToken);
        _disposed = true;
    }

    // In case the Dispose method is not called, we ensure that GDI+ is shut down when the object is finalized.
    ~GdiPlusIconLoader()
    {
        Dispose(false);
    }
}
