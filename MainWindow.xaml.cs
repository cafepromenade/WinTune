using System;
using System.Collections.Generic;
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
            case "archives":
            case "archive":
                Navigator.GoToModule?.Invoke("module.archives");
                break;
            case "media":
                Navigator.GoToModule?.Invoke("module.media");
                break;
            case "regedit":
            case "registry":
                Navigator.GoToModule?.Invoke("module.regedit");
                break;
            case "services":
                Navigator.GoToModule?.Invoke("module.services");
                break;
            case "tasks":
            case "scheduledtasks":
                Navigator.GoToModule?.Invoke("module.tasks");
                break;
            case "devices":
                Navigator.GoToModule?.Invoke("module.devices");
                break;
            case "startup":
                Navigator.GoToModule?.Invoke("module.startup");
                break;
            case "rename":
                Navigator.GoToModule?.Invoke("module.rename");
                break;
            case "bulkops":
            case "bulk":
                Navigator.GoToModule?.Invoke("module.bulkops");
                break;
            case "duplicates":
            case "dupes":
                Navigator.GoToModule?.Invoke("module.duplicates");
                break;
            case "disk":
            case "diskanalyzer":
                Navigator.GoToModule?.Invoke("module.disk");
                break;
            case "drives":
                Navigator.GoToModule?.Invoke("module.drives");
                break;
            case "uninstall":
            case "apps":
                Navigator.GoToModule?.Invoke("module.uninstall");
                break;
            case "windows":
            case "windowmanager":
                Navigator.GoToModule?.Invoke("module.windows");
                break;
            case "keyboard":
            case "remap":
                Navigator.GoToModule?.Invoke("module.keyboard");
                break;
            case "hosts":
                Navigator.GoToModule?.Invoke("module.hosts");
                break;
            case "mouse":
                Navigator.GoToModule?.Invoke("module.mouse");
                break;
            case "recorder":
            case "record":
                Navigator.GoToModule?.Invoke("module.recorder");
                break;
            case "monitor":
            case "sysmon":
                Navigator.GoToModule?.Invoke("module.monitor");
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
        // 按分組插入分類；切換到 "tools" 分組時加一個標題。
        // Insert categories grouped; emit a "Tools" header when the group switches.
        var headers = new Dictionary<string, string>
        {
            ["recipes"] = "Recipes · 一鍵流程",
            ["tools"] = "Tools · 工具",
        };
        string? lastGroup = null;
        foreach (var cat in Categories.All)
        {
            if (cat.Group != lastGroup && headers.TryGetValue(cat.Group, out var hdr))
                NavView.MenuItems.Add(new NavigationViewItemHeader { Content = hdr });
            lastGroup = cat.Group;

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
            case "module.archives":
                NavFrame.Navigate(typeof(ArchivesModule));
                break;
            case "module.media":
                NavFrame.Navigate(typeof(MediaModule));
                break;
            case "module.regedit":
                NavFrame.Navigate(typeof(RegistryEditor));
                break;
            case "module.services":
                NavFrame.Navigate(typeof(ServicesModule));
                break;
            case "module.tasks":
                NavFrame.Navigate(typeof(ScheduledTasksModule));
                break;
            case "module.devices":
                NavFrame.Navigate(typeof(DevicesModule));
                break;
            case "module.startup":
                NavFrame.Navigate(typeof(StartupModule));
                break;
            case "module.rename":
                NavFrame.Navigate(typeof(RenameModule));
                break;
            case "module.bulkops":
                NavFrame.Navigate(typeof(BulkOpsModule));
                break;
            case "module.duplicates":
                NavFrame.Navigate(typeof(DuplicatesModule));
                break;
            case "module.disk":
                NavFrame.Navigate(typeof(DiskAnalyzerModule));
                break;
            case "module.drives":
                NavFrame.Navigate(typeof(DrivesModule));
                break;
            case "module.uninstall":
                NavFrame.Navigate(typeof(AppUninstallerModule));
                break;
            case "module.windows":
                NavFrame.Navigate(typeof(WindowManagerModule));
                break;
            case "module.keyboard":
                NavFrame.Navigate(typeof(KeyboardModule));
                break;
            case "module.hosts":
                NavFrame.Navigate(typeof(HostsEditorModule));
                break;
            case "module.mouse":
                NavFrame.Navigate(typeof(MouseModule));
                break;
            case "module.recorder":
                NavFrame.Navigate(typeof(ScreenRecorderModule));
                break;
            case "module.monitor":
                NavFrame.Navigate(typeof(SystemMonitorModule));
                break;
            default:
                var cat = Categories.All.FirstOrDefault(c => c.Id == tag);
                if (cat is not null)
                    NavFrame.Navigate(typeof(CategoryPage), cat);
                break;
        }
    }
}
