# Language Pack API

- **Unity Package**: `com.meta.xr.sdk.platform`
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-language-packs/
- **Namespace**: `Oculus.Platform`

## Overview

The Language Pack API is part of the Horizon Platform SDK. It lets you ship per-locale assets (translated text, localized audio, captions) separately from your main APK, keeping install size small and letting users download only the languages they need. It provides two operations:

1. **`LanguagePack.GetCurrent()`** -- Get the currently installed/selected language pack and its install path
2. **`LanguagePack.SetCurrent(tag)`** -- Set the active language; auto-downloads the pack if not yet installed

Language packs are a special kind of Asset File (`AssetType.LANGUAGE_PACK`) -- progress events flow through `AssetFile.SetDownloadUpdateNotificationCallback`.

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Configure language packs** in the Developer Dashboard. Each pack is tagged with a BCP-47 locale (e.g., `en-US`, `fr-FR`, `ja-JP`) and uploads its asset payload.
2. **Subscribe to download updates immediately after init** -- language pack downloads emit progress through the shared Asset File callback.

## API Usage

#### Subscribe to Download Updates

Language pack downloads emit progress through the shared Asset File callback. Subscribe immediately after init.

```csharp
async void Start()
{
    var msg = await Core.AsyncInitialize(appId);
    if (msg.IsError) return;

    // Subscribe before triggering any language change
    AssetFile.SetDownloadUpdateNotificationCallback(OnLanguagePackDownloadUpdate);
    isInitialized = true;
}

private void OnLanguagePackDownloadUpdate(Message<AssetFileDownloadUpdate> msg)
{
    if (msg.IsError) return;
    var u = msg.Data;
    Debug.Log($"Language pack {u.AssetId}: {u.BytesTransferredLong}/{u.BytesTotalLong}");
}
```

#### Get Current Language Pack

```csharp
public async Task<string> GetCurrentLanguageTag()
{
    if (!Core.IsInitialized()) return null;

    var msg = await LanguagePack.GetCurrent();
    if (msg.IsError) return null;

    AssetDetails details = msg.Data;
    Debug.Log($"Current language: {details.Language} (path: {details.Filepath})");
    return details.Language;
}
```

**Return type**: `Request<AssetDetails>`

#### Switch Language (with Auto-Download)

`SetCurrent` sets the user's preferred language and, if the pack isn't installed yet, kicks off a download automatically.

```csharp
public async Task<string> SwitchLanguage(string bcp47Tag)
{
    if (!Core.IsInitialized()) return null;

    var msg = await LanguagePack.SetCurrent(bcp47Tag);
    if (msg.IsError)
    {
        Debug.LogError($"SetCurrent({bcp47Tag}) failed: {msg.GetError().Message}");
        return null;
    }

    // The result is an AssetFileDownloadResult -- the path to the now-installed pack
    string filepath = msg.Data.Filepath;
    Debug.Log($"Language pack '{bcp47Tag}' ready at {filepath}");
    return filepath;
}
```

**Parameters**: `tag: string` -- BCP-47 locale tag (e.g., `"fr-FR"`)

**Return type**: `Request<AssetFileDownloadResult>`

> **Wait for completion**: `SetCurrent` returns when the download is fully done. If you want progress updates, the `AssetFile.SetDownloadUpdateNotificationCallback` you registered fires as bytes transfer.

#### Load Localized Assets at Runtime

After `SetCurrent`, `Filepath` points to the language pack directory. Load your strings/audio/textures from there:

```csharp
public class LocalizationLoader : MonoBehaviour
{
    private Dictionary<string, string> strings = new();

    public async Task LoadStringsForLanguage(string bcp47Tag)
    {
        var msg = await LanguagePack.SetCurrent(bcp47Tag);
        if (msg.IsError) return;

        string root = msg.Data.Filepath;
        string stringsPath = System.IO.Path.Combine(root, "strings.json");
        if (System.IO.File.Exists(stringsPath))
        {
            string json = System.IO.File.ReadAllText(stringsPath);
            strings = JsonUtility.FromJson<StringTable>(json).ToDictionary();
        }
    }

    public string T(string key) => strings.TryGetValue(key, out var v) ? v : key;
}
```

## Complete Language Pack Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class LanguagePackManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    public event Action<string, long, long> DownloadProgress; // (lang or null, transferred, total)
    public event Action<string, string> LanguageChanged;       // (lang, filepath)

    private bool isInitialized;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }

        AssetFile.SetDownloadUpdateNotificationCallback(OnDownloadUpdate);
        isInitialized = true;
    }

    private void OnDownloadUpdate(Message<AssetFileDownloadUpdate> msg)
    {
        if (msg.IsError) return;
        var u = msg.Data;
        DownloadProgress?.Invoke(null, u.BytesTransferredLong, u.BytesTotalLong);
    }

    public async Task<AssetDetails> GetCurrentAsync()
    {
        if (!isInitialized) return null;
        var msg = await LanguagePack.GetCurrent();
        return msg.IsError ? null : msg.Data;
    }

    public async Task<string> SetLanguageAsync(string bcp47Tag)
    {
        if (!isInitialized) return null;

        var msg = await LanguagePack.SetCurrent(bcp47Tag);
        if (msg.IsError) return null;

        LanguageChanged?.Invoke(bcp47Tag, msg.Data.Filepath);
        return msg.Data.Filepath;
    }
}
```

## Data Types

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `LanguagePack.GetCurrent()` | `Request<AssetDetails>` | Currently installed/selected language pack |
| `LanguagePack.SetCurrent(tag)` | `Request<AssetFileDownloadResult>` | Set active language; auto-downloads if not installed |

### Models

| Type | Key Fields |
|------|------------|
| `AssetDetails` | `Language` (BCP-47 tag), `Filepath` (local path), `DownloadStatus`, `AssetType` (= `LANGUAGE_PACK`) |
| `AssetFileDownloadResult` | `AssetId`, `Filepath` |

### Common BCP-47 Tags

| Tag | Language |
|-----|----------|
| `en-US` | English (US) |
| `en-GB` | English (UK) |
| `fr-FR` | French (France) |
| `de-DE` | German |
| `es-ES` | Spanish (Spain) |
| `es-419` | Spanish (Latin America) |
| `ja-JP` | Japanese |
| `ko-KR` | Korean |
| `zh-Hans-CN` | Chinese (Simplified) |
| `pt-BR` | Portuguese (Brazil) |

Use only the tags you've configured language packs for in the Dashboard.

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Using ISO-639 codes only (e.g., `"fr"`) without region | Always use full BCP-47 tags with region (`"fr-FR"`, `"es-419"`) -- that's what the Dashboard expects. |
| Subscribing to download updates *after* `SetCurrent` | Subscribe to `AssetFile.SetDownloadUpdateNotificationCallback` immediately after init so you don't miss early progress events. |
| Hardcoding `Filepath` | Always read it from `GetCurrent`/`SetCurrent` response. Paths can change across reinstalls. |
| Calling `SetCurrent` with a tag you don't have a pack for | Will fail. Verify the language is configured in the Dashboard. |
| Trying to manage language packs via the regular Asset File API | Use `LanguagePack.SetCurrent` -- it knows how to coordinate "active language" state with the system locale. |
| Falling back silently when the user's preferred language isn't available | Show an explicit picker; let the user choose from supported languages. |

## Important Notes

1. **Subscribe to `AssetFile.SetDownloadUpdateNotificationCallback` immediately after init** -- language pack downloads emit through this shared callback. Don't subscribe after calling `SetCurrent` or you'll miss early progress events.

2. **Always use full BCP-47 tags** (`xx-XX`) -- that's what the Dashboard accepts. Validate tags against your configured language packs before calling `SetCurrent`.

3. **`SetCurrent` auto-downloads if needed** and returns when complete. Show a "Downloading..." UI while waiting. After completion, reload all your in-memory localized assets from the new `Filepath`.

4. **Inside the language pack `Filepath`**, organize assets by your own convention (e.g., `strings.json`, `audio/voiceover.wav`, `textures/`). Read at runtime via standard Unity APIs.

5. **Language packs share the Asset File lifecycle** -- see the Asset File reference for additional download management details.

## Useful Links

- [Meta Quest Language Packs Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-language-packs/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Platform SDK Overview](https://developer.oculus.com/documentation/unity/ps-platform-intro/)
