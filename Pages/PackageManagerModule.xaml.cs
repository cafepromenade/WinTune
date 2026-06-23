using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 套件管理（UniGetUI 式，包 winget）· In-app package manager — install common dependencies in one click,
/// and search / install / uninstall any winget package. No redirect (wraps winget). Bilingual.
/// </summary>
public sealed partial class PackageManagerModule : Page
{
    private HashSet<string> _installed = new(StringComparer.OrdinalIgnoreCase);

    public PackageManagerModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => { Render(); BuildDeps(); };
        Loaded += async (_, _) => { Render(); BuildDeps(); await RefreshInstalled(); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "Package Manager · 套件管理";
        HeaderBlurb.Text = P("Install WinTune's engines and common tools in one click, or search and install any app — powered by winget, all in-app.",
            "一鍵安裝 WinTune 嘅引擎同常用工具，或者搜尋安裝任何 app — 用 winget 引擎，全部喺 app 內。");
        SearchBox.PlaceholderText = P("Search winget (e.g. vscode, vlc, obs)…", "搜尋 winget（例如 vscode、vlc、obs）…");
        UpgradesBtn.Content = P("Upgrades", "可更新");
        DepsHeader.Text = P("Common dependencies", "常用相依");
        InstallAllBtn.Content = P("Install all missing", "安裝全部欠缺");
        ResultsHeader.Text = "";
    }

    private async Task RefreshInstalled()
    {
        Busy.IsActive = true;
        try { _installed = await PackageService.InstalledIds(); } catch { }
        Busy.IsActive = false;
        BuildDeps();
    }

    private void BuildDeps()
    {
        DepsPanel.Children.Clear();
        foreach (var dep in PackageService.Deps)
        {
            bool installed = _installed.Contains(dep.Id);
            DepsPanel.Children.Add(Row($"{dep.En} · {dep.Zh}", dep.Id, installed,
                onAction: btn => InstallDep(dep.Id, btn), isInstalledAction: false));
        }
    }

    private Border Row(string title, string id, bool installed, Action<Button> onAction, bool isInstalledAction)
    {
        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var texts = new StackPanel { Spacing = 1, VerticalAlignment = VerticalAlignment.Center };
        texts.Children.Add(new TextBlock { Text = title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 13, TextWrapping = TextWrapping.Wrap });
        texts.Children.Add(new TextBlock { Text = id, FontSize = 11, FontFamily = new FontFamily("Consolas"), Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"] });
        Grid.SetColumn(texts, 0);
        grid.Children.Add(texts);

        var btn = new Button { Padding = new Thickness(12, 4, 12, 4) };
        if (installed)
        {
            btn.Content = P("Installed", "已安裝");
            btn.IsEnabled = false;
        }
        else
        {
            btn.Content = P("Install", "安裝");
            btn.Click += (_, _) => onAction(btn);
        }
        Grid.SetColumn(btn, 1);
        grid.Children.Add(btn);

        return new Border
        {
            Padding = new Thickness(14, 10, 14, 10),
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = grid,
        };
    }

    private async void InstallDep(string id, Button btn)
    {
        btn.IsEnabled = false;
        btn.Content = P("Installing…", "安裝緊…");
        var r = await PackageService.Install(id);
        btn.Content = r.Success ? P("Installed", "已安裝") : P("Retry", "重試");
        btn.IsEnabled = !r.Success;
        if (r.Success) _installed.Add(id);
    }

    private async void InstallAll_Click(object sender, RoutedEventArgs e)
    {
        InstallAllBtn.IsEnabled = false;
        InstallAllBtn.Content = P("Installing…", "安裝緊…");
        foreach (var dep in PackageService.Deps)
        {
            if (_installed.Contains(dep.Id)) continue;
            var r = await PackageService.Install(dep.Id);
            if (r.Success) _installed.Add(dep.Id);
        }
        BuildDeps();
        InstallAllBtn.Content = P("Install all missing", "安裝全部欠缺");
        InstallAllBtn.IsEnabled = true;
    }

    private async void Search_Submitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var q = (args.QueryText ?? "").Trim();
        if (q.Length < 2) return;
        ResultsHeader.Text = P("Searching…", "搜尋緊…");
        ResultsPanel.Children.Clear();
        Busy.IsActive = true;
        List<PkgResult> results;
        try { results = await PackageService.Search(q); } catch { results = new(); }
        Busy.IsActive = false;

        ResultsHeader.Text = P($"Results — {results.Count}", $"結果 — {results.Count}");
        foreach (var r in results)
        {
            string capturedId = r.Id;
            ResultsPanel.Children.Add(Row($"{r.Name}  ({r.Version})", r.Id, _installed.Contains(r.Id),
                onAction: btn => InstallDep(capturedId, btn), isInstalledAction: false));
        }
    }

    private async void Upgrades_Click(object sender, RoutedEventArgs e)
    {
        ResultsHeader.Text = P("Checking upgrades…", "檢查更新緊…");
        ResultsPanel.Children.Clear();
        Busy.IsActive = true;
        List<PkgResult> ups;
        try { ups = await PackageService.Upgradable(); } catch { ups = new(); }
        Busy.IsActive = false;
        ResultsHeader.Text = P($"Upgradable — {ups.Count}", $"可更新 — {ups.Count}");
        foreach (var u in ups)
        {
            string capturedId = u.Id;
            var border = Row($"{u.Name}  ({u.Version})", u.Id, false, onAction: async btn =>
            {
                btn.IsEnabled = false; btn.Content = P("Updating…", "更新緊…");
                var r = await PackageService.Upgrade(capturedId);
                btn.Content = r.Success ? P("Updated", "已更新") : P("Retry", "重試");
                btn.IsEnabled = !r.Success;
            }, isInstalledAction: false);
            // relabel the action button to "Update"
            ResultsPanel.Children.Add(border);
        }
    }
}
