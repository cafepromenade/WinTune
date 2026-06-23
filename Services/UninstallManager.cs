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
    public string Publisher { get; set; } = "";
    public string Version { get; set; } = "";

    public string ShortName
    {
        get
        {
            var dot = Name.LastIndexOf('.');
            return dot >= 0 && dot < Name.Length - 1 ? Name[(dot + 1)..] : Name;
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
            "@(Get-AppxPackage | Where-Object { -not $_.IsFramework } | Select-Object Name,PackageFullName,@{n='Publisher';e={$_.Publisher}},@{n='Version';e={$_.Version.ToString()}} | Sort-Object Name) | ConvertTo-Json -Compress",
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

    public static Task<TweakResult> Uninstall(AppInfo app, CancellationToken ct = default)
        => ShellRunner.RunPowershell(
            $"Remove-AppxPackage -Package '{app.PackageFullName.Replace("'", "''")}' -ErrorAction Stop; 'Removed {app.ShortName}'",
            elevated: false, ct);
}
