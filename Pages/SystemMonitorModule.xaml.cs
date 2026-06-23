using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 應用程式內系統監察 · In-app system monitor — live CPU/RAM/network and top processes (with end-task).
/// Pure managed + P/Invoke, updates each second. No redirect. Bilingual.
/// </summary>
public sealed partial class SystemMonitorModule : Page
{
    public sealed class ProcRow
    {
        public int Pid { get; init; }
        public string Name { get; init; } = "";
        public string MemText { get; init; } = "";
    }

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private const double CpuTrack = 188, RamTrack = 208;

    public SystemMonitorModule()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => Tick();
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += (_, _) => { Render(); SystemMonitor.CpuPercent(); SystemMonitor.Network(1); Tick(); _timer.Start(); };
        Unloaded += (_, _) => _timer.Stop();
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "System Monitor · 系統監察";
        HeaderBlurb.Text = P("Live CPU, memory and network, plus the heaviest processes — end any with one click. Updates every second.",
            "即時 CPU、記憶體同網絡，再加最食資源嘅程序 — 一撳就結束。每秒更新。");
        CpuLabel.Text = P("CPU", "CPU");
        RamLabel.Text = P("Memory", "記憶體");
        NetLabel.Text = P("Network", "網絡");
        UpLabel.Text = P("Uptime", "運行時間");
    }

    private void Tick()
    {
        var cpu = SystemMonitor.CpuPercent();
        CpuValue.Text = $"{Math.Round(cpu)}%";
        CpuBar.Width = CpuTrack * cpu / 100.0;

        var (memPct, used, total) = SystemMonitor.Memory();
        RamValue.Text = $"{Math.Round(memPct)}%";
        RamBar.Width = RamTrack * memPct / 100.0;
        RamSub.Text = $"{SystemMonitor.Bytes(used)} / {SystemMonitor.Bytes(total)}";

        var (down, up) = SystemMonitor.Network(1);
        NetDown.Text = $"↓ {SystemMonitor.Bytes(down)}/s";
        NetUp.Text = $"↑ {SystemMonitor.Bytes(up)}/s";

        UpValue.Text = SystemMonitor.Uptime();

        ProcList.ItemsSource = SystemMonitor.TopByMemory(14)
            .Select(p => new ProcRow { Pid = p.Pid, Name = p.Name, MemText = SystemMonitor.Bytes(p.MemoryBytes) })
            .ToList();
    }

    private void Kill_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is ProcRow row)
        {
            SystemMonitor.Kill(row.Pid);
            Tick();
        }
    }
}
