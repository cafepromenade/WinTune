using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 應用程式內 VPN 與網狀網 · In-app VPN &amp; mesh — NordVPN (wraps NordVPN.exe CLI) and Tailscale
/// (wraps tailscale CLI). Connect/disconnect, status, devices, ping. No redirect. Bilingual.
/// </summary>
public sealed partial class VpnMeshModule : Page
{
    public sealed class PeerRow
    {
        public string Name { get; init; } = "";
        public string Ip { get; init; } = "";
        public SolidColorBrush Dot { get; init; } = new(Colors.Gray);
    }

    private bool _tsAvailable;

    public VpnMeshModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += async (_, _) => { Render(); await CheckEngines(); if (_tsAvailable) await RefreshTailscale(); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private void Render()
    {
        HeaderTitle.Text = "VPN & Mesh · VPN 與網狀網";
        HeaderBlurb.Text = P("Control NordVPN and Tailscale in-app by wrapping their command-line tools — connect, disconnect, see your mesh devices and ping them.",
            "喺 app 內透過包住佢哋嘅命令列工具控制 NordVPN 同 Tailscale — 連接、斷開、睇網狀網裝置同 ping。");

        NordTitle.Text = "NordVPN";
        NordConnectBtn.Content = P("Connect", "連接");
        NordDisconnectBtn.Content = P("Disconnect", "斷開");
        NordGroupBtn.Content = P("Connect group", "連接群組");
        FillCountries();
        FillGroups();

        TsTitle.Text = "Tailscale";
        TsUpBtn.Content = P("Up (connect)", "Up（連接）");
        TsDownBtn.Content = P("Down", "Down");
        TsStatusBtn.Content = P("Status", "狀態");
        TsIpBtn.Content = P("My IP", "我嘅 IP");
        TsPingBtn.Content = P("Ping", "Ping");
        TsPeersHeader.Text = P("Mesh devices", "網狀網裝置");
    }

    private void FillCountries()
    {
        int sel = NordCountryBox.SelectedIndex < 0 ? 0 : NordCountryBox.SelectedIndex;
        NordCountryBox.Items.Clear();
        foreach (var (en, zh) in NordVpnService.Countries) NordCountryBox.Items.Add($"{en} · {zh}");
        NordCountryBox.SelectedIndex = sel;
    }

    private void FillGroups()
    {
        int sel = NordGroupBox.SelectedIndex < 0 ? 0 : NordGroupBox.SelectedIndex;
        NordGroupBox.Items.Clear();
        foreach (var g in NordVpnService.Groups) NordGroupBox.Items.Add(g.Replace('_', ' '));
        NordGroupBox.SelectedIndex = sel;
    }

    private async Task CheckEngines()
    {
        if (!NordVpnService.Installed)
        {
            NordEngineBar.IsOpen = true;
            NordEngineBar.Severity = InfoBarSeverity.Warning;
            NordEngineBar.Title = P("NordVPN not found", "搵唔到 NordVPN");
            NordEngineBar.Message = P("Install NordVPN (search it in the Package Manager), sign in once, then use these controls.",
                "安裝 NordVPN（喺套件管理搜尋），登入一次，再用呢度嘅控制。");
        }
        else NordEngineBar.IsOpen = false;

        _tsAvailable = await TailscaleService.IsAvailable();
        if (!_tsAvailable)
        {
            TsEngineBar.IsOpen = true;
            TsEngineBar.Severity = InfoBarSeverity.Warning;
            TsEngineBar.Title = P("Tailscale not found", "搵唔到 Tailscale");
            TsEngineBar.Message = P("Install Tailscale (search it in the Package Manager), sign in once, then use these controls.",
                "安裝 Tailscale（喺套件管理搜尋），登入一次，再用呢度嘅控制。");
        }
        else TsEngineBar.IsOpen = false;
    }

    // ---- NordVPN ----
    private async void NordConnect_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        string? name = NordCountryBox.SelectedIndex <= 0 ? null : NordVpnService.Countries[NordCountryBox.SelectedIndex].en;
        NordNotify(InfoBarSeverity.Informational, P("Connecting…", "連接緊…"), name ?? P("Quick connect", "快速連接"));
        var r = await NordVpnService.Connect(name);
        NordNotify(r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error, P("NordVPN", "NordVPN"), Msg(r));
    }

    private async void NordDisconnect_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var r = await NordVpnService.Disconnect();
        NordNotify(r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error, P("Disconnected", "已斷開"), Msg(r));
    }

    private async void NordGroup_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (NordGroupBox.SelectedIndex < 0) return;
        var group = NordVpnService.Groups[NordGroupBox.SelectedIndex];
        NordNotify(InfoBarSeverity.Informational, P("Connecting…", "連接緊…"), group);
        var r = await NordVpnService.ConnectGroup(group);
        NordNotify(r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error, P("NordVPN", "NordVPN"), Msg(r));
    }

    // ---- Tailscale ----
    private async void TsUp_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TsBusy.IsActive = true; var r = await TailscaleService.Up(); TsBusy.IsActive = false;
        TsConsole.Text = Msg(r); await RefreshTailscale();
    }

    private async void TsDown_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TsBusy.IsActive = true; var r = await TailscaleService.Down(); TsBusy.IsActive = false;
        TsConsole.Text = Msg(r); await RefreshTailscale();
    }

    private async void TsStatus_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => await RefreshTailscale();

    private async void TsIp_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TsBusy.IsActive = true; TsConsole.Text = await TailscaleService.Ip(); TsBusy.IsActive = false;
    }

    private async void TsPing_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var host = (TsPingBox.Text ?? "").Trim();
        if (host.Length == 0) return;
        TsBusy.IsActive = true; TsConsole.Text = await TailscaleService.Ping(host); TsBusy.IsActive = false;
    }

    private async Task RefreshTailscale()
    {
        TsBusy.IsActive = true;
        try
        {
            TsConsole.Text = await TailscaleService.Status();
            var peers = await TailscaleService.Peers();
            TsPeers.Items.Clear();
            foreach (var p in peers)
                TsPeers.Items.Add(new PeerRow
                {
                    Name = p.Self ? $"{p.Name} ({P("this PC", "呢部機")})" : p.Name,
                    Ip = p.Ip,
                    Dot = new SolidColorBrush(p.Online ? Color.FromArgb(255, 0x2E, 0x7D, 0x32) : Colors.Gray),
                });
        }
        catch { }
        TsBusy.IsActive = false;
    }

    private static string Msg(WinTune.Models.TweakResult r)
        => (Loc.I.IsCantonesePrimary ? r.Message?.Zh : r.Message?.En) ?? "";

    private void NordNotify(InfoBarSeverity sev, string title, string msg)
    {
        NordResult.Severity = sev; NordResult.Title = title; NordResult.Message = msg; NordResult.IsOpen = true;
    }
}
