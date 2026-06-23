using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>一個已安裝嘅商店／UWP 應用程式 · One installed Store/UWP app.</summary>
public sealed class AppInfo
{
    public string Name { get; set; } = "";
    public string PackageFullName { get; set; } = "";
    public string PackageFamilyName { get; set; } = "";
    public string InstallLocation { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string Version { get; set; } = "";

    /// <summary>安裝大細（位元組，0 = 未計）· Install size in bytes (0 = not yet computed).</summary>
    public long SizeBytes { get; set; }

    public string ShortName
    {
        get
        {
            var dot = Name.LastIndexOf('.');
            return dot >= 0 && dot < Name.Length - 1 ? Name[(dot + 1)..] : Name;
        }
    }

    /// <summary>人睇得明嘅大細 · Human-readable size, or "—" when unknown.</summary>
    public string SizeText
    {
        get
        {
            if (SizeBytes <= 0) return "—";
            double b = SizeBytes;
            string[] u = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            while (b >= 1024 && i < u.Length - 1) { b /= 1024; i++; }
            return $"{b:0.#} {u[i]}";
        }
    }
}

/// <summary>
/// 應用程式內解除安裝（商店／UWP 應用程式）· In-app uninstaller for Store/UWP apps via
/// Get-AppxPackage / Remove-AppxPackage. Frameworks (VCLibs/.NET.Native/UI.Xaml) are excluded so the
/// shared runtimes can't be removed. No Settings redirect.
/// </summary>
public static class UninstallManager
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public static async Task<List<AppInfo>> ListAsync(CancellationToken ct = default)
    {
        var json = await ShellRunner.CapturePowershellJson(
            "@(Get-AppxPackage | Where-Object { -not $_.IsFramework } | Select-Object Name,PackageFullName,PackageFamilyName,InstallLocation,@{n='Publisher';e={$_.Publisher}},@{n='Version';e={$_.Version.ToString()}} | Sort-Object Name) | ConvertTo-Json -Compress",
            ct);
        try
        {
            var list = JsonSerializer.Deserialize<List<AppInfo>>(json, JsonOpts);
            if (list is not null && list.Count > 0) return list;
        }
        catch { /* maybe single */ }
        try
        {
            var one = JsonSerializer.Deserialize<AppInfo>(json, JsonOpts);
            if (one is not null) return new List<AppInfo> { one };
        }
        catch { /* give up */ }
        return new List<AppInfo>();
    }

    /// <summary>計一個 app 嘅安裝大細 · Compute one app's on-disk size by summing its install folder.
    /// Lazy (call per visible row) so listing stays fast. Returns bytes (0 when unknown).</summary>
    public static async Task<long> ComputeSizeAsync(AppInfo app, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(app.InstallLocation)) return 0;
        var loc = app.InstallLocation.Replace("'", "''");
        var outp = await ShellRunner.CapturePowershell(
            $"$s=(Get-ChildItem -LiteralPath '{loc}' -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum; if($s){{[long]$s}}else{{0}}",
            ct);
        return long.TryParse((outp ?? "").Trim(), out var n) ? n : 0;
    }

    /// <summary>一般解除安裝 · Standard uninstall (leaves per-user AppData behind).</summary>
    public static Task<TweakResult> Uninstall(AppInfo app, CancellationToken ct = default)
        => ShellRunner.RunPowershell(
            $"Remove-AppxPackage -Package '{app.PackageFullName.Replace("'", "''")}' -ErrorAction Stop; 'Removed {app.ShortName}'",
            elevated: false, ct);

    /// <summary>
    /// 深層解除安裝 · Deep uninstall: remove the package, then clear the app's leftovers —
    /// %LocalAppData%\Packages\&lt;PackageFamilyName&gt; (settings/cache the normal uninstall keeps).
    /// </summary>
    public static async Task<TweakResult> DeepUninstall(AppInfo app, CancellationToken ct = default)
    {
        var r = await Uninstall(app, ct);
        if (!r.Success) return r;

        if (!string.IsNullOrEmpty(app.PackageFamilyName))
        {
            var fam = app.PackageFamilyName.Replace("'", "''");
            // Clear the per-user package data folder if it lingers after removal.
            await ShellRunner.RunPowershell(
                $"$p = Join-Path $env:LocalAppData (Join-Path 'Packages' '{fam}'); if (Test-Path -LiteralPath $p) {{ Remove-Item -LiteralPath $p -Recurse -Force -ErrorAction SilentlyContinue }}; 'cleared'",
                elevated: false, ct);
        }
        return new TweakResult(true,
            new LocalizedText($"Removed {app.ShortName} and cleared its leftover data.",
                $"已移除 {app.ShortName} 並清走佢嘅殘留資料。"), r.Output);
    }
}
