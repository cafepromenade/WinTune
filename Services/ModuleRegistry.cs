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
        new() { Tag = "module.services", En = "Services", Zh = "服務", Glyph = "", Keywords = "services start stop startup type 服務" },
        new() { Tag = "module.tasks", En = "Scheduled Tasks", Zh = "排程工作", Glyph = "", Keywords = "scheduled task scheduler run 排程" },
        new() { Tag = "module.devices", En = "Devices", Zh = "裝置", Glyph = "", Keywords = "device manager hardware driver 裝置 驅動" },
        new() { Tag = "module.startup", En = "Startup Apps", Zh = "開機程式", Glyph = "", Keywords = "startup autostart logon run 開機 自啟動" },
        new() { Tag = "module.rename", En = "Batch Rename", Zh = "批次改名", Glyph = "", Keywords = "rename bulk powerrename regex 改名 批次" },
        new() { Tag = "module.bulkops", En = "Bulk File Ops", Zh = "批次檔案操作", Glyph = "", Keywords = "bulk file move copy delete attributes 批次 檔案" },
        new() { Tag = "module.duplicates", En = "Duplicate Finder", Zh = "重複檔案搜尋", Glyph = "", Keywords = "duplicate hash find dedupe 重複" },
        new() { Tag = "module.disk", En = "Disk Analyser", Zh = "磁碟分析", Glyph = "", Keywords = "disk space treemap analyse folder size 磁碟 空間" },
        new() { Tag = "module.drives", En = "Drives", Zh = "磁碟機", Glyph = "", Keywords = "drive volume format bitlocker 磁碟機" },
        new() { Tag = "module.uninstall", En = "App Uninstaller", Zh = "應用程式解除安裝", Glyph = "", Keywords = "uninstall remove app program winget 解除安裝" },
        new() { Tag = "module.windows", En = "Window Manager", Zh = "視窗管理", Glyph = "", Keywords = "window tile cascade always on top 視窗" },
        new() { Tag = "module.keyboard", En = "Keyboard Remapper", Zh = "鍵盤重新對應", Glyph = "", Keywords = "keyboard remap key sharpkeys 鍵盤" },
        new() { Tag = "module.hosts", En = "Hosts Editor", Zh = "hosts 編輯器", Glyph = "", Keywords = "hosts block domain dns 封鎖" },
        new() { Tag = "module.mouse", En = "Mouse & Pointer", Zh = "滑鼠與指標", Glyph = "", Keywords = "mouse pointer acceleration speed 滑鼠 指標" },
        new() { Tag = "module.recorder", En = "Screen Recorder", Zh = "螢幕錄影", Glyph = "", Keywords = "record screen capture gdigrab 錄影" },
        new() { Tag = "module.monitor", En = "System Monitor", Zh = "系統監察", Glyph = "", Keywords = "cpu ram memory network task manager priority affinity 監察 工作管理員" },
        new() { Tag = "module.connections", En = "Connections", Zh = "連線", Glyph = "", Keywords = "tcp udp connections netstat tcpview port 連線" },
        new() { Tag = "module.events", En = "Event Viewer", Zh = "事件檢視器", Glyph = "", Keywords = "event log viewer system application 事件 記錄" },
        new() { Tag = "module.mixer", En = "Volume Mixer", Zh = "音量混合器", Glyph = "", Keywords = "volume mixer audio per-app mute 音量 靜音" },
        new() { Tag = "module.contextmenu", En = "Context Menu", Zh = "右鍵選單", Glyph = "", Keywords = "context menu right click verb 右鍵 選單" },
        new() { Tag = "module.awake", En = "Awake", Zh = "保持喚醒", Glyph = "", Keywords = "awake keep awake no sleep caffeine 唔瞓 喚醒" },
        new() { Tag = "module.colorpicker", En = "Color Picker", Zh = "螢幕取色", Glyph = "", Keywords = "color picker hex rgb hsl eyedropper 取色 顏色" },
        new() { Tag = "module.envvars", En = "Environment Variables", Zh = "環境變數", Glyph = "", Keywords = "environment variables path user system env 環境變數" },
    };

    public static IEnumerable<ModuleInfo> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return All;
        var q = query.Trim().ToLowerInvariant();
        return All.Where(m => m.Haystack.Contains(q));
    }
}
