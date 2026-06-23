using System;
using Microsoft.UI.Xaml;
using WinTune.Services;

namespace WinTune;

/// <summary>
/// 應用程式進入點 · Application entry point and global theme handling.
/// </summary>
public partial class App : Application
{
    public static Window? Shell { get; private set; }

    /// <summary>由命令列 "--page &lt;id&gt;" 設定嘅起始頁 · Start page from the command line.</summary>
    public static string? StartPage { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    private static string? _exportDocsDir;

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        ParseArgs();

        // 無頭模式：匯出每個功能嘅 Markdown 然後退出 · headless docs export then exit.
        if (_exportDocsDir is not null)
        {
            try
            {
                int n = WinTune.Services.DocsExporter.Export(_exportDocsDir);
                System.IO.File.WriteAllText(System.IO.Path.Combine(_exportDocsDir, "_export_count.txt"), n.ToString());
            }
            catch { /* best effort */ }
            Exit();
            return;
        }

        Shell = new MainWindow();
        ApplyThemeFromSettings();
        Shell.Activate();
    }

    private static void ParseArgs()
    {
        var argv = Environment.GetCommandLineArgs();
        for (int i = 1; i < argv.Length - 1; i++)
        {
            if (string.Equals(argv[i], "--page", StringComparison.OrdinalIgnoreCase))
                StartPage = argv[i + 1].Trim().ToLowerInvariant();
            else if (string.Equals(argv[i], "--export-docs", StringComparison.OrdinalIgnoreCase))
                _exportDocsDir = argv[i + 1];
        }
    }

    /// <summary>套用使用者揀選嘅佈景主題 · Apply the user's saved theme to the window root.</summary>
    public static void ApplyThemeFromSettings()
    {
        var theme = SettingsStore.Get("theme", "Default");
        SetTheme(theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        });
    }

    public static void SetTheme(ElementTheme theme)
    {
        if (Shell?.Content is FrameworkElement root)
            root.RequestedTheme = theme;
    }
}
