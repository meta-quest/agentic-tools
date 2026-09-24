# Rich Presence API

> **Deprecated**: Rich Presence has been deprecated in favor of [Group Presence](https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-group-presence). New integrations should use the Group Presence API instead. See the migration notes at the bottom of this file.

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.richpresence` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-group-presence |
| **Minimum OS** | HzOS v85 |
| **Maven Artifact** | `horizon-platform-sdk-rich-presence-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

The Rich Presence API allows Meta VR Android apps to manage the user's presence information, including what they are doing and where they are in the app. Public operations:

1. **`clear()`** -- Clear the current rich presence
2. **`getDestinations(coroutineScope)`** -- Retrieve all available destinations that the presence can be set to (paginated)
3. **`set(richPresenceOptions)`** -- Set the rich presence with a combination of options (destination, deeplink message, joinability)

## API Usage

### Clear Rich Presence

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException

val richPresence = RichPresence()

try {
    richPresence.clear()
} catch (e: RichPresenceException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `Unit` (no return value)

### Get Available Destinations

Retrieve all destinations that the presence can be set to. Returns paginated results.

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.models.Destination
import kotlinx.coroutines.CoroutineScope

val richPresence = RichPresence()

try {
    val pagedResults = richPresence.getDestinations(coroutineScope)

    if (pagedResults.hasNext()) {
        val destinations: List<Destination> = pagedResults.next()
        for (destination in destinations) {
            val apiName = destination.apiName          // API name for setting presence
            val displayName = destination.displayName  // Human-readable name
            val deeplink = destination.deeplinkMessage // Optional deeplink message
            val uri = destination.shareableUri         // Optional shareable URI
        }
    }
} catch (e: RichPresenceException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `coroutineScope: CoroutineScope` -- the coroutine scope for paginated fetching
**Return type**: `PagedResults<Destination>` -- a paginated result set of `Destination` objects

### Set Rich Presence

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.options.RichPresenceOptions

val richPresence = RichPresence()

val options = RichPresenceOptions.builder()
    .withApiName("my_destination")
    .withDeeplinkMessageOverride("Playing Level 5")
    .withIsJoinable(true)
    .build()

try {
    richPresence.set(options)
} catch (e: RichPresenceException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `richPresenceOptions: RichPresenceOptions` -- the options for the rich presence
**Return type**: `Unit` (no return value)

## Data Types

### `Destination` Model (returned by `getDestinations()`)

| Property | Type | Description |
|----------|------|-------------|
| `apiName` | `String` | API name used when setting presence to this destination |
| `deeplinkMessage` | `String?` | Deeplink message for this destination (may be `null`) |
| `displayName` | `String` | Human-readable display name of the destination |
| `shareableUri` | `String?` | URI for deeplinking directly to this destination (may be `null`) |

### `RichPresenceOptions` (input to `set()`)

Built using the builder pattern: `RichPresenceOptions.builder()...build()`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `apiName` | `String` | `""` | The API name of the destination |
| `deeplinkMessageOverride` | `String` | `""` | Override for the deeplink message |
| `isJoinable` | `Boolean` | `false` | Whether the presence is joinable by others |

**Builder methods:**
- `withApiName(apiName: String): Builder`
- `withDeeplinkMessageOverride(deeplinkMessageOverride: String): Builder`
- `withIsJoinable(isJoinable: Boolean): Builder`
- `build(): RichPresenceOptions`

### `RichPresenceExtraContext` Enum

Specifies extra context information to display about the user's presence.

| Value | Int | Description |
|-------|-----|-------------|
| `UNKNOWN` | 0 | The extra context is unknown |
| `NONE` | 1 | Display nothing |
| `CURRENT_CAPACITY` | 2 | Display current amount with the user over the max |
| `STARTED_AGO` | 3 | Display how long ago the match/game/race started |
| `ENDING_IN` | 4 | Display how soon the match/game/race will end |
| `LOOKING_FOR_A_MATCH` | 5 | Display that the user is looking for a match |

## Error Handling

All methods throw `RichPresenceException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

Rich Presence has no package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Important Notes

1. **Rich Presence is deprecated** -- this entire API has been deprecated in favor of [Group Presence](https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-group-presence). New integrations should use `GroupPresence` instead. Each Rich Presence method has a direct Group Presence equivalent: `clear()` -> `GroupPresence.clear()`, `getDestinations()` -> `GroupPresence.getDestinations()`, `set()` -> `GroupPresence.set()`.

2. **`getDestinations()` is not a `suspend` function** -- it takes a `CoroutineScope` parameter and returns `PagedResults<Destination>`. The returned object handles pagination internally using the provided scope. Call `hasNext()` and `next()` to iterate through pages.

3. **`RichPresenceOptions` uses the builder pattern** -- all fields have defaults (empty string for strings, `false` for booleans), so you only need to set the fields you want to change.

4. **Requires HzOS v85+** -- all Rich Presence methods require HzOS v85 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

5. **Migration path to Group Presence** -- when migrating, replace `RichPresence` with `GroupPresence`, `RichPresenceOptions` with `GroupPresenceOptions`, and `RichPresenceException` with `GroupPresenceException`. The destination model is shared between both APIs.
