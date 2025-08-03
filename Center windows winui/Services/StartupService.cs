using System.Diagnostics;
using CenterWindow.Contracts.Services;
using Microsoft.Win32;

namespace CenterWindow.Services;
public class StartupService : IStartupService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "CenterWindows";

    private static string ExecutablePath => Process.GetCurrentProcess().MainModule.FileName;

    public void SetStartupEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);

        if (enabled)
        {
            key?.SetValue(AppName, ExecutablePath);
        }
        else
        {
            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }

    public bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        var value = key?.GetValue(AppName) as string;
        return value == ExecutablePath;
    }
}
