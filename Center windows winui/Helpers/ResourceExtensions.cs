using CenterWindow.Contracts.Services;
using Microsoft.Windows.ApplicationModel.Resources;

namespace CenterWindow.Helpers;

public static class ResourceExtensions
{
    private static Lazy<ResourceLoader> _resourceLoader = new(() => new ResourceLoader());
    private static Lazy<ILocalizationService> _localizationService = new(App.GetService<ILocalizationService>);

    public static string GetLocalized(this string resourceKey, string? resourceMap = null)
    {
        string stringValue;
        if (resourceMap is null)
        {
            stringValue = _resourceLoader.Value.GetString(resourceKey);
        }
        else
        {
            stringValue = _localizationService.Value.GetString(resourceKey, resourceMap);
        }
        return stringValue;
    }

    public static void Refresh()
    {
        // Reset WinRT resource context
        Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();

        // Substituting the Lazy instances to force a new instance
        _resourceLoader = new(() => new ResourceLoader());
        _localizationService = new(App.GetService<ILocalizationService>);
    }
}
