using System;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>
/// 由 MainWindow 設定嘅導覽鈎子 · Navigation hooks wired up by MainWindow,
/// so pages (e.g. the dashboard tiles) can drive the NavigationView.
/// </summary>
public static class Navigator
{
    public static Action<AppCategory>? GoToCategory { get; set; }
    public static Action? GoToSettings { get; set; }
}
