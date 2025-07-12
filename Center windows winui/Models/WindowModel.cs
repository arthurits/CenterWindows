namespace CenterWindow.Models;
public class WindowModel
{
    public IntPtr Handle { get; }
    public string Title { get; }
    public WindowRect Bounds { get; }

    public WindowModel(IntPtr handle, string title, WindowRect bounds)
    {
        Handle = handle;
        Title = title;
        Bounds = bounds;
    }
}
