using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace WinTune.Services;

public sealed class ProcInfo
{
    public int Pid { get; init; }
    public string Name { get; init; } = "";
    public long MemoryBytes { get; init; }
}

/// <summary>
/// 即時系統監察（純 managed + P/Invoke）· Live system monitor: CPU% via GetSystemTimes deltas, RAM via
/// GlobalMemoryStatusEx, network rates via NetworkInterface byte deltas, and top processes by memory.
/// </summary>
public static class SystemMonitor
{
    // ---- CPU via GetSystemTimes ----
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out long idle, out long kernel, out long user);

    private static long _pIdle, _pKernel, _pUser;
    private static bool _cpuPrimed;

    public static double CpuPercent()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user)) return 0;
        if (!_cpuPrimed) { _pIdle = idle; _pKernel = kernel; _pUser = user; _cpuPrimed = true; return 0; }

        long dIdle = idle - _pIdle, dKernel = kernel - _pKernel, dUser = user - _pUser;
        _pIdle = idle; _pKernel = kernel; _pUser = user;

        long total = dKernel + dUser; // kernel already includes idle
        if (total <= 0) return 0;
        double busy = total - dIdle;
        return Math.Clamp(busy * 100.0 / total, 0, 100);
    }

    // ---- Memory ----
    [StructLayout(LayoutKind.Sequential)]
    private struct MemStatus
    {
        public uint dwLength, dwMemoryLoad;
        public ulong ullTotalPhys, ullAvailPhys, ullTotalPageFile, ullAvailPageFile, ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemStatus b);

    public static (double percent, long usedBytes, long totalBytes) Memory()
    {
        var m = new MemStatus { dwLength = (uint)Marshal.SizeOf<MemStatus>() };
        GlobalMemoryStatusEx(ref m);
        long total = (long)m.ullTotalPhys, used = (long)(m.ullTotalPhys - m.ullAvailPhys);
        return (m.dwMemoryLoad, used, total);
    }

    // ---- Network rates ----
    private static long _pRx, _pTx;
    private static bool _netPrimed;

    public static (double downBps, double upBps) Network(double seconds)
    {
        long rx = 0, tx = 0;
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
                var s = ni.GetIPv4Statistics();
                rx += s.BytesReceived;
                tx += s.BytesSent;
            }
        }
        catch { }

        if (!_netPrimed) { _pRx = rx; _pTx = tx; _netPrimed = true; return (0, 0); }
        double down = Math.Max(0, (rx - _pRx) / Math.Max(0.001, seconds));
        double up = Math.Max(0, (tx - _pTx) / Math.Max(0.001, seconds));
        _pRx = rx; _pTx = tx;
        return (down, up);
    }

    public static List<ProcInfo> TopByMemory(int n)
    {
        var list = new List<ProcInfo>();
        foreach (var p in Process.GetProcesses())
        {
            try { list.Add(new ProcInfo { Pid = p.Id, Name = p.ProcessName, MemoryBytes = p.WorkingSet64 }); }
            catch { }
            finally { p.Dispose(); }
        }
        return list.OrderByDescending(x => x.MemoryBytes).Take(n).ToList();
    }

    public static bool Kill(int pid)
    {
        try { using var p = Process.GetProcessById(pid); p.Kill(); return true; }
        catch { return false; }
    }

    public static string Bytes(double b)
    {
        string[] u = { "B", "KB", "MB", "GB", "TB" };
        int i = 0;
        while (b >= 1024 && i < u.Length - 1) { b /= 1024; i++; }
        return $"{Math.Round(b, 1)} {u[i]}";
    }

    public static string Uptime()
    {
        var ts = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
    }
}
