using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>
/// 包住 7-Zip (7z.exe) · A thin wrapper over the 7-Zip command line, scoped to the
/// archive/source the user selected in the Archives module.
/// </summary>
public static class ArchiveService
{
    private static string? _exe;

    /// <summary>7z.exe 嘅完整路徑 · Resolved full path to 7z.exe (or "7z.exe" on PATH).</summary>
    public static string SevenZip
    {
        get
        {
            if (_exe is not null) return _exe;
            var fromReg = RegistryHelper.GetValue(RegRoot.HKLM, @"SOFTWARE\7-Zip", "Path")?.ToString();
            foreach (var c in new[]
                     {
                         fromReg is null ? null : Path.Combine(fromReg, "7z.exe"),
                         @"C:\Program Files\7-Zip\7z.exe",
                         @"C:\Program Files (x86)\7-Zip\7z.exe",
                     })
            {
                if (!string.IsNullOrEmpty(c) && File.Exists(c)) { _exe = c; return _exe; }
            }
            _exe = "7z.exe"; // fall back to PATH
            return _exe;
        }
    }

    public static bool IsInstalled =>
        File.Exists(@"C:\Program Files\7-Zip\7z.exe") ||
        File.Exists(@"C:\Program Files (x86)\7-Zip\7z.exe") ||
        RegistryHelper.KeyExists(RegRoot.HKLM, @"SOFTWARE\7-Zip");

    public static string Archive => AppState.CurrentArchivePath;
    public static string Source => AppState.CurrentSourcePath;
    public static bool HasArchive => !string.IsNullOrWhiteSpace(Archive);
    public static bool HasSource => !string.IsNullOrWhiteSpace(Source);

    private static string OutDir()
    {
        try
        {
            var dir = Path.GetDirectoryName(Archive) ?? Directory.GetCurrentDirectory();
            var name = Path.GetFileNameWithoutExtension(Archive);
            return Path.Combine(dir, name + "_extracted");
        }
        catch { return Path.Combine(Path.GetTempPath(), "wintune_extracted"); }
    }

    /// <summary>
    /// 執行 7z，將 {archive}/{src}/{outdir} 換成實際路徑。
    /// Run 7z with {archive}/{src}/{outdir} substituted (and quoted) at runtime.
    /// </summary>
    public static Task<TweakResult> RunArgs(string args, bool needsArchive = true, bool needsSource = false,
        CancellationToken ct = default)
    {
        if (needsArchive && !HasArchive)
            return Task.FromResult(TweakResult.Fail("No archive selected.", "未揀壓縮檔。"));
        if (needsSource && !HasSource)
            return Task.FromResult(TweakResult.Fail("No source file/folder selected.", "未揀來源檔案／資料夾。"));

        var resolved = args
            .Replace("{archive}", Quote(Archive))
            .Replace("{src}", Quote(Source))
            .Replace("{outdir}", Quote(OutDir()));

        var workDir = HasArchive
            ? Path.GetDirectoryName(Archive)
            : (HasSource ? Path.GetDirectoryName(Source) : null);

        return ShellRunner.RunIn(workDir, SevenZip, resolved, elevated: false, ct);
    }

    private static string Quote(string p) => string.IsNullOrEmpty(p) ? "" : $"\"{p.TrimEnd('\\')}\"";

    // ---- convenience for the bespoke page ----
    public static Task<TweakResult> List(CancellationToken ct = default) => RunArgs("l {archive}", true, false, ct);

    public static Task<TweakResult> Test(CancellationToken ct = default) => RunArgs("t {archive}", true, false, ct);

    public static Task<TweakResult> Benchmark(CancellationToken ct = default) => RunArgs("b", false, false, ct);

    public static Task<TweakResult> ExtractHere(CancellationToken ct = default)
        => RunArgs("x {archive} -o{outdir} -y", true, false, ct);

    public static Task<TweakResult> Create(string format, int level, string? password, CancellationToken ct = default)
    {
        var pw = string.IsNullOrEmpty(password) ? "" : $" -p\"{password}\" -mhe=on";
        return RunArgs($"a -t{format} -mx={level}{pw} {{archive}} {{src}}", true, true, ct);
    }
}
