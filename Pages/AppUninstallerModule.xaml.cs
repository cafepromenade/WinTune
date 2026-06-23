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
    }

    private void Filter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ApplyFilter(sender.Text ?? string.Empty);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await Reload();

    private async void Uninstall_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || (sender as FrameworkElement)?.DataContext is not AppInfo app) return;

        var dlg = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = P("Uninstall app?", "解除安裝？"),
            Content = $"{app.ShortName}\n{app.Name}\n\n" + P("Remove this app for the current user? You can reinstall it from the Microsoft Store.",
                "幫目前使用者移除呢個應用程式？之後可以由 Microsoft Store 重裝。"),
            PrimaryButtonText = P("Uninstall", "解除安裝"),
            CloseButtonText = P("Cancel", "取消"),
            DefaultButton = ContentDialogButton.Close,
        };
        if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;

        _busy = true;
        try
        {
            var r = await UninstallManager.Uninstall(app);
            bool needAdmin = !r.Success && !AdminHelper.IsElevated;
            ResultBar.Severity = r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            ResultBar.Title = r.Success ? P("Done", "完成") : P("Failed", "失敗");
            ResultBar.Message = r.Success
                ? P($"Uninstalled {app.ShortName}.", $"已解除安裝 {app.ShortName}。")
                : needAdmin
                    ? P($"'{app.ShortName}' may need administrator rights (system app).", $"「{app.ShortName}」可能需要管理員權限（系統應用程式）。")
                    : $"{app.ShortName} — {(r.Output ?? "").Split('\n').FirstOrDefault()}";
            ResultBar.IsOpen = true;
        }
        finally { _busy = false; }
        await Reload();
    }
}
