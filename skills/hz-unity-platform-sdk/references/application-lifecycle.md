# Application Lifecycle API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-app-to-app-travel/
- **Namespace**: Oculus.Platform

## Overview

The Application Lifecycle API is part of the Horizon Platform SDK Unity package. It provides operations for Meta Quest Unity applications to detect how the user launched the app and handle deeplinks and invites:

1. **`ApplicationLifecycle.GetLaunchDetailsRequest()`** -- Get the current launch intent (cold start or last warm start)
2. **`ApplicationLifecycle.LogDeeplinkResultRequest(trackingId, result)`** -- Report whether the app honored a deeplink
3. **`ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(cb)`** -- Subscribe to warm-start intent changes

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Register your app** at [developer.oculus.com/manage](https://developer.oculus.com/manage/)
2. **Set up Destinations** in the Developer Dashboard (used for deeplinks)
3. **Note your App ID**

## API Usage

### Register Notification Callback and Process Launch (Immediately After Init)

Subscribe to `launch_intent_changed` **immediately after init**, before fetching launch details, so you don't miss any pending events.

```csharp
async void Start()
{
    var msg = await Core.AsyncInitialize(appId);
    if (msg.IsError) return;

    // Warm-start hook: fires when a new launch intent arrives while the app is running
    ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(OnLaunchIntentChanged);

    // Cold-start: read the launch details that brought us here
    await ProcessLaunchDetails();
}
```

### Read Launch Details (Cold Start)

```csharp
private async Task ProcessLaunchDetails()
{
    var msg = await ApplicationLifecycle.GetLaunchDetailsRequest();
    if (msg.IsError) return;

    LaunchDetails details = msg.Data;
    Debug.Log($"Launched as {details.LaunchType}, dest={details.DestinationApiName}, deeplink={details.DeeplinkMessage}");

    HandleLaunchDetails(details);
}

private void HandleLaunchDetails(LaunchDetails details)
{
    switch (details.LaunchType)
    {
        case LaunchType.Normal:
            GoToMainMenu();
            break;

        case LaunchType.Invite:
            TravelToLobby(
                details.DestinationApiName,
                details.LobbySessionID,
                details.MatchSessionID,
                details.DeeplinkMessage,
                details.UsersOptional);
            ReportDeeplinkResult(details.TrackingID, success: true);
            break;

        case LaunchType.Deeplink:
            HandleDeeplink(details.DeeplinkMessage, details.LaunchSource);
            ReportDeeplinkResult(details.TrackingID, success: true);
            break;

        case LaunchType.Coordinated:  // deprecated
        case LaunchType.Unknown:
        default:
            GoToMainMenu();
            break;
    }
}
```

### Report Deeplink Result

After handling a deeplink, **always** call `LogDeeplinkResultRequest` so the platform can track success rates.

```csharp
private async Task ReportDeeplinkResult(string trackingId, bool success)
{
    if (string.IsNullOrEmpty(trackingId)) return;
    var result = success ? LaunchResult.Success : LaunchResult.FailedRoomFull;
    await ApplicationLifecycle.LogDeeplinkResultRequest(trackingId, result);
}
```

### Handle Warm-Start Intent Changes

If the app is already running and the user accepts a new invite, re-fetch launch details:

```csharp
private async void OnLaunchIntentChanged(Message<string> msg)
{
    if (msg.IsError) return;

    Debug.Log($"Warm-start intent changed: {msg.Data}");
    var detailsMsg = await ApplicationLifecycle.GetLaunchDetailsRequest();
    if (!detailsMsg.IsError)
    {
        HandleLaunchDetails(detailsMsg.Data);
    }
}
```

The string payload is opaque -- always re-fetch full details with `GetLaunchDetailsRequest`.

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ApplicationLifecycle.GetLaunchDetailsRequest()` | `Request<LaunchDetails>` | Get the current launch intent (cold or last warm) |
| `ApplicationLifecycle.LogDeeplinkResultRequest(trackingId, result)` | `Request` | Report whether your app honored a deeplink |
| `ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(cb)` | (void) | Subscribe to warm-start intent changes |

### `LaunchDetails` Model

| Field | Type | Description |
|-------|------|-------------|
| `LaunchType` | `LaunchType` | How the user arrived (Normal, Invite, Deeplink, etc.) |
| `DeeplinkMessage` | `string` | Opaque string set via `Application.LaunchOtherApp` or `GroupPresence.SetDeeplinkMessageOverride` |
| `DestinationApiName` | `string` | The Destination the user wants to go to |
| `LobbySessionID` | `string` | The lobby session for invite/deeplink |
| `MatchSessionID` | `string` | The match session for invite/deeplink |
| `LaunchSource` | `string` | Which surface the deeplink came from (events, rich presence, etc.) |
| `TrackingID` | `string` | Pass to `LogDeeplinkResultRequest` to report success/failure |
| `UsersOptional` | `UserList` | If provided, users the launcher wants to be with (nullable) |

### `LaunchType` Enum

| Value | Description |
|-------|-------------|
| `Normal` | Standard launch from user's library |
| `Invite` | User accepted an invite from a friend |
| `Deeplink` | Launched from another app's `Application.LaunchOtherApp` call |
| `Coordinated` | Deprecated -- treat like `Unknown` |
| `Unknown` | Unrecognized launch type |

### `LaunchResult` Enum

| Value | When to use |
|-------|-------------|
| `Success` | Took user where they wanted to go |
| `FailedRoomFull` | Lobby full |
| `FailedGameAlreadyStarted` | Match in progress, can't join |
| `FailedGameNotFound` | Lobby/match no longer exists |
| `FailedUserDeclined` | User chose not to follow the deeplink (rare) |
| `FailedOtherReason` | Anything else |
| `Unknown` | Unrecognized result |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Always going to main menu regardless of `LaunchType` | Branch on `LaunchType` and route invite/deeplink launches to the right destination. |
| Not subscribing to `launch_intent_changed` | Warm-start invites won't be handled. Subscribe immediately after init. |
| Skipping `LogDeeplinkResult` | The platform tracks deeplink success. Always log a result for `Invite` and `Deeplink` launches with a `TrackingID`. |
| Parsing the warm-start `string` payload | It's opaque. Always re-fetch with `GetLaunchDetailsRequest`. |
| Forgetting to null-check `UsersOptional` | Nullable. Check before iterating. |
| Calling Lifecycle APIs before init | Always check `Core.IsInitialized()`. |
| Treating `Coordinated` as active | Deprecated. Treat it like `Unknown`. |
| Calling `GetLaunchDetailsRequest` repeatedly hoping for updates | Cold-start details only change on warm-start; subscribe to `launch_intent_changed` instead. |

## Examples

### Complete Application Lifecycle Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AppLifecycleManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    public event Action<LaunchDetails> LaunchProcessed;

    async void Start()
    {
        var initMsg = await Core.AsyncInitialize(appId);
        if (initMsg.IsError) { Debug.LogError(initMsg.GetError().Message); return; }

        ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(OnIntentChanged);
        await ProcessCurrentLaunch();
    }

    private async Task ProcessCurrentLaunch()
    {
        var msg = await ApplicationLifecycle.GetLaunchDetailsRequest();
        if (msg.IsError) return;
        LaunchProcessed?.Invoke(msg.Data);
        await ReportSuccessIfNeeded(msg.Data);
    }

    private async void OnIntentChanged(Message<string> msg)
    {
        if (msg.IsError) return;
        var detailsMsg = await ApplicationLifecycle.GetLaunchDetailsRequest();
        if (detailsMsg.IsError) return;
        LaunchProcessed?.Invoke(detailsMsg.Data);
        await ReportSuccessIfNeeded(detailsMsg.Data);
    }

    private async Task ReportSuccessIfNeeded(LaunchDetails d)
    {
        if (d.LaunchType == LaunchType.Invite || d.LaunchType == LaunchType.Deeplink)
        {
            if (!string.IsNullOrEmpty(d.TrackingID))
            {
                await ApplicationLifecycle.LogDeeplinkResultRequest(d.TrackingID, LaunchResult.Success);
            }
        }
    }
}
```

Usage in gameplay code:

```csharp
appLifecycle.LaunchProcessed += details =>
{
    switch (details.LaunchType)
    {
        case LaunchType.Invite: TravelToLobby(details); break;
        case LaunchType.Deeplink: HandleDeeplink(details); break;
        default: GoToMainMenu(); break;
    }
};
```

## Important Notes

1. **Subscribe to `SetLaunchIntentChangedNotificationCallback` immediately after init** -- before any other async work, so you don't miss pending events.

2. **Branch on `LaunchType`** -- invite and deeplink launches should bypass the main menu and take the user directly to the requested destination.

3. **Always report deeplink results** -- for any launch with a `TrackingID`, call `LogDeeplinkResultRequest` with the most accurate `LaunchResult` enum value.

4. **Warm-start payload is opaque** -- never parse the string from the callback. Always re-fetch via `GetLaunchDetailsRequest`.

5. **Coordinate with Group Presence** -- after processing an Invite/Deeplink launch, set `GroupPresence` to reflect the user's new location.

## Useful Links

- [Meta Quest App-to-App Travel & Deeplinks (Unity)](https://developer.oculus.com/documentation/unity/ps-app-to-app-travel/)
- [Destinations Overview](https://developer.oculus.com/documentation/unity/ps-destinations-overview/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
