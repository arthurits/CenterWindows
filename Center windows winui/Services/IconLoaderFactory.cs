using CenterWindow.Contracts.Services;

namespace CenterWindow.Services;
public class IconLoaderFactory : IIconLoaderFactory
{
    private readonly GdiPlusIconLoader _gdi;
    private readonly Win2DIconLoader _win2d;

    public IconLoaderFactory(GdiPlusIconLoader gdi, Win2DIconLoader win2d)
    {
        _gdi   = gdi;
        _win2d = win2d;
    }

    public IIconLoader GetLoader(IconLoaderType type) =>
        type switch
        {
            IconLoaderType.GdiPlus => _gdi,
            IconLoaderType.Win2D => _win2d,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}