using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinTune.Catalog;
using WinTune.Models;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 設定：語言、佈景主題、管理員、關於。
/// Settings: language, theme, administrator and about.
/// </summary>
public sealed partial class SettingsPage : Page
{
    private bool _suppress;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Build();
        Loc.I.LanguageChanged += OnLang;
        Unloaded += (_, _) => Loc.I.LanguageChanged -= OnLang;
    }

    private void OnLang(object? sender, EventArgs e) => Build();

    private void Build()
    {
        Root.Children.Clear();

        Root.Children.Add(new TextBlock
        {
            Text = "Settings · 設定",
            Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
        });

        Root.Children.Add(BuildLanguageCard());
        Root.Children.Add(BuildThemeCard());
        Root.Children.Add(BuildBackupCard());
        Root.Children.Add(BuildFullBackupCard());
        Root.Children.Add(BuildAdminCard());
        Root.Children.Add(BuildAboutCard());
    }

    private Border BuildFullBackupCard()
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(Heading(
            Loc.I.Pick("Full backup — export / import everything", "完整備份 — 匯出／匯入所有嘢"),
            Loc.I.Pick("One portable .zip with EVERYTHING: settings, applied tweaks & recipes, the package list, custom programs and the clipboard history (with its git repo). Rebuild a new PC 1:1.",
                "一個可攜 .zip 包晒所有嘢：設定、已套用嘅調校同流程、套件清單、自訂程式，同剪貼簿歷史（連 git repo）。新機可以 1:1 重建。")));

        var bar = new InfoBar { IsClosable = true, IsOpen = false };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };

        var export = new Button { Content = Loc.I.Pick("Export everything…", "匯出所有嘢…") };
        export.Click += async (_, _) =>
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileSavePicker { SuggestedFileName = BackupService.SuggestedName };
                picker.FileTypeChoices.Add(Loc.I.Pick("WinTune backup", "WinTune 備份"), new System.Collections.Generic.List<string> { ".zip" });
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Shell));
                var f = await picker.PickSaveFileAsync();
                if (f is null) return;
                Show(bar, InfoBarSeverity.Informational, Loc.I.Pick("Exporting…", "匯出緊…"),
                    Loc.I.Pick("Snapshotting settings, clipboard repo and the package list.", "正喺度影低設定、剪貼簿 repo 同套件清單。"));
                export.IsEnabled = false;
                var r = await BackupService.ExportAsync(f.Path);
                export.IsEnabled = true;
                Show(bar, r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error,
                    r.Success ? Loc.I.Pick("Exported.", "已匯出。") : Loc.I.Pick("Export failed", "匯出失敗"),
                    Loc.I.Pick(r.Message, r.MessageZh));
            }
            catch (Exception ex) { export.IsEnabled = true; Show(bar, InfoBarSeverity.Error, Loc.I.Pick("Export failed", "匯出失敗"), ex.Message); }
        };

        var import = new Button { Content = Loc.I.Pick("Import everything…", "匯入所有嘢…") };
        import.Click += async (_, _) =>
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.FileTypeFilter.Add(".zip");
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Shell));
                var f = await picker.PickSingleFileAsync();
                if (f is null) return;

                var confirm = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = Loc.I.Pick("Restore from backup?", "由備份還原？"),
                    Content = Loc.I.Pick("This replaces WinTune's current data (settings, clipboard history, lists) with the backup. Your current data is moved aside to a .bak folder first, so nothing is lost.",
                        "呢個會用備份取代 WinTune 而家嘅資料（設定、剪貼簿歷史、清單）。會先將而家嘅資料搬去一個 .bak 資料夾，唔會無咗。"),
                    PrimaryButtonText = Loc.I.Pick("Restore", "還原"),
                    CloseButtonText = Loc.I.Pick("Cancel", "取消"),
                    DefaultButton = ContentDialogButton.Close,
                };
                if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

                var r = BackupService.Import(f.Path);
                if (!r.Success)
                {
                    Show(bar, InfoBarSeverity.Error, Loc.I.Pick("Import failed", "匯入失敗"), Loc.I.Pick(r.Message, r.MessageZh));
                    return;
                }
                App.ApplyThemeFromSettings();
                Build(); // reflect restored language/theme
                Show(bar, InfoBarSeverity.Success, Loc.I.Pick("Restored.", "已還原。"), Loc.I.Pick(r.Message, r.MessageZh));

                // Offer to reinstall the captured package set.
                var pkgDlg = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = Loc.I.Pick("Reinstall apps too?", "順埋重裝應用程式？"),
                    Content = Loc.I.Pick("The backup includes a winget package list. Reinstall those apps now via winget? This can take a while.",
                        "備份內有 winget 套件清單。而家用 winget 重裝嗰啲應用程式？可能要一陣。"),
                    PrimaryButtonText = Loc.I.Pick("Reinstall apps", "重裝應用程式"),
                    CloseButtonText = Loc.I.Pick("Not now", "暫時唔使"),
                    DefaultButton = ContentDialogButton.Close,
                };
                if (await pkgDlg.ShowAsync() == ContentDialogResult.Primary)
                {
                    Show(bar, InfoBarSeverity.Informational, Loc.I.Pick("Reinstalling apps…", "重裝應用程式緊…"),
                        Loc.I.Pick("Running winget import — this may take several minutes.", "行緊 winget import — 可能要幾分鐘。"));
                    var pr = await BackupService.ReinstallPackagesAsync();
                    Show(bar, pr.Success ? InfoBarSeverity.Success : InfoBarSeverity.Warning,
                        pr.Success ? Loc.I.Pick("Apps reinstalled.", "應用程式已重裝。") : Loc.I.Pick("Some apps skipped", "部分應用程式略過咗"),
                        Loc.I.Pick(pr.Message, pr.MessageZh));
                }
            }
            catch (Exception ex) { Show(bar, InfoBarSeverity.Error, Loc.I.Pick("Import failed", "匯入失敗"), ex.Message); }
        };

        row.Children.Add(export);
        row.Children.Add(import);
        panel.Children.Add(row);
        panel.Children.Add(bar);
        return Card(panel);
    }

    private Border BuildBackupCard()
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(Heading(
            Loc.I.Pick("Import / export settings", "匯入／匯出設定"),
            Loc.I.Pick("Save WinTune's settings to a file, or load them back.", "將 WinTune 嘅設定存做檔案，或者載返入嚟。")));

        var bar = new InfoBar { IsClosable = true, IsOpen = false };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };

        var export = new Button { Content = Loc.I.Pick("Export…", "匯出…") };
        export.Click += async (_, _) =>
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileSavePicker { SuggestedFileName = "wintune-settings" };
                picker.FileTypeChoices.Add("JSON", new System.Collections.Generic.List<string> { ".json" });
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Shell));
                var f = await picker.PickSaveFileAsync();
                if (f is not null)
                {
                    SettingsStore.ExportTo(f.Path);
                    Show(bar, InfoBarSeverity.Success, Loc.I.Pick("Exported.", "已匯出。"), f.Path);
                }
            }
            catch (Exception ex) { Show(bar, InfoBarSeverity.Error, Loc.I.Pick("Export failed", "匯出失敗"), ex.Message); }
        };

        var import = new Button { Content = Loc.I.Pick("Import…", "匯入…") };
        import.Click += async (_, _) =>
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.FileTypeFilter.Add(".json");
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Shell));
                var f = await picker.PickSingleFileAsync();
                if (f is not null)
                {
                    int n = SettingsStore.ImportFrom(f.Path);
                    App.ApplyThemeFromSettings();
                    Show(bar, InfoBarSeverity.Success,
                        Loc.I.Pick($"Imported {n} setting(s).", $"已匯入 {n} 項設定。"),
                        Loc.I.Pick("Restart WinTune to fully apply.", "重啟 WinTune 完全生效。"));
                }
            }
            catch (Exception ex) { Show(bar, InfoBarSeverity.Error, Loc.I.Pick("Import failed", "匯入失敗"), ex.Message); }
        };

        row.Children.Add(export);
        row.Children.Add(import);
        panel.Children.Add(row);
        panel.Children.Add(bar);
        return Card(panel);
    }

    private static void Show(InfoBar bar, InfoBarSeverity sev, string title, string msg)
    {
        bar.Severity = sev; bar.Title = title; bar.Message = msg; bar.IsOpen = true;
    }

    private Border BuildLanguageCard()
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(Heading(
            Loc.I.Pick("Primary language", "主要語言"),
            "界面永遠雙語顯示；呢度只係揀邊個排前面。Both languages always show; this picks which leads."));

        _suppress = true;
        var radios = new RadioButtons();
        radios.Items.Add("English");
        radios.Items.Add("粵語 (Cantonese)");
        radios.SelectedIndex = Loc.I.Language == AppLanguage.English ? 0 : 1;
        radios.SelectionChanged += (_, _) =>
        {
            if (_suppress) return;
            Loc.I.Language = radios.SelectedIndex == 0 ? AppLanguage.English : AppLanguage.Cantonese;
        };
        _suppress = false;
        panel.Children.Add(radios);
        return Card(panel);
    }

    private Border BuildThemeCard()
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(Heading(
            Loc.I.Pick("App theme", "應用程式主題"),
            Loc.I.Pick("Light, dark or follow Windows.", "淺色、深色或者跟 Windows。")));

        var current = SettingsStore.Get("theme", "Default");
        var radios = new RadioButtons();
        radios.Items.Add(Loc.I.Pick("Use system setting", "跟系統設定"));
        radios.Items.Add(Loc.I.Pick("Light", "淺色"));
        radios.Items.Add(Loc.I.Pick("Dark", "深色"));
        radios.SelectedIndex = current switch { "Light" => 1, "Dark" => 2, _ => 0 };
        radios.SelectionChanged += (_, _) =>
        {
            var (key, theme) = radios.SelectedIndex switch
            {
                1 => ("Light", ElementTheme.Light),
                2 => ("Dark", ElementTheme.Dark),
                _ => ("Default", ElementTheme.Default),
            };
            SettingsStore.Set("theme", key);
            App.SetTheme(theme);
        };
        panel.Children.Add(radios);
        return Card(panel);
    }

    private Border BuildAdminCard()
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(Heading(
            Loc.I.Pick("Administrator rights", "管理員權限"),
            Loc.I.Pick("Needed for system-wide tweaks (HKLM, services, power).",
                "全系統調校需要（HKLM、服務、電源）。")));

        if (AdminHelper.IsElevated)
        {
            panel.Children.Add(new TextBlock
            {
                Text = Loc.I.Pick("✓ Running as administrator.", "✓ 正以管理員身分運行。"),
                Foreground = (Brush)Application.Current.Resources["SystemFillColorSuccessBrush"],
            });
        }
        else
        {
            var b = new Button { Content = "Relaunch as administrator · 以管理員身分重新啟動" };
            b.Click += (_, _) =>
            {
                if (AdminHelper.RelaunchElevated())
                    Application.Current.Exit();
            };
            panel.Children.Add(b);
        }
        return Card(panel);
    }

    private Border BuildAboutCard()
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(Heading("WinTune · 視窗調校", null));
        panel.Children.Add(Muted(Loc.I.Pick(
            $"{TweakCatalog.Count} bilingual features for Windows 11.",
            $"{TweakCatalog.Count} 項 Windows 11 雙語功能。")));
        panel.Children.Add(Muted("Version 1.0.0"));
        panel.Children.Add(Muted(Loc.I.Pick(
            "Always review what a tweak does before applying it.",
            "套用之前，請睇清楚每項調校做乜。")));
        return Card(panel);
    }

    // ---- small builders ----
    private static StackPanel Heading(string title, string? subtitle)
    {
        var p = new StackPanel { Spacing = 1 };
        p.Children.Add(new TextBlock { Text = title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 15 });
        if (!string.IsNullOrEmpty(subtitle))
            p.Children.Add(Muted(subtitle));
        return p;
    }

    private static TextBlock Muted(string text) => new()
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        FontSize = 12,
        Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
    };

    private static Border Card(UIElement content) => new()
    {
        Padding = new Thickness(16, 14, 16, 14),
        Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
        BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(8),
        Child = content,
    };
}
