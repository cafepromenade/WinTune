using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinTune.Catalog;
using WinTune.Controls;
using WinTune.Models;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 概覽：系統摘要、全域搜尋、分類入口。
/// Dashboard: system summary, global bilingual search, and category tiles.
/// </summary>
public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += (_, _) => Render();
    }

    private void Render()
    {
        HeroTitle.Text = "WinTune · 視窗調校";
        HeroSubtitle.Text = Loc.I.Pick(
            "An all-in-one, fully bilingual control center that really tunes Windows 11.",
            "全方位、全雙語嘅控制中心，真係會幫你調校 Windows 11。");
        CountBadge.Text = Loc.I.Pick(
            $"{TweakCatalog.Count} features across {Categories.All.Length} categories",
            $"{TweakCatalog.Count} 項功能，分 {Categories.All.Length} 個分類");

        RenderAdminBar();
        RenderModuleTiles();
        RenderStats();
        RenderCategoryTiles();

        ModulesHeader.Text = Loc.I.Pick("Suite modules", "套件模組");
        StatsHeader.Text = Loc.I.Pick("System at a glance", "系統一覽");
        BrowseHeader.Text = Loc.I.Pick("Browse categories", "瀏覽分類");
        SearchBox.PlaceholderText = Loc.I.Pick(
            "Search all features (English or 粵語)…", "搜尋全部功能（英文或粵語）…");
    }

    private void RenderAdminBar()
    {
        if (AdminHelper.IsElevated)
        {
            AdminBar.Severity = InfoBarSeverity.Success;
            AdminBar.Title = Loc.I.Pick("Administrator", "管理員");
            AdminBar.Message = Loc.I.Pick(
                "Running elevated — every tweak is available.",
                "正以管理員身分運行 — 全部調校都用得。");
            AdminBar.ActionButton = null;
        }
        else
        {
            AdminBar.Severity = InfoBarSeverity.Warning;
            AdminBar.Title = Loc.I.Pick("Standard user", "標準使用者");
            AdminBar.Message = Loc.I.Pick(
                "Some system-wide tweaks need administrator rights.",
                "部分全系統調校需要管理員權限。");
            var relaunch = new Button { Content = "Relaunch as admin · 以管理員身分重新啟動" };
            relaunch.Click += (_, _) =>
            {
                if (AdminHelper.RelaunchElevated())
                    Application.Current.Exit();
            };
            AdminBar.ActionButton = relaunch;
        }
    }

    private void RenderStats()
    {
        StatsPanel.Children.Clear();
        AddStat("", "Operating system", "作業系統", SystemInfo.OsFull);
        AddStat("", "Processor", "處理器", $"{SystemInfo.CpuName}  ({SystemInfo.LogicalProcessors} threads · {SystemInfo.Architecture})");
        AddStat("", "Memory", "記憶體", SystemInfo.RamUsage);
        AddStat("", "Graphics", "顯示卡", SystemInfo.GpuName);
        AddStat("", "System drive", "系統磁碟", SystemInfo.SystemDrive);
        AddStat("", "Uptime", "運行時間", $"{SystemInfo.Uptime}  ({Loc.I.Pick("since", "由")} {SystemInfo.BootTime})");
    }

    private void AddStat(string glyph, string en, string zh, string value)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var icon = new FontIcon { Glyph = glyph, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Left };
        Grid.SetColumn(icon, 0);

        var label = new TextBlock
        {
            Text = $"{en} · {zh}",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        };
        Grid.SetColumn(label, 1);

        var val = new TextBlock
        {
            Text = value,
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
        };
        Grid.SetColumn(val, 2);

        grid.Children.Add(icon);
        grid.Children.Add(label);
        grid.Children.Add(val);
        StatsPanel.Children.Add(grid);
    }

    private void RenderModuleTiles()
    {
        var tiles = new List<UIElement>
        {
            ModuleTile("", "Git & GitHub", "Git 與 GitHub",
                Loc.I.Pick("Repos, commits, chunked upload, GitHub CLI", "儲存庫、提交、分批上載、GitHub CLI"),
                () => Navigator.GoToModule?.Invoke("module.git")),
            ModuleTile("", "Archives", "壓縮檔",
                Loc.I.Pick("Create/extract/test with 7-Zip, 100 ops", "用 7-Zip 建立／解壓／測試，100 項操作"),
                () => Navigator.GoToModule?.Invoke("module.archives")),
            ModuleTile("", "Registry Editor", "登錄編輯器",
                Loc.I.Pick("Browse & edit the registry in-app", "喺 app 內瀏覽同編輯登錄檔"),
                () => Navigator.GoToModule?.Invoke("module.regedit")),
            ModuleTile("", "Services", "服務",
                Loc.I.Pick("Start/stop/configure services in-app", "喺 app 內啟動／停止／設定服務"),
                () => Navigator.GoToModule?.Invoke("module.services")),
            ModuleTile("", "Scheduled Tasks", "排程工作",
                Loc.I.Pick("Run/enable/disable tasks in-app", "喺 app 內執行／啟用／停用工作"),
                () => Navigator.GoToModule?.Invoke("module.tasks")),
            ModuleTile("", "Devices", "裝置",
                Loc.I.Pick("Enable/disable devices in-app", "喺 app 內啟用／停用裝置"),
                () => Navigator.GoToModule?.Invoke("module.devices")),
            ModuleTile("", "Windows 11 control", "Windows 11 控制",
                Loc.I.Pick($"{Categories.All.Length - 1} tweak categories below", $"下面有 {Categories.All.Length - 1} 個調校分類"),
                () => Navigator.GoToCategory?.Invoke(Categories.Appearance)),
        };

        ModuleRepeater.Layout = new UniformGridLayout { MinItemWidth = 320, MinItemHeight = 76, MinRowSpacing = 4, MinColumnSpacing = 4 };
        ModuleRepeater.ItemsSource = tiles;
    }

    private Button ModuleTile(string glyph, string titleEn, string titleZh, string sub, Action onClick)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        content.Children.Add(new FontIcon { Glyph = glyph, FontSize = 24, VerticalAlignment = VerticalAlignment.Center });
        var texts = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        texts.Children.Add(new TextBlock { Text = $"{titleEn} · {titleZh}", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        texts.Children.Add(new TextBlock { Text = sub, FontSize = 12, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        content.Children.Add(texts);
        var button = new Button
        {
            Content = content,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 8, 8),
            MinWidth = 300,
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void RenderCategoryTiles()
    {
        var tiles = new List<UIElement>();
        foreach (var cat in Categories.All)
        {
            var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            content.Children.Add(new FontIcon { Glyph = cat.Glyph, FontSize = 22, VerticalAlignment = VerticalAlignment.Center });
            var texts = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            texts.Children.Add(new TextBlock { Text = $"{cat.Name.En} · {cat.Name.Zh}", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            texts.Children.Add(new TextBlock
            {
                Text = Loc.I.Pick($"{TweakCatalog.CountFor(cat)} features", $"{TweakCatalog.CountFor(cat)} 項功能"),
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            });
            content.Children.Add(texts);

            var button = new Button
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 8, 8),
                MinWidth = 280,
            };
            var captured = cat;
            button.Click += (_, _) => Navigator.GoToCategory?.Invoke(captured);
            tiles.Add(button);
        }

        CategoryRepeater.Layout = new UniformGridLayout
        {
            MinItemWidth = 300,
            MinItemHeight = 72,
            MinRowSpacing = 4,
            MinColumnSpacing = 4,
        };
        CategoryRepeater.ItemsSource = tiles;
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        var query = sender.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(query))
        {
            SearchResults.Visibility = Visibility.Collapsed;
            BrowseSection.Visibility = Visibility.Visible;
            SearchResults.Children.Clear();
            return;
        }

        BrowseSection.Visibility = Visibility.Collapsed;
        SearchResults.Visibility = Visibility.Visible;
        SearchResults.Children.Clear();

        var matches = TweakCatalog.Search(query).Take(60).ToList();
        var header = new TextBlock
        {
            Text = Loc.I.Pick($"{matches.Count} result(s)", $"{matches.Count} 個結果"),
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
        };
        SearchResults.Children.Add(header);

        foreach (var t in matches)
        {
            var card = new TweakCard();
            card.SetTweak(t);
            SearchResults.Children.Add(card);
        }
    }
}
