# Notifications API

- **Kotlin Package**: `horizon.platform.notifications`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-platform-intro/
- **Minimum OS**: HzOS v83
- **Maven Artifact**: `horizon-platform-sdk-notifications-kotlin`

- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

The Notifications API provides two operations for Meta VR Android apps to send notifications:

1. **`triggerNotification(requestId, userIds)`** -- Trigger an event-based notification (defined server-side, referenced by `requestId`) for delivery to specific `userIds`. Use for server-defined re-engagement events.
2. **`deviceNotification(config)`** -- Send a device notification that displays a toast and/or feeds into the notification feed. Configure with a title, message, optional media attachment, action buttons, and icons.

> **Two different notification packages — don't confuse them:**
> - **`notifications`** (this doc) — your app *sends/triggers* a notification (`triggerNotification`, `deviceNotification`).
> - **`push_notification`** ([push-notification.md](push-notification.md)) — your app *registers to receive* server-triggered push notifications (the "Headset Push API"). Separate package, `register()` / `unregister()`.

> **Setup**: For dependency installation, service connection initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

#### Trigger an Event-Based Notification

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException

val notifications = Notifications()

try {
    // requestId references a notification event configured server-side;
    // userIds are the recipients.
    notifications.triggerNotification(
        requestId = "weekly_streak_reminder",
        userIds = listOf("1234567890", "9876543210"),
    )
} catch (e: NotificationsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `requestId: String` -- unique identifier of the server-defined notification to trigger
- `userIds: List<String>` -- the users to deliver the notification to

**Return type**: `Unit` (returns nothing on success)

#### Send a Device Notification

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException
import horizon.platform.notifications.configs.DeviceNotificationConfig

val notifications = Notifications()

try {
    val config = DeviceNotificationConfig.builder()
        .withTitle("New Achievement Unlocked")
        .withMessage("You earned the 'First Steps' badge!")
        .build()

    notifications.deviceNotification(config)
} catch (e: NotificationsException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `deviceNotificationConfig: DeviceNotificationConfig` -- a configuration object built via the builder pattern containing notification content and optional action settings.

**Return type**: `Unit` (returns nothing on success)

Same call pattern with more builder options:
- **Action button**: `.withActionDisplayType(ActionDisplayType.Iconable)`, `.withActionTitle("Accept")`, `.withActionIcon(ActionIcon.Accept)`, and one of `.withActionAppId(...)` / `.withActionPackageName(...)` / `.withActionIntentData(...)` (optionally `.withActionIntentExtras(...)`).
- **Media**: `.withMediaAttachmentUri("https://example.com/avatar.png")`, `.withNdid("notif-${System.currentTimeMillis()}")`.
- **Toast-only**: `.withIsToastOnly(true)`.

## Data Types

### `DeviceNotificationConfig` (passed to `deviceNotification()`)

Built using the builder pattern via `DeviceNotificationConfig.builder()`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `title` | `String` | `""` | The notification title |
| `message` | `String` | `""` | The notification message body |
| `mediaAttachmentUri` | `String?` | `null` | URI of an image to attach |
| `ndid` | `String?` | `null` | Unique notification delivery ID for tracking |
| `isToastOnly` | `Boolean?` | `null` | If `true`, show only as toast (not in feed) |
| `appPackageNameForAppIcon` | `String?` | `null` | Package name for custom app icon (deprecated) |
| `actionDisplayType` | `ActionDisplayType?` | `null` | How action buttons are displayed |
| `actionTitle` | `String?` | `null` | Title text for the action button |
| `actionIcon` | `ActionIcon?` | `null` | Icon for the action button |
| `actionAppId` | `String?` | `null` | App ID to launch on action click |
| `actionPackageName` | `String?` | `null` | Package name to launch on action click |
| `actionIntentData` | `String?` | `null` | Intent data for deep linking |
| `actionIntentExtras` | `String?` | `null` | JSON list of intent extras |

### `ActionDisplayType` Enum

Determines how notification action buttons are rendered.

| Value | Int | Description |
|-------|-----|-------------|
| `Iconable` | 0 | Actions displayed with colored icons |
| `IconableColorless` | 1 | Actions displayed with colorless icons |
| `TextOnly` | 2 | Actions displayed as text only, without icons |
| `Unknown` | 3 | Unknown display type |

### `ActionIcon` Enum

Specifies the icon for a notification action button.

| Value | Int | | Value | Int |
|-------|-----|-|-------|-----|
| `Accept` | 0 | | `Remove` | 11 |
| `Close` | 1 | | `Friends` | 12 |
| `Destination` | 2 | | `Chat` | 13 |
| `Call` | 3 | | `DestinationOutline` | 14 |
| `DismissCall` | 4 | | `Travel` | 15 |
| `AddFriend` | 5 | | `Download` | 16 |
| `Info` | 6 | | `Check` | 17 |
| `Party` | 7 | | `Share` | 18 |
| `Play` | 8 | | `Unknown` | 19 |
| `FollowAccept` | 9 | | | |
| `FollowReject` | 10 | | | |

`Destination` is filled style, `DestinationOutline` is outline style.

## Error Handling

`deviceNotification()` throws `NotificationsException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

This package does not define any package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Important Notes

1. **Requires HzOS v83+** -- `deviceNotification()` requires HzOS v83 or later. On older OS versions, it returns status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

2. **Use unique `ndid` values** -- each notification must have a unique notification delivery ID (`ndid`). If you reuse the same `ndid` for a different notification, the new notification will not be displayed. Use timestamps or UUIDs.

3. **Toast-only vs. feed notifications** -- by default, notifications appear both as a toast and in the notification feed. Set `isToastOnly = true` for a transient toast that does not persist in the feed.

4. **Action button configuration** -- to add an action button, set at minimum `actionTitle` and one of `actionAppId`, `actionPackageName`, or `actionIntentData`. Use `actionDisplayType` to control rendering (with icon, without icon, or text only).

5. **`appPackageNameForAppIcon` is deprecated** -- this field for customizing the notification app icon is deprecated. Avoid using it in new code.
