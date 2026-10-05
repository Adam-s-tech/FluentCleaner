# FluentCleaner Classic | User Guide

> Old-school shell. Current cleaning engine.

FluentCleaner Classic is the small WinForms edition of FluentCleaner for Windows. It uses .NET Framework 4.8, starts without a bundled runtime, and keeps its interface deliberately direct: choose what to inspect, analyze it, review the result, then decide whether to clean.

The visual starting point is the CCleaner 2.x era. That familiarity is intentional, but the technology underneath is not frozen in 2006. Classic uses the shared `FluentCleaner.Core` parser and models, current Winapp2 databases, runtime JSON localization, modern DPI handling, and a two-phase scan/clean engine written in C#.

FluentCleaner is not affiliated with Piriform, CCleaner, or Gen Digital and contains no CCleaner program code.

## Modern or Classic?

Both editions use the same cleaning engine and Winapp2 parser, but serve different Windows users. **Modern** provides the current WinUI 3 experience, while **Classic** favors a tiny footprint, immediate startup, broad compatibility, and a deliberately direct desktop interface. They are versioned and released independently; neither edition replaces the other.

See the current [Modern vs. Classic comparison](https://github.com/builtbybel/FluentCleaner#-download-latest-stable) for downloads, requirements, and deployment differences. General project questions are answered in the [FluentCleaner FAQ](https://github.com/builtbybel/FluentCleaner#faq).

## Familiar by design, ahead where it matters

Classic looks older than it is. In several practical areas it already goes beyond the last lean CCleaner 5.x generation:

- independently updateable Winapp2 and optional Winapp3 databases
- inspectable, text-based cleaning rules instead of an opaque internal rule set
- a built-in Custom Cleaner editor and read-only Rule Lab
- global file, folder, and registry exclusions
- preservation of excluded registry branches even when a parent key is cleaned
- warning prompts taken directly from database rules
- detection of running browsers before their data is cleaned
- built-in scheduled cleaning without manually configuring Task Scheduler
- optional explanations through Groq, OpenAI, Anthropic, or another OpenAI-compatible provider
- runtime JSON language packs that can be added without recompiling the app
- portable settings, settings export/import, and no advertising or telemetry

This is not nostalgia wrapped around an old cleaner. It is a current rule-driven cleaner wearing a deliberately familiar interface.

## Quick start

1. Download the Classic ZIP from the official [FluentCleaner releases](https://github.com/builtbybel/FluentCleaner/releases).
2. Extract the complete archive to a writable folder. Do not run it from inside the ZIP.
3. Start `FluentCleaner.Classic.exe`.
4. Review the checked entries under **Windows** and **Applications**.
5. Click **Analyze**. Analysis is read-only; nothing is deleted.
6. Review the summary and open individual entries when you want to see exact file or registry paths.
7. Click **Run Cleaner** only when the result is what you expect.

For a first run, leave Winapp3 disabled and avoid selecting everything at once. A larger result is not automatically a better result.

## Quick tips

- Start with the database defaults, analyze first, and enable additional entries only when you understand what they remove.
- Right-click a result to inspect its exact source rule or protect a file, folder, or registry key without typing the exclusion manually.
- Use a complete `PATH` exclusion for large directory trees you never want scanned; FluentCleaner can skip the branch before enumerating it.
- Keep personal or experimental rules in the `Custom` folder. A Custom Cleaner with the same name can override a built-in entry without modifying the database.
- Close browsers before analyzing browser data so locked files and immediately recreated databases do not distort the result.
- Place `settings.json` beside the executable before launch when you want a self-contained portable configuration.

## The Cleaner view

The left side contains detected cleaning entries grouped by category. An entry appears only when its `Detect`, `DetectFile`, or equivalent detection rule matches the current system. The checkboxes decide which entries participate in a normal analysis.

The right side moves through two states:

- **Summary:** totals per cleaner entry, including file count, registry count, and estimated size.
- **Details:** the concrete file paths and registry items found for an entry.

Right-click an entry or category for focused actions:

- analyze or clean only that selection
- check or uncheck a complete category
- restore database defaults
- open the raw rule in Rule Lab
- request an optional plain-language explanation

In the detail view, a file, folder, or registry key can be added directly to global exclusions from its context menu.

### Analyze and clean are separate phases

**Analyze** expands variables and wildcards, walks matching directories, applies exclusions, checks registry targets, removes duplicate file matches, and builds a deletion plan. It does not modify the system.

**Run Cleaner** consumes that plan. Files that became locked, disappeared, or lost access between analysis and cleaning are skipped. Registry exclusions are checked again immediately before deletion.

This separation is why the analysis result is described as approximate: files can change while applications and Windows continue running.

### Browser and database warnings

Before cleaning, Classic can warn when a selected browser is still running. Open browsers may lock files or immediately recreate data, producing incomplete or confusing results.

Entries containing a Winapp2 `Warning=` field also receive a confirmation prompt. Read it. Warnings are commonly attached to rules that reset preferences, remove history, sign users out, or affect data that is more personal than ordinary cache files.

## How FluentCleaner calculates its results

FluentCleaner counts resources it can actually queue for deletion:

- every matching file counts as one file
- matching registry keys or values are counted separately
- directories are not included in the file count
- empty directories removed by `REMOVESELF` do not add bytes or items to the summary
- a file matched by multiple rules is counted only once within that cleaner entry
- inaccessible or hard-locked files are left out of the result
- reparse points such as junctions and symbolic links are not followed during recursive scans

This can make the object count differ from another cleaner even when both find the same data. For example, BleachBit may report 98 objects for a result containing 73 files and 25 directories; FluentCleaner reports 73 files because it does not count the directories as removable file data.

File sizes are summed in bytes and displayed using binary divisions:

```text
136,576,389 bytes / 1,048,576 = 130.2 MB in FluentCleaner
136,576,389 bytes / 1,000,000 = 136.6 MB in a decimal display
```

The underlying byte total is the same. FluentCleaner currently uses the familiar `KB`, `MB`, and `GB` labels even though its calculations correspond to binary KiB, MiB, and GiB units.

Registry items have no meaningful file size and therefore do not contribute to the displayed byte total.

## Cleaning databases

Open **Options > Settings** to choose which databases are loaded.

### Winapp2.ini

Winapp2 is the recommended everyday database. It is a large, community-maintained collection of declarative rules for Windows and installed applications. FluentCleaner uses its own upstream-supported flavor with FluentCleaner-specific defaults and adjustments.

The database can be updated independently of the application. A restart is requested after an update so the complete entry list is rebuilt consistently.

### Winapp3.ini

Winapp3 is an experimental power-user database. Its rules can be much broader and more aggressive than Winapp2. Some remove application state rather than disposable cache data.

Enabling Winapp3 does not make FluentCleaner "clean better" by itself. Review every selected entry and warning, keep unfamiliar rules disabled, and never use **Check all** casually. Winapp3 is explicitly opt-in and used at your own risk.

### Custom database

A separate `.ini` database can be loaded alongside the built-in databases. This is useful for private rules, portable applications, testing, or content not suitable for the main Winapp2 project.

Use **Browse** to select the file and **Reload** after changing it. Database files use the Winapp2 rule format; see the [Winapp2 format guide](../Winapp2-Format_EN.md).

## Custom Cleaners and Rule Lab

The **Custom** section manages individual `.ini` cleaners stored in the `Custom` folder beside the executable.

- **New** creates a cleaner from scratch or from the included template.
- Double-click or use **Edit** to change its raw rules.
- Its checkbox enables or disables it immediately. Disabled files use the `.ini.disabled` suffix.
- **Test in Rule Lab** opens an editable copy for a read-only dry run.

Rule Lab parses the text currently in its editor and shows which files and registry items it would match. It never writes the edited text back automatically and never deletes the dry-run result. Copy the rule or save it through the Custom Cleaner editor when satisfied.

Be especially careful with `RECURSE`, `REMOVESELF`, broad environment paths, and registry rules. Start narrowly, dry-run, inspect exact paths, then widen the rule only when necessary.

## Cookies to keep

Open **Options > Cookies** to scan supported browser profiles. Move websites to **Cookies to keep** when their login cookies should survive a selected browser-cookie clean.

Chromium uses one shared cookie database format, so the same engine supports Microsoft Edge, Google Chrome, Brave, Chromium, Vivaldi, and Opera profiles. Mozilla Firefox uses its own database adapter. Close running browsers before refreshing the list or cleaning; an open browser may lock its database.

The selected Winapp2 entry still decides whether browser cookies are cleaned. With an empty keep-list, FluentCleaner performs the normal whole-file cleanup. With protected websites present, the database is cleaned row by row instead. If safe selective access fails, FluentCleaner skips the database and never falls back to deleting it completely.

## Global exclusions

Open **Options > Exclusions**, enable global exclusions, and add one rule per entry:

```text
PATH|C:\Folder
FILE|C:\Folder|*.log
REG|HKCU\Software\Example
```

- `PATH` without a filename pattern protects the complete directory tree. FluentCleaner skips that branch before enumerating its files, which can also reduce scan time.
- `FILE` protects one file or a filename pattern. The directory still has to be scanned to determine which files match.
- `REG` protects the specified registry key and everything below it. If a broader parent key is cleaned, the protected branch and the parents required to reach it remain intact.

Environment variables are supported and are usually preferable to hard-coded system locations:

```text
PATH|%WinDir%
PATH|%ProgramFiles%
PATH|%LocalAppData%\MyApp\Cache
```

FluentCleaner also contains a small read-only protected-path list for data that should never be removed, regardless of database rules. Use **View protected paths** to inspect it.

## Options

| Page | Purpose |
|---|---|
| **Settings** | Language, Winapp2, Winapp3, custom database, reload and database updates |
| **Cookies** | Website cookies that should survive browser cleaning |
| **Exclusions** | Global file, folder and registry protection |
| **Tasks** | Commands executed sequentially after a completed clean |
| **History** | Up to 50 recorded clean runs with items removed and bytes freed |
| **AI** | Provider, API key, endpoint testing, and optional explanations |
| **Scheduler** | Daily, Monday-weekly, or logon cleaning through Windows Task Scheduler |
| **About** | Version, project links, translator credit, and settings import/export |

Settings pages save when you leave the page or leave Options.

### Post-clean tasks

Post-clean commands run through `cmd.exe`, one line at a time, after cleaning finishes. They are best-effort and each command is given up to ten seconds. Treat this field like a batch file: only add commands you understand and trust.

### Clean history

History records the time, successfully removed item count, and bytes freed for up to 50 runs. It does not store the deleted file contents.

### AI explanations

AI explanations are optional and inactive until you configure a provider and key. Supported choices are Groq, OpenAI, Anthropic, and custom OpenAI-compatible chat-completions endpoints such as OpenRouter.

No cleaner data is sent automatically. A request is made only when you explicitly choose **Explain with AI** or test a provider. API keys are stored in `settings.json`; they are not protected by Windows Credential Manager. Remove secrets before sharing an exported settings file.

AI output is explanatory text, not a cleaning instruction. The actual scan and clean remain deterministic and rule-based.

## Scheduled and command-line cleaning

The Scheduler creates a normal Windows task named `FluentCleaner AutoClean`. It launches the same executable with `/AUTO`, runs only while the user is logged on, and does not require stored Windows credentials.

```text
FluentCleaner.Classic.exe /AUTO
FluentCleaner.Classic.exe /AUTO /SHUTDOWN
```

`/AUTO` loads enabled databases and Custom cleaners, detects applicable entries, cleans the saved selection, runs post-clean tasks, writes a log, and exits without showing the interface. `/SHUTDOWN` powers off the PC after that sequence.

Review the selected cleaners interactively before enabling a schedule. When no saved selection exists, `/AUTO` falls back to the database defaults.

The log is appended here:

```text
%APPDATA%\FluentCleaner\auto.log
```

## Settings and portable mode

By default, settings are stored as JSON rather than in the registry:

```text
%APPDATA%\FluentCleaner\settings.json
```

To use portable settings, place a file named `settings.json` next to `FluentCleaner.Classic.exe` **before starting the app**. The local file then takes priority and the About page displays **Portable mode**.

Settings can be exported or imported under **Options > About**. Restart after importing so every view and database is rebuilt from the imported configuration.

Portable mode covers application settings. The `/AUTO` activity log is still written to `%APPDATA%\FluentCleaner\auto.log`.

## Languages

Classic discovers loose JSON language packs from its `Localization` folder at startup. Missing strings fall back to English, and a new translation can be installed without recompiling FluentCleaner.

After adding or changing a language file, restart the app. See [Translating FluentCleaner Classic](TRANSLATING_CLASSIC.md) for the complete process and translator-credit metadata.

## Troubleshooting

### A cleaner entry is missing

The application or component may not satisfy the rule's detection keys. Confirm that the intended database is enabled, then restart after updating or changing database files.

### Analyze finds less than another cleaner

Compare exact file paths and byte totals, not only the displayed object count or rounded MB value. Other tools may count directories as objects or use decimal size units.

### A browser cleaner finds little data

Close the browser and analyze again. Background browser processes can keep databases and cache files locked.

### A file reappears after cleaning

Windows or the owning application may recreate it immediately. That does not necessarily mean deletion failed.

### A protected file still appears as a scanned path

Whole excluded directory branches are skipped. File-pattern exclusions still require the containing directory to be inspected before matching files can be filtered out.

### Some system files are not removed

Classic runs as the current user. Files protected by permissions or held without delete sharing are skipped rather than forcing access. Run elevated only when you understand why a specific rule requires it.

## Reporting problems

Use [GitHub Issues](https://github.com/builtbybel/FluentCleaner/issues) and include:

- Classic version
- Windows version and display scaling when the issue is visual
- enabled databases
- exact cleaner entry names
- the relevant paths from the analysis result
- steps required to reproduce the behavior

For safety reports, provide the selected entry and affected path. A screenshot showing that Winapp3 is enabled is not enough to determine which aggressive rule was selected.

Only download FluentCleaner from the official [GitHub repository](https://github.com/builtbybel/FluentCleaner).

## Notes from under the hood

Classic keeps a few deliberate implementation details out of the way during normal use, but they explain why the application behaves the way it does.

### The window wins the startup race

The main shell is displayed before system information, database parsing, and application detection begin. The Custom and Options views are not even created until they are opened for the first time. Once created, views stay alive so navigation preserves selection, scroll position, and unfinished work.

That is why Classic can appear immediately while its status line is still discovering cleaner entries in the background.

### A dry run uses the real engine

Rule Lab is not a separate approximation of FluentCleaner's behavior. It feeds the current editor text into the same Winapp2 parser and the same read-only analysis service used by the Cleaner view. The only missing step is deletion.

If Rule Lab reports a file, the real analyzer would reach it under the same rule, settings, and exclusions. This makes it useful for developing rules rather than merely previewing their text.

### Custom rules can replace built-in rules cleanly

Entries are identified by name. When loaded databases contain the same entry more than once, the last definition wins. Files from the `Custom` folder are loaded last, so a custom entry can replace a built-in definition without modifying Winapp2.ini and without being overwritten by the next database update.

Disabling a Custom Cleaner is equally simple: Classic renames its file from `.ini` to `.ini.disabled`. There is no private registration database behind the Custom page.

### Every parsed entry carries its source with it

The parser keeps the original rule block alongside the structured entry. **Show source** therefore displays the exact `Detect`, `FileKey`, `RegKey`, `ExcludeKey`, `Warning`, and default lines that produced the visible cleaner—not a reconstructed summary.

### Windows performs the final filename matching

FluentCleaner resolves variables and directory wildcards itself, but matching files are enumerated through Windows. This preserves native filesystem behavior, including legacy 8.3 short-name aliases that a simple managed regular expression could miss.

When one rule contains several filename patterns, the directory tree is walked once and duplicate matches are collapsed before the result is counted.

### Analyze asks whether a file could really be deleted

Reading a file's size does not prove that it can be removed. During analysis, Classic requests delete access with Windows file-sharing rules before adding the file to the result. A file held open without delete sharing is omitted instead of inflating the promised cleanup size with something the cleaner already knows it cannot remove.

The check is still a moment-in-time answer: another process can open, close, create, or replace the file before **Run Cleaner** is pressed.

### Recursive scans avoid Windows filesystem traps

Junctions and symbolic links are not followed. Windows contains compatibility links that can lead back into paths already visited; following them blindly can duplicate results or create an endless directory loop.

Complete `PATH` exclusions are checked before a directory branch is entered. File-pattern exclusions are applied later because the directory must first be inspected to know which filenames match.

### Registry exclusions are structural

A registry exclusion is not only a final "do not delete this exact key" check. When a rule targets a parent tree containing an excluded descendant, FluentCleaner walks the tree, removes unprotected siblings and values, and retains the excluded branch together with the parent keys required to reach it.

The exclusion list is built during analysis and rebuilt before cleaning, so a protected registry branch never relies solely on an earlier scan result.

### Scheduled cleaning is not a second cleaner

The Scheduler does not install a service or maintain a hidden background engine. Windows Task Scheduler launches the same `FluentCleaner.Classic.exe` with `/AUTO`, and that path uses the same databases, parser, detection, exclusions, cleaning service, and post-clean tasks as the visible application.

### Both FluentCleaner editions can share settings without erasing each other

Modern and Classic use the same default `%APPDATA%\FluentCleaner\settings.json`. Classic understands only the settings it needs, but preserves unknown JSON fields when saving. A Modern-only preference can therefore survive a Classic save, and vice versa.

Portable `settings.json` files remain edition-local when each executable is kept in its own folder.

### A language pack is allowed to be incomplete

Classic resolves every visible string through a fallback chain: selected JSON language, English, then the raw key name. A missing or malformed community translation does not prevent the application from starting, and translator metadata never falls back to another language where it could credit the wrong person.

### The window remembers the screen, not only its coordinates

Classic stores normal window bounds even when it closes maximized. At the next start it restores the normal bounds first and applies maximized state last. A saved position is accepted only when it still intersects a connected display, preventing a window from being stranded on a monitor that no longer exists.
