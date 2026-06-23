using System.Collections.Generic;
using System.Linq;

namespace WinTune.Services;

/// <summary>一個應用程式內模組（頁面）· One in-app module (page) for page-search.</summary>
public sealed class ModuleInfo
{
    public string Tag { get; init; } = "";
    public string En { get; init; } = "";
    public string Zh { get; init; } = "";
    public string Glyph { get; init; } = "";
    public string Keywords { get; init; } = "";

    public string Haystack => $"{En} {Zh} {Keywords}".ToLowerInvariant();
}

/// <summary>
/// 所有模組頁面嘅登記（畀搜尋用）· Registry of every module page, used by the master/page search.
/// </summary>
public static class ModuleRegistry
{
    public static readonly List<ModuleInfo> All = new()
    {
        new() { Tag = "dashboard", En = "Dashboard", Zh = "概覽", Glyph = "", Keywords = "home overview start 主頁 概覽" },
        new() { Tag = "module.git", En = "Git & GitHub", Zh = "Git 與 GitHub", Glyph = "", Keywords = "git github commit push repo clone uploader 版本控制" },
        new() { Tag = "module.archives", En = "Archives", Zh = "壓縮檔", Glyph = "", Keywords = "zip 7z rar tar gzip compress extract 解壓 壓縮" },
        new() { Tag = "module.media", En = "Media", Zh = "媒體", Glyph = "", Keywords = "ffmpeg video audio convert trim gif 影片 音訊 轉檔" },
        new() { Tag = "module.regedit", En = "Registry Editor", Zh = "登錄編輯器", Glyph = "", Keywords = "registry regedit hive key value 登錄檔" },
        new() { Tag = "module.doctors", En = "System Doctors", Zh = "系統醫生", Glyph = ((char)0xE95E).ToString(), Keywords = "doctor repair fix rescue printer spooler dns network sleep wake taskbar start search index explorer icon thumbnail cache ownership permissions 修復 醫生 救援 列印 網絡 睡眠 喚醒 工作列 搜尋 圖示 縮圖 擁有權 權限" },
        new() { Tag = "module.services", En = "Services", Zh = "服務", Glyph = "", Keywords = "services start stop startup type 服務" },
        new() { Tag = "module.tasks", En = "Scheduled Tasks", Zh = "排程工作", Glyph = "", Keywords = "scheduled task scheduler run 排程" },
        new() { Tag = "module.devices", En = "Devices", Zh = "裝置", Glyph = "", Keywords = "device manager hardware driver 裝置 驅動" },
        new() { Tag = "module.vivetool", En = "ViVeTool", Zh = "功能旗標", Glyph = ((char)0xE9D5).ToString(), Keywords = "vivetool vive feature flag experiment hidden file explorer tabs new start menu modern context menu snap layouts energy saver click to do 功能 旗標 實驗 隱藏 分頁 開始功能表" },
        new() { Tag = "module.startup", En = "Startup Apps", Zh = "開機程式", Glyph = "", Keywords = "startup autostart logon run 開機 自啟動" },
        new() { Tag = "module.rename", En = "Batch Rename", Zh = "批次改名", Glyph = "", Keywords = "rename bulk powerrename regex 改名 批次" },
        new() { Tag = "module.bulkops", En = "Bulk File Ops", Zh = "批次檔案操作", Glyph = "", Keywords = "bulk file move copy delete attributes 批次 檔案" },
        new() { Tag = "module.duplicates", En = "Duplicate Finder", Zh = "重複檔案搜尋", Glyph = "", Keywords = "duplicate hash find dedupe 重複" },
        new() { Tag = "module.disk", En = "Disk Analyser", Zh = "磁碟分析", Glyph = "", Keywords = "disk space treemap analyse folder size 磁碟 空間" },
        new() { Tag = "module.drives", En = "Drives", Zh = "磁碟機", Glyph = "", Keywords = "drive volume format bitlocker 磁碟機" },
        new() { Tag = "module.uninstall", En = "App Uninstaller", Zh = "應用程式解除安裝", Glyph = "", Keywords = "uninstall remove app program winget 解除安裝" },
        new() { Tag = "module.windows", En = "Window Manager", Zh = "視窗管理", Glyph = "", Keywords = "window tile cascade always on top 視窗" },
        new() { Tag = "module.keyboard", En = "Keyboard Remapper", Zh = "鍵盤重新對應", Glyph = "", Keywords = "keyboard remap key sharpkeys 鍵盤" },
        new() { Tag = "module.hotkeys", En = "Hotkey & Macro Runner", Zh = "熱鍵與巨集", Glyph = ((char)0xE765).ToString(), Keywords = "hotkey macro shortcut chord registerhotkey send keys autohotkey text expander snippet trigger expand abbreviation 熱鍵 巨集 快捷鍵 文字展開 片語 縮寫" },
        new() { Tag = "module.hosts", En = "Hosts Editor", Zh = "hosts 編輯器", Glyph = "", Keywords = "hosts block domain dns 封鎖" },
        new() { Tag = "module.mouse", En = "Mouse & Pointer", Zh = "滑鼠與指標", Glyph = "", Keywords = "mouse pointer acceleration speed 滑鼠 指標" },
        new() { Tag = "module.recorder", En = "Screen Recorder", Zh = "螢幕錄影", Glyph = "", Keywords = "record screen capture gdigrab 錄影" },
        new() { Tag = "module.capture", En = "Capture Studio", Zh = "擷取工作室", Glyph = ((char)0xE722).ToString(), Keywords = "capture snip screenshot region gif ocr text recognize clipboard 截圖 擷取 區域 文字辨識 認字" },
        new() { Tag = "module.monitor", En = "System Monitor", Zh = "系統監察", Glyph = "", Keywords = "cpu ram memory network task manager priority affinity 監察 工作管理員" },
        new() { Tag = "module.battery", En = "Battery & Thermal", Zh = "電池與散熱", Glyph = ((char)0xE83E).ToString(), Keywords = "battery thermal temperature wear health cpu gpu fan powercfg batteryreport energy 電池 溫度 散熱 風扇 耗損 健康" },
        new() { Tag = "module.connections", En = "Connections", Zh = "連線", Glyph = "", Keywords = "tcp udp connections netstat tcpview port 連線" },
        new() { Tag = "module.events", En = "Event Viewer", Zh = "事件檢視器", Glyph = "", Keywords = "event log viewer system application 事件 記錄" },
        new() { Tag = "module.mixer", En = "Volume Mixer", Zh = "音量混合器", Glyph = "", Keywords = "volume mixer audio per-app mute 音量 靜音" },
        new() { Tag = "module.contextmenu", En = "Context Menu", Zh = "右鍵選單", Glyph = "", Keywords = "context menu right click verb 右鍵 選單" },
        new() { Tag = "module.awake", En = "Awake", Zh = "保持喚醒", Glyph = "", Keywords = "awake keep awake no sleep caffeine 唔瞓 喚醒" },
        new() { Tag = "module.colorpicker", En = "Color Picker", Zh = "螢幕取色", Glyph = "", Keywords = "color picker hex rgb hsl eyedropper 取色 顏色" },
        new() { Tag = "module.envvars", En = "Environment Variables", Zh = "環境變數", Glyph = "", Keywords = "environment variables path user system env 環境變數" },
        new() { Tag = "module.clipboard", En = "Clipboard", Zh = "剪貼簿", Glyph = ((char)0xE77F).ToString(), Keywords = "clipboard history text image file convert win+v 剪貼簿 歷史" },
        new() { Tag = "module.packages", En = "Package Manager", Zh = "套件管理", Glyph = ((char)0xECAA).ToString(), Keywords = "winget package install uninstall upgrade scoop choco dependencies unigetui 套件 安裝 相依" },
        new() { Tag = "module.adb", En = "Android (ADB)", Zh = "Android（ADB）", Glyph = ((char)0xE8EA).ToString(), Keywords = "android adb apk logcat shell screenshot reboot fastboot scrcpy 手機 安卓" },
        new() { Tag = "module.vpn", En = "VPN & Mesh", Zh = "VPN 與網狀網", Glyph = ((char)0xE945).ToString(), Keywords = "vpn nordvpn tailscale mesh connect exit node ping 連線 網狀網" },
        new() { Tag = "module.homeassistant", En = "Home Assistant", Zh = "家居助理", Glyph = ((char)0xE80F).ToString(), Keywords = "home assistant ha smart home rest api template scene script light climate thermostat camera notify intent calendar 智能家居 家居助理" },
        new() { Tag = "module.comms", En = "Communications", Zh = "通訊", Glyph = ((char)0xE8BD).ToString(), Keywords = "communications mail email outlook mailto draft attach teams meeting call discord telegram slack phone link tel sms deep link 通訊 信件 電郵 草稿 會議 電話" },
        new() { Tag = "module.configbackup", En = "Config & Backup", Zh = "設定與備份", Glyph = ((char)0xE8F7).ToString(), Keywords = "config backup snapshot restore export import bundle zip git schedule mirror reg winget integrity 設定 備份 快照 還原 匯出 匯入 排程 鏡像" },
        new() { Tag = "module.native", En = "Native Utilities", Zh = "原生工具", Glyph = ((char)0xE950).ToString(), Keywords = "wifi password saved nearby scan smb shares sessions brightness ddc certificate users logoff disconnect gpu disk counters process modules bluetooth pinvoke wlan 原生 密碼 共享 亮度 憑證 藍牙 模組" },
        new() { Tag = "module.powertoys", En = "PowerToys Extras", Zh = "PowerToys 額外工具", Glyph = ((char)0xE945).ToString(), Keywords = "powertoys image resizer ocr text extractor always on top topmost paste plain text 圖片縮放 文字擷取 置頂 純文字" },
        new() { Tag = "module.wslvm", En = "WSL & VM Launcher", Zh = "WSL 與 VM 啟動器", Glyph = ((char)0xEC7A).ToString(), Keywords = "wsl linux distro ubuntu debian windows sandbox wsb virtual machine vm hyper-v export import 子系統 沙盒 虛擬機" },
        new() { Tag = "module.fonts", En = "Font Manager", Zh = "字型管理", Glyph = ((char)0xE8D2).ToString(), Keywords = "font fonts install preview uninstall ttf otf typeface typography 字型 字款 安裝 預覽 移除" },
        new() { Tag = "module.onedrive", En = "OneDrive", Zh = "OneDrive", Glyph = ((char)0xE753).ToString(), Keywords = "onedrive files on demand pin dehydrate online only cloud free space storage sense sync 雲端 釘選 脫水 釋放空間 同步 隨選" },
        new() { Tag = "module.timeunit", En = "Time & Unit Tools", Zh = "時間與單位工具", Glyph = ((char)0xE823).ToString(), Keywords = "time zone timezone world clock converter convert unit length mass temperature 時間 時區 世界時鐘 換算 單位" },
        new() { Tag = "module.settingshub", En = "Settings & Control Panel", Zh = "設定與控制台", Glyph = ((char)0xE713).ToString(), Keywords = "settings control panel ms-settings applet cpl launcher open page 設定 控制台 啟動器 面板" },
    };

    public static IEnumerable<ModuleInfo> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return All;
        var q = query.Trim().ToLowerInvariant();
        return All.Where(m => m.Haystack.Contains(q));
    }
}
