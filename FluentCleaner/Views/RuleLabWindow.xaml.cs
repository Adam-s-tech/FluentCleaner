using FluentCleaner.Models;
using FluentCleaner.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace FluentCleaner.Views;

/* Rule Lab ; a standalone window that inspects and tests a Winapp2 ini block.
   The rules are editable and a live Dry Run re-parses whatever is in the box and
   scans it read-only (AnalyzeAsync deletes nothing), so a rule can be tweaked and
   the hits previewed instantly. Nothing is ever written back to the database.
 */
public sealed partial class RuleLabWindow : Window
{
    //cancels an in-flight Dry Run;a broad RECURSE scan shouldn't outlive the window
    private CancellationTokenSource? _cts;
    private bool _scanning;   //while true the Dry Run button acts as Cancel

    //title is the entry name, rawIni the block to inspect, theme so we match the app
    public RuleLabWindow(string title, string rawIni, ElementTheme theme)
    {
        InitializeComponent();

        Root.RequestedTheme = theme;
        Title               = ResourceService.Fmt("RuleLabTitle", title);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(860, 580));

        //open dead center on the main window, not wherever Windows drops it
        if ((Application.Current as App)?.MainWindow?.AppWindow is { } owner)
            AppWindow.Move(new Windows.Graphics.PointInt32(
                owner.Position.X + (owner.Size.Width  - 860) / 2,
                owner.Position.Y + (owner.Size.Height - 580) / 2));

        //ditch the default chrome for our own icon+title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppTitleBar.Subtitle = title;   //the entry name next to "Rule Lab"

        //caption buttons follow the SYSTEM theme, not our forced one
        var caption = AppWindow.TitleBar;
        caption.ButtonBackgroundColor         = Microsoft.UI.Colors.Transparent;
        caption.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        caption.PreferredTheme = theme switch
        {
            ElementTheme.Light => TitleBarTheme.Light,
            ElementTheme.Dark  => TitleBarTheme.Dark,
            _                  => TitleBarTheme.UseDefaultAppMode
        };

        DryRunButton.Content = ResourceService.Get("DlgSourceDryRun");
        Summary.Text         = ResourceService.Get("DlgSourceHint");
        CopyButton.Content   = ResourceService.Get("DlgSourceCopy");
        ReportButton.Content = ResourceService.Get("DlgSourceReport");
        CloseButton.Content  = ResourceService.Get("DlgExplainClose");

        //WinUI TextBox shows only the first line when the text carries \r\n;normalize to \n
        SourceBox.Text = rawIni.Replace("\r\n", "\n").Replace("\r", "\n");

        //close the window mid-scan ; kill the scan too
        Closed += (_, _) => { _cts?.Cancel(); _cts?.Dispose(); };
    }

    private async void DryRun_Click(object sender, RoutedEventArgs e)
    {
        //second click while scanning = cancel;the button doubles as Cancel
        if (_scanning)
        {
            _cts?.Cancel();
            return;
        }

        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _scanning            = true;
        DryRunButton.Content = ResourceService.Get("DlgSourceCancel");
        Ring.IsActive        = true;
        Summary.Text         = ResourceService.Get("DlgSourceScanning");
        ResultsBox.Text      = "";

        try
        {
            var parsed = new Winapp2Parser().Parse(SourceBox.Text);
            if (parsed.Count == 0)
            {
                Summary.Text = ResourceService.Get("DlgSourceNoEntry");
                return;
            }

            //scan every section, not just the first;a custom file often bundles many
            var svc        = new CleaningService();
            var lines      = new List<string>();
            long totalBytes = 0;
            int  totalFiles = 0;

            foreach (var entry in parsed)
            {
                //read-only scan;walks the keys, touches nothing
                var res = await svc.AnalyzeAsync(entry, token: token);
                totalFiles += res.FilesToDelete.Count;
                totalBytes += res.TotalBytes;

                if (res.FilesToDelete.Count == 0 && res.RegistryToDelete.Count == 0)
                    continue;

                //group hits under the section name so multi-entry files stay readable
                lines.Add("[" + entry.Name + "]");
                lines.AddRange(res.FilesToDelete.Take(1000));
                foreach (var rk in res.RegistryToDelete)
                    lines.Add("[reg] " + rk.KeyPath + (rk.ValueName is null ? "" : "\\" + rk.ValueName));
                if (res.FilesToDelete.Count > 1000)
                    lines.Add(ResourceService.Fmt("DlgSourceMore", res.FilesToDelete.Count - 1000));
                lines.Add("");
            }

            //join with \n;\r\n would collapse the box to a single line
            ResultsBox.Text = string.Join("\n", lines);
            Summary.Text    = ResourceService.Fmt("DlgSourceFound", totalFiles, ScanResult.FormatBytes(totalBytes));
        }
        catch (OperationCanceledException)
        {
            Summary.Text = ResourceService.Get("DlgSourceCancelled");
        }
        catch (Exception ex)
        {
            Summary.Text = ResourceService.Fmt("DlgSourceError", ex.Message);
        }
        finally
        {
            _scanning            = false;
            DryRunButton.Content = ResourceService.Get("DlgSourceDryRun");
            Ring.IsActive        = false;
        }
    }

    //Copy the current (edited) rules
    private void Copy_Click(object sender, RoutedEventArgs e) => CopyToClipboard(SourceBox.Text);

    //Report: copy then open the issues page ready to paste
    private async void Report_Click(object sender, RoutedEventArgs e)
    {
        CopyToClipboard(SourceBox.Text);
        await AppLinks.OpenAsync(AppLinks.Issues);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    // drops text on the clipboard;used by Copy and Report
    private static void CopyToClipboard(string text)
    {
        var data = new DataPackage();
        data.SetText(text);
        Clipboard.SetContent(data);
    }
}
