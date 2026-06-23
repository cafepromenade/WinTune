using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WinTune.Services;

/// <summary>
/// 簡單嘅 JSON 設定儲存（適用於未封裝嘅 app）。
/// Lightweight JSON settings store written to %LOCALAPPDATA%\WinTune\settings.json,
/// so it works for unpackaged WinUI apps (no package identity required).
/// </summary>
public static class SettingsStore
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinTune");

    private static readonly string FilePath = Path.Combine(Dir, "settings.json");

    private static readonly object Gate = new();
    private static Dictionary<string, string> _cache = Load();

    private static Dictionary<string, string> Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }
        catch { /* 損壞就重來 · ignore corrupt file */ }
        return new();
    }

    public static string Get(string key, string fallback)
    {
        lock (Gate)
        {
            return _cache.TryGetValue(key, out var v) ? v : fallback;
        }
    }

    public static void Set(string key, string value)
    {
        lock (Gate)
        {
            _cache[key] = value;
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_cache,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* best effort */ }
        }
    }
}
