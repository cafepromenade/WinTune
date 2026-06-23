using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>一部 Tailscale 網狀網裝置 · One Tailscale peer.</summary>
public sealed class TsPeer
{
    public string Name { get; set; } = "";
    public string Ip { get; set; } = "";
    public bool Online { get; set; }
    public bool Self { get; set; }
}

/// <summary>
/// 應用程式內 Tailscale 控制（包 tailscale.exe CLI）· In-app Tailscale control wrapping the tailscale CLI
/// — up/down, status, IP, ping, and a parsed device list (status --json). No redirect.
/// </summary>
public static class TailscaleService
{
    private static readonly string[] Paths = { @"C:\Program Files\Tailscale\tailscale.exe" };
    public static string Exe => Paths.FirstOrDefault(File.Exists) ?? "tailscale";

    public static async Task<bool> IsAvailable(CancellationToken ct = default)
    {
        if (Paths.Any(File.Exists)) return true;
        try { return (await Cap("version", ct)).Trim().Length > 0 && !(await Cap("version", ct)).Contains("not recognized", StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }

    private static Task<string> Cap(string args, CancellationToken ct)
        => ShellRunner.CapturePowershell($"& \"{Exe}\" {args} 2>&1 | Out-String -Width 400", ct);

    public static Task<string> Status(CancellationToken ct = default) => Cap("status", ct);
    public static Task<string> Ip(CancellationToken ct = default) => Cap("ip -4", ct);
    public static Task<string> Ping(string host, CancellationToken ct = default) => Cap($"ping {host}", ct);
    public static Task<string> NetCheck(CancellationToken ct = default) => Cap("netcheck", ct);

    public static Task<TweakResult> Up(CancellationToken ct = default) => ShellRunner.Run(Exe, "up", false, ct);
    public static Task<TweakResult> Down(CancellationToken ct = default) => ShellRunner.Run(Exe, "down", false, ct);

    public static async Task<List<TsPeer>> Peers(CancellationToken ct = default)
    {
        var peers = new List<TsPeer>();
        string json;
        try { json = await Cap("status --json", ct); } catch { return peers; }
        int brace = json.IndexOf('{');
        if (brace < 0) return peers;
        try
        {
            using var doc = JsonDocument.Parse(json.Substring(brace));
            var root = doc.RootElement;
            if (root.TryGetProperty("Self", out var self)) AddPeer(peers, self, true);
            if (root.TryGetProperty("Peer", out var peer) && peer.ValueKind == JsonValueKind.Object)
                foreach (var p in peer.EnumerateObject()) AddPeer(peers, p.Value, false);
        }
        catch { }
        return peers;
    }

    private static void AddPeer(List<TsPeer> list, JsonElement e, bool self)
    {
        string name = e.TryGetProperty("HostName", out var hn) ? hn.GetString() ?? "" : "";
        if (string.IsNullOrEmpty(name) && e.TryGetProperty("DNSName", out var dn)) name = (dn.GetString() ?? "").TrimEnd('.');
        string ip = "";
        if (e.TryGetProperty("TailscaleIPs", out var ips) && ips.ValueKind == JsonValueKind.Array && ips.GetArrayLength() > 0)
            ip = ips[0].GetString() ?? "";
        bool online = self || (e.TryGetProperty("Online", out var on) && on.ValueKind == JsonValueKind.True);
        if (name.Length > 0 || ip.Length > 0)
            list.Add(new TsPeer { Name = name, Ip = ip, Online = online, Self = self });
    }
}
