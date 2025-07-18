using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;
using Microsoft.UI.Xaml;

namespace CenterWindow.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string _settingsKey = nameof(AppSettings.ThemeName);

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    private readonly ILocalSettingsService<AppSettings> _localSettingsService;

    public ThemeSelectorService(ILocalSettingsService<AppSettings> localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        await Task.CompletedTask;
    }

    public ElementTheme GetTheme()
    {
        if (App.MainWindow.Content is FrameworkElement frameworkElement)
        {
            return frameworkElement.ActualTheme;
        }
        return ElementTheme.Default;
    }

    public string GetThemeName()
    {
        return GetTheme().ToString();
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            TitleBarHelper.UpdateTitleBar(Theme);
        }

        await Task.CompletedTask;
    }

    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingKeyAsync<string>(_settingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingKeyAsync(_settingsKey, theme.ToString());
    }
}
