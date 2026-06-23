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

        // 背景運行：關窗收入系統匣，剪貼簿監察繼續運行。
        // Keep running when closed: close hides to the tray; the clipboard monitor keeps going.
        ClipboardService.Start(DispatcherQueue);
        TrayService.Install(ShowFromTray, QuitFromTray, "WinTune · 視窗調校");
        AppWindow.Closing += OnAppWindowClosing;
    }

    private bool _reallyQuit;

    private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (_reallyQuit || !TrayService.IsInstalled) return;
        args.Cancel = true;       // don't exit — hide to the tray so background work continues
        AppWindow.Hide();
    }

    private void ShowFromTray()
    {
        AppWindow.Show();
        Activate();
    }

    private void QuitFromTray()
    {
        _reallyQuit = true;
        TrayService.Remove();
        Application.Current.Exit();
    }

    private void ApplyStartPage()
    {
        if (App.StartPage is string sp && sp.StartsWith("search:", StringComparison.OrdinalIgnoreCase))
        {
            var query = sp.Substring("search:".Length);
            NavView.Loaded += (_, _) => DispatcherQueue.TryEnqueue(() =>
            {
                NavView.SelectedItem = null;
                NavFrame.Navigate(typeof(SearchResultsPage), query);
            });
            return;
        }
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
            case "connections":
            case "netstat":
            case "tcp":
                Navigator.GoToModule?.Invoke("module.connections");
                break;
            case "events":
            case "eventlog":
            case "eventviewer":
                Navigator.GoToModule?.Invoke("module.events");
                break;
            case "mixer":
            case "volume":
            case "audio":
                Navigator.GoToModule?.Invoke("module.mixer");
                break;
            case "contextmenu":
            case "rightclick":
                Navigator.GoToModule?.Invoke("module.contextmenu");
                break;
            case "awake":
                Navigator.GoToModule?.Invoke("module.awake");
                break;
            case "colorpicker":
            case "color":
                Navigator.GoToModule?.Invoke("module.colorpicker");
                break;
            case "envvars":
            case "env":
                Navigator.GoToModule?.Invoke("module.envvars");
                break;
            case "clipboard":
            case "clip":
                Navigator.GoToModule?.Invoke("module.clipboard");
                break;
            case "packages":
            case "winget":
            case "install":
                Navigator.GoToModule?.Invoke("module.packages");
                break;
            case "adb":
            case "android":
                Navigator.GoToModule?.Invoke("module.adb");
                break;
            case null:
            case "":
            case "dashboard":
                break;
            case "about":
                NavFrame.Navigate(typeof(AboutPage));
                break;
            case "settings":
                NavFrame.Navigate(typeof(SettingsPage));
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
        // 將分類收納入可摺疊嘅分組，令導覽唔會太逼。
        // Nest tweak categories under collapsible groups so the pane stays tidy.
        foreach (var cat in Categories.All)
        {
            var parent = cat.Group switch
            {
                "recipes" => RecipesGroup,
                "tools" => ToolsGroup,
                _ => TweaksGroup,
            };
            parent.MenuItems.Add(new NavigationViewItem
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
            var item = FindByTag(cat.Id);
            if (item is not null) NavView.SelectedItem = item;
        };

        Navigator.GoToSettings = () => NavFrame.Navigate(typeof(SettingsPage));

        Navigator.GoToModule = key =>
        {
            var item = FindByTag(key);
            if (item is not null) NavView.SelectedItem = item;
            else NavFrame.Navigate(MapType(key)); // fall back to direct navigation if not in the pane
        };
    }

    /// <summary>Resolve a nav item by Tag, searching nested groups recursively (pane + footer).</summary>
    private NavigationViewItem? FindByTag(string tag)
        => FindByTag(NavView.MenuItems, tag) ?? FindByTag(NavView.FooterMenuItems, tag);

    private static NavigationViewItem? FindByTag(System.Collections.Generic.IList<object> items, string tag)
    {
        foreach (var o in items)
        {
            if (o is NavigationViewItem nvi)
            {
                if ((nvi.Tag as string) == tag) return nvi;
                var child = FindByTag(nvi.MenuItems, tag);
                if (child is not null) return child;
            }
        }
        return null;
    }

    private static Type MapType(string key) => key switch
    {
        "module.git" => typeof(GitHubModule),
        "module.archives" => typeof(ArchivesModule),
        "module.media" => typeof(MediaModule),
        "module.regedit" => typeof(RegistryEditor),
        "module.services" => typeof(ServicesModule),
        "module.tasks" => typeof(ScheduledTasksModule),
        "module.devices" => typeof(DevicesModule),
        "module.startup" => typeof(StartupModule),
        "module.rename" => typeof(RenameModule),
        "module.bulkops" => typeof(BulkOpsModule),
        "module.duplicates" => typeof(DuplicatesModule),
        "module.disk" => typeof(DiskAnalyzerModule),
        "module.drives" => typeof(DrivesModule),
        "module.uninstall" => typeof(AppUninstallerModule),
        "module.windows" => typeof(WindowManagerModule),
        "module.keyboard" => typeof(KeyboardModule),
        "module.hosts" => typeof(HostsEditorModule),
        "module.mouse" => typeof(MouseModule),
        "module.recorder" => typeof(ScreenRecorderModule),
        "module.monitor" => typeof(SystemMonitorModule),
        "module.connections" => typeof(ConnectionsModule),
        "module.events" => typeof(EventViewerModule),
        "module.mixer" => typeof(VolumeMixerModule),
        "module.contextmenu" => typeof(ContextMenuModule),
        "module.awake" => typeof(AwakeModule),
        "module.colorpicker" => typeof(ColorPickerModule),
        "module.envvars" => typeof(EnvVarsModule),
        "module.clipboard" => typeof(ClipboardModule),
        "module.packages" => typeof(PackageManagerModule),
        "module.adb" => typeof(AndroidAdbModule),
        _ => typeof(DashboardPage),
    };

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        var q = sender.Text ?? "";
        if (q.Trim().Length == 0) { sender.ItemsSource = null; return; }
        var sugg = ModuleRegistry.Search(q).Select(m => $"{m.En} · {m.Zh}")
            .Concat(TweakCatalog.Search(q).Take(6).Select(t => $"{t.Title.En} · {t.Title.Zh}"))
            .Take(10).ToList();
        sender.ItemsSource = sugg;
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var q = args.QueryText;
        if (!string.IsNullOrWhiteSpace(q)) NavFrame.Navigate(typeof(SearchResultsPage), q);
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
            case "module.connections":
                NavFrame.Navigate(typeof(ConnectionsModule));
                break;
            case "module.events":
                NavFrame.Navigate(typeof(EventViewerModule));
                break;
            case "module.mixer":
                NavFrame.Navigate(typeof(VolumeMixerModule));
                break;
            case "module.contextmenu":
                NavFrame.Navigate(typeof(ContextMenuModule));
                break;
            case "module.awake":
                NavFrame.Navigate(typeof(AwakeModule));
                break;
            case "module.colorpicker":
                NavFrame.Navigate(typeof(ColorPickerModule));
                break;
            case "module.envvars":
                NavFrame.Navigate(typeof(EnvVarsModule));
                break;
            case "module.clipboard":
                NavFrame.Navigate(typeof(ClipboardModule));
                break;
            case "module.packages":
                NavFrame.Navigate(typeof(PackageManagerModule));
                break;
            case "module.adb":
                NavFrame.Navigate(typeof(AndroidAdbModule));
                break;
            default:
                var cat = Categories.All.FirstOrDefault(c => c.Id == tag);
                if (cat is not null)
                    NavFrame.Navigate(typeof(CategoryPage), cat);
                break;
        }
    }
}
