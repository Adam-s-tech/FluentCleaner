# FluentCleaner Inspector

Inspect and compare Winapp2 cleaner databases directly in the browser using the same parser source as FluentCleaner.

## What it does

- Opens compatible Winapp2 INI files locally in the browser.
- Summarizes file, registry, recursive, warning and duplicate rules.
- Checks `settings.json` selections against the loaded database.
- Compares two database versions.
- Exports selected signature names in the JSON-array format already accepted by FluentCleaner Classic's `/FROMCRAPCHECK` handoff.

No imported file is uploaded or sent to a server.

## Run locally

```powershell
dotnet run --project FluentCleaner.Inspector
```

## GitHub Pages

The repository workflow publishes the static WebAssembly output to GitHub Pages. In the repository settings, choose **Pages → Build and deployment → GitHub Actions**.

The workflow assumes the repository name is `FluentCleaner` and changes the site's base path accordingly.
