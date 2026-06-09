# Push Notification API

- **Kotlin Package**: `horizon.platform.pushnotification`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-platform-intro/
- **Minimum OS**: HzOS v77
- **Maven Artifact**: `horizon-platform-sdk-push-notification-kotlin`

> **GA as of 2026.** Push Notification ("Headset Push API") is now available to all 3P developers — it is no longer partner-only.

## Overview

The Push Notification API lets a Meta Quest Android app **register the device/user to receive server-triggered push notifications** (the "Headset Push" flow — server-defined toasts that re-engage users). It provides two operations:

1. **`register()`** -- Register the device to receive push notifications. Returns a `PushNotificationResult` whose `id` is the registered notification target you push to.
2. **`unregister()`** -- Unregister the device from receiving push notifications. Returns a `Boolean` indicating success.

> **Don't confuse this with the `notifications` package** ([notifications.md](notifications.md)): `notifications` *sends/triggers* notifications from your app (`triggerNotification`, `deviceNotification`); `push_notification` (this doc) *registers to receive* server-triggered pushes.

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

#### Register for Push Notifications

```kotlin
import horizon.platform.pushnotification.PushNotification
import horizon.platform.pushnotification.PushNotificationException
import horizon.platform.pushnotification.models.PushNotificationResult

val pushNotification = PushNotification()

try {
    val result: PushNotificationResult = pushNotification.register()
    val targetId = result.id   // the registered notification id to push to
    // Persist/forward targetId to your backend so it can push to this device.
} catch (e: PushNotificationException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `PushNotificationResult` -- contains `id` (the registered notification target string)

#### Unregister from Push Notifications

```kotlin
import horizon.platform.pushnotification.PushNotification
import horizon.platform.pushnotification.PushNotificationException

val pushNotification = PushNotification()

try {
    val success: Boolean = pushNotification.unregister()
} catch (e: PushNotificationException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `Boolean` -- `true` if unregistration succeeded

## Data Types

### `PushNotificationResult` Interface (returned by `register()`)

| Property | Type | Description |
|----------|------|-------------|
| `id` | `String` | The registered notification id — the target you push notifications to |

## Error Handling

All methods throw `PushNotificationException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### `PushNotificationStatusCode` (package-specific codes)

| Value | Code | Meaning |
|-------|------|---------|
| `AppIdEmptyOrBlank` | 2001 | The application ID provided for registration is empty/whitespace |
| `TargetIdEmptyOrBlank` | 2002 | The notification target ID returned from the backend is null/empty/whitespace |
| `RegisterNotificationFailed` | 2003 | The registration request to the backend failed (network, backend, or invalid params) |
| `UnregisterNotificationFailed` | 2004 | The unregistration request to the backend failed |

Common status codes (0–6, 190, 1001–1005, 1007) also apply — see [common-setup.md](common-setup.md).

## Important Notes

1. **Receive vs send** — this package only *registers to receive*. To actually deliver a notification you trigger it from your backend (or via the `notifications` package's `triggerNotification`).
2. **Forward `result.id` to your backend** — the registered `id` is what your server targets when sending a push; persist it server-side after `register()`.
3. **Provider gates (multiple).** A `ProviderFeatureNotEnabled` (1002) means a gate is off. On a dev device, registration typically needs **three** overrides, not one: `horizon_platform_sdk:oc_3p_headset_push`, `horizon_platform_sdk:3p_headset_push_api_psdk`, and `horizon_platform_sdk:notification_provider_enabled` (set via `maui mdc o …`). See `psdk-push-notif-e2e-validation/references/troubleshooting.md`. These dev-device GK/MC overrides are distinct from server-side entitlement.
4. **GA** — no Meta partnership approval required as of 2026 (previously partner-only).
