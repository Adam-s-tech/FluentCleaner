using FluentCleaner.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentCleaner.Views;

public sealed partial class NewCleanerDialog : ContentDialog
{
    public string EntryName    => nameBox.Text.Trim();
    public string EntryContent => contentBox.Text.Trim();
    public bool   IsScript     => btnPs1.IsChecked == true;

    public NewCleanerDialog(CustomEntryVm? existing)
    {
        InitializeComponent();

        // Localized strings that can't use x:Uid (ContentDialog buttons) or share plain code-side keys.
        PrimaryButtonText       = ResourceService.Get("DlgNewCleanerSave");
        CloseButtonText         = ResourceService.Get("DlgNewCleanerCancel");
        nameBox.Header          = ResourceService.Get("DlgNewCleanerNameHeader");
        nameBox.PlaceholderText = ResourceService.Get("DlgNewCleanerNamePlaceholder");
        promptBox.PlaceholderText = ResourceService.Get("DlgNewCleanerAiPlaceholder");
        contentLabel.Text       = ResourceService.Get("DlgNewCleanerContentLabel");
        testBtn.Content         = ResourceService.Get("DlgNewCleanerTest");

        Title = existing is null ? ResourceService.Get("DlgNewCleanerTitleNew") : ResourceService.Fmt("DlgNewCleanerTitleEdit", existing.Name);

        // Generation is available when the selected provider has a saved key
        // or its standard environment variable is present.
        aiRow.Visibility = AiExplainer.HasConfiguredKey ? Visibility.Visible : Visibility.Collapsed;

        if (existing is not null)
        {
            nameBox.Text     = existing.Name;
            btnIni.IsChecked = !existing.IsScript;
            btnPs1.IsChecked =  existing.IsScript;
            btnIni.IsEnabled = false;
            btnPs1.IsEnabled = false;

            try { contentBox.Text = File.ReadAllText(existing.FilePath); } catch { }
        }

        //Test only makes sense for ini;scripts can't be dry-run
        testBtn.Visibility = IsScript ? Visibility.Collapsed : Visibility.Visible;
    }

    // --- Event handlers --------------------------------------------------
    private void TypeBtn_Click(object sender, RoutedEventArgs e)
    {
        var isPs1 = sender == btnPs1;
        btnIni.IsChecked = !isPs1;
        btnPs1.IsChecked =  isPs1;
        testBtn.Visibility = isPs1 ? Visibility.Collapsed : Visibility.Visible;
    }

    // opens the current draft in Rule Lab so it can be dry-run before saving
    private void TestBtn_Click(object sender, RoutedEventArgs e)
    {
        if (IsScript) return;
        var name = EntryName.Length > 0 ? EntryName : "Draft";
        new RuleLabWindow(name, contentBox.Text, RequestedTheme).Activate();
    }

    private void TemplateBtn_Click(object sender, RoutedEventArgs e) =>
        contentBox.Text = IsScript ? Ps1Template : IniTemplate;

    private async void GenerateBtn_Click(object sender, RoutedEventArgs e)
    {
        var desc = promptBox.Text.Trim();
        if (string.IsNullOrEmpty(desc)) return;

        generateBtn.IsEnabled = false;
        generateBtn.Content   = ResourceService.Get("DlgNewCleanerGenerating");
        contentBox.Text       = IsScript
            ? await AiExplainer.GenerateScriptAsync(desc)
            : await AiExplainer.GenerateEntryAsync(desc);
        generateBtn.IsEnabled = true;
        generateBtn.Content   = ResourceService.Get("DlgNewCleanerGenerateBtn");
    }

    // --- Templates -------------------------------------------------------

    private const string IniTemplate =
        "[My App Name]\n" +
        "; Section: groups this entry on the Cleaner page (optional)\n" +
        "Section=Applications\n" +
        "\n" +
        "; DetectFile: entry only appears if this path exists (optional)\n" +
        "; Variables: %LocalAppData%  %AppData%  %Temp%  %WinDir%  %ProgramFiles%  %UserProfile%\n" +
        "DetectFile=%LocalAppData%\\MyApp\\*\n" +
        "\n" +
        "; FileKey: <folder path> | <file pattern> | optional flag\n" +
        ";   flag RECURSE    — delete files in all subfolders too\n" +
        ";   flag REMOVESELF — like RECURSE, also removes empty folders afterwards\n" +
        ";   multiple patterns: *.tmp;*.log\n" +
        "FileKey1=%LocalAppData%\\MyApp\\|*.log\n" +
        "FileKey2=%LocalAppData%\\MyApp\\Cache\\|*.*|RECURSE\n" +
        "FileKey3=%AppData%\\MyApp\\Temp\\|*.tmp;*.bak|REMOVESELF\n" +
        "\n" +
        "; RegKey: <HIVE>\\<SubKey>            — deletes the entire key\n" +
        "; RegKey: <HIVE>\\<SubKey>|ValueName  — deletes only that one value\n" +
        "; Hives: HKCU  HKLM  HKCR  HKU  HKCC\n" +
        "RegKey1=HKCU\\Software\\MyApp\\RecentFiles\n" +
        "RegKey2=HKCU\\Software\\MyApp\\Settings|LastSession";

    private const string Ps1Template =
        "# PowerShell Cleanup Script\n" +
        "# Paste your PowerShell code here.\n";
}
