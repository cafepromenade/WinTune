using Microsoft.UI.Xaml;
using WinTune.Services;

namespace WinTune;

/// <summary>
/// 應用程式進入點 · Application entry point and global theme handling.
/// </summary>
public partial class App : Application
{
    public static Window? Shell { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Shell = new MainWindow();
        ApplyThemeFromSettings();
        Shell.Activate();
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
