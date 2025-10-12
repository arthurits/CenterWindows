using CenterWindow.Contracts.Services;
using Microsoft.Win32;

namespace CenterWindow.Services;
public class StartupService : IStartupService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "CenterWindows";
    private const string StartupArgument = "--startup";

    private static string ExecutablePath => Environment.ProcessPath ?? string.Empty;

    /// <summary>
    /// Set the registry key to enable or disable the application launching at system startup.
    /// </summary>
    /// <param name="enabled"><see langword="True"/> if the app should be launched, <see langword="false"/> otherwise</param>
    public void SetStartupEnabled(bool enabled)
    {
        // Make sure the registry key exists
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true)
                      ?? Registry.CurrentUser.CreateSubKey(RegistryKeyPath, writable: true);

        //using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);

        if (string.IsNullOrWhiteSpace(ExecutablePath))
        {
            if (enabled)
            {
                //key?.SetValue(AppName, ExecutablePath);

                // Include the --startup argument to let the app know it was launched at startup
                var value = $"\"{ExecutablePath}\" {StartupArgument}";
                key?.SetValue(AppName, value);
            }
            else
            {
                key?.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
        
    }

    /// <summary>
    /// Check if the application is set, on the registry, to launch at system startup.
    /// </summary>
    /// <returns><see langword="True"/> if the registry key exists, <see langword="false"/> otherwise</returns>
    public bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        var value = key?.GetValue(AppName) as string;
        //return value == ExecutablePath;

        // Verify that the value matches the expected format with the --startup argument
        var expected = $"\"{ExecutablePath}\" {StartupArgument}";
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
