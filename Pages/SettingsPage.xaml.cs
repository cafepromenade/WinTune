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
        Root.Children.Add(BuildAdminCard());
        Root.Children.Add(BuildAboutCard());
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
