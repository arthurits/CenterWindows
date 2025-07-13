namespace CenterWindow.Models;
public class WindowModel
{
    public IntPtr Hwnd { get; }
    public string Title { get; }
    
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    // These properties will be used for binding in the UI
    public string Handle => Hwnd.ToString("X");
    public string Rect => $"{X},{Y}  {Width}×{Height}";

    public WindowModel(IntPtr handle, string title, int x, int y, int width, int height)
    {
        Hwnd = handle;
        Title = title;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
