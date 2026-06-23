using WinTune.Models;
using WinTune.Services;

namespace WinTune.Catalog;

/// <summary>
/// 建立壓縮操作 · Factory for 7-Zip operations on the selected archive/source.
/// 由 <see cref="ArchiveOperations"/> 目錄使用。Used by the generated <see cref="ArchiveOperations"/> catalog.
/// </summary>
public static class ArchiveTweak
{
    /// <summary>執行一條 7z 指令（args 內可用 {archive} {src} {outdir} 佔位符）。</summary>
    public static TweakDefinition Op(string id, string enT, string zhT, string enD, string zhD,
        string enBtn, string zhBtn, string args, bool needsArchive = true, bool needsSource = false,
        bool destructive = false, string? keywords = null)
        => Tweak.Action(id, enT, zhT, enD, zhD, enBtn, zhBtn,
            ct => ArchiveService.RunArgs(args, needsArchive, needsSource, ct),
            requiresAdmin: false, destructive: destructive, keywords: keywords);
}
