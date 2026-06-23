using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinTune.Services;

namespace WinTune.Pages;

/// <summary>
/// 應用程式內環境變數編輯器（PowerToys 式）· In-app Environment Variables editor — view/add/edit/delete
/// User and System variables. System scope needs admin. No redirect. Bilingual.
/// </summary>
public sealed partial class EnvVarsModule : Page
{
    public sealed class VarRow
    {
        public string Name { get; init; } = "";
        public string Value { get; init; } = "";
    }

    private readonly ObservableCollection<VarRow> _rows = new();

    public EnvVarsModule()
    {
        InitializeComponent();
        List.ItemsSource = _rows;
        Loc.I.LanguageChanged += (_, _) => Render();
        Loaded += (_, _) => { Render(); RefreshList(); };
    }

    private string P(string en, string zh) => Loc.I.Pick(en, zh);
    private bool Machine => TargetBox.SelectedIndex == 1;

    private void Render()
    {
        HeaderTitle.Text = "Environment Variables · 環境變數";
        HeaderBlurb.Text = P("View, add, edit and delete environment variables. User variables apply without admin; System variables need WinTune running as administrator.",
            "檢視、新增、編輯同刪除環境變數。使用者變數唔使管理員；系統變數要 WinTune 以管理員身分執行。");
        NameBox.PlaceholderText = P("Variable name", "變數名稱");
        ValueBox.PlaceholderText = P("Value", "值");
        AddBtn.Content = P("Set", "設定");
        ColName.Text = P("Name", "名稱");
        ColValue.Text = P("Value", "值");

        int sel = TargetBox.SelectedIndex < 0 ? 0 : TargetBox.SelectedIndex;
        TargetBox.Items.Clear();
        TargetBox.Items.Add(P("User variables", "使用者變數"));
        TargetBox.Items.Add(P("System variables (admin)", "系統變數（管理員）"));
        TargetBox.SelectedIndex = sel;
    }

    private void RefreshList()
    {
        _rows.Clear();
        foreach (var v in EnvVarService.List(Machine))
            _rows.Add(new VarRow { Name = v.Name, Value = v.Value });
        ColValue.Text = P($"Value — {_rows.Count}", $"值 — {_rows.Count} 個");
    }

    private void Target_Changed(object sender, SelectionChangedEventArgs e) { if (IsLoaded) RefreshList(); }
    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshList();

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        string name = (NameBox.Text ?? "").Trim();
        string value = ValueBox.Text ?? "";
        if (name.Length == 0) { Show(InfoBarSeverity.Warning, P("Enter a variable name", "請輸入變數名稱"), ""); return; }
        try
        {
            EnvVarService.Set(name, value, Machine);
            Show(InfoBarSeverity.Success, P("Saved", "已儲存"), $"{name} = {value}");
            NameBox.Text = ""; ValueBox.Text = "";
            RefreshList();
        }
        catch (Exception ex)
        {
            Show(InfoBarSeverity.Error, P("Could not save", "儲存唔到"),
                Machine ? P("System variables need administrator rights — relaunch WinTune as admin.", "系統變數要管理員權限 — 請以管理員身分重開 WinTune。") : ex.Message);
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is VarRow row)
        {
            NameBox.Text = row.Name;
            ValueBox.Text = row.Value;
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not VarRow row) return;
        try { EnvVarService.Delete(row.Name, Machine); RefreshList(); }
        catch (Exception ex)
        {
            Show(InfoBarSeverity.Error, P("Could not delete", "刪唔到"),
                Machine ? P("System variables need administrator rights.", "系統變數要管理員權限。") : ex.Message);
        }
    }

    private void Show(InfoBarSeverity sev, string title, string msg)
    {
        ResultBar.Severity = sev; ResultBar.Title = title; ResultBar.Message = msg; ResultBar.IsOpen = true;
    }
}
