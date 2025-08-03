namespace CenterWindow.Contracts.Services;
public interface IStartupService
{
    void SetStartupEnabled(bool enabled);
    bool IsStartupEnabled();
}
