using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Catalog;
using WinTune.Controls;
using WinTune.Models;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 壓縮模組 · Archives module: wraps 7-Zip for create/extract/list/test plus ~100 advanced operations.
/// </summary>
public sealed partial class ArchivesModule : Page
{
    private List<TweakDefinition>? _ops;
    private bool _busy;

    public ArchivesModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += OnLang;
        Unloaded += (_, _) => Loc.I.LanguageChanged -= OnLang;
        Loaded += (_, _) => { Render(); BuildQuickOps(); PopulateOps(string.Empty); RefreshSelection(); };
    }

    private void OnLang(object? sender, EventArgs e) { Render(); BuildQuickOps(); PopulateOps(OpsFilter.Text ?? string.Empty); }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "Archives · 壓縮檔";
        HeaderBlurb.Text = P("Create, extract, list and test archives with 7-Zip — zip, 7z, tar, gzip, bzip2, xz and more.",
            "用 7-Zip 建立、解壓、列出同測試壓縮檔 — zip、7z、tar、gzip、bzip2、xz 等等。");
        SelLabel.Text = P("Selection", "選擇");
        ArcCap.Text = P("Archive", "壓縮檔");
        SrcCap.Text = P("Source", "來源");
        OpenArcBtn.Content = P("Open…", "開啟…");
        NewArcBtn.Content = P("New…", "新建…");
        SrcFileBtn.Content = P("File…", "檔案…");
        SrcFolderBtn.Content = P("Folder…", "資料夾…");
        CreateLabel.Text = P("Create archive (format · level · password)", "建立壓縮檔（格式 · 等級 · 密碼）");
        PasswordBox.PlaceholderText = P("Optional password…", "可選密碼…");
        CreateBtn.Content = P("Create", "建立");
        OpsFilter.PlaceholderText = P("Filter operations…", "篩選操作…");
        AdvancedHeader.Text = P($"Advanced operations ({GitOpsCount()})", $"進階操作（{GitOpsCount()}）");

        if (!ArchiveService.IsInstalled)
        {
            EngineBar.IsOpen = true;
            EngineBar.Severity = InfoBarSeverity.Warning;
            EngineBar.Title = P("7-Zip not found", "搵唔到 7-Zip");
            EngineBar.Message = P("Click to install 7-Zip automatically (winget) — no restart needed.", "撳一下自動安裝 7-Zip（winget）— 唔使重開。");
            EngineBar.ActionButton = EngineBars.AutoInstallButton(
                "7zip.7zip", "Install 7-Zip automatically", "自動安裝 7-Zip",
                () => { Render(); return Task.CompletedTask; }, ArchiveService.Rescan);
        }
        else { EngineBar.IsOpen = false; EngineBar.ActionButton = null; }
    }

    private int GitOpsCount() => (_ops ??= ArchiveOperations.All().ToList()).Count;

    private void RefreshSelection()
    {
        ArchiveBox.Text = AppState.CurrentArchivePath;
        SourceBox.Text = AppState.CurrentSourcePath;
    }

    private void BuildQuickOps()
    {
        QuickOps.Children.Clear();
        AddQuick(P("List", "列出"), () => ArchiveService.List());
        AddQuick(P("Test", "測試"), () => ArchiveService.Test());
        AddQuick(P("Extract here", "解壓到旁邊"), () => ArchiveService.ExtractHere());
        AddQuick(P("Benchmark", "效能測試"), () => ArchiveService.Benchmark());
    }

    private void AddQuick(string label, Func<Task<TweakResult>> run)
    {
        var btn = new Button { Content = label };
        btn.Click += async (_, _) => await RunAndShow(btn, run);
        QuickOps.Children.Add(btn);
    }

    private async Task RunAndShow(Button btn, Func<Task<TweakResult>> run)
    {
        if (_busy) return;
        _busy = true;
        var label = btn.Content;
        btn.IsEnabled = false;
        btn.Content = new ProgressRing { IsActive = true, Width = 16, Height = 16 };
        OutBorder.Visibility = Visibility.Visible;
        OutText.Text = P("Running…", "執行緊…");
        try
        {
            var r = await run();
            var head = r.Success ? P("✓ Done", "✓ 完成") : P("✗ Failed", "✗ 失敗");
            OutText.Text = head + "\n" + (string.IsNullOrWhiteSpace(r.Output)
                ? ((Loc.I.IsCantonesePrimary ? r.Message?.Zh : r.Message?.En) ?? "")
                : r.Output);
        }
        catch (Exception ex) { OutText.Text = ex.Message; }
        finally { btn.Content = label; btn.IsEnabled = true; _busy = false; RefreshSelection(); }
    }

    // ---- pickers ----
    private static void Init(object picker)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Shell);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
    }

    private async void OpenArchive_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        foreach (var ext in new[] { ".7z", ".zip", ".rar", ".tar", ".gz", ".bz2", ".xz", ".cab", ".iso", ".wim", "*" })
            picker.FileTypeFilter.Add(ext);
        Init(picker);
        var f = await picker.PickSingleFileAsync();
        if (f is not null) { AppState.CurrentArchivePath = f.Path; RefreshSelection(); }
    }

    private async void NewArchive_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker { SuggestedFileName = "archive" };
        picker.FileTypeChoices.Add("7-Zip", new List<string> { ".7z" });
        picker.FileTypeChoices.Add("Zip", new List<string> { ".zip" });
        picker.FileTypeChoices.Add("Tar", new List<string> { ".tar" });
        Init(picker);
        var f = await picker.PickSaveFileAsync();
        if (f is not null) { AppState.CurrentArchivePath = f.Path; RefreshSelection(); }
    }

    private async void SourceFile_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add("*");
        Init(picker);
        var f = await picker.PickSingleFileAsync();
        if (f is not null) { AppState.CurrentSourcePath = f.Path; RefreshSelection(); }
    }

    private async void SourceFolder_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");
        Init(picker);
        var f = await picker.PickSingleFolderAsync();
        if (f is not null) { AppState.CurrentSourcePath = f.Path; RefreshSelection(); }
    }

    private async void Create_Click(object sender, RoutedEventArgs e)
    {
        var fmt = (FormatBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "7z";
        var level = int.Parse((LevelBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "5");
        var pwd = PasswordBox.Password;
        await RunAndShow(CreateBtn, () => ArchiveService.Create(fmt, level, string.IsNullOrEmpty(pwd) ? null : pwd));
    }

    private void OpsFilter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            PopulateOps(sender.Text ?? string.Empty);
    }

    private void PopulateOps(string filter)
    {
        _ops ??= ArchiveOperations.All().ToList();
        OpsPanel.Children.Clear();
        IEnumerable<TweakDefinition> shown = _ops;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            shown = _ops.Where(t => t.SearchHaystack.Contains(f));
        }
        foreach (var op in shown)
        {
            var card = new TweakCard();
            card.SetTweak(op);
            OpsPanel.Children.Add(card);
        }
    }
}
