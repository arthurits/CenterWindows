using CenterWindow.Models;

namespace CenterWindow.Contracts.Services;
public interface IWindowEnumerationService
{
    IReadOnlyList<WindowModel> GetDesktopWindows();
}
