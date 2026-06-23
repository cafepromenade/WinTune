using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.System;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 應用程式內 Android ADB 主控台 · In-app Android ADB console (wraps adb) — devices, install APK, shell,
/// logcat, screenshot, reboot, wireless connect. No redirect. Bilingual.
/// </summary>
public sealed partial class AndroidAdbModule : Page
{
    public AndroidAdbModule()
    {
        InitializeComponent();
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += async (_, _) => { Render(); await CheckEngine(); await RefreshDevices(); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);

    private string? SelectedSerial => (DeviceBox.SelectedItem as AdbDevice)?.Serial;

    private void Render()
    {
        HeaderTitle.Text = "Android (ADB) · Android（ADB）";
        HeaderBlurb.Text = P("Manage Android devices over adb — list devices, install APKs, run shell commands, pull logcat, take a screenshot, reboot, or connect wirelessly. Enable USB debugging on the phone first.",
            "用 adb 管理 Android 裝置 — 列出裝置、安裝 APK、執行 shell 指令、攞 logcat、截圖、重啟，或者無線連接。手機要先開 USB 偵錯。");
        ConnectBtn.Content = P("Connect", "連接");
        InstallBtn.Content = P("Install APK", "安裝 APK");
        ShotBtn.Content = P("Screenshot", "截圖");
        LogcatBtn.Content = P("Logcat", "Logcat");
        PackagesBtn.Content = P("Packages", "已裝套件");
        RebootBtn.Content = P("Reboot", "重啟");
        RebootSystem.Text = P("Reboot to system", "重啟入系統");
        RebootBootloader.Text = P("Reboot to bootloader", "重啟入 bootloader");
        RebootRecovery.Text = P("Reboot to recovery", "重啟入 recovery");
        ShellRunBtn.Content = P("Run", "執行");
    }

    private async Task CheckEngine()
    {
        bool ok = await AdbService.IsAvailable();
        EngineBar.IsOpen = !ok;
        if (!ok)
        {
            EngineBar.Severity = InfoBarSeverity.Warning;
            EngineBar.Title = P("adb not found", "搵唔到 adb");
            EngineBar.Message = P("Click to install it automatically (Google Platform Tools via winget) — no restart needed.",
                "撳一下自動安裝（用 winget 裝 Google Platform Tools）— 唔使重啟。");
            var btn = new Button { Content = P("Install adb automatically", "自動安裝 adb") };
            btn.Click += async (_, _) =>
            {
                btn.IsEnabled = false;
                btn.Content = P("Installing…", "安裝緊…");
                await PackageService.AutoInstall("Google.PlatformTools");
                await CheckEngine();
                if (await AdbService.IsAvailable()) await RefreshDevices();
            };
            EngineBar.ActionButton = btn;
        }
        else EngineBar.ActionButton = null;
    }

    private async Task RefreshDevices()
    {
        Busy.IsActive = true;
        var devices = await AdbService.Devices();
        Busy.IsActive = false;
        DeviceBox.Items.Clear();
        foreach (var d in devices) DeviceBox.Items.Add(d);
        if (DeviceBox.Items.Count > 0) DeviceBox.SelectedIndex = 0;
        if (devices.Count == 0 && !EngineBar.IsOpen)
            Notify(InfoBarSeverity.Informational, P("No devices", "冇裝置"), P("Plug in a phone with USB debugging on, or connect wirelessly.", "插一部開咗 USB 偵錯嘅手機，或者無線連接。"));
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) { await CheckEngine(); await RefreshDevices(); }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        var ip = (IpBox.Text ?? "").Trim();
        if (ip.Length == 0) return;
        Busy.IsActive = true;
        var r = await AdbService.Connect(ip);
        Busy.IsActive = false;
        Notify(r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Warning, P("adb connect", "adb 連接"), Msg(r));
        await RefreshDevices();
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        if (!Require(out var serial)) return;
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".apk");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Shell));
        var f = await picker.PickSingleFileAsync();
        if (f is null) return;
        Busy.IsActive = true;
        var r = await AdbService.Install(serial, f.Path);
        Busy.IsActive = false;
        Notify(r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error, P("Install APK", "安裝 APK"), Msg(r));
    }

    private async void Shot_Click(object sender, RoutedEventArgs e)
    {
        if (!Require(out var serial)) return;
        Busy.IsActive = true;
        var local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), $"adb-{DateTime.Now:yyyyMMdd-HHmmss}.png");
        var r = await AdbService.Screenshot(serial, local);
        Busy.IsActive = false;
        if (r.Success && File.Exists(local))
        {
            ShotImage.Source = new BitmapImage(new Uri(local));
            ShotImage.Visibility = Visibility.Visible;
            Notify(InfoBarSeverity.Success, P("Screenshot saved", "已儲存截圖"), local);
        }
        else Notify(InfoBarSeverity.Error, P("Screenshot failed", "截圖失敗"), Msg(r));
    }

    private async void Logcat_Click(object sender, RoutedEventArgs e)
    {
        if (!Require(out var serial)) return;
        Busy.IsActive = true;
        Console.Text = await AdbService.Logcat(serial, 400);
        Busy.IsActive = false;
    }

    private async void Packages_Click(object sender, RoutedEventArgs e)
    {
        if (!Require(out var serial)) return;
        Busy.IsActive = true;
        Console.Text = await AdbService.Packages(serial);
        Busy.IsActive = false;
    }

    private async void RebootSystem_Click(object sender, RoutedEventArgs e) => await DoReboot("");
    private async void RebootBootloader_Click(object sender, RoutedEventArgs e) => await DoReboot("bootloader");
    private async void RebootRecovery_Click(object sender, RoutedEventArgs e) => await DoReboot("recovery");

    private async Task DoReboot(string mode)
    {
        if (!Require(out var serial)) return;
        var r = await AdbService.Reboot(serial, mode);
        Notify(r.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error, P("Reboot", "重啟"), Msg(r));
    }

    private void ShellBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) { e.Handled = true; ShellRun_Click(sender, e); }
    }

    private async void ShellRun_Click(object sender, RoutedEventArgs e)
    {
        if (!Require(out var serial)) return;
        var cmd = (ShellBox.Text ?? "").Trim();
        if (cmd.Length == 0) return;
        Busy.IsActive = true;
        Console.Text = await AdbService.Shell(serial, cmd);
        Busy.IsActive = false;
    }

    private bool Require(out string serial)
    {
        serial = SelectedSerial ?? "";
        if (serial.Length == 0)
        {
            Notify(InfoBarSeverity.Warning, P("Pick a device first", "請先揀一部裝置"), "");
            return false;
        }
        return true;
    }

    private static string Msg(WinTune.Models.TweakResult r)
        => (Loc.I.IsCantonesePrimary ? r.Message?.Zh : r.Message?.En) ?? "";

    private void Notify(InfoBarSeverity sev, string title, string msg)
    {
        ResultBar.Severity = sev; ResultBar.Title = title; ResultBar.Message = msg; ResultBar.IsOpen = true;
    }
}
