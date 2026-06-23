using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinTune.Models;

namespace WinTune.Services;

/// <summary>
/// 建立調校項目嘅工廠 · Factory helpers for building <see cref="TweakDefinition"/>s declaratively.
///
/// 設計目標：分類目錄只需要提供「資料」（真實 registry 路徑／指令 + 雙語文字），
/// 唔使寫行為邏輯。Goal: catalog files supply data only (real registry paths/commands +
/// bilingual text); behaviour lives here.
/// </summary>
public static class Tweak
{
    private static string[] Keys(string? kw) => string.IsNullOrWhiteSpace(kw)
        ? Array.Empty<string>()
        : kw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// 由登錄檔值支援嘅開關 · A toggle backed by a registry value.
    /// offValue 為 null 表示「關」時刪除個值 · a null offValue deletes the value when turned off.
    /// </summary>
    public static TweakDefinition RegToggle(
        string id, string enT, string zhT, string enD, string zhD,
        RegRoot root, string path, string name,
        object onValue, object? offValue,
        RegistryValueKind kind = RegistryValueKind.DWord,
        bool requiresAdmin = false, RestartScope restart = RestartScope.None, string? keywords = null)
        => new()
        {
            Id = id,
            Title = new(enT, zhT),
            Description = new(enD, zhD),
            Kind = TweakKind.Toggle,
            RequiresAdmin = requiresAdmin,
            Restart = restart,
            Keywords = Keys(keywords),
            GetIsOn = () => RegistryHelper.ValueEquals(root, path, name, onValue),
            SetIsOn = on =>
            {
                if (on) RegistryHelper.SetValue(root, path, name, onValue, kind);
                else if (offValue is null) RegistryHelper.DeleteValue(root, path, name);
                else RegistryHelper.SetValue(root, path, name, offValue, kind);
            },
        };

    /// <summary>
    /// 自訂讀寫邏輯嘅開關 · A toggle with custom read/write logic (e.g. multiple registry values).
    /// </summary>
    public static TweakDefinition CustomToggle(
        string id, string enT, string zhT, string enD, string zhD,
        Func<bool> getIsOn, Action<bool> setIsOn,
        bool requiresAdmin = false, RestartScope restart = RestartScope.None, string? keywords = null)
        => new()
        {
            Id = id,
            Title = new(enT, zhT),
            Description = new(enD, zhD),
            Kind = TweakKind.Toggle,
            RequiresAdmin = requiresAdmin,
            Restart = restart,
            Keywords = Keys(keywords),
            GetIsOn = getIsOn,
            SetIsOn = setIsOn,
        };

    /// <summary>由登錄檔值支援嘅多選一 · A choice backed by a single registry value.</summary>
    public static TweakDefinition RegChoice(
        string id, string enT, string zhT, string enD, string zhD,
        RegRoot root, string path, string name, RegistryValueKind kind,
        (string en, string zh, object value)[] options,
        bool requiresAdmin = false, RestartScope restart = RestartScope.None, string? keywords = null)
    {
        var choices = new List<TweakChoice>();
        foreach (var o in options)
            choices.Add(new TweakChoice(new LocalizedText(o.en, o.zh), o.value.ToString()!));

        return new TweakDefinition
        {
            Id = id,
            Title = new(enT, zhT),
            Description = new(enD, zhD),
            Kind = TweakKind.Choice,
            RequiresAdmin = requiresAdmin,
            Restart = restart,
            Keywords = Keys(keywords),
            Choices = choices,
            GetCurrentChoice = () =>
            {
                foreach (var o in options)
                    if (RegistryHelper.ValueEquals(root, path, name, o.value))
                        return o.value.ToString();
                return null;
            },
            SetChoice = val =>
            {
                foreach (var o in options)
                    if (string.Equals(o.value.ToString(), val, StringComparison.OrdinalIgnoreCase))
                    {
                        RegistryHelper.SetValue(root, path, name, o.value, kind);
                        return;
                    }
            },
        };
    }

    /// <summary>一次性動作 · A one-shot action with a custom async body.</summary>
    public static TweakDefinition Action(
        string id, string enT, string zhT, string enD, string zhD,
        string enBtn, string zhBtn,
        Func<CancellationToken, Task<TweakResult>> run,
        bool requiresAdmin = false, bool destructive = false,
        RestartScope restart = RestartScope.None, string? keywords = null)
        => new()
        {
            Id = id,
            Title = new(enT, zhT),
            Description = new(enD, zhD),
            Kind = TweakKind.Action,
            RequiresAdmin = requiresAdmin,
            Destructive = destructive,
            Restart = restart,
            Keywords = Keys(keywords),
            ActionLabel = new(enBtn, zhBtn),
            RunAsync = run,
        };

    /// <summary>執行外部程序嘅動作 · An action that runs an external process.</summary>
    public static TweakDefinition Shell(
        string id, string enT, string zhT, string enD, string zhD,
        string enBtn, string zhBtn, string fileName, string arguments,
        bool requiresAdmin = false, bool destructive = false,
        RestartScope restart = RestartScope.None, string? keywords = null)
        => Action(id, enT, zhT, enD, zhD, enBtn, zhBtn,
            ct => ShellRunner.Run(fileName, arguments, requiresAdmin, ct),
            requiresAdmin, destructive, restart, keywords);

    /// <summary>執行 PowerShell 嘅動作 · An action that runs a PowerShell snippet.</summary>
    public static TweakDefinition Powershell(
        string id, string enT, string zhT, string enD, string zhD,
        string enBtn, string zhBtn, string script,
        bool requiresAdmin = false, bool destructive = false,
        RestartScope restart = RestartScope.None, string? keywords = null)
        => Action(id, enT, zhT, enD, zhD, enBtn, zhBtn,
            ct => ShellRunner.RunPowershell(script, requiresAdmin, ct),
            requiresAdmin, destructive, restart, keywords);

    /// <summary>執行 cmd 指令嘅動作 · An action that runs a cmd.exe command line.</summary>
    public static TweakDefinition Cmd(
        string id, string enT, string zhT, string enD, string zhD,
        string enBtn, string zhBtn, string command,
        bool requiresAdmin = false, bool destructive = false,
        RestartScope restart = RestartScope.None, string? keywords = null)
        => Action(id, enT, zhT, enD, zhD, enBtn, zhBtn,
            ct => ShellRunner.RunCmd(command, requiresAdmin, ct),
            requiresAdmin, destructive, restart, keywords);

    /// <summary>唯讀資訊項目 · A read-only information row.</summary>
    public static TweakDefinition Info(
        string id, string enT, string zhT, string enD, string zhD,
        Func<string> getter, string? keywords = null)
        => new()
        {
            Id = id,
            Title = new(enT, zhT),
            Description = new(enD, zhD),
            Kind = TweakKind.Info,
            Keywords = Keys(keywords),
            GetInfo = getter,
        };
}
