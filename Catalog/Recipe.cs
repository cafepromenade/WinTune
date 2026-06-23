using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinTune.Models;
using WinTune.Services;

namespace WinTune.Catalog;

/// <summary>
/// 一鍵流程 · "Recipes": one button that runs several real steps in sequence and reports each.
/// </summary>
public static class Recipe
{
    public delegate Task<TweakResult> Step(CancellationToken ct);

    public static (string label, Step run) Cmd(string label, string command, bool admin = false)
        => (label, ct => ShellRunner.RunCmd(command, admin, ct));

    public static (string label, Step run) Ps(string label, string script, bool admin = false)
        => (label, ct => ShellRunner.RunPowershell(script, admin, ct));

    public static (string label, Step run) Reg(string label, Action act)
        => (label, ct =>
        {
            act();
            return Task.FromResult(TweakResult.Ok("set", "已設定"));
        });

    public static TweakDefinition Make(string id, string enT, string zhT, string enD, string zhD,
        string enBtn, string zhBtn, bool admin, bool destructive, params (string label, Step run)[] steps)
        => Tweak.Action(id, enT, zhT, enD, zhD, enBtn, zhBtn,
            async ct =>
            {
                var sb = new StringBuilder();
                bool ok = true;
                int i = 0;
                foreach (var s in steps)
                {
                    ct.ThrowIfCancellationRequested();
                    i++;
                    try
                    {
                        var r = await s.run(ct);
                        sb.AppendLine($"{(r.Success ? "✓" : "✗")} [{i}/{steps.Length}] {s.label}");
                        if (!r.Success) ok = false;
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"✗ [{i}/{steps.Length}] {s.label}: {ex.Message}");
                        ok = false;
                    }
                }
                return new TweakResult(ok,
                    new LocalizedText(ok ? "Recipe complete." : "Recipe finished with some failures.",
                        ok ? "流程完成。" : "流程做完，但有部分失敗。"),
                    sb.ToString().TrimEnd());
            },
            requiresAdmin: admin, destructive: destructive);
}
