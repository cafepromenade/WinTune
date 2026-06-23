using System;

namespace WinTune.Services;

/// <summary>
/// 全域應用程式狀態 · Cross-cutting app state shared between suite modules.
/// </summary>
public static class AppState
{
    private static string _currentRepoPath = string.Empty;

    /// <summary>Git 模組目前選中嘅儲存庫資料夾 · The repo folder the Git module is operating on.</summary>
    public static string CurrentRepoPath
    {
        get => _currentRepoPath;
        set
        {
            if (_currentRepoPath == value) return;
            _currentRepoPath = value ?? string.Empty;
            SettingsStore.Set("git.repo", _currentRepoPath);
            RepoChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static event EventHandler? RepoChanged;

    static AppState()
    {
        _currentRepoPath = SettingsStore.Get("git.repo", string.Empty);
    }
}
