# Notifications API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-notifications/
- **Namespace**: Oculus.Platform

## Overview

The Notifications API is part of the Horizon Platform SDK Unity package. It provides operations for Meta Quest Unity applications to send on-device notifications (toast + notification feed):

1. **`Notifications.DeviceNotification(config)`** -- Send a device notification with toast and optional feed persistence

**Note:** This API covers **device notifications** -- notifications sent from your running app. For server-sent push notifications delivered while your app is not running, use the separate `push_notification` package.

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## API Usage

### Send a Simple Notification

```csharp
public async Task NotifyAchievementUnlocked(string achievementName)
{
    if (!Core.IsInitialized()) return;

    var config = new DeviceNotificationConfig();
    config.SetTitle("Achievement Unlocked!");
    config.SetMessage($"You earned: {achievementName}");
    config.SetNdid($"achievement_{achievementName}_{DateTime.UtcNow.Ticks}"); // unique
    config.SetIsToastOnly(false); // also persist in notification feed

    var msg = await Notifications.DeviceNotification(config);
    if (msg.IsError)
    {
        Debug.LogError($"Notification failed: {msg.GetError().Message}");
    }
}
```

### The `Ndid` Rule

`Ndid` is the **Notification Delivery ID**. Each notification you send **must** have a unique `Ndid`. If you reuse one, the new notification is silently suppressed.

```csharp
config.SetNdid($"my_event_{DateTime.UtcNow.Ticks}");
```

### Toast-Only vs Feed-Persistent

| `IsToastOnly` | Behavior |
|----------------|----------|
| `true` | Shows the toast, does NOT add to notification feed (transient) |
| `false` | Shows the toast AND persists in feed until dismissed (default) |

Use `true` for ephemeral status updates ("Score submitted!"); use `false` for things the user might want to revisit ("Daily quest complete").

### Add a Media Attachment

```csharp
config.SetMediaAttachmentUri("https://your-cdn.com/badge.png");
```

The URI must be reachable from the device. Local file URIs are not supported.

### Add an Action Button

```csharp
public async Task NotifyWithJoinAction(string lobbyName)
{
    var config = new DeviceNotificationConfig();
    config.SetTitle("Lobby ready");
    config.SetMessage($"Your friends are waiting in {lobbyName}");
    config.SetNdid($"lobby_ready_{DateTime.UtcNow.Ticks}");

    config.SetActionTitle("Join");
    config.SetActionIcon(ActionIcon.Play);
    config.SetActionDisplayType(ActionDisplayType.Iconable);

    // Tapping the action launches your app via intent
    config.SetActionPackageName("com.yourstudio.yourapp");
    config.SetActionIntentData("yourapp://lobby/" + lobbyName);

    await Notifications.DeviceNotification(config);
}
```

### Custom App Icon (Cross-App Notifications)

When your app sends a notification on behalf of another app (rare), set:

```csharp
config.SetAppPackageNameForAppIcon("com.otherstudio.theirapp");
```

For most use cases, leave this unset -- the notification uses your app's own icon.

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Notifications.DeviceNotification(config)` | `Request` | Show a system toast and (optionally) add to notification feed |

### `DeviceNotificationConfig` Fields

| Field | Setter | Required | Notes |
|-------|--------|----------|-------|
| `Title` | `SetTitle` | Yes | Notification title |
| `Message` | `SetMessage` | Yes | Body text |
| `Ndid` | `SetNdid` | Yes | **Must be unique per notification** |
| `IsToastOnly` | `SetIsToastOnly` | No (default false) | True = toast only, no feed entry |
| `MediaAttachmentUri` | `SetMediaAttachmentUri` | No | Reachable URI for an image |
| `AppPackageNameForAppIcon` | `SetAppPackageNameForAppIcon` | No | Override app icon (rare) |
| `ActionDisplayType` | `SetActionDisplayType` | No | Iconable, IconableColorless, TextOnly |
| `ActionTitle` | `SetActionTitle` | No (required if any action set) | Action button label |
| `ActionIcon` | `SetActionIcon` | No | One of the `ActionIcon` enum values |
| `ActionAppId` | `SetActionAppId` | No | Open this Quest app on tap |
| `ActionPackageName` | `SetActionPackageName` | No | Open this Android package on tap |
| `ActionIntentData` | `SetActionIntentData` | No | Android intent data URI |
| `ActionIntentExtras` | `SetActionIntentExtras` | No | JSON string of intent extras |

### `ActionDisplayType` Enum

| Value | Visual |
|-------|--------|
| `Iconable` | Colored icon + label |
| `IconableColorless` | Monochrome icon + label |
| `TextOnly` | Label only, no icon |
| `Unknown` | Reserved |

### `ActionIcon` Enum

| Icon | Typical use |
|------|-------------|
| `Accept` | Accept invite/request |
| `Close` | Dismiss |
| `Destination` / `DestinationOutline` | Navigation |
| `Call` / `DismissCall` | Voice call accept/decline |
| `AddFriend` | Friend request |
| `Info` | More details |
| `Party` | Party/group action |
| `Play` | Start game/media |
| `FollowAccept` / `FollowReject` | Follow request |
| `Remove` | Delete |
| `Friends` | Friend-related |
| `Chat` | Open chat |
| `Travel` | Travel between apps |
| `Download` | Download content |
| `Check` | Confirm |
| `Share` | Share |
| `Unknown` | Reserved |

### Action Target Routing

| Field set | Behavior |
|-----------|----------|
| `ActionPackageName` only | Opens that app's main activity |
| `ActionAppId` only | Opens that Quest app by App ID |
| `ActionPackageName` + `ActionIntentData` | Sends Android intent with data URI to that package |
| `ActionPackageName` + `ActionIntentData` + `ActionIntentExtras` (JSON) | Full intent with extras |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Reusing the same `Ndid` across notifications | Each `Ndid` is unique. The platform silently suppresses duplicates. Include a timestamp/counter. |
| Forgetting to set `Ndid` at all | Required. The notification will not be delivered. |
| Confusing device notifications with push notifications | This API fires **while your app is running**. For server-sent push to a not-running app, use the `push_notification` package. |
| Local file URIs for media attachments | Not supported. Host the image at an HTTPS URL. |
| Setting an action with no `ActionTitle` | The action button needs a label. |
| Setting both `ActionAppId` and `ActionPackageName` | Pick one. Both is undefined behavior. |
| Spamming notifications per frame | Rate-limit yourself; the user will hate you and the platform may throttle. |
| Calling Notifications before init | Always check `Core.IsInitialized()`. |
| Setting `IsToastOnly=true` for important persistent info | Toast-only means it disappears immediately. Use `false` for things the user should be able to revisit. |

## Examples

### Complete Notifications Helper

```csharp
using Oculus.Platform;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class QuestNotifications
{
    private static int sequence;

    public static async Task<bool> Toast(string title, string message)
    {
        if (!Core.IsInitialized()) return false;

        var config = new DeviceNotificationConfig();
        config.SetTitle(title);
        config.SetMessage(message);
        config.SetNdid($"toast_{DateTime.UtcNow.Ticks}_{++sequence}");
        config.SetIsToastOnly(true);

        var msg = await Notifications.DeviceNotification(config);
        return !msg.IsError;
    }

    public static async Task<bool> Persistent(string title, string message, string mediaUri = null)
    {
        if (!Core.IsInitialized()) return false;

        var config = new DeviceNotificationConfig();
        config.SetTitle(title);
        config.SetMessage(message);
        config.SetNdid($"persist_{DateTime.UtcNow.Ticks}_{++sequence}");
        config.SetIsToastOnly(false);
        if (!string.IsNullOrEmpty(mediaUri)) config.SetMediaAttachmentUri(mediaUri);

        var msg = await Notifications.DeviceNotification(config);
        return !msg.IsError;
    }

    public static async Task<bool> WithAction(
        string title, string message,
        string actionLabel, ActionIcon icon, string targetPackage, string intentData = null)
    {
        if (!Core.IsInitialized()) return false;

        var config = new DeviceNotificationConfig();
        config.SetTitle(title);
        config.SetMessage(message);
        config.SetNdid($"action_{DateTime.UtcNow.Ticks}_{++sequence}");
        config.SetActionTitle(actionLabel);
        config.SetActionIcon(icon);
        config.SetActionDisplayType(ActionDisplayType.Iconable);
        config.SetActionPackageName(targetPackage);
        if (!string.IsNullOrEmpty(intentData)) config.SetActionIntentData(intentData);

        var msg = await Notifications.DeviceNotification(config);
        return !msg.IsError;
    }
}
```

Usage:

```csharp
await QuestNotifications.Toast("Saved", "Game saved successfully");
await QuestNotifications.Persistent("Daily Quest", "Defeat 10 enemies for a reward!", mediaUri: "https://cdn.example.com/quest.png");
await QuestNotifications.WithAction(
    "Friend Online",
    "Bob is now playing your game",
    actionLabel: "Invite",
    icon: ActionIcon.AddFriend,
    targetPackage: "com.yourstudio.yourapp",
    intentData: "yourapp://invite/bob");
```

## Important Notes

1. **Every `Ndid` must be unique** -- use timestamps and/or counters to guarantee uniqueness. Duplicate `Ndid` values cause the notification to be silently suppressed.

2. **Toast-only vs persistent** -- use `IsToastOnly = true` for ephemeral status (auto-save complete, score submitted). Use `false` for actionable items the user might revisit (achievements, quest unlocks).

3. **Device notifications vs push notifications** -- this API delivers in-app notifications while your app is running. For server-sent push to non-running apps, use the separate `push_notification` package.

4. **Action buttons require `ActionTitle`** -- set it whenever you set any action field. Pick one routing target: `ActionAppId` OR `ActionPackageName` (with optional intent data/extras).

5. **Rate-limit notifications** -- don't fire notifications per frame or per gameplay tick. Batch related events; show one summary.

6. **Media attachments must be hosted** -- local file URIs are not supported. Use HTTPS URLs reachable from the device.

## Useful Links

- [Meta Quest Notifications Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-notifications/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
