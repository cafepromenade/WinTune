using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WinTune.Catalog;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>
/// 為每一個功能寫一個 Markdown 檔 · Writes one Markdown file per feature.
/// 透過 `WinTune.exe --export-docs &lt;dir&gt;` 觸發（無視窗，寫完即退出）。
/// Triggered by `WinTune.exe --export-docs &lt;dir&gt;` (headless; exits when done).
/// </summary>
public static class DocsExporter
{
    private static readonly UTF8Encoding Utf8 = new(false);

    public static int Export(string dir)
    {
        Directory.CreateDirectory(dir);

        var all = new List<(TweakDefinition t, string module)>();
        foreach (var t in TweakCatalog.All)
            all.Add((t, $"{t.Category.Name.En} · {t.Category.Name.Zh}"));
        foreach (var t in GitOperations.All())
            all.Add((t, "Git & GitHub · Git 與 GitHub"));
        foreach (var t in ArchiveOperations.All())
            all.Add((t, "Archives · 壓縮檔"));

        foreach (var (t, module) in all)
            File.WriteAllText(Path.Combine(dir, Sanitize(t.Id) + ".md"), BuildMd(t, module), Utf8);

        WriteIndex(dir, all);
        return all.Count;
    }

    private static string BuildMd(TweakDefinition t, string module)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {t.Title.En} · {t.Title.Zh}");
        sb.AppendLine();
        sb.AppendLine("| Field · 欄位 | Value · 值 |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| **ID** | `{t.Id}` |");
        sb.AppendLine($"| **Module · 模組** | {module} |");
        sb.AppendLine($"| **Type · 種類** | {t.Kind} |");
        sb.AppendLine($"| **Administrator · 管理員** | {(t.RequiresAdmin ? "Yes · 需要" : "No · 唔使")} |");
        sb.AppendLine($"| **Destructive · 具破壞性** | {(t.Destructive ? "Yes · 係" : "No · 唔係")} |");
        sb.AppendLine($"| **Restart · 重啟** | {t.Restart} |");
        if (t.ActionLabel is not null)
            sb.AppendLine($"| **Action · 動作** | {t.ActionLabel.En} · {t.ActionLabel.Zh} |");
        sb.AppendLine();
        sb.AppendLine("## English");
        sb.AppendLine(t.Description.En);
        sb.AppendLine();
        sb.AppendLine("## 粵語");
        sb.AppendLine(t.Description.Zh);
        if (t.Keywords.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine($"_Keywords · 關鍵字: {string.Join(", ", t.Keywords)}_");
        }
        sb.AppendLine();
        sb.AppendLine("_Part of WinTune · WinTune 套件嘅一部分_");
        return sb.ToString();
    }

    private static void WriteIndex(string dir, List<(TweakDefinition t, string module)> all)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# WinTune — Feature Reference · 功能總覽");
        sb.AppendLine();
        sb.AppendLine($"**{all.Count}** features, each with its own page. · **{all.Count}** 項功能，每項一頁。");
        sb.AppendLine();
        foreach (var grp in all.GroupBy(x => x.module).OrderBy(g => g.Key))
        {
            sb.AppendLine($"## {grp.Key} ({grp.Count()})");
            foreach (var (t, _) in grp.OrderBy(x => x.t.Id))
                sb.AppendLine($"- [{t.Title.En} · {t.Title.Zh}]({Sanitize(t.Id)}.md)");
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(dir, "README.md"), sb.ToString(), Utf8);
    }

    private static string Sanitize(string id)
    {
        var chars = id.Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '-').ToArray();
        return new string(chars);
    }
}
