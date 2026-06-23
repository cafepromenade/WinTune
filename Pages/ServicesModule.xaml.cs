using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Models;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 應用程式內服務管理員 · In-app Services Manager — list, search, start/stop/restart and set startup type,
/// all without leaving WinTune (no services.msc redirect). Bilingual.
/// </summary>
public sealed partial class ServicesModule : Page
{
    private List<ServiceInfo> _all = new();
    private bool _busy;

    public ServicesModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += async (_, _) => { Render(); await Reload(); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "Services · 服務";
        FilterBox.PlaceholderText = P("Filter services…", "篩選服務…");
        RefreshBtn.Content = P("Refresh", "重新整理");
        StartupLabel.Text = P("Startup type for selected:", "選中服務嘅啟動類型：");
        AutoBtn.Content = P("Automatic", "自動");
        ManualBtn.Content = P("Manual", "手動");
        DisabledBtn.Content = P("Disabled", "停用");
        if (!AdminHelper.IsElevated)
        {
            ResultBar.Severity = InfoBarSeverity.Informational;
            ResultBar.Title = P("Tip", "提示");
            ResultBar.Message = P("Relaunch WinTune as administrator to start/stop services.",
                "以管理員身分重開 WinTune 先可以啟動／停止服務。");
            ResultBar.IsOpen = true;
        }
    }

    private async Task Reload()
    {
        if (_busy) return;
        _busy = true;
        CountText.Text = P("Loading…", "載入緊…");
        try
        {
            _all = await ServiceManager.ListAsync();
            ApplyFilter(FilterBox.Text ?? string.Empty);
        }
        finally { _busy = false; }
    }

    private void ApplyFilter(string filter)
    {
        IEnumerable<ServiceInfo> shown = _all;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            shown = _all.Where(s => s.DisplayName.ToLowerInvariant().Contains(f) || s.Name.ToLowerInvariant().Contains(f));
        }
        var listed = shown.ToList();
        List.ItemsSource = listed;
        CountText.Text = P($"{listed.Count} / {_all.Count} services", $"{listed.Count} / {_all.Count} 個服務");
    }

    private void Filter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ApplyFilter(sender.Text ?? string.Empty);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await Reload();

    private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        bool has = List.SelectedItem is ServiceInfo;
        AutoBtn.IsEnabled = has;
        ManualBtn.IsEnabled = has;
        DisabledBtn.IsEnabled = has;
        if (List.SelectedItem is ServiceInfo s)
            StartupLabel.Text = P($"Startup for {s.DisplayName}:", $"{s.DisplayName} 嘅啟動類型：");
    }

    private static ServiceInfo? Svc(object sender) => (sender as FrameworkElement)?.DataContext as ServiceInfo;

    private async void Start_Click(object sender, RoutedEventArgs e) => await Run(Svc(sender), n => ServiceManager.Start(n), P("Start", "啟動"));
    private async void Stop_Click(object sender, RoutedEventArgs e) => await Run(Svc(sender), n => ServiceManager.Stop(n), P("Stop", "停止"));
    private async void Restart_Click(object sender, RoutedEventArgs e) => await Run(Svc(sender), n => ServiceManager.Restart(n), P("Restart", "重啟"));

    private async void Auto_Click(object sender, RoutedEventArgs e) => await Run(List.SelectedItem as ServiceInfo, n => ServiceManager.SetStartup(n, "Automatic"), P("Set Automatic", "設自動"));
    private async void Manual_Click(object sender, RoutedEventArgs e) => await Run(List.SelectedItem as ServiceInfo, n => ServiceManager.SetStartup(n, "Manual"), P("Set Manual", "設手動"));
    private async void Disabled_Click(object sender, RoutedEventArgs e) => await Run(List.SelectedItem as ServiceInfo, n => ServiceManager.SetStartup(n, "Disabled"), P("Set Disabled", "設停用"));

    private async Task Run(ServiceInfo? svc, Func<string, Task<TweakResult>> op, string verb)
    {
        if (svc is null || _busy) return;
        _busy = true;
        try
        {
            var r = await op(svc.Name);
            bool needAdmin = !r.Success && !AdminHelper.IsElevated;
            ResultBar.Severity = r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            ResultBar.Title = r.Success ? P("Done", "完成") : P("Failed", "失敗");
            ResultBar.Message = needAdmin
                ? P($"{verb} '{svc.DisplayName}' needs administrator rights.", $"{verb}「{svc.DisplayName}」需要管理員權限。")
                : $"{verb} '{svc.DisplayName}' — {(r.Success ? "OK" : (r.Output ?? ""))}";
            ResultBar.IsOpen = true;
        }
        finally { _busy = false; }
        await Reload();
    }
}
