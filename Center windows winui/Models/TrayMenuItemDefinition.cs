namespace CenterWindow.Models;

public class TrayMenuItemDefinition
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<TrayMenuItemDefinition> Children { get; } = [];
}
