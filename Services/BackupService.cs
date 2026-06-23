using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace WinTune.Services;

/// <summary>一次匯出嘅結果 · Result of one full export.</summary>
public sealed record BackupResult(bool Success, string Message, string MessageZh, string? Path = null);

/// <summary>
/// 全量備份／還原 · Full export/import of EVERYTHING WinTune owns, into a single portable .zip.
///
/// 內容 · Contents — the whole %LocalAppData%\WinTune data tree (settings.json, the clipboard history
/// and its local git repo, custom-program list, package list, applied-tweak state — anything WinTune
/// persists there) plus a fresh winget package snapshot, so a new machine can be rebuilt 1:1.
/// No redirect; everything runs in-app.
/// </summary>
public static class BackupService
{
    /// <summary>WinTune 嘅資料根目錄 · The single folder where WinTune persists all of its state.</summary>
    public static string DataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinTune");

    /// <summary>備份檔嘅建議檔名 · Suggested default file name for a backup.</summary>
    public static string SuggestedName => $"WinTune-backup-{DateTime.Now:yyyyMMdd-HHmmss}";

    /// <summary>備份檔副檔名 · Backup file extension.</summary>
    public const string Extension = ".wintune.zip";

    /// <summary>
    /// 匯出所有嘢去一個 zip · Export everything to a single zip at <paramref name="targetZipPath"/>.
    /// Captures the WinTune data folder verbatim and adds a live winget package list snapshot.
    /// </summary>
    public static async Task<BackupResult> ExportAsync(string targetZipPath, CancellationToken ct = default)
    {
        try
        {
            // Flush in-memory settings to disk first so the export is current.
            try { SettingsStore.Set("backup.lastExport", DateTime.UtcNow.ToString("o")); } catch { }

            Directory.CreateDirectory(DataDir);

            // Snapshot the installed winget packages into the data folder so it travels inside the zip.
            try
            {
                var pkgPath = Path.Combine(DataDir, "winget-packages.json");
                var export = await ShellRunner.CapturePowershell(
                    "winget export -o \"" + pkgPath.Replace("\"", "") + "\" --include-versions --accept-source-agreements --disable-interactivity 2>&1 | Out-String", ct);
                // If winget produced nothing usable, leave any prior snapshot in place rather than a broken file.
                if (!File.Exists(pkgPath)) { /* best effort — continue without it */ }
            }
            catch { /* winget snapshot is best-effort */ }

            if (File.Exists(targetZipPath))
            {
                try { File.Delete(targetZipPath); } catch { }
            }

            // Copy the data dir to a temp staging folder so an open file (e.g. live git index) can't abort the zip.
            var staging = Path.Combine(Path.GetTempPath(), "WinTuneBackup-" + Guid.NewGuid().ToString("N"));
            try
            {
                CopyDirectory(DataDir, staging, ct);
                ZipFile.CreateFromDirectory(staging, targetZipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            finally
            {
                try { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); } catch { }
            }

            return new BackupResult(true,
                $"Exported everything to {Path.GetFileName(targetZipPath)}.",
                $"已將所有嘢匯出到 {Path.GetFileName(targetZipPath)}。",
                targetZipPath);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new BackupResult(false, $"Export failed: {ex.Message}", $"匯出失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 由 zip 還原所有嘢 · Restore everything from a backup zip. The current data folder is moved aside to
    /// a timestamped *.bak first so nothing is lost, then the zip's contents are written into the data dir.
    /// Settings are reloaded in-process. winget packages are NOT auto-reinstalled — call
    /// <see cref="ReinstallPackagesAsync"/> after import if the user opts in.
    /// </summary>
    public static BackupResult Import(string sourceZipPath)
    {
        try
        {
            if (!File.Exists(sourceZipPath))
                return new BackupResult(false, "Backup file not found.", "搵唔到備份檔。");

            Directory.CreateDirectory(DataDir);

            // Move the current data aside (don't destroy it) so import is reversible.
            var backupAside = DataDir + ".bak-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            try
            {
                if (Directory.Exists(DataDir))
                {
                    Directory.Move(DataDir, backupAside);
                    Directory.CreateDirectory(DataDir);
                }
            }
            catch { /* if the move fails (locked file), fall through and overwrite in place */ }

            ZipFile.ExtractToDirectory(sourceZipPath, DataDir, overwriteFiles: true);

            // Reload settings from the freshly-restored file so the running app reflects the import.
            SettingsStore.Reload();

            return new BackupResult(true,
                "Restored everything. Some changes apply after a restart.",
                "已還原所有嘢。部分變更要重啟先生效。",
                DataDir);
        }
        catch (Exception ex)
        {
            return new BackupResult(false, $"Import failed: {ex.Message}", $"匯入失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 由備份還原 winget 套件 · Reinstall the winget package set captured inside a previously-imported
    /// backup (winget-packages.json). Runs the real winget import; long-running.
    /// </summary>
    public static async Task<BackupResult> ReinstallPackagesAsync(CancellationToken ct = default)
    {
        try
        {
            var pkgPath = Path.Combine(DataDir, "winget-packages.json");
            if (!File.Exists(pkgPath))
                return new BackupResult(false, "No package list in this backup.", "呢個備份冇套件清單。");

            var r = await ShellRunner.RunCmd(
                $"winget import -i \"{pkgPath}\" --accept-source-agreements --accept-package-agreements --ignore-versions --ignore-unavailable",
                false, ct);
            return new BackupResult(r.Success,
                r.Success ? "Reinstalled packages from the backup." : "Some packages could not be reinstalled.",
                r.Success ? "已由備份重裝套件。" : "部分套件重裝唔到。");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new BackupResult(false, $"Package reinstall failed: {ex.Message}", $"套件重裝失敗：{ex.Message}");
        }
    }

    private static void CopyDirectory(string src, string dst, CancellationToken ct)
    {
        Directory.CreateDirectory(dst);
        foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            Directory.CreateDirectory(dir.Replace(src, dst));
        }
        foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            try { File.Copy(file, file.Replace(src, dst), overwrite: true); }
            catch { /* skip files locked by another process (best effort) */ }
        }
    }
}
