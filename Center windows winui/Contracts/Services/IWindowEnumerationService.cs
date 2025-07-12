using CenterWindow.Models;

namespace CenterWindow.Contracts.Services;
public interface IWindowEnumerationService
{
    IEnumerable<WindowModel> GetDesktopWindows();
}
