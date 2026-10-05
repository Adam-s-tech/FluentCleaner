using FluentCleaner.Services;
using System.Collections.ObjectModel;

namespace FluentCleaner.ViewModels;

//Self-contained CLI module for AppX debloating.
// CliViewModel owns one instance and forwards every "appx ..." command here.
// No winapp2 types, no CleanerEntry;this is now fully standalone baby!
public class CliDebloatModule
{
    private static readonly string WinappxPath =
        Path.Combine(AppContext.BaseDirectory, "Winappx.ini");

    // Cached after InitAsync; also used by GetSuggestions
    private List<AppxEntry>? _entries;

    // --- Init -------------------------------------------------------------------

    // Pre-load Winappx.ini so autocomplete works before the first appx command
    public async Task InitAsync()
    {
        if (AppSettings.Instance.EnableWinappx && File.Exists(WinappxPath))
            _entries = await AppxService.ParseDatabaseAsync(WinappxPath);
    }

    // --- Autocomplete -----------------------------------------------------------

    // Returns the completable part after "appx ";CliViewModel prepends the prefix.
    // "remove F"  : ["remove Firefox", "remove Family Safety", ...]
    // "sc"        : ["scan"]
    public IEnumerable<string> GetSuggestions(string query)
    {
        if (query.StartsWith("remove ", StringComparison.OrdinalIgnoreCase))
        {
            var nameQuery = query["remove ".Length..];
            return (_entries?.Select(e => e.Name) ?? [])
                .Where(n => n.Contains(nameQuery, StringComparison.OrdinalIgnoreCase))
                .Select(n => "remove " + n);
        }

        return new[] { "list", "scan", "remove all" }
            .Where(n => n.StartsWith(query, StringComparison.OrdinalIgnoreCase));
    }

    // --- Command dispatch -------------------------------------------------------

    // setBusy is a delegate so the module can toggle CliViewModel.IsBusy
    // without knowing anything about the ViewModel itself.
    public async Task ExecuteAsync(string arg, ObservableCollection<string> output, Action<bool> setBusy)
    {
        var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var sub   = parts.Length > 0 ? parts[0].ToLowerInvariant() : "";
        var name  = parts.Length > 1 ? parts[1] : "";

        switch (sub)
        {
            case "list":   await ListAllAsync(output, setBusy);       break;
            case "scan":   await ScanAsync(output, setBusy);          break;
            case "remove": await RemoveAsync(name, output, setBusy);  break;
            default:
                output.Add(ResourceService.Get("CLI_AppxUsage"));
                break;
        }
    }

    // --- Commands ---------------------------------------------------------------

    // Lists every AppX package currently installed on the system (no filter)
    private async Task ListAllAsync(ObservableCollection<string> output, Action<bool> setBusy)
    {
        setBusy(true);
        output.Add(ResourceService.Get("CLI_AppxListStart"));
        var names = await AppxService.GetInstalledNamesAsync();

        if (names.Count == 0)
        {
            output.Add(ResourceService.Get("CLI_AppxNoPackages"));
            setBusy(false);
            return;
        }

        foreach (var n in names)
            output.Add($"  {n}");
        output.Add(ResourceService.Fmt("CLI_AppxListDone", names.Count));
        setBusy(false);
    }

    // Lists Winappx.ini entries that are currently installed (bloatware only)
    private async Task ScanAsync(ObservableCollection<string> output, Action<bool> setBusy)
    {
        var entries = await GetEntriesAsync(output);
        if (entries.Count == 0) return;

        setBusy(true);
        output.Add(ResourceService.Get("CLI_AppxScanStart"));
        var found = await AppxService.ScanInstalledAsync(entries);

        if (found.Count == 0)
        {
            output.Add(ResourceService.Get("CLI_AppxNothingFound"));
            setBusy(false);
            return;
        }

        foreach (var e in found)
            output.Add($"  {e.Name}  [{e.PackageName}]");
        output.Add(ResourceService.Fmt("CLI_AppxScanDone", found.Count));
        setBusy(false);
    }

    // Removes a single named entry or every installed entry from the list
    private async Task RemoveAsync(string name, ObservableCollection<string> output, Action<bool> setBusy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            output.Add(ResourceService.Get("CLI_AppxRemoveUsage"));
            return;
        }

        var entries = await GetEntriesAsync(output);
        if (entries.Count == 0) return;

        List<AppxEntry> targets;

        if (name.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            output.Add(ResourceService.Get("CLI_AppxRemoveScanStart"));
            targets = await AppxService.ScanInstalledAsync(entries);
            if (targets.Count == 0) { output.Add(ResourceService.Get("CLI_AppxNothingToRemove")); return; }
        }
        else
        {
            // Exact match first, then partial against Winappx.ini display names
            var entry = entries.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                     ?? entries.FirstOrDefault(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            // Fall back: treat the raw input as a package name (e.g. from 'appx list')
            entry ??= new AppxEntry(name, name, null);

            targets = [entry];
        }

        setBusy(true);
        int removed = 0;

        foreach (var e in targets)
        {
            if (e.Warning is not null) output.Add($"  [!] {e.Warning}");
            output.Add(ResourceService.Fmt("CLI_AppxRemoving", e.Name));

            var ok = await AppxService.RemoveAsync(e);
            if (ok) { output.Add(ResourceService.Fmt("CLI_AppxRemoveOk", e.Name)); removed++; }
            else    output.Add(ResourceService.Fmt("CLI_AppxRemoveErr", e.Name));
        }

        output.Add(removed > 0
            ? ResourceService.Fmt("CLI_AppxRemoveDone", removed)
            : ResourceService.Get("CLI_AppxNothingRemoved"));
        setBusy(false);
    }

    // --- Helpers ----------------------------------------------------------------

    // Resolves _entries, loading from disk on first call if InitAsync was skipped.
    // Always re-checks EnableWinappx so toggling it in Settings takes effect immediately;
    // no restart required. The cache is only used when the setting is enabled.
    private async Task<List<AppxEntry>> GetEntriesAsync(ObservableCollection<string> output)
    {
        if (!AppSettings.Instance.EnableWinappx)
        {
            _entries = null;   // drop cache so re-enable picks up a fresh load
            output.Add(ResourceService.Get("CLI_AppxDisabled"));
            return [];
        }
        if (_entries is not null) return _entries;
        if (!File.Exists(WinappxPath))
        {
            output.Add(ResourceService.Get("CLI_AppxNotFound"));
            return [];
        }
        _entries = await AppxService.ParseDatabaseAsync(WinappxPath);
        return _entries;
    }
}
