# Notifications API

- **Kotlin Package**: `horizon.platform.notifications`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-platform-intro/
- **Minimum OS**: HzOS v83
- **Maven Artifact**: `horizon-platform-sdk-notifications-kotlin`

## Overview

The Notifications API is part of the Horizon Platform SDK. It provides an operation for Meta Quest Android applications to manage and display notifications:

1. **`deviceNotification(config)`** -- Send a device notification that displays a toast and/or feeds into the notification feed. Configure the notification with a title, message, optional media attachment, action buttons, and icons.

> **Setup**: For dependency installation, service connection initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

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

    // Notification sent successfully
} catch (e: NotificationsException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `deviceNotificationConfig: DeviceNotificationConfig` -- a configuration object built via the builder pattern containing notification content and optional action settings.

**Return type**: `Unit` (the method returns nothing on success)

**Key properties of `DeviceNotificationConfig`** (set via builder):
- `title: String` -- The notification title (default: `""`)
- `message: String` -- The notification message body (default: `""`)
- `mediaAttachmentUri: String?` -- URI of an image to attach to the notification (default: `null`)
- `ndid: String?` -- Notification delivery ID for tracking; must be unique per notification instance (default: `null`)
- `isToastOnly: Boolean?` -- If `true`, the notification shows only as a toast and does not appear in the notification feed (default: `null`)
- `actionDisplayType: ActionDisplayType?` -- How action buttons are displayed (default: `null`)
- `actionTitle: String?` -- Title text for the action button (default: `null`)
- `actionIcon: ActionIcon?` -- Icon for the action button (default: `null`)
- `actionAppId: String?` -- App ID to open when the action button is clicked (default: `null`)
- `actionPackageName: String?` -- Package name to open when the action button is clicked (default: `null`)
- `actionIntentData: String?` -- Intent data for the action button deep link (default: `null`)
- `actionIntentExtras: String?` -- JSON list of intent extras for the action button (default: `null`)

#### Send a Device Notification with Action Button

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException
import horizon.platform.notifications.configs.DeviceNotificationConfig
import horizon.platform.notifications.enums.ActionDisplayType
import horizon.platform.notifications.enums.ActionIcon

val notifications = Notifications()

try {
    val config = DeviceNotificationConfig.builder()
        .withTitle("Friend Request")
        .withMessage("Alex wants to be your friend!")
        .withMediaAttachmentUri("https://example.com/avatar.png")
        .withActionDisplayType(ActionDisplayType.Iconable)
        .withActionTitle("Accept")
        .withActionIcon(ActionIcon.Accept)
        .withActionAppId("<target-app-id>")
        .build()

    notifications.deviceNotification(config)
} catch (e: NotificationsException) {
    // Handle error
}
```

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

| Value | Int | Description |
|-------|-----|-------------|
| `Accept` | 0 | Accept icon (invitations, requests) |
| `Close` | 1 | Close icon (dismissing notifications) |
| `Destination` | 2 | Destination icon, filled (navigation) |
| `Call` | 3 | Call icon (initiating calls) |
| `DismissCall` | 4 | Dismiss call icon (declining calls) |
| `AddFriend` | 5 | Add friend icon (friend requests) |
| `Info` | 6 | Info icon (additional information) |
| `Party` | 7 | Party icon (group-related actions) |
| `Play` | 8 | Play icon (media playback, games) |
| `FollowAccept` | 9 | Follow accept icon |
| `FollowReject` | 10 | Follow reject icon |
| `Remove` | 11 | Remove icon (deleting items) |
| `Friends` | 12 | Friends icon (friend-related actions) |
| `Chat` | 13 | Chat icon (messaging) |
| `DestinationOutline` | 14 | Destination icon, outline style |
| `Travel` | 15 | Travel icon (transportation) |
| `Download` | 16 | Download icon |
| `Check` | 17 | Check icon (confirmation) |
| `Share` | 18 | Share icon (sharing content) |
| `Unknown` | 19 | Unknown icon |

## Error Handling

`deviceNotification()` throws `NotificationsException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes

This package does not define any package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Basic Toast Notification

Send a simple notification that appears only as a toast (not in the notification feed).

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException
import horizon.platform.notifications.configs.DeviceNotificationConfig

suspend fun showToast(title: String, message: String) {
    val client = Notifications()
    try {
        val config = DeviceNotificationConfig.builder()
            .withTitle(title)
            .withMessage(message)
            .withIsToastOnly(true)
            .build()

        client.deviceNotification(config)
    } catch (e: NotificationsException) {
        // Handle error -- log or show fallback message
    }
}
```

### Example 2: Notification with Media and Action Button

Send a rich notification with an image attachment and an action button that opens another app.

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException
import horizon.platform.notifications.configs.DeviceNotificationConfig
import horizon.platform.notifications.enums.ActionDisplayType
import horizon.platform.notifications.enums.ActionIcon

suspend fun sendRichNotification(
    title: String,
    message: String,
    imageUri: String,
    targetAppId: String,
) {
    val client = Notifications()
    try {
        val config = DeviceNotificationConfig.builder()
            .withTitle(title)
            .withMessage(message)
            .withMediaAttachmentUri(imageUri)
            .withNdid("notif-${System.currentTimeMillis()}")
            .withActionDisplayType(ActionDisplayType.Iconable)
            .withActionTitle("Open")
            .withActionIcon(ActionIcon.Play)
            .withActionAppId(targetAppId)
            .build()

        client.deviceNotification(config)
    } catch (e: NotificationsException) {
        // Handle error
    }
}
```

### Example 3: Notification with Deep Link Intent

Send a notification with an action button that deep links into a specific screen of your app.

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException
import horizon.platform.notifications.configs.DeviceNotificationConfig
import horizon.platform.notifications.enums.ActionDisplayType
import horizon.platform.notifications.enums.ActionIcon

suspend fun sendDeepLinkNotification(
    title: String,
    message: String,
    intentData: String,
    packageName: String,
) {
    val client = Notifications()
    try {
        val config = DeviceNotificationConfig.builder()
            .withTitle(title)
            .withMessage(message)
            .withNdid("deeplink-${System.currentTimeMillis()}")
            .withActionDisplayType(ActionDisplayType.TextOnly)
            .withActionTitle("View Details")
            .withActionPackageName(packageName)
            .withActionIntentData(intentData)
            .build()

        client.deviceNotification(config)
    } catch (e: NotificationsException) {
        // Handle error
    }
}
```

### Example 4: Repository Pattern with Error Handling

Wrap the Notifications API in a repository for clean architecture with structured error handling.

```kotlin
import horizon.platform.notifications.Notifications
import horizon.platform.notifications.NotificationsException
import horizon.platform.notifications.configs.DeviceNotificationConfig
import horizon.platform.notifications.enums.ActionDisplayType
import horizon.platform.notifications.enums.ActionIcon

sealed class NotificationResult {
    data object Success : NotificationResult()
    data class Error(val message: String) : NotificationResult()
}

class NotificationRepository {
    private val notifications = Notifications()

    suspend fun sendNotification(
        title: String,
        message: String,
        imageUri: String? = null,
        toastOnly: Boolean = false,
        actionTitle: String? = null,
        actionIcon: ActionIcon? = null,
        actionAppId: String? = null,
    ): NotificationResult {
        return try {
            val builder = DeviceNotificationConfig.builder()
                .withTitle(title)
                .withMessage(message)
                .withNdid("notif-${System.currentTimeMillis()}")

            imageUri?.let { builder.withMediaAttachmentUri(it) }

            if (toastOnly) {
                builder.withIsToastOnly(true)
            }

            if (actionTitle != null) {
                builder.withActionTitle(actionTitle)
                builder.withActionDisplayType(ActionDisplayType.Iconable)
                actionIcon?.let { builder.withActionIcon(it) }
                actionAppId?.let { builder.withActionAppId(it) }
            }

            notifications.deviceNotification(builder.build())
            NotificationResult.Success
        } catch (e: NotificationsException) {
            NotificationResult.Error(e.message ?: "Failed to send notification")
        }
    }
}
```

### Example 5: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class NotificationUiState(
    val isSending: Boolean = false,
    val lastResult: String? = null,
    val error: String? = null,
)

class NotificationViewModel(
    private val repository: NotificationRepository = NotificationRepository()
) : ViewModel() {
    private val _uiState = MutableStateFlow(NotificationUiState())
    val uiState: StateFlow<NotificationUiState> = _uiState

    fun sendNotification(title: String, message: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isSending = true, error = null)
            when (val result = repository.sendNotification(title, message)) {
                is NotificationResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isSending = false,
                        lastResult = "Notification sent successfully",
                    )
                }
                is NotificationResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isSending = false,
                        error = result.message,
                    )
                }
            }
        }
    }

    fun sendToastNotification(title: String, message: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isSending = true, error = null)
            when (val result = repository.sendNotification(title, message, toastOnly = true)) {
                is NotificationResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isSending = false,
                        lastResult = "Toast notification sent",
                    )
                }
                is NotificationResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isSending = false,
                        error = result.message,
                    )
                }
            }
        }
    }
}
```

## Important Notes

1. **Requires HzOS v83+** -- `deviceNotification()` requires HzOS v83 or later. On older OS versions, it returns status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

2. **Use unique `ndid` values** -- each notification must have a unique notification delivery ID (`ndid`). If you reuse the same `ndid` for a different notification, the new notification will not be displayed. Use timestamps or UUIDs to generate unique values.

3. **Toast-only vs. feed notifications** -- by default, notifications appear both as a toast and in the notification feed. Set `isToastOnly = true` if you only want a transient toast that does not persist in the feed.

4. **Action button configuration** -- to add an action button to a notification, set at minimum `actionTitle` and one of `actionAppId`, `actionPackageName`, or `actionIntentData` to define what happens when the button is clicked. Use `actionDisplayType` to control how the button is rendered (with icon, without icon, or text only).

5. **`appPackageNameForAppIcon` is deprecated** -- this field for customizing the notification app icon is deprecated. Avoid using it in new code.
