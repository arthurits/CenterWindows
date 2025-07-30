using CenterWindow.Contracts.Services;
using CenterWindow.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Windows.System.UserProfile;

namespace CenterWindow.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page, IDisposable
{
    public SettingsViewModel ViewModel { get; }
    private readonly ILocalizationService _localizationService;

    public SettingsPage()
    {
        //ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        ViewModel = App.GetService<SettingsViewModel>();
        _localizationService = App.GetService<ILocalizationService>();
        DataContext = ViewModel;
    }

    public void Dispose()
    {
        
    }

    /// <summary>
    /// Load available cultures into the ComboBox. Sets the default culture based on the app's primary language.
    /// </summary>
    private void LoadCultures()
    {
        // Get the list of available languages from the localization service
        var cultures = _localizationService.GetAvailableLanguages();
        //var languagesInGerman = _localizationService.GetAvailableLanguages(targetCulture: new CultureInfo("de"));

        // Asing the list to the ComboBox
        //LanguageComboBox.ItemsSource = cultures;

        // Set the default culture based on the app's primary language
        var language = Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
        var defaultCulture = !string.IsNullOrEmpty(language) ? language : GlobalizationPreferences.Languages[0];

        //// Search for the selected culture in the list
        //var selectedCulture = cultures.FirstOrDefault(lang => lang.LanguageTag == defaultCulture);

        //// Si no hay coincidencia exacta, buscar por prefijo (ej. "es" → "es-ES")
        //selectedCulture ??= cultures.FirstOrDefault(lang => defaultCulture.StartsWith(lang.LanguageTag + "-") || lang.LanguageTag.StartsWith(defaultCulture + "-"));


        // Search for the selected culture in the list
        var cultureList = cultures.ToList();

        // Look for an exact match first, then check for prefixes.
        var selectedCultureIndex = cultureList.FindIndex(lang =>
         lang.LanguageTag == defaultCulture ||
         defaultCulture.StartsWith(lang.LanguageTag + "-") ||
         lang.LanguageTag.StartsWith(defaultCulture + "-"));

        var selectedCulture = selectedCultureIndex >= 0 ? cultureList[selectedCultureIndex] : null;

        //if (selectedCulture is not null)
        //{
        //    //LanguageComboBox.SelectedItem = selectedCulture;
        //    LanguageComboBox.SelectedIndex = selectedCultureIndex;
        //}
        //else
        //{
        //    LanguageComboBox.SelectedIndex = 0; // Fallback to the first item if not found
        //}
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //var newIndex = LanguageComboBox.SelectedIndex;

        //if (_previousIndex != newIndex)
        //{
        //    if (LanguageComboBox.SelectedItem is CultureOption selected)
        //    {

        //        _localizationService.SetAppLanguage(selected.LanguageTag);

        //        // Update the ComboBox items using the actual language. Avoid infinite loop due to LanguageChanged event
        //        var cultures = _localizationService.GetAvailableLanguages();
        //        // Changing the ItemsSource sets the SelectedIndex to -1, so we set _previousIndex to -1 to avoid unnecessary updates
        //        _previousIndex = -1;
        //        LanguageComboBox.ItemsSource = cultures;
        //        // Re-select the new index after updating the items and avoit infine loop
        //        _previousIndex = newIndex;
        //        LanguageComboBox.SelectedIndex = newIndex;


        //    }
        //}
    }
}
