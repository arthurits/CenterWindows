﻿using System.Globalization;
using CenterWindow.Models;

namespace CenterWindow.Contracts.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    event EventHandler LanguageChanged;
    string GetString(string key);
    string GetString(string key, string resourceMap);
    void SetAppLanguage(string languageCode, string invariantCulture = "en", bool notifyLanguageChanged = true);
    IEnumerable<CultureOption> GetAvailableLanguages(
        string resourceFileName = "Resources",
        bool invariantCulture = true,
        bool loopAllKeys = false,
        CultureInfo? targetCulture = null);
}
