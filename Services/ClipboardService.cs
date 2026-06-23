using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace WinTune.Services;

public enum ClipKind { Text, Image, Files }

/// <summary>歷史入面一條剪貼簿項目 · One captured clipboard entry.</summary>
public sealed class ClipItem
{
    public ClipKind Kind { get; set; }
    public string Text { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public List<string> Files { get; set; } = new();
    public string Time { get; set; } = "";

    public string Preview => Kind switch
    {
        ClipKind.Text => Text.Length > 200 ? Text.Substring(0, 200) + "…" : Text,
        ClipKind.Image => ImagePath,
        ClipKind.Files => string.Join("\n", Files),
        _ => "",
    };
}

/// <summary>
/// 背景剪貼簿監察 + 歷史（PowerToys/Win+V 式，包圖片同檔案）· Background clipboard monitor + history.
/// Captures text, images (saved as PNG) and copied files; survives while WinTune runs in the tray.
/// Persists to %LocalAppData%\WinTune\clipboard. No redirect.
/// </summary>
public static class ClipboardService
{
    public static ObservableCollection<ClipItem> History { get; } = new();
    public static event Action? Changed;

    private static DispatcherQueue? _dq;
    private static bool _started;
    private static bool _suppress;
    private const int MaxItems = 200;

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public static string Dir
    {
        get
        {
            var d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinTune", "clipboard");
            Directory.CreateDirectory(d);
            return d;
        }
    }

    private static string Manifest => Path.Combine(Dir, "history.json");

    public static void Start(DispatcherQueue dq)
    {
        if (_started) return;
        _started = true;
        _dq = dq;
        Load();
        try { Clipboard.ContentChanged += OnContentChanged; } catch { }
    }

    private static void OnContentChanged(object? sender, object e)
    {
        if (_suppress) { _suppress = false; return; }
        _dq?.TryEnqueue(async () => await Capture());
    }

    private static async Task Capture()
    {
        try
        {
            var view = Clipboard.GetContent();

            if (view.Contains(StandardDataFormats.Bitmap))
            {
                var bmpRef = await view.GetBitmapAsync();
                using var ras = await bmpRef.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(ras);
                var sb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                var path = Path.Combine(Dir, $"clip-{Stamp()}.png");
                using (var fs = File.Open(path, FileMode.Create))
                using (var outStream = fs.AsRandomAccessStream())
                {
                    var enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outStream);
                    enc.SetSoftwareBitmap(sb);
                    await enc.FlushAsync();
                }
                Add(new ClipItem { Kind = ClipKind.Image, ImagePath = path, Time = Now() });
            }
            else if (view.Contains(StandardDataFormats.StorageItems))
            {
                var items = await view.GetStorageItemsAsync();
                var files = items.Select(i => i.Path).Where(p => !string.IsNullOrEmpty(p)).ToList();
                if (files.Count > 0)
                    Add(new ClipItem { Kind = ClipKind.Files, Files = files, Time = Now() });
            }
            else if (view.Contains(StandardDataFormats.Text))
            {
                var text = await view.GetTextAsync();
                if (!string.IsNullOrEmpty(text) &&
                    !(History.FirstOrDefault() is { Kind: ClipKind.Text } last && last.Text == text))
                    Add(new ClipItem { Kind = ClipKind.Text, Text = text, Time = Now() });
            }
        }
        catch { }
    }

    private static void Add(ClipItem item)
    {
        History.Insert(0, item);
        while (History.Count > MaxItems)
        {
            var drop = History[History.Count - 1];
            History.RemoveAt(History.Count - 1);
            TryDeleteImage(drop);
        }
        Save();
        Changed?.Invoke();
    }

    /// <summary>Put an item back on the clipboard.</summary>
    public static void CopyBack(ClipItem item)
    {
        try
        {
            var dp = new DataPackage();
            if (item.Kind == ClipKind.Text) dp.SetText(item.Text);
            else if (item.Kind == ClipKind.Image && File.Exists(item.ImagePath))
                dp.SetBitmap(Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(item.ImagePath)));
            else if (item.Kind == ClipKind.Files) dp.SetText(string.Join(Environment.NewLine, item.Files));
            else return;
            _suppress = true;
            Clipboard.SetContent(dp);
        }
        catch { _suppress = false; }
    }

    public static void Remove(ClipItem item)
    {
        History.Remove(item);
        TryDeleteImage(item);
        Save();
        Changed?.Invoke();
    }

    public static void Clear()
    {
        foreach (var i in History) TryDeleteImage(i);
        History.Clear();
        Save();
        Changed?.Invoke();
    }

    /// <summary>Convert an image item to another format; returns the saved path.</summary>
    public static async Task<string> SaveImageAs(ClipItem item, string ext)
    {
        if (item.Kind != ClipKind.Image || !File.Exists(item.ImagePath)) throw new InvalidOperationException("not an image");
        var encoderId = ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => BitmapEncoder.JpegEncoderId,
            ".bmp" => BitmapEncoder.BmpEncoderId,
            ".gif" => BitmapEncoder.GifEncoderId,
            ".tif" or ".tiff" => BitmapEncoder.TiffEncoderId,
            _ => BitmapEncoder.PngEncoderId,
        };
        var outPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), $"WinTune-{Stamp()}{ext}");

        using var inFs = File.OpenRead(item.ImagePath);
        using var inRas = inFs.AsRandomAccessStream();
        var decoder = await BitmapDecoder.CreateAsync(inRas);
        var sb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

        using (var outFs = File.Open(outPath, FileMode.Create))
        using (var outRas = outFs.AsRandomAccessStream())
        {
            var enc = await BitmapEncoder.CreateAsync(encoderId, outRas);
            enc.SetSoftwareBitmap(sb);
            await enc.FlushAsync();
        }
        return outPath;
    }

    private static void TryDeleteImage(ClipItem item)
    {
        if (item.Kind == ClipKind.Image && File.Exists(item.ImagePath))
            try { File.Delete(item.ImagePath); } catch { }
    }

    private static void Save()
    {
        try { File.WriteAllText(Manifest, JsonSerializer.Serialize(History.ToList(), JsonOpts)); } catch { }
    }

    private static void Load()
    {
        try
        {
            if (!File.Exists(Manifest)) return;
            var list = JsonSerializer.Deserialize<List<ClipItem>>(File.ReadAllText(Manifest), JsonOpts);
            if (list is null) return;
            History.Clear();
            foreach (var i in list) History.Add(i);
        }
        catch { }
    }

    private static string Stamp() => DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
    private static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}
