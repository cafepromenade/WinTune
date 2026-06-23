using WinTune.Models;

namespace WinTune.Catalog;

/// <summary>
/// 全部調校分類 · The fixed set of tweak categories, with bilingual names and Segoe Fluent glyphs.
/// </summary>
public static class Categories
{
    public static readonly AppCategory Appearance = new()
    {
        Id = "appearance",
        Name = new("Appearance & Personalisation", "外觀與個人化"),
        Blurb = new("Dark mode, accent colour, transparency, animations and visual effects.",
            "深色模式、強調色、透明度、動畫同視覺特效。"),
        Glyph = "",
    };

    public static readonly AppCategory Explorer = new()
    {
        Id = "explorer",
        Name = new("File Explorer", "檔案總管"),
        Blurb = new("Show file extensions, hidden files, classic menus and Explorer behaviour.",
            "顯示副檔名、隱藏檔案、經典選單同檔案總管行為。"),
        Glyph = "",
    };

    public static readonly AppCategory Taskbar = new()
    {
        Id = "taskbar",
        Name = new("Taskbar & Start", "工作列與開始功能表"),
        Blurb = new("Taskbar alignment, Search, Widgets, Task View and Start menu layout.",
            "工作列對齊、搜尋、小工具、工作檢視同開始功能表版面。"),
        Glyph = "",
    };

    public static readonly AppCategory Privacy = new()
    {
        Id = "privacy",
        Name = new("Privacy & Telemetry", "私隱與遙測"),
        Blurb = new("Advertising ID, telemetry, activity history, location and tailored ads.",
            "廣告 ID、遙測、活動記錄、定位同個人化廣告。"),
        Glyph = "",
    };

    public static readonly AppCategory Performance = new()
    {
        Id = "performance",
        Name = new("Performance & Power", "效能與電源"),
        Blurb = new("Power plans, hibernation, fast startup, game mode and responsiveness.",
            "電源計劃、休眠、快速啟動、遊戲模式同反應速度。"),
        Glyph = "",
    };

    public static readonly AppCategory Network = new()
    {
        Id = "network",
        Name = new("Network & Internet", "網絡與互聯網"),
        Blurb = new("Flush DNS, reset Winsock, change DNS servers and inspect connections.",
            "清 DNS、重設 Winsock、轉 DNS 伺服器同檢視連線。"),
        Glyph = "",
    };

    public static readonly AppCategory Cleanup = new()
    {
        Id = "cleanup",
        Name = new("Cleanup & Storage", "清理與儲存"),
        Blurb = new("Temp files, caches, Recycle Bin, Windows Update cache and thumbnails.",
            "暫存檔、快取、回收筒、Windows Update 快取同縮圖。"),
        Glyph = "",
    };

    public static readonly AppCategory Security = new()
    {
        Id = "security",
        Name = new("Security", "安全"),
        Blurb = new("UAC, Defender, SmartScreen, firewall and account protections.",
            "UAC、Defender、SmartScreen、防火牆同帳戶保護。"),
        Glyph = "",
    };

    public static readonly AppCategory System = new()
    {
        Id = "system",
        Name = new("System & Boot", "系統與開機"),
        Blurb = new("Long paths, restore points, boot options, clipboard and developer mode.",
            "長路徑、還原點、開機選項、剪貼簿同開發人員模式。"),
        Glyph = "",
    };

    public static readonly AppCategory Apps = new()
    {
        Id = "apps",
        Name = new("Apps & Startup", "應用程式與啟動"),
        Blurb = new("Startup items, winget upgrades, running processes and Explorer restart.",
            "啟動項目、winget 更新、執行中程序同重啟檔案總管。"),
        Glyph = "",
    };

    public static readonly AppCategory PowerTools = new()
    {
        Id = "powertools",
        Name = new("Power Tools", "進階工具"),
        Blurb = new("God Mode, hosts file, restart to UEFI, system repair and quick power actions.",
            "上帝模式、hosts 檔、重啟入 UEFI、系統修復同快速電源動作。"),
        Glyph = "",
    };

    public static readonly AppCategory Launcher = new()
    {
        Id = "launcher",
        Name = new("Launcher & Elevation", "啟動器與提權"),
        Blurb = new("Create a no-UAC elevated launcher via Task Scheduler, and run the suite as admin.",
            "用工作排程器整一個免 UAC 提權啟動器，以管理員身分運行套件。"),
        Glyph = "",
    };

    public static readonly AppCategory Info = new()
    {
        Id = "info",
        Name = new("System Information", "系統資訊"),
        Blurb = new("Live read-out of OS build, CPU, RAM, GPU, disk, uptime and activation.",
            "即時顯示系統版本、CPU、RAM、GPU、磁碟、運行時間同啟用狀態。"),
        Glyph = "",
    };

    /// <summary>顯示次序 · Display order in the navigation pane.</summary>
    public static readonly AppCategory[] All =
    {
        Appearance, Explorer, Taskbar, Privacy, Performance, Network,
        Cleanup, Security, System, Apps, PowerTools, Launcher, Info,
    };
}
