# Asset File API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-assetfiles/
- **Namespace**: Oculus.Platform

## Overview

The Asset File API is part of the Horizon Platform SDK Unity package. It provides operations for Meta Quest Unity applications to manage downloadable asset files (DLC, expansion packs, optional content):

1. **`AssetFile.GetList()`** -- List all assets configured for the app
2. **`AssetFile.StatusByName(name)`** -- Get status of one asset by name
3. **`AssetFile.StatusById(id)`** -- Get status of one asset by ID
4. **`AssetFile.DownloadByName(name)`** -- Download one asset by name
5. **`AssetFile.DownloadById(id)`** -- Download one asset by ID
6. **`AssetFile.DownloadByNameList(names)`** -- Batch download (all-or-nothing)
7. **`AssetFile.DownloadByIdList(ids)`** -- Batch download by IDs
8. **`AssetFile.DownloadCancelByName(name)`** -- Cancel a download
9. **`AssetFile.DeleteByName(name)`** -- Delete an installed asset
10. **`AssetFile.DeleteById(id)`** -- Delete by ID
11. **`AssetFile.GetNextAssetDetailsListPage(list)`** -- Paginate the asset list

**Notification Callback (register immediately after init):**
1. **`SetDownloadUpdateNotificationCallback(cb)`** -- Live download progress events

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Register your app** at [developer.oculus.com/manage](https://developer.oculus.com/manage/)
2. **Configure your assets** in the Developer Dashboard under your app's "Builds > Asset Files" section. Each asset gets an **API Name** (case-sensitive) and **ID**. Upload the actual file payload there.
3. **Note your App ID** and asset names

## API Usage

### Register Download Update Callback (Immediately After Init)

```csharp
async void Start()
{
    var msg = await Core.AsyncInitialize(appId);
    if (msg.IsError) return;

    AssetFile.SetDownloadUpdateNotificationCallback(OnDownloadUpdate);
}

private void OnDownloadUpdate(Message<AssetFileDownloadUpdate> msg)
{
    if (msg.IsError) return;
    var u = msg.Data;
    Debug.Log($"Asset {u.AssetId}: {u.BytesTransferredLong}/{u.BytesTotalLong} bytes, status={u.TransferState}");
}
```

### List All Assets

```csharp
public async Task<List<AssetDetails>> ListAssets()
{
    if (!Core.IsInitialized()) return new();

    var msg = await AssetFile.GetList();
    if (msg.IsError) return new();

    var result = new List<AssetDetails>(msg.Data);
    var page = msg.Data;
    while (page.HasNextPage)
    {
        var nextMsg = await AssetFile.GetNextAssetDetailsListPage(page);
        if (nextMsg.IsError) break;
        result.AddRange(nextMsg.Data);
        page = nextMsg.Data;
    }
    return result;
}
```

### Check Status of a Specific Asset

```csharp
public async Task<bool> IsAssetInstalled(string assetName)
{
    var msg = await AssetFile.StatusByName(assetName);
    if (msg.IsError) return false;
    return msg.Data.DownloadStatus == "installed";
}
```

### Download an Asset

```csharp
public async Task<string> DownloadAsset(string assetName)
{
    if (!Core.IsInitialized()) return null;

    var msg = await AssetFile.DownloadByName(assetName);
    if (msg.IsError)
    {
        Debug.LogError($"Download {assetName}: {msg.GetError().Message}");
        return null;
    }
    Debug.Log($"Downloaded {assetName} -> {msg.Data.Filepath}");
    return msg.Data.Filepath;
}
```

The returned `Filepath` is the absolute path on the device -- load it via standard Unity APIs (e.g., `File.ReadAllBytes`, `AssetBundle.LoadFromFile`).

### Batch Download

```csharp
public async Task DownloadMany(string[] assetNames)
{
    var msg = await AssetFile.DownloadByNameList(assetNames);
    if (msg.IsError)
    {
        Debug.LogError($"Batch download: {msg.GetError().Message}");
        return;
    }
    Debug.Log($"Batch session ID: {msg.Data}");
    // Track progress via the SetDownloadUpdateNotificationCallback you registered
}
```

**Atomic semantics**: For batch downloads, **all assets must succeed or fail together**. There's no partial success.

### Cancel a Download

```csharp
public async Task<bool> CancelDownload(string assetName)
{
    var msg = await AssetFile.DownloadCancelByName(assetName);
    return !msg.IsError && msg.Data.Success;
}
```

### Delete an Installed Asset

```csharp
public async Task<bool> DeleteAsset(string assetName)
{
    var msg = await AssetFile.DeleteByName(assetName);
    return !msg.IsError && msg.Data.Success;
}
```

### Loading Asset Bundles from Downloaded Assets

A common pattern: ship Unity AssetBundles as Asset Files, then load at runtime.

```csharp
public async Task<AssetBundle> LoadAssetBundle(string assetName)
{
    var statusMsg = await AssetFile.StatusByName(assetName);
    if (statusMsg.IsError) return null;

    string filepath = statusMsg.Data.Filepath;

    // Download if not yet installed
    if (statusMsg.Data.DownloadStatus != "installed")
    {
        var dlMsg = await AssetFile.DownloadByName(assetName);
        if (dlMsg.IsError) return null;
        filepath = dlMsg.Data.Filepath;
    }

    var bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
    while (!bundleRequest.isDone) await Task.Yield();
    return bundleRequest.assetBundle;
}
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `AssetFile.GetList()` | `Request<AssetDetailsList>` | List all assets configured for the app |
| `AssetFile.StatusByName(name)` | `Request<AssetDetails>` | Status of one asset by name |
| `AssetFile.StatusById(id)` | `Request<AssetDetails>` | Status of one asset by ID |
| `AssetFile.DownloadByName(name)` | `Request<AssetFileDownloadResult>` | Download one asset by name |
| `AssetFile.DownloadById(id)` | `Request<AssetFileDownloadResult>` | Download one asset by ID |
| `AssetFile.DownloadByNameList(names)` | `Request<int>` | Batch download (all-or-nothing) |
| `AssetFile.DownloadByIdList(ids)` | `Request<int>` | Batch download by IDs |
| `AssetFile.DownloadCancelByName(name)` | `Request<AssetFileDownloadCancelResult>` | Cancel a download |
| `AssetFile.DeleteByName(name)` | `Request<AssetFileDeleteResult>` | Delete an installed asset |
| `AssetFile.DeleteById(id)` | `Request<AssetFileDeleteResult>` | Delete by ID |
| `AssetFile.SetDownloadUpdateNotificationCallback(cb)` | (void) | Subscribe to download progress events |
| `AssetFile.GetNextAssetDetailsListPage(list)` | `Request<AssetDetailsList>` | Paginate the asset list |

### `AssetDetails` Model

| Field | Type | Description |
|-------|------|-------------|
| `AssetId` | `ulong` | Numeric asset ID |
| `Filepath` | `string` | Local install path (when installed) -- your runtime loads from here |
| `DownloadStatus` | `string` | `installed`, `available`, `in_progress`, etc. |
| `IapStatus` | `string` | `free`, `entitled`, `not_entitled` (for paid DLC) |
| `Metadata` | `string` | Developer-defined metadata string |
| `AssetType` | `string` | `default`, `store`, `language_pack` |

### `AssetFileDownloadResult` Model

| Field | Type | Description |
|-------|------|-------------|
| `AssetId` | `ulong` | Numeric asset ID |
| `Filepath` | `string` | Local path to the downloaded file |

### `AssetFileDownloadUpdate` Model (from notification callback)

| Field | Type | Description |
|-------|------|-------------|
| `AssetId` | `ulong` | Numeric asset ID |
| `BytesTransferredLong` | `long` | Bytes downloaded so far |
| `BytesTotalLong` | `long` | Total download size in bytes |
| `TransferState` | `string` | Current transfer state |

### `AssetFileDeleteResult` Model

| Field | Type | Description |
|-------|------|-------------|
| `AssetId` | `ulong` | Numeric asset ID |
| `Filepath` | `string` | Path of the deleted file |
| `Success` | `bool` | Whether deletion succeeded |

### `AssetFileDownloadCancelResult` Model

| Field | Type | Description |
|-------|------|-------------|
| `AssetId` | `ulong` | Numeric asset ID |
| `Filepath` | `string` | Path of the cancelled download |
| `Success` | `bool` | Whether cancellation succeeded |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Subscribing to download updates *after* starting downloads | Subscribe immediately after init so you don't miss early progress events. |
| Polling `Status` instead of using the callback | The callback delivers progress in real time; polling is wasteful. |
| Assuming partial success on batch downloads | All-or-nothing. If one asset fails, the whole batch fails. |
| Hardcoding asset paths | Always call `Status` or `Download` to get the current `Filepath` -- it's not stable across reinstalls. |
| Forgetting to handle `IapStatus` for paid DLC | If the asset is paid DLC, check `IapStatus == "entitled"` before attempting download. |
| Mixing IDs and names | Use one consistently. Names are easier to read in code; IDs are more stable. |
| Skipping cancel on user back-out | Without calling `DownloadCancelByName`, the download continues in the background. |
| Calling Asset File APIs before init | Always check `Core.IsInitialized()`. |

## Examples

### Complete Asset File Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AssetFileManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    public event Action<ulong, long, long, string> ProgressUpdated; // (assetId, transferred, total, state)

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
        ProgressUpdated?.Invoke(u.AssetId, u.BytesTransferredLong, u.BytesTotalLong, u.TransferState.ToString());
    }

    public async Task<List<AssetDetails>> ListAssetsAsync()
    {
        if (!isInitialized) return new();
        var msg = await AssetFile.GetList();
        if (msg.IsError) return new();
        return new List<AssetDetails>(msg.Data);
    }

    public async Task<string> EnsureDownloadedAsync(string assetName)
    {
        if (!isInitialized) return null;

        var statusMsg = await AssetFile.StatusByName(assetName);
        if (!statusMsg.IsError && statusMsg.Data.DownloadStatus == "installed")
            return statusMsg.Data.Filepath;

        var dlMsg = await AssetFile.DownloadByName(assetName);
        return dlMsg.IsError ? null : dlMsg.Data.Filepath;
    }

    public async Task<bool> CancelAsync(string assetName)
    {
        if (!isInitialized) return false;
        var msg = await AssetFile.DownloadCancelByName(assetName);
        return !msg.IsError && msg.Data.Success;
    }

    public async Task<bool> DeleteAsync(string assetName)
    {
        if (!isInitialized) return false;
        var msg = await AssetFile.DeleteByName(assetName);
        return !msg.IsError && msg.Data.Success;
    }
}
```

## Important Notes

1. **Subscribe to download updates immediately after init** -- before issuing any downloads, so you don't miss early progress events.

2. **Use the callback for progress, not polling** -- `SetDownloadUpdateNotificationCallback` delivers real-time progress. Polling `StatusByName` repeatedly is wasteful.

3. **Batch downloads are all-or-nothing** -- `DownloadByNameList` either succeeds for all assets or fails entirely. There is no partial success.

4. **Never hardcode asset file paths** -- always get the current `Filepath` from `StatusByName` or `DownloadByName`. Paths are not stable across reinstalls.

5. **Check `IapStatus` for paid DLC** -- if the asset is behind a paywall, verify `IapStatus == "entitled"` before attempting download.

6. **Provide cancel and delete affordances** -- call `DownloadCancelByName` on user-initiated back-outs, and provide a "Delete" option for users to free space.

7. **Asset names are case-sensitive** -- they must match exactly what's configured in the Developer Dashboard.

## Useful Links

- [Meta Quest Asset Files Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-assetfiles/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
