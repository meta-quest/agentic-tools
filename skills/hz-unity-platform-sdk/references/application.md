# Application API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-application/
- **Namespace**: Oculus.Platform

## Overview

The Application API is part of the Horizon Platform SDK Unity package. It provides operations for Meta Quest Unity applications to query app versions, perform in-app self-updates, and launch other Quest apps:

1. **`Application.GetVersion()`** -- Get installed and latest available version info
2. **`Application.StartAppDownload()`** -- Start downloading the latest update
3. **`Application.CheckAppDownloadProgress()`** -- Poll download progress
4. **`Application.CancelAppDownload()`** -- Cancel an in-progress download
5. **`Application.InstallAppUpdateAndRelaunch(opts)`** -- Install update; exits and relaunches the app
6. **`Application.LaunchOtherApp(appId, opts)`** -- Launch another Quest app with optional deeplink

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## API Usage

### Check for Updates

```csharp
public async Task<bool> IsUpdateAvailable()
{
    if (!Core.IsInitialized()) return false;

    var msg = await Application.GetVersion();
    if (msg.IsError)
    {
        Debug.LogError($"GetVersion failed: {msg.GetError().Message}");
        return false;
    }

    ApplicationVersion v = msg.Data;
    Debug.Log($"Installed: {v.CurrentName} ({v.CurrentCode}), Latest: {v.LatestName} ({v.LatestCode})");
    return v.LatestCode > v.CurrentCode;
}
```

`ReleaseDate` is Unix epoch seconds. Convert before displaying:

```csharp
DateTime releasedAt = DateTimeOffset.FromUnixTimeSeconds(v.ReleaseDate).UtcDateTime;
```

### In-App Self-Update Flow

The full flow is **download -> monitor -> install (which exits and relaunches your app)**.

```csharp
public async Task<bool> DownloadAndInstall(string deeplinkOnReturn = null)
{
    if (!Core.IsInitialized()) return false;

    // 1) Confirm an update is available
    var verMsg = await Application.GetVersion();
    if (verMsg.IsError) return false;
    if (verMsg.Data.LatestCode <= verMsg.Data.CurrentCode)
    {
        Debug.Log("Already up to date.");
        return false;
    }

    // 2) Start the download
    var startMsg = await Application.StartAppDownload();
    if (startMsg.IsError)
    {
        Debug.LogError($"StartAppDownload: {startMsg.GetError().Message}");
        return false;
    }

    // 3) Poll progress and update UI
    StartCoroutine(PollDownloadProgress());

    // 4) Install the downloaded update -- this EXITS your app
    var opts = new ApplicationOptions();
    if (!string.IsNullOrEmpty(deeplinkOnReturn))
        opts.SetDeeplinkMessage(deeplinkOnReturn);

    var installMsg = await Application.InstallAppUpdateAndRelaunch(opts);
    if (installMsg.IsError)
    {
        Debug.LogError($"Install: {installMsg.GetError().Message}");
        return false;
    }
    // App will exit after this. Code below this line generally won't run.
    return true;
}

private IEnumerator PollDownloadProgress()
{
    while (true)
    {
        yield return new WaitForSeconds(0.5f);
        Application.CheckAppDownloadProgress().OnComplete(msg =>
        {
            if (msg.IsError) return;
            AppDownloadProgressResult p = msg.Data;
            Debug.Log($"Download status: {p.StatusCode}, {p.DownloadBytes} bytes");
            UpdateProgressBar(p);
        });
    }
}
```

**Important**: `InstallAppUpdateAndRelaunch` causes your app to **exit**. Save user state to disk before calling it. Use the optional `deeplinkOnReturn` to drop the user back into the same place after relaunch (read it via `ApplicationLifecycle.GetLaunchDetailsRequest`).

### Cancel a Download

```csharp
public async Task Cancel()
{
    var msg = await Application.CancelAppDownload();
    if (!msg.IsError) Debug.Log("Download cancelled");
}
```

### Launch Another App

```csharp
public async Task<bool> LaunchApp(ulong otherAppId, string deeplinkMessage = null)
{
    if (!Core.IsInitialized()) return false;

    ApplicationOptions opts = null;
    if (!string.IsNullOrEmpty(deeplinkMessage))
    {
        opts = new ApplicationOptions();
        opts.SetDeeplinkMessage(deeplinkMessage);
    }

    var msg = await Application.LaunchOtherApp(otherAppId, opts);
    if (msg.IsError)
    {
        Debug.LogError($"LaunchOtherApp({otherAppId}): {msg.GetError().Message}");
        return false;
    }

    Debug.Log($"Launched app {otherAppId}, response: {msg.Data}");
    return true;
}
```

If the user doesn't have the target app installed, the platform **automatically takes them to that app's Store page**. No special handling needed. The receiving app reads your `deeplinkMessage` via `ApplicationLifecycle.GetLaunchDetailsRequest()` (`LaunchType.Deeplink`).

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Application.GetVersion()` | `Request<ApplicationVersion>` | Installed + latest available version info |
| `Application.StartAppDownload()` | `Request<AppDownloadResult>` | Start downloading the latest update |
| `Application.CheckAppDownloadProgress()` | `Request<AppDownloadProgressResult>` | Poll download progress |
| `Application.CancelAppDownload()` | `Request<AppDownloadResult>` | Cancel an in-progress download |
| `Application.InstallAppUpdateAndRelaunch(opts)` | `Request<AppDownloadResult>` | Install update; exits and relaunches |
| `Application.LaunchOtherApp(appId, opts)` | `Request<string>` | Launch another Quest app, with optional deeplink |

### `ApplicationVersion` Model

| Field | Type | Description |
|-------|------|-------------|
| `CurrentCode` | `int` | Installed version code (integer) |
| `CurrentName` | `string` | Installed version display name |
| `LatestCode` | `int` | Latest available version code |
| `LatestName` | `string` | Latest available version display name |
| `Size` | `long` | Download size in bytes |
| `ReleaseDate` | `long` | Unix epoch seconds of the latest release |

### `AppDownloadProgressResult` Model

| Field | Type | Description |
|-------|------|-------------|
| `StatusCode` | `AppStatus` | Current download/install status |
| `DownloadBytes` | `long` | Bytes downloaded so far |

### `ApplicationOptions`

| Field | Setter | Description |
|-------|--------|-------------|
| `DeeplinkMessage` | `SetDeeplinkMessage` | Opaque string passed to the target/relaunched app |

### `AppStatus` Enum

| Value | Description |
|-------|-------------|
| `EntitledNotDownloaded` | User owns but hasn't downloaded |
| `Downloading` | Download in progress |
| `Installing` | Install in progress |
| `Installed` | Fully installed |
| `Uninstalling` | Uninstall in progress |

### `AppInstallResult` Enum

| Value | Description |
|-------|-------------|
| `Success` | Install succeeded |
| `Failure` | Install failed |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Treating `ReleaseDate` as ISO string | It's Unix epoch seconds. Convert with `DateTimeOffset.FromUnixTimeSeconds`. |
| Comparing version names instead of codes | Always compare `CurrentCode` vs `LatestCode` (integers). Names are display-only. |
| Forgetting `InstallAppUpdateAndRelaunch` exits the app | Save state to disk first. Anything in-memory is lost. |
| Not checking download progress | Without polling `CheckAppDownloadProgress`, the user has no feedback during the (potentially long) download. |
| Special-casing "app not installed" for `LaunchOtherApp` | The platform handles it -- takes the user to the Store page. No special code needed. |
| Skipping `ApplicationOptions` for `LaunchOtherApp` | If you want the receiving app to know why you launched it, set `DeeplinkMessage`. The receiver reads via `ApplicationLifecycle.GetLaunchDetailsRequest`. |
| Calling Application APIs before init | Always check `Core.IsInitialized()`. |

## Examples

### Complete Application Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationHelper : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    private bool isInitialized;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        isInitialized = !msg.IsError;
    }

    public async Task<ApplicationVersion> GetVersion()
    {
        if (!isInitialized) return null;
        var msg = await Application.GetVersion();
        return msg.IsError ? null : msg.Data;
    }

    public async Task<bool> HasUpdate()
    {
        var v = await GetVersion();
        return v != null && v.LatestCode > v.CurrentCode;
    }

    public async Task<bool> LaunchApp(ulong otherAppId, string deeplink = null)
    {
        if (!isInitialized) return false;
        ApplicationOptions opts = null;
        if (!string.IsNullOrEmpty(deeplink))
        {
            opts = new ApplicationOptions();
            opts.SetDeeplinkMessage(deeplink);
        }
        var msg = await Application.LaunchOtherApp(otherAppId, opts);
        return !msg.IsError;
    }
}
```

## Important Notes

1. **Always compare version codes, not names** -- `CurrentCode` and `LatestCode` are integers suitable for comparison. Version names are display-only.

2. **Save state before `InstallAppUpdateAndRelaunch`** -- your app process exits during installation. Persist anything important to disk first. Use `ApplicationOptions.SetDeeplinkMessage` to restore context after relaunch.

3. **Poll download progress for UX** -- `StartAppDownload` completes when the download finishes. Use `CheckAppDownloadProgress` in a coroutine for live progress UI.

4. **Don't pre-check app installation for cross-app launch** -- `LaunchOtherApp` automatically redirects to the Store page if the target app isn't installed.

5. **Pass deeplink messages for app-to-app travel** -- the receiving app reads it via `ApplicationLifecycle.GetLaunchDetailsRequest` with `LaunchType.Deeplink`.

## Useful Links

- [Meta Quest Application Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-application/)
- [App-to-App Travel](https://developer.oculus.com/documentation/unity/ps-app-to-app-travel/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
