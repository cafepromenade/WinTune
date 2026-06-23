using System;
using System.Collections.Generic;
using System.Diagnostics;
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
/// Git 與 GitHub 模組 · The Git &amp; GitHub suite module: repo picker, quick actions, commit,
/// the chunked uploader, and 111 advanced operations.
/// </summary>
public sealed partial class GitHubModule : Page
{
    private List<TweakDefinition>? _ops;

    public GitHubModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += OnLang;
        Unloaded += (_, _) => Loc.I.LanguageChanged -= OnLang;
        Loaded += async (_, _) => { Render(); BuildQuickActions(); PopulateOps(string.Empty); await Refresh(); };
    }

    private void OnLang(object? sender, EventArgs e)
    {
        Render();
        BuildQuickActions();
        PopulateOps(OpsFilter.Text ?? string.Empty);
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "Git & GitHub · Git 與 GitHub";
        HeaderBlurb.Text = P(
            "Drive any local repository — stage, commit, branch, sync — plus the chunked uploader and the GitHub CLI.",
            "操作任何本地儲存庫 — 暫存、提交、分支、同步 — 仲有分批上載同 GitHub CLI。");
        RepoLabel.Text = P("Repository", "儲存庫");
        BrowseBtn.Content = P("Browse…", "瀏覽…");
        TerminalBtn.Content = P("Terminal", "終端機");
        CommitLabel.Text = P("Stage all & commit", "暫存全部並提交");
        CommitMessageBox.PlaceholderText = P("Commit message…", "提交訊息…");
        CommitBtn.Content = P("Commit", "提交");
        ChunkLabel.Text = P("Chunked upload (size per commit, push one at a time)",
            "分批上載（每個 commit 大細，逐個 push）");
        ChunkBlurb.Text = P(
            "Splits everything that needs uploading into commits no larger than the chosen size (MB), then pushes them one commit at a time.",
            "將所有要上載嘅嘢切成唔超過指定大細（MB）嘅 commit，然後逐個 commit push 上去。");
        ChunkMessageBox.PlaceholderText = P("Commit message prefix…", "提交訊息前綴…");
        ChunkUploadBtn.Content = P("Chunk & push", "分批推送");
        OpsFilter.PlaceholderText = P("Filter operations…", "篩選操作…");
        AdvancedHeader.Text = P($"Advanced operations ({GitOperations.All().Count()})",
            $"進階操作（{GitOperations.All().Count()}）");
    }

    private async Task Refresh()
    {
        RepoPathBox.Text = AppState.CurrentRepoPath;
        if (!GitService.HasRepo)
        {
            RepoStatus.Text = P("No folder selected — Browse to pick a repository.",
                "未揀資料夾 — 撳「瀏覽」揀一個儲存庫。");
            return;
        }
        RepoStatus.Text = P("Checking…", "檢查緊…");
        bool isRepo = await GitService.IsGitRepo();
        if (!isRepo)
        {
            RepoStatus.Text = P("This folder is not a git repository. Use the “git init” quick action, or pick another.",
                "呢個資料夾唔係 git 儲存庫。可以用「git init」快捷鍵，或者揀第二個。");
            return;
        }
        var branch = await GitService.CurrentBranch();
        var pending = await GitService.PendingFiles();
        long totalBytes = pending.Sum(p => p.size);
        RepoStatus.Text = P(
            $"Branch: {branch}  ·  {pending.Count} pending change(s)  ·  {Mb(totalBytes)} MB",
            $"分支：{branch}  ·  {pending.Count} 項待處理改動  ·  {Mb(totalBytes)} MB");
    }

    private static string Mb(long bytes) => Math.Round(bytes / 1024.0 / 1024.0, 1).ToString("0.0");

    private void BuildQuickActions()
    {
        QuickActions.Children.Clear();
        AddQuick("", P("Refresh", "重新整理"), () => Task.FromResult(TweakResult.Ok("", "")));
        AddQuick("", P("git init", "git init"), () => GitService.RunRaw("init"));
        AddQuick("", P("Stage all", "暫存全部"), () => GitService.RunRaw("add -A"));
        AddQuick("", P("Pull", "拉取"), () => GitService.RunRaw("pull"));
        AddQuick("", P("Fetch", "抓取"), () => GitService.RunRaw("fetch --all --prune"));
        AddQuick("", P("Push", "推送"), () => GitService.RunRaw("push"));
    }

    private void AddQuick(string glyph, string label, Func<Task<TweakResult>> run)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        content.Children.Add(new FontIcon { Glyph = glyph, FontSize = 14 });
        content.Children.Add(new TextBlock { Text = label });
        var btn = new Button { Content = content };
        btn.Click += async (_, _) =>
        {
            btn.IsEnabled = false;
            try
            {
                var r = await run();
                if (!string.IsNullOrWhiteSpace(r.Output))
                    RepoStatus.Text = r.Output!.Length > 400 ? r.Output[..400] + " …" : r.Output;
                await Refresh();
            }
            finally { btn.IsEnabled = true; }
        };
        QuickActions.Children.Add(btn);
    }

    private async void Browse_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Shell);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var folder = await picker.PickSingleFolderAsync();
            if (folder is not null)
            {
                AppState.CurrentRepoPath = folder.Path;
                await Refresh();
            }
        }
        catch (Exception ex)
        {
            RepoStatus.Text = ex.Message;
        }
    }

    private void Terminal_Click(object sender, RoutedEventArgs e)
    {
        if (!GitService.HasRepo) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = "wt.exe", Arguments = $"-d \"{GitService.Repo}\"", UseShellExecute = true });
        }
        catch
        {
            try { Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = $"/K cd /d \"{GitService.Repo}\"", UseShellExecute = true }); }
            catch { /* ignore */ }
        }
    }

    private async void Commit_Click(object sender, RoutedEventArgs e)
    {
        var msg = CommitMessageBox.Text?.Trim();
        if (string.IsNullOrEmpty(msg))
        {
            RepoStatus.Text = P("Enter a commit message first.", "請先輸入提交訊息。");
            return;
        }
        CommitBtn.IsEnabled = false;
        try
        {
            await GitService.RunRaw("add -A");
            var r = await GitService.RunRaw($"commit -m \"{msg.Replace("\"", "'")}\"");
            RepoStatus.Text = (r.Output ?? string.Empty).Trim();
            if (r.Success) CommitMessageBox.Text = string.Empty;
            await Refresh();
        }
        finally { CommitBtn.IsEnabled = true; }
    }

    private async void ChunkUpload_Click(object sender, RoutedEventArgs e)
    {
        if (!GitService.HasRepo)
        {
            RepoStatus.Text = P("Pick a repository first.", "請先揀儲存庫。");
            return;
        }
        long maxBytes = (long)(Math.Max(1, ChunkSizeBox.Value) * 1024 * 1024);
        var message = string.IsNullOrWhiteSpace(ChunkMessageBox.Text) ? "WinTune chunked upload" : ChunkMessageBox.Text.Trim();

        ChunkLogBorder.Visibility = Visibility.Visible;
        ChunkLog.Text = string.Empty;
        ChunkUploadBtn.IsEnabled = false;
        var label = ChunkUploadBtn.Content;
        ChunkUploadBtn.Content = new ProgressRing { IsActive = true, Width = 18, Height = 18 };

        var progress = new Progress<string>(s => ChunkLog.Text += s);
        try
        {
            var r = await GitService.ChunkedUpload(maxBytes, message, progress, CancellationToken.None);
            ChunkLog.Text += "\n" + (Loc.I.IsCantonesePrimary ? r.Message?.Zh : r.Message?.En);
            await Refresh();
        }
        catch (Exception ex)
        {
            ChunkLog.Text += "\n" + ex.Message;
        }
        finally
        {
            ChunkUploadBtn.Content = label;
            ChunkUploadBtn.IsEnabled = true;
        }
    }

    private void OpsFilter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            PopulateOps(sender.Text ?? string.Empty);
    }

    private void PopulateOps(string filter)
    {
        _ops ??= GitOperations.All().ToList();
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
