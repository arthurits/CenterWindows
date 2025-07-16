using CenterWindow.Contracts.Services;
using Microsoft.Windows.ApplicationModel.Resources;

namespace CenterWindow.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();
    public static readonly ILocalizationService _localizationService = App.GetService<ILocalizationService>();

    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
    public static string GetLocalized(this string resourceKey, string resourceMap) => _localizationService.GetString(resourceKey, resourceMap);
}
