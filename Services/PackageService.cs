using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>一個套件搜尋結果 · One winget package row.</summary>
public sealed class PkgResult
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public string Source { get; set; } = "";
}

/// <summary>一個常用相依工具 · One curated common dependency.</summary>
public sealed class DepInfo
{
    public string En { get; init; } = "";
    public string Zh { get; init; } = "";
    public string Id { get; init; } = "";
}

/// <summary>
/// 套件管理（包 winget，UniGetUI 式）· In-app package manager wrapping winget — search, install, uninstall,
/// list upgrades, and one-click install of common dependencies. No redirect (wraps the real winget engine).
/// </summary>
public static class PackageService
{
    /// <summary>winget ships with Windows 11; assume present and surface any real error from the command.</summary>
    public static bool WingetAvailable => true;

    /// <summary>Engines WinTune itself uses, plus common dev tools — exact winget IDs (reliable).</summary>
    public static readonly DepInfo[] Deps =
    {
        new() { En = "FFmpeg (media engine)", Zh = "FFmpeg（媒體引擎）", Id = "Gyan.FFmpeg" },
        new() { En = "7-Zip", Zh = "7-Zip", Id = "7zip.7zip" },
        new() { En = "Git", Zh = "Git", Id = "Git.Git" },
        new() { En = "Android Platform Tools (adb)", Zh = "Android 平台工具（adb）", Id = "Google.PlatformTools" },
        new() { En = "Python 3", Zh = "Python 3", Id = "Python.Python.3.12" },
        new() { En = "Node.js LTS", Zh = "Node.js LTS", Id = "OpenJS.NodeJS.LTS" },
        new() { En = "PowerShell 7", Zh = "PowerShell 7", Id = "Microsoft.PowerShell" },
        new() { En = "Windows Terminal", Zh = "Windows 終端機", Id = "Microsoft.WindowsTerminal" },
        new() { En = "VLC media player", Zh = "VLC 播放器", Id = "VideoLAN.VLC" },
        new() { En = "Notepad++", Zh = "Notepad++", Id = "Notepad++.Notepad++" },
    };

    public static async Task<List<PkgResult>> Search(string query, CancellationToken ct = default)
    {
        var outp = await ShellRunner.CapturePowershell(
            $"winget search --query \"{query.Replace("\"", "")}\" --accept-source-agreements --disable-interactivity | Out-String -Width 400", ct);
        return ParseTable(outp);
    }

    public static async Task<List<PkgResult>> Upgradable(CancellationToken ct = default)
    {
        var outp = await ShellRunner.CapturePowershell(
            "winget upgrade --accept-source-agreements --disable-interactivity | Out-String -Width 400", ct);
        return ParseTable(outp);
    }

    /// <summary>All installed package ids in one winget call (fast bulk check).</summary>
    public static async Task<HashSet<string>> InstalledIds(CancellationToken ct = default)
    {
        var outp = await ShellRunner.CapturePowershell(
            "winget list --accept-source-agreements --disable-interactivity | Out-String -Width 400", ct);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in ParseTable(outp)) set.Add(r.Id);
        return set;
    }

    public static async Task<bool> IsInstalled(string id, CancellationToken ct = default)
    {
        var outp = await ShellRunner.CapturePowershell(
            $"winget list --id {id} -e --accept-source-agreements --disable-interactivity | Out-String -Width 400", ct);
        return outp.Contains(id, StringComparison.OrdinalIgnoreCase);
    }

    public static Task<TweakResult> Install(string id, CancellationToken ct = default)
        => ShellRunner.RunCmd($"winget install --id {id} -e --silent --accept-source-agreements --accept-package-agreements --disable-interactivity", false, ct);

    public static Task<TweakResult> Uninstall(string id, CancellationToken ct = default)
        => ShellRunner.RunCmd($"winget uninstall --id {id} -e --silent --disable-interactivity", false, ct);

    public static Task<TweakResult> Upgrade(string id, CancellationToken ct = default)
        => ShellRunner.RunCmd($"winget upgrade --id {id} -e --silent --accept-source-agreements --accept-package-agreements --disable-interactivity", false, ct);

    /// <summary>Parse winget's column table using the header's column start positions (best-effort for ASCII names).</summary>
    private static List<PkgResult> ParseTable(string outp)
    {
        var res = new List<PkgResult>();
        if (string.IsNullOrWhiteSpace(outp)) return res;
        var lines = outp.Replace("\r", "").Split('\n');

        int hdr = -1;
        for (int i = 0; i < lines.Length; i++)
            if (lines[i].Contains("Id") && lines[i].Contains("Version")) { hdr = i; break; }
        if (hdr < 0 || hdr + 2 > lines.Length) return res;

        var h = lines[hdr];
        int idCol = h.IndexOf("Id", StringComparison.Ordinal);
        int verCol = h.IndexOf("Version", StringComparison.Ordinal);
        int availCol = h.IndexOf("Available", StringComparison.Ordinal);
        int matchCol = h.IndexOf("Match", StringComparison.Ordinal);
        int srcCol = h.IndexOf("Source", StringComparison.Ordinal);
        int endVer = Min4(availCol, matchCol, srcCol, h.Length);

        string Cut(string ln, int a, int b)
        {
            if (a < 0 || a >= ln.Length) return "";
            b = Math.Min(b, ln.Length);
            return b > a ? ln.Substring(a, b - a).Trim() : "";
        }

        for (int i = hdr + 2; i < lines.Length; i++)
        {
            var ln = lines[i];
            if (ln.Trim().Length == 0 || ln.TrimStart().StartsWith("---")) continue;
            var name = Cut(ln, 0, idCol);
            var id = Cut(ln, idCol, verCol);
            var ver = Cut(ln, verCol, endVer);
            var src = srcCol > 0 && srcCol < ln.Length ? ln.Substring(Math.Min(srcCol, ln.Length)).Trim() : "";
            if (id.Length > 0 && !id.Contains(' '))
                res.Add(new PkgResult { Name = name, Id = id, Version = ver, Source = src });
        }
        return res;
    }

    private static int Min4(int a, int b, int c, int d)
    {
        int m = d;
        if (a > 0) m = Math.Min(m, a);
        if (b > 0) m = Math.Min(m, b);
        if (c > 0) m = Math.Min(m, c);
        return m;
    }
}
