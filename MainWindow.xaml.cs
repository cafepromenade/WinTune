using System;
using System.Linq;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Catalog;
using WinTune.Pages;
using WinTune.Services;

namespace WinTune;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");

        // 全螢幕 kiosk 模式 · Full-screen kiosk presentation.
        AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

        BuildCategoryMenu();
        WireNavigator();

        NavFrame.Navigate(typeof(DashboardPage));
        ApplyStartPage();
    }

    private void ApplyStartPage()
    {
        switch (App.StartPage)
        {
            case "git":
            case "github":
                Navigator.GoToModule?.Invoke("module.git");
                break;
            case null:
            case "":
            case "dashboard":
                break;
            default:
                var cat = Categories.All.FirstOrDefault(c => c.Id == App.StartPage);
                if (cat is not null)
                    Navigator.GoToCategory?.Invoke(cat);
                break;
        }
    }

    private void BuildCategoryMenu()
    {
        // 喺「概覽」同分隔線後面插入全部分類 · insert all categories after the Dashboard + separator.
        foreach (var cat in Categories.All)
        {
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = $"{cat.Name.En} · {cat.Name.Zh}",
                Tag = cat.Id,
                Icon = new FontIcon { Glyph = cat.Glyph },
            });
        }
    }

    private void WireNavigator()
    {
        Navigator.GoToCategory = cat =>
        {
            var item = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => (i.Tag as string) == cat.Id);
            if (item is not null)
                NavView.SelectedItem = item;
        };

        Navigator.GoToSettings = () => NavFrame.Navigate(typeof(SettingsPage));

        Navigator.GoToModule = key =>
        {
            var item = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => (i.Tag as string) == key);
            if (item is not null)
                NavView.SelectedItem = item;
        };
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (NavFrame.CanGoBack) NavFrame.GoBack();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is not NavigationViewItem item) return;
        var tag = item.Tag as string;

        switch (tag)
        {
            case "dashboard":
                NavFrame.Navigate(typeof(DashboardPage));
                break;
            case "about":
                NavFrame.Navigate(typeof(AboutPage));
                break;
            case "module.git":
                NavFrame.Navigate(typeof(GitHubModule));
                break;
            default:
                var cat = Categories.All.FirstOrDefault(c => c.Id == tag);
                if (cat is not null)
                    NavFrame.Navigate(typeof(CategoryPage), cat);
                break;
        }
    }
}
