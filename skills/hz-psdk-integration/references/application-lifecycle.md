# Application Lifecycle API

- **Kotlin Package**: `horizon.platform.applicationlifecycle`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-application-lifecycle
- **Minimum OS**: HzOS v78
- **Maven Artifact**: `horizon-platform-sdk-application-lifecycle-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

Three operations for handling app-to-app travel, invite/rich-presence deeplinks, and tracking deeplink effectiveness:

1. **`launchIntentChanged()`** -- Flow that fires when a launch intent is received (cold or warm start)
2. **`getLaunchDetails()`** -- details on how the app was started (launch type, deeplink message, destination, session IDs)
3. **`logDeeplinkResult(trackingId, result)`** -- report whether a deeplink attempt succeeded or failed, and the failure reason

All methods are on `ApplicationLifecycle()` and throw `ApplicationLifecycleException` on failure. Package paths:

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.models.LaunchDetails
import horizon.platform.applicationlifecycle.enums.LaunchType
import horizon.platform.applicationlifecycle.enums.LaunchResult
```

## API Usage

#### Retrieve Launch Details

```kotlin
val applicationLifecycle = ApplicationLifecycle()
val details: LaunchDetails = applicationLifecycle.getLaunchDetails()
val launchType = details.launchType           // LaunchType enum
val deeplinkMsg = details.deeplinkMessage     // opaque deeplink string, nullable
val destination = details.destinationApiName  // destination API name, nullable
val launchSource = details.launchSource       // where the deeplink came from, nullable
val lobbyId = details.lobbySessionId          // nullable
val matchId = details.matchSessionId          // nullable
val trackingId = details.trackingId           // deeplink tracking ID, nullable
val users = details.users                     // List<User>, nullable
```
**Returns** `LaunchDetails` -- an immutable object describing how the application was launched.

#### Listen for Launch Intent Changes

Detects a new launch intent received while the app is already running (warm start).

```kotlin
val applicationLifecycle = ApplicationLifecycle()
applicationLifecycle.launchIntentChanged().collect { intentType: String ->
    // A new launch intent was received -- call getLaunchDetails() for full details
    val details = applicationLifecycle.getLaunchDetails()
    // Handle the new launch intent
}
```
**Returns** `Flow<String>` -- a Kotlin Flow emitting the type of launch intent received.

#### Log Deeplink Result

```kotlin
val applicationLifecycle = ApplicationLifecycle()
val trackingId = applicationLifecycle.getLaunchDetails().trackingId
if (trackingId != null) {
    applicationLifecycle.logDeeplinkResult(trackingId, LaunchResult.Success)
}
```
**Parameters**:
- `trackingId: String` -- unique tracking ID from the deeplink attempt (`LaunchDetails.trackingId`)
- `result: LaunchResult` -- whether the deeplink succeeded or failed, and the failure reason

## Data Types

### `LaunchDetails` Model (returned by `getLaunchDetails()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `deeplinkMessage` | `String?` | `null` | Opaque deeplink string provided by the developer |
| `destinationApiName` | `String?` | `null` | The intended destination the user wants to go to |
| `launchSource` | `String?` | `null` | Distinguishes where the deeplink came from (e.g., events, rich presence) |
| `launchType` | `LaunchType` | `LaunchType.Unknown` | How the application was launched |
| `lobbySessionId` | `String?` | `null` | The intended lobby session the user wants to join |
| `matchSessionId` | `String?` | `null` | The intended match session the user wants to join |
| `trackingId` | `String?` | `null` | Unique identifier for tracking the deeplinking flow |
| `users` | `List<User>?` | `null` | The intended users the user wants to be with |

### `LaunchType` Enum

| Value | Int | Description |
|-------|-----|-------------|
| `Unknown` | 0 | Launch type is unknown |
| `Normal` | 1 | Normal launch from the user's library |
| `Invite` | 2 | Launch from the user accepting an invite |
| `Coordinated` | 3 | **Deprecated** |
| `Deeplink` | 4 | Launched from a deeplink (e.g., app-to-app travel) |

### `LaunchResult` Enum (parameter for `logDeeplinkResult()`)

| Value | Int | Description |
|-------|-----|-------------|
| `Unknown` | 0 | Launch result is unknown |
| `Success` | 1 | The application launched successfully |
| `FailedRoomFull` | 2 | Launch failed because the room was full |
| `FailedGameAlreadyStarted` | 3 | Launch failed because the game has already started |
| `FailedRoomNotFound` | 4 | Launch failed because the room could not be found |
| `FailedUserDeclined` | 5 | Launch failed because the user declined the invitation |
| `FailedOtherReason` | 6 | Launch failed for some other reason |

### `User` Model (nested in `LaunchDetails.users`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `displayName` | `String?` | `null` | Displayable name chosen by the user |
| `id` | `String` | `""` | Unique user identifier |
| `imageUrl` | `String?` | `null` | URL of the user's profile picture |
| `oculusId` | `String?` | `null` | The user's Oculus ID |

## Error Handling

All methods throw `ApplicationLifecycleException` (extends `HzPlatformSdkException`) on failure. Wrap calls in try/catch. The `launchIntentChanged()` Flow throws from within `collect`; use `.catch { }` for stream errors.

### Status Codes (`ApplicationLifecycleStatusCode`)

This API uses only the common status codes. See [common-setup.md](common-setup.md) for the full table.

## Important Notes

1. **`launchIntentChanged()` returns a `Flow<String>`** (not a suspend function) -- collect it within a coroutine scope. It emits whenever a new launch intent is received, including during warm starts.
2. **Call `getLaunchDetails()` after a `launchIntentChanged()` event** -- the event only provides the intent type string; call `getLaunchDetails()` to get deeplink message, destination, session IDs, and users.
3. **Always log deeplink results** -- when launched via `LaunchType.Deeplink` or `LaunchType.Invite`, call `logDeeplinkResult()` with the `trackingId` from `LaunchDetails` to report success/failure. This helps Meta track deeplink effectiveness.
4. **`LaunchType.Coordinated` is deprecated** -- do not design new flows around it; handle it as a fallback case only.
5. **The `users` field requires the Users SDK** -- to access `LaunchDetails.users`, add the `horizon-platform-sdk-users-kotlin` dependency. Each `User` contains `id`, `displayName`, `imageUrl`, and `oculusId`.
6. **OS gating** -- `getLaunchDetails()` and `launchIntentChanged()` require HzOS v78+; `logDeeplinkResult()` requires v83+. On older OS they return 1003 (`ProviderOperationNotSupported`). Require a minimum OS in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.
