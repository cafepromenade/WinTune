using System.Collections.Generic;
using Microsoft.Win32;
using WinTune.Models;
using WinTune.Services;
using static WinTune.Catalog.Recipe;

namespace WinTune.Catalog;

/// <summary>
/// 預設一鍵流程 · Built-in recipes that bundle common multi-step chores into a single button.
/// </summary>
public static class Recipes
{
    private const string Adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string Personalize = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string Restart = "taskkill /f /im explorer.exe & start explorer.exe";

    private static void Set(string path, string name, object val, RegistryValueKind kind = RegistryValueKind.DWord)
        => RegistryHelper.SetValue(RegRoot.HKCU, path, name, val, kind);

    public static IEnumerable<TweakDefinition> All() => new List<TweakDefinition>
    {
        Make("recipe.quick-clean", "Quick cleanup", "快速清理",
            "Flush DNS, clear temp files, empty the Recycle Bin and clear the thumbnail cache.",
            "清 DNS、清暫存檔、清空回收筒同清縮圖快取。",
            "Run", "執行", admin: false, destructive: true,
            Cmd("Flush DNS", "ipconfig /flushdns"),
            Cmd("Clear user temp", "del /q /f /s \"%TEMP%\\*\""),
            Ps("Empty Recycle Bin", "Clear-RecycleBin -Force -ErrorAction SilentlyContinue"),
            Ps("Clear thumbnail cache", "Remove-Item \"$env:LocalAppData\\Microsoft\\Windows\\Explorer\\thumbcache_*.db\" -Force -ErrorAction SilentlyContinue")),

        Make("recipe.privacy", "Privacy hardening", "私隱強化",
            "Disable advertising ID, telemetry, activity history, tailored experiences and feedback prompts.",
            "熄廣告 ID、遙測、活動記錄、個人化體驗同意見回饋提示。",
            "Apply", "套用", admin: true, destructive: false,
            Reg("Advertising ID off", () => Set(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0)),
            Reg("Tailored experiences off", () => Set(@"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0)),
            Reg("Feedback frequency 0", () => Set(@"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0)),
            Cmd("Telemetry policy = 0", "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v AllowTelemetry /t REG_DWORD /d 0 /f", admin: true),
            Cmd("Activity history off", "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v PublishUserActivities /t REG_DWORD /d 0 /f", admin: true)),

        Make("recipe.gaming", "Gaming mode", "遊戲模式",
            "Turn on Game Mode, disable Game DVR background recording and switch to the High performance power plan.",
            "開遊戲模式、熄 Game DVR 背景錄影、轉做高效能電源計劃。",
            "Apply", "套用", admin: true, destructive: false,
            Reg("Game Mode on", () => Set(@"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1)),
            Reg("Game DVR off", () => Set(@"System\GameConfigStore", "GameDVR_Enabled", 0)),
            Cmd("High performance plan", "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", admin: true)),

        Make("recipe.dev", "Developer setup", "開發人員設定",
            "Enable Developer Mode and long paths, show file extensions and hidden files, then restart Explorer.",
            "開開發人員模式同長路徑、顯示副檔名同隱藏檔案，再重啟檔案總管。",
            "Apply", "套用", admin: true, destructive: false,
            Cmd("Developer Mode on", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock\" /v AllowDevelopmentWithoutDevLicense /t REG_DWORD /d 1 /f", admin: true),
            Cmd("Long paths on", "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\" /v LongPathsEnabled /t REG_DWORD /d 1 /f", admin: true),
            Reg("Show file extensions", () => Set(Adv, "HideFileExt", 0)),
            Reg("Show hidden files", () => Set(Adv, "Hidden", 1)),
            Cmd("Restart Explorer", Restart)),

        Make("recipe.explorer-classic", "Classic Explorer", "經典檔案總管",
            "Show extensions and hidden files, restore the classic right-click menu, open to This PC, then restart Explorer.",
            "顯示副檔名同隱藏檔案、還原經典右鍵選單、開去本機，再重啟檔案總管。",
            "Apply", "套用", admin: false, destructive: false,
            Reg("Show file extensions", () => Set(Adv, "HideFileExt", 0)),
            Reg("Show hidden files", () => Set(Adv, "Hidden", 1)),
            Reg("Open to This PC", () => Set(Adv, "LaunchTo", 1)),
            Reg("Classic context menu", () => RegistryHelper.SetDefault(RegRoot.HKCU, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "")),
            Cmd("Restart Explorer", Restart)),

        Make("recipe.declutter-taskbar", "Declutter taskbar", "簡化工作列",
            "Dark mode, taskbar icons left-aligned, hide Search, Widgets and the Copilot/Chat button, then restart Explorer.",
            "深色模式、工作列靠左、收埋搜尋、小工具同 Copilot/Chat 掣，再重啟檔案總管。",
            "Apply", "套用", admin: false, destructive: false,
            Reg("Dark mode", () => { Set(Personalize, "AppsUseLightTheme", 0); Set(Personalize, "SystemUsesLightTheme", 0); }),
            Reg("Taskbar left", () => Set(Adv, "TaskbarAl", 0)),
            Reg("Hide Search", () => Set(@"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0)),
            Reg("Hide Widgets", () => Set(Adv, "TaskbarDa", 0)),
            Reg("Hide Copilot/Chat", () => Set(Adv, "TaskbarMn", 0)),
            Cmd("Restart Explorer", Restart)),

        Make("recipe.perf-boost", "Performance boost", "效能提升",
            "Set visual effects to best performance, remove the Start/Run startup delay and use the High performance power plan.",
            "視覺效果調做最佳效能、攞走開機延遲、用高效能電源計劃。",
            "Apply", "套用", admin: true, destructive: false,
            Reg("Visual FX = performance", () => Set(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2)),
            Reg("No startup delay", () => Set(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0)),
            Cmd("High performance plan", "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", admin: true)),

        Make("recipe.network-reset", "Network reset", "網絡重置",
            "Flush DNS, reset Winsock, reset the TCP/IP stack and clear the ARP cache. Reboot afterwards.",
            "清 DNS、重設 Winsock、重設 TCP/IP、清 ARP 快取。之後請重新開機。",
            "Reset", "重置", admin: true, destructive: true,
            Cmd("Flush DNS", "ipconfig /flushdns"),
            Cmd("Reset Winsock", "netsh winsock reset", admin: true),
            Cmd("Reset TCP/IP", "netsh int ip reset", admin: true),
            Cmd("Flush ARP", "netsh interface ip delete arpcache", admin: true)),

        Make("recipe.update-all", "Update everything", "全部更新",
            "Upgrade all apps via winget and open the Microsoft Store updates page.",
            "用 winget 更新所有應用程式，再開 Microsoft Store 更新頁。",
            "Update", "更新", admin: true, destructive: false,
            Cmd("winget upgrade --all", "winget upgrade --all --include-unknown --accept-source-agreements --accept-package-agreements", admin: true),
            Cmd("Open Store updates", "start ms-windows-store://downloadsandupdates")),

        Make("recipe.health-check", "System health check", "系統健康檢查",
            "Run an SFC verify-only pass, a DISM CheckHealth and report physical disk health (read-only).",
            "行 SFC 純驗證、DISM CheckHealth，再報告實體磁碟健康（唯讀）。",
            "Check", "檢查", admin: true, destructive: false,
            Cmd("SFC verify-only", "sfc /verifyonly", admin: true),
            Cmd("DISM CheckHealth", "DISM /Online /Cleanup-Image /CheckHealth", admin: true),
            Ps("Disk health", "Get-PhysicalDisk | Select-Object FriendlyName,MediaType,HealthStatus,OperationalStatus | Format-Table -Auto | Out-String")),

        Make("recipe.free-space", "Free up space", "釋放空間",
            "Clear the Windows Update cache, run DISM component cleanup, empty the Recycle Bin and clear Prefetch.",
            "清 Windows Update 快取、做 DISM 元件清理、清空回收筒同清 Prefetch。",
            "Run", "執行", admin: true, destructive: true,
            Cmd("Clear update cache", "net stop wuauserv & rd /s /q C:\\Windows\\SoftwareDistribution\\Download & net start wuauserv", admin: true),
            Cmd("DISM component cleanup", "Dism.exe /Online /Cleanup-Image /StartComponentCleanup", admin: true),
            Ps("Empty Recycle Bin", "Clear-RecycleBin -Force -ErrorAction SilentlyContinue"),
            Cmd("Clear Prefetch", "del /q /f /s C:\\Windows\\Prefetch\\*", admin: true)),

        Make("recipe.lock-down", "Security lock-down", "保安加固",
            "Turn the firewall on for all profiles, set SmartScreen to Warn and UAC to its default level.",
            "所有設定檔開防火牆、SmartScreen 設做警告、UAC 設返預設等級。",
            "Apply", "套用", admin: true, destructive: false,
            Cmd("Firewall on (all profiles)", "netsh advfirewall set allprofiles state on", admin: true),
            Cmd("SmartScreen = Warn", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\" /v SmartScreenEnabled /t REG_SZ /d Warn /f", admin: true),
            Cmd("UAC default", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" /v ConsentPromptBehaviorAdmin /t REG_DWORD /d 5 /f", admin: true)),
    };
}
