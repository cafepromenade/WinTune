using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>一部 ADB 裝置 · One adb device row.</summary>
public sealed class AdbDevice
{
    public string Serial { get; set; } = "";
    public string State { get; set; } = "";
    public string Model { get; set; } = "";

    public string Display => string.IsNullOrEmpty(Model) ? $"{Serial} ({State})" : $"{Model} · {Serial} ({State})";
}

/// <summary>
/// 應用程式內 Android ADB 主控台（包真實 adb 引擎）· In-app Android ADB console wrapping adb.exe — list
/// devices, install/uninstall APKs, shell, logcat, screenshot, reboot and wireless connect. No redirect.
/// adb comes from Google.PlatformTools (install it from the Package Manager).
/// </summary>
public static class AdbService
{
    private static Task<string> Capture(string args, CancellationToken ct)
        => ShellRunner.CapturePowershell($"adb {args} 2>&1 | Out-String -Width 400", ct);

    public static async Task<bool> IsAvailable(CancellationToken ct = default)
    {
        try { return (await Capture("version", ct)).Contains("Android Debug Bridge", StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }

    public static async Task<List<AdbDevice>> Devices(CancellationToken ct = default)
    {
        var list = new List<AdbDevice>();
        string outp;
        try { outp = await Capture("devices -l", ct); } catch { return list; }

        bool started = false;
        foreach (var raw in outp.Replace("\r", "").Split('\n'))
        {
            var line = raw.Trim();
            if (line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase)) { started = true; continue; }
            if (!started || line.Length == 0 || line.StartsWith("*")) continue;

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            var dev = new AdbDevice { Serial = parts[0], State = parts[1] };
            foreach (var p in parts)
                if (p.StartsWith("model:", StringComparison.OrdinalIgnoreCase))
                    dev.Model = p.Substring("model:".Length).Replace('_', ' ');
            list.Add(dev);
        }
        return list;
    }

    public static Task<TweakResult> Install(string serial, string apkPath, CancellationToken ct = default)
        => ShellRunner.RunCmd($"adb -s {serial} install -r \"{apkPath}\"", false, ct);

    public static Task<TweakResult> Uninstall(string serial, string package, CancellationToken ct = default)
        => ShellRunner.RunCmd($"adb -s {serial} uninstall {package}", false, ct);

    /// <param name="mode">"" (system), "bootloader" or "recovery".</param>
    public static Task<TweakResult> Reboot(string serial, string mode, CancellationToken ct = default)
        => ShellRunner.RunCmd($"adb -s {serial} reboot {mode}".TrimEnd(), false, ct);

    public static Task<TweakResult> Connect(string ipPort, CancellationToken ct = default)
        => ShellRunner.RunCmd($"adb connect {ipPort}", false, ct);

    public static Task<TweakResult> Disconnect(string ipPort, CancellationToken ct = default)
        => ShellRunner.RunCmd($"adb disconnect {ipPort}", false, ct);

    public static Task<TweakResult> KillServer(CancellationToken ct = default)
        => ShellRunner.RunCmd("adb kill-server", false, ct);

    public static Task<string> Shell(string serial, string command, CancellationToken ct = default)
        => Capture($"-s {serial} shell {command}", ct);

    public static Task<string> Logcat(string serial, int lines, CancellationToken ct = default)
        => Capture($"-s {serial} logcat -d -t {lines}", ct);

    public static Task<string> Packages(string serial, CancellationToken ct = default)
        => Capture($"-s {serial} shell pm list packages", ct);

    /// <summary>Capture a device screenshot to a local PNG (screencap on the device, then pull).</summary>
    public static async Task<TweakResult> Screenshot(string serial, string localPath, CancellationToken ct = default)
    {
        const string remote = "/sdcard/wintune_screen.png";
        var cap = await ShellRunner.RunCmd($"adb -s {serial} shell screencap -p {remote}", false, ct);
        if (!cap.Success) return cap;
        return await ShellRunner.RunCmd($"adb -s {serial} pull {remote} \"{localPath}\"", false, ct);
    }
}
