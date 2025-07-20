using CommunityToolkit.Mvvm.ComponentModel;
using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;

namespace CenterWindow.ViewModels;

public partial class AboutViewModel : ObservableRecipient
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial string ProductName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string VersionDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Copyright { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CompanyName { get; set; } = string.Empty;

    public AboutViewModel(ILocalizationService localizationService)
    {
        // Subscribe to localization service events
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;

        OnLanguageChanged(null, EventArgs.Empty);
    }

    public void Dispose()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        ProductName = $"{"StrProductName".GetLocalized("About")} {AboutProperties.GetProductName()}";
        VersionDescription = $"{"StrVersionDescription".GetLocalized("About")} {AboutProperties.GetVersionDescription()}";
        Copyright = $"{"StrCopyright".GetLocalized("About")} {AboutProperties.GetCopyright()}";
        CompanyName = $"{"StrCompanyName".GetLocalized("About")} {AboutProperties.GetCompanyName()}";
    }
}