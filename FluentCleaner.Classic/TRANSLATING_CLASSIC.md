# Help FluentCleaner Classic speak your language

FluentCleaner Classic discovers and loads translations from plain JSON files at runtime. You can add a new language or improve an existing one without recompiling the application, installing Visual Studio, or working with compiled resource files.

Classic language packs are deliberately separate from those of the modern WinUI 3 edition. Both applications have independent release cycles, different interfaces, and different space constraints, so a translation can evolve safely without unexpectedly breaking the other edition.

## Create a translation

1. Open **Options > Settings**
2. Click **Open localization folder**
3. Copy `en.json`
4. Rename the copy to the appropriate locale, for example `fr.json`, `es.json`, or `pt-BR.json`
5. Translate the text values
6. Restart FluentCleaner Classic and select the new language

Available languages are discovered automatically from the `Localization` folder. Regional variants are supported, and any missing entry safely falls back to English.

> [!IMPORTANT]
> Keep all JSON keys and placeholders such as `{0}`, `{1}`, `\r\n`, and escaped characters unchanged. Translate only their text values.

## Add your translator credit

Every language file begins with three metadata fields:

```json
{
  "_Meta_LanguageDisplayName": "Français (French)",
  "_Meta_TranslatorName": "Your Name",
  "_Meta_TranslatorWebsite": "https://example.com"
}
```

- `_Meta_LanguageDisplayName` is the final name shown in the language selector
  - For languages using the Latin alphabet, combine the native and English names when useful, for example `Deutsch (German)` or `Français (French)`
  - For languages using another writing system, use a recognizable English name, for example `Bulgarian`, `Greek`, or `Chinese (Simplified)`
- `_Meta_TranslatorName` adds your credit to the About page
- `_Meta_TranslatorWebsite` makes the credit clickable when it contains a valid website

Translator name and website are optional, but we would love to give contributors visible credit for their work.

## Share your translation

When your translation is ready, you can:

- Open a pull request and add the JSON file to the `Localization` folder https://github.com/builtbybel/FluentCleaner/tree/main/FluentCleaner.Classic/Localization
- Open a GitHub issue and attach the completed JSON file

Corrections and improvements to existing translations are just as welcome as entirely new languages.

Thank you for helping FluentCleaner Classic feel at home everywhere.
