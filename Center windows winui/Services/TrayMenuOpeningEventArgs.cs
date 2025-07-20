using CenterWindow.Models;

namespace CenterWindow.Services;

public class TrayMenuOpeningEventArgs : EventArgs
{
    public List<TrayMenuItemDefinition> Items { get; } = [];
}
