﻿using Microsoft.UI.Xaml.Controls;

namespace CenterWindow.Contracts.Services;

public interface INavigationViewService
{
    IList<object>? MenuItems { get; }

    object? SettingsItem { get; }

    void Initialize(NavigationView navigationView);

    void UnregisterEvents();

    NavigationViewItem? GetSelectedItem(Type pageType);

    // Copilot suggested the following method
    void SyncSelectedItem(Type pageType);
}
