namespace CenterWindow.Models;

public class TrayMenuItemDefinition
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsSeparator { get; set; } = false;
    public List<TrayMenuItemDefinition> Children { get; } = [];
}
