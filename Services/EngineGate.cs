using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace WinTune.Services;

/// <summary>
/// 引擎一鍵安裝閘 · Reusable one-click engine gate. Any module that wraps an external engine
/// (ffmpeg / 7-Zip / Docker / VeraCrypt / scrcpy …) can call <see cref="Show"/> to turn a passive
/// "X not found" warning into a real one-click auto-install via winget — no redirect, no restart.
/// </summary>
public static class EngineGate
{
    private static string P(string en, string zh) => Loc.I.Pick(en, zh);

    /// <summary>
    /// 喺 InfoBar 顯示引擎狀態 · Render an engine-status InfoBar. When the engine is missing, attaches a
    /// one-click install button that runs winget and then re-checks. Returns immediately; the optional
    /// <paramref name="onInstalled"/> callback fires after a successful install so the caller can refresh.
    /// </summary>
    /// <param name="bar">The InfoBar to drive.</param>
    /// <param name="installed">Whether the engine is currently present.</param>
    /// <param name="engineNameEn">Engine display name (English), e.g. "ffmpeg".</param>
    /// <param name="engineNameZh">Engine display name (Cantonese), e.g. "ffmpeg".</param>
    /// <param name="wingetId">Exact winget id, e.g. "Gyan.FFmpeg".</param>
    /// <param name="recheck">Async predicate re-evaluated after install to confirm the engine is now present.</param>
    /// <param name="onInstalled">Optional callback invoked after a confirmed successful install.</param>
    public static void Show(InfoBar bar, bool installed, string engineNameEn, string engineNameZh,
        string wingetId, Func<Task<bool>>? recheck = null, Func<Task>? onInstalled = null)
    {
        if (installed)
        {
            bar.IsOpen = false;
            bar.ActionButton = null;
            return;
        }

        bar.IsOpen = true;
        bar.Severity = InfoBarSeverity.Warning;
        bar.Title = P($"{engineNameEn} not found", $"搵唔到 {engineNameZh}");
        bar.Message = P($"Click to install {engineNameEn} automatically (winget · {wingetId}) — no restart needed.",
            $"撳一下自動安裝 {engineNameZh}（winget · {wingetId}）— 唔使重啟。");

        var btn = new Button { Content = P($"Install {engineNameEn} automatically", $"自動安裝 {engineNameZh}") };
        btn.Click += async (_, _) =>
        {
            btn.IsEnabled = false;
            btn.Content = P("Installing…", "安裝緊…");
            bool ok = await PackageService.AutoInstall(wingetId);

            // Confirm with the caller's own probe when available (PATH/registry may lag winget's exit code).
            bool present = ok;
            if (recheck is not null)
            {
                try { present = await recheck(); } catch { present = ok; }
            }

            if (present)
            {
                bar.IsOpen = false;
                bar.ActionButton = null;
                if (onInstalled is not null)
                {
                    try { await onInstalled(); } catch { /* caller's refresh is best-effort */ }
                }
            }
            else
            {
                bar.Severity = InfoBarSeverity.Error;
                bar.Title = P("Install failed", "安裝失敗");
                bar.Message = P($"Could not install {engineNameEn} automatically. Check your connection and try again, or install '{wingetId}' from the Package Manager module.",
                    $"自動安裝 {engineNameZh} 失敗。請檢查網絡再試，或者喺套件管理模組安裝「{wingetId}」。");
                btn.IsEnabled = true;
                btn.Content = P("Retry", "再試");
            }
        };
        bar.ActionButton = btn;
    }
}
