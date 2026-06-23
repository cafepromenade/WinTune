using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Catalog;
using WinTune.Controls;
using WinTune.Models;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 媒體模組 · In-app Media module: wraps ffmpeg/ffprobe for convert/extract/trim/gif plus ~60 ops.
/// </summary>
public sealed partial class MediaModule : Page
{
    private List<TweakDefinition>? _ops;
    private bool _busy;

    public MediaModule()
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
        HeaderTitle.Text = "Media · 媒體";
        HeaderBlurb.Text = P("Convert, trim, extract audio, make GIFs and inspect video/audio with ffmpeg — all in-app.",
            "用 ffmpeg 轉檔、剪裁、抽聲、整 GIF、檢視影片／音訊 — 全部喺 app 內。");
        SelLabel.Text = P("Files", "檔案");
        InCap.Text = P("Input", "輸入");
        OutCap.Text = P("Output", "輸出");
        InputBtn.Content = P("Open…", "開啟…");
        OutputBtn.Content = P("Save as…", "另存…");
        OpsFilter.PlaceholderText = P("Filter operations…", "篩選操作…");
        AdvancedHeader.Text = P($"Advanced operations ({(_ops ??= MediaOperations.All().ToList()).Count})",
            $"進階操作（{(_ops ??= MediaOperations.All().ToList()).Count}）");

        EngineGate.Show(EngineBar, MediaService.IsInstalled, "ffmpeg", "ffmpeg", "Gyan.FFmpeg",
            recheck: () => Task.FromResult(MediaService.Rescan()));
    }

    private void RefreshSelection()
    {
        InputBox.Text = AppState.CurrentMediaInput;
        OutputBox.Text = AppState.CurrentMediaOutput;
    }

    private void BuildQuickOps()
    {
        QuickOps.Children.Clear();
        AddQuick(P("To MP4", "轉 MP4"), () => MediaService.Quick(".converted.mp4", "-i {in} -c:v libx264 -c:a aac -movflags +faststart {out}"));
        AddQuick(P("Extract MP3", "抽 MP3"), () => MediaService.Quick(".mp3", "-i {in} -vn -c:a libmp3lame -q:a 2 {out}"));
        AddQuick(P("Make GIF", "整 GIF"), () => MediaService.Quick(".gif", "-i {in} -vf \"fps=12,scale=480:-1:flags=lanczos\" {out}"));
        AddQuick(P("Compress", "壓細"), () => MediaService.Quick(".compressed.mp4", "-i {in} -c:v libx264 -crf 28 -c:a aac {out}"));
        AddQuick(P("Mute", "靜音"), () => MediaService.Quick(".muted.mp4", "-i {in} -c:v copy -an {out}"));
        AddQuick(P("Info", "資訊"), () => MediaService.Info());
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
        OutText.Text = P("Running ffmpeg…", "執行緊 ffmpeg…");
        try
        {
            var r = await run();
            var head = r.Success ? P("✓ Done", "✓ 完成") : P("✗ Failed", "✗ 失敗");
            var body = string.IsNullOrWhiteSpace(r.Output)
                ? ((Loc.I.IsCantonesePrimary ? r.Message?.Zh : r.Message?.En) ?? "")
                : r.Output!;
            OutText.Text = head + "\n" + (body.Length > 4000 ? body[^4000..] : body);
        }
        catch (Exception ex) { OutText.Text = ex.Message; }
        finally { btn.Content = label; btn.IsEnabled = true; _busy = false; RefreshSelection(); }
    }

    private static void Init(object picker)
    {
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Shell));
    }

    private async void PickInput_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        foreach (var ext in new[] { ".mp4", ".mkv", ".mov", ".avi", ".webm", ".m4v", ".mp3", ".wav", ".flac", ".aac", ".m4a", ".ogg", ".opus", "*" })
            picker.FileTypeFilter.Add(ext);
        Init(picker);
        var f = await picker.PickSingleFileAsync();
        if (f is not null) { AppState.CurrentMediaInput = f.Path; RefreshSelection(); }
    }

    private async void PickOutput_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker { SuggestedFileName = "output" };
        picker.FileTypeChoices.Add("MP4", new List<string> { ".mp4" });
        picker.FileTypeChoices.Add("MP3", new List<string> { ".mp3" });
        picker.FileTypeChoices.Add("GIF", new List<string> { ".gif" });
        picker.FileTypeChoices.Add("WAV", new List<string> { ".wav" });
        picker.FileTypeChoices.Add("WebM", new List<string> { ".webm" });
        Init(picker);
        var f = await picker.PickSaveFileAsync();
        if (f is not null) { AppState.CurrentMediaOutput = f.Path; RefreshSelection(); }
    }

    private void OpsFilter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            PopulateOps(sender.Text ?? string.Empty);
    }

    private void PopulateOps(string filter)
    {
        _ops ??= MediaOperations.All().ToList();
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
