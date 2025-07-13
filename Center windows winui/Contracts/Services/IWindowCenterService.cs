namespace CenterWindow.Contracts.Services;
public interface IWindowCenterService
{
    void CenterWindow(IntPtr hWnd, byte alpha);
}