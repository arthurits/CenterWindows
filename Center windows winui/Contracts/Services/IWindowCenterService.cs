using Microsoft.UI.Xaml;

namespace CenterWindow.Contracts.Services;
public interface IWindowCenterService
{
    void CenterWindow(IntPtr hWnd, byte alpha);
    void SetWindowTransparency(IntPtr hWnd, byte alpha);
}