using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Catalog;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 設定與控制台總匯 · Settings &amp; Control Panel hub — a bilingual, searchable launcher that lists
/// every common <c>ms-settings:</c> page plus every Control Panel applet (<c>control /name …</c>
/// and <c>*.cpl</c>) and opens each target via Process.Start.
/// 呢個係唯一允許嘅「啟動器」式集線器：好多 Windows applet 喺 app 內冇等價物，所以直接打開。
/// This is the one allowed launcher-style hub: many Windows applets have no in-app equivalent.
/// </summary>
public sealed partial class SettingsHubModule : Page
{
    /// <summary>畀 UI 綁定用嘅項目包裝（提供雙語標籤）· Binding wrapper exposing language-aware labels.</summary>
    public sealed class HubItem
    {
        public SettingsHubEntry Entry { get; init; } = null!;
        public string En => Entry.En;
        public string Zh => Entry.Zh;
        public string CommandText => Entry.CommandText;
        public string KindLabel => Loc.I.Pick(KindEn, KindZh);
        public string OpenLabel => Loc.I.Pick("Open", "開啟");

        private string KindEn => Entry.Kind switch
        {
            SettingsHubKind.Settings => "Settings",
            SettingsHubKind.ControlApplet => "Applet",
            SettingsHubKind.Cpl => "CPL",
            _ => "",
        };

        private string KindZh => Entry.Kind switch
        {
            SettingsHubKind.Settings => "設定",
            SettingsHubKind.ControlApplet => "控制台",
            SettingsHubKind.Cpl => "CPL",
            _ => "",
        };
    }

    public SettingsHubModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => { Render(); ApplyFilter(FilterBox.Text ?? string.Empty); };
        Loaded += (_, _) => { Render(); ApplyFilter(string.Empty); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "Settings & Control Panel · 設定與控制台";
        HeaderBlurb.Text = P(
            "Search and open any Windows Settings page or Control Panel applet. Some open the modern Settings app, others the classic Control Panel.",
            "搜尋同開啟任何 Windows 設定頁面或者控制台 applet。部分會開現代「設定」app，部分會開傳統控制台。");
        FilterBox.PlaceholderText = P("Search settings & applets…", "搜尋設定同 applet…");
    }

    private void ApplyFilter(string filter)
    {
        IEnumerable<SettingsHubEntry> shown = SettingsHubCatalog.Search(filter);
        var items = shown.Select(e => new HubItem { Entry = e }).ToList();
        List.ItemsSource = items;
        CountText.Text = P($"{items.Count} / {SettingsHubCatalog.All.Count} items",
                           $"{items.Count} / {SettingsHubCatalog.All.Count} 個項目");
        EmptyText.Text = P("Nothing matches your search.", "冇項目符合你嘅搜尋。");
        EmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Filter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ApplyFilter(sender.Text ?? string.Empty);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.DataContext is not HubItem item) return;
        var entry = item.Entry;
        bool ok = Launch(entry);

        ResultBar.Severity = ok ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        ResultBar.Title = ok ? P("Opened", "已開啟") : P("Failed", "失敗");
        ResultBar.Message = ok
            ? $"{entry.En} · {entry.Zh} — {entry.CommandText}"
            : P($"Couldn't open {entry.En}.", $"開唔到 {entry.En}。");
        ResultBar.IsOpen = true;
    }

    /// <summary>用 Process.Start 啟動真實目標 · Launch the real target with Process.Start.</summary>
    private static bool Launch(SettingsHubEntry entry)
    {
        try
        {
            switch (entry.Kind)
            {
                case SettingsHubKind.Settings:
                    // ms-settings: URI — shell-execute opens the Settings app.
                    Process.Start(new ProcessStartInfo { FileName = entry.Target, UseShellExecute = true });
                    return true;

                case SettingsHubKind.ControlApplet:
                    // control.exe /name <CanonicalName>
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "control.exe",
                        Arguments = $"/name {entry.Target}",
                        UseShellExecute = true,
                    });
                    return true;

                case SettingsHubKind.Cpl:
                    if (entry.Target.Equals("control.exe", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Process.Start(new ProcessStartInfo { FileName = "control.exe", UseShellExecute = true });
                    }
                    else
                    {
                        // control.exe <name.cpl>
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "control.exe",
                            Arguments = entry.Target,
                            UseShellExecute = true,
                        });
                    }
                    return true;

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
}
