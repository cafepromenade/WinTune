using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 應用程式內解除安裝（商店／UWP）· In-app Store/UWP app uninstaller — list, search, silent per-row
/// uninstall via Remove-AppxPackage. Frameworks excluded. No Settings redirect. Bilingual.
/// </summary>
public sealed partial class AppUninstallerModule : Page
{
    private List<AppInfo> _all = new();
    private bool _busy;

    public AppUninstallerModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += async (_, _) => { Render(); await Reload(); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "App Uninstaller · 應用程式解除安裝";
        HeaderBlurb.Text = P("Remove Store/UWP apps (bloatware) silently — shared frameworks are hidden so they can't be removed. Reinstall from the Store anytime.",
            "靜靜哋移除商店／UWP 應用程式（臃腫程式）— 共用框架已經收埋，唔會誤刪。隨時可以由商店重裝。");
        FilterBox.PlaceholderText = P("Filter apps (e.g. Xbox, Bing, Clipchamp)…", "篩選應用程式（例如 Xbox、Bing、Clipchamp）…");
        RefreshBtn.Content = P("Refresh", "重新整理");
    }

    private async Task Reload()
    {
        if (_busy) return;
        _busy = true;
        CountText.Text = P("Loading…", "載入緊…");
        try
        {
            _all = await UninstallManager.ListAsync();
            ApplyFilter(FilterBox.Text ?? string.Empty);
        }
        finally { _busy = false; }
    }

    private void ApplyFilter(string filter)
    {
        IEnumerable<AppInfo> shown = _all;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            shown = _all.Where(a => a.Name.ToLowerInvariant().Contains(f) || a.Publisher.ToLowerInvariant().Contains(f));
        }
        var listed = shown.ToList();
        List.ItemsSource = listed;
        CountText.Text = P($"{listed.Count} / {_all.Count} apps", $"{listed.Count} / {_all.Count} 個應用程式");
        _ = ComputeSizesAsync(listed);
    }

    /// <summary>Lazily compute install sizes for the visible rows, then rebind so SizeText shows.</summary>
    private async Task ComputeSizesAsync(List<AppInfo> rows)
    {
        bool any = false;
        foreach (var a in rows)
        {
            if (a.SizeBytes != 0) continue;
            try { a.SizeBytes = await UninstallManager.ComputeSizeAsync(a); } catch { a.SizeBytes = -1; }
            any = true;
        }
        // Rebind only if these rows are still the ones shown (avoid clobbering a newer filter).
        if (any && ReferenceEquals(List.ItemsSource, rows))
        {
            List.ItemsSource = null;
            List.ItemsSource = rows;
        }
    }

    private void Filter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ApplyFilter(sender.Text ?? string.Empty);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await Reload();

    private void Actions_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.DataContext is not AppInfo app) return;
        var mf = new MenuFlyout();

        var u = new MenuFlyoutItem { Text = P("Uninstall", "解除安裝"), Icon = new FontIcon { Glyph = ((char)0xE74D).ToString() } };
        u.Click += async (_, _) => await DoUninstall(app, deep: false);
        mf.Items.Add(u);

        var d = new MenuFlyoutItem
        {
            Text = P("Deep uninstall (clear leftovers)", "深層解除安裝（清殘留）"),
            Icon = new FontIcon { Glyph = ((char)0xE74D).ToString() },
        };
        d.Click += async (_, _) => await DoUninstall(app, deep: true);
        mf.Items.Add(d);

        mf.ShowAt(b);
    }

    private async Task DoUninstall(AppInfo app, bool deep)
    {
        if (_busy) return;

        var sizeNote = app.SizeBytes > 0 ? P($"\nInstall size: {app.SizeText}", $"\n安裝大細：{app.SizeText}") : "";
        var dlg = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = deep ? P("Deep uninstall?", "深層解除安裝？") : P("Uninstall app?", "解除安裝？"),
            Content = $"{app.ShortName}\n{app.Name}{sizeNote}\n\n" + (deep
                ? P("Remove this app AND clear its leftover settings/cache (LocalAppData\\Packages). You can reinstall it from the Microsoft Store.",
                    "移除呢個應用程式，仲會清走佢殘留嘅設定／快取（LocalAppData\\Packages）。之後可以由 Microsoft Store 重裝。")
                : P("Remove this app for the current user? You can reinstall it from the Microsoft Store.",
                    "幫目前使用者移除呢個應用程式？之後可以由 Microsoft Store 重裝。")),
            PrimaryButtonText = deep ? P("Deep uninstall", "深層解除安裝") : P("Uninstall", "解除安裝"),
            CloseButtonText = P("Cancel", "取消"),
            DefaultButton = ContentDialogButton.Close,
        };
        if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;

        _busy = true;
        try
        {
            var r = deep ? await UninstallManager.DeepUninstall(app) : await UninstallManager.Uninstall(app);
            bool needAdmin = !r.Success && !AdminHelper.IsElevated;
            ResultBar.Severity = r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            ResultBar.Title = r.Success ? P("Done", "完成") : P("Failed", "失敗");
            ResultBar.Message = r.Success
                ? (r.Message?.Get(Loc.I.Language) ?? P($"Uninstalled {app.ShortName}.", $"已解除安裝 {app.ShortName}。"))
                : needAdmin
                    ? P($"'{app.ShortName}' may need administrator rights (system app).", $"「{app.ShortName}」可能需要管理員權限（系統應用程式）。")
                    : $"{app.ShortName} — {(r.Output ?? "").Split('\n').FirstOrDefault()}";
            ResultBar.IsOpen = true;
        }
        finally { _busy = false; }
        await Reload();
    }
}
