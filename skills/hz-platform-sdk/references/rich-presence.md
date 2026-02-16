# Rich Presence API

> **Deprecated**: Rich Presence has been deprecated in favor of [Group Presence](https://developers.meta.com/horizon/documentation/android-apps/ps-group-presence-overview). New integrations should use the Group Presence API instead. See the migration notes at the bottom of this file.

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.richpresence` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-group-presence-overview |
| **Minimum OS** | HzOS v85 |
| **Maven Artifact** | `horizon-platform-sdk-rich-presence-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Overview

The Rich Presence API allows Meta Quest Android applications to manage the user's presence information, including what they are doing and where they are in the app. The API provides the following public operations:

1. **`clear()`** -- Clear the current rich presence
2. **`getDestinations(coroutineScope)`** -- Retrieve all available destinations that the presence can be set to (paginated)
3. **`set(richPresenceOptions)`** -- Set the rich presence with a combination of options (destination, deeplink message, joinability)

## API Usage

### Clear Rich Presence

Remove the current user's rich presence.

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

Retrieve all destinations that the presence can be set to. This method returns paginated results.

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.models.Destination
import kotlinx.coroutines.CoroutineScope

val richPresence = RichPresence()

try {
    val pagedResults = richPresence.getDestinations(coroutineScope)

    // Check if there are results
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

Set the user's rich presence with a combination of options.

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

## Examples

### Example 1: Set Rich Presence with a Destination

Set the user's rich presence to a specific destination with a custom deeplink message.

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.options.RichPresenceOptions

suspend fun setPresenceToDestination(
    destinationApiName: String,
    deeplinkMessage: String,
    isJoinable: Boolean
) {
    val client = RichPresence()
    val options = RichPresenceOptions.builder()
        .withApiName(destinationApiName)
        .withDeeplinkMessageOverride(deeplinkMessage)
        .withIsJoinable(isJoinable)
        .build()

    try {
        client.set(options)
    } catch (e: RichPresenceException) {
        // Handle error
        Log.e("RichPresence", "Failed to set presence: ${e.message}")
    }
}
```

### Example 2: List All Available Destinations

Retrieve and display all available destinations using pagination.

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.models.Destination
import kotlinx.coroutines.CoroutineScope

suspend fun getAllDestinations(coroutineScope: CoroutineScope): List<Destination> {
    val client = RichPresence()
    val allDestinations = mutableListOf<Destination>()

    try {
        val pagedResults = client.getDestinations(coroutineScope)
        while (pagedResults.hasNext()) {
            val page = pagedResults.next()
            allDestinations.addAll(page)
        }
    } catch (e: RichPresenceException) {
        Log.e("RichPresence", "Failed to get destinations: ${e.message}")
    }

    return allDestinations
}
```

### Example 3: Clear and Reset Rich Presence

Clear the current rich presence and handle the case where the user leaves a session.

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException

sealed class ClearPresenceResult {
    object Success : ClearPresenceResult()
    data class Error(val message: String) : ClearPresenceResult()
}

suspend fun clearPresence(): ClearPresenceResult {
    val client = RichPresence()
    return try {
        client.clear()
        ClearPresenceResult.Success
    } catch (e: RichPresenceException) {
        ClearPresenceResult.Error(e.message ?: "Unknown error")
    }
}
```

### Example 4: Full MVVM Integration with Destination Selection

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.models.Destination
import horizon.platform.richpresence.options.RichPresenceOptions
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class RichPresenceUiState(
    val destinations: List<Destination> = emptyList(),
    val isLoading: Boolean = false,
    val isPresenceSet: Boolean = false,
    val error: String? = null,
)

class RichPresenceViewModel : ViewModel() {
    private val client = RichPresence()
    private val _uiState = MutableStateFlow(RichPresenceUiState())
    val uiState: StateFlow<RichPresenceUiState> = _uiState

    fun loadDestinations() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val pagedResults = client.getDestinations(viewModelScope)
                val destinations = mutableListOf<Destination>()
                while (pagedResults.hasNext()) {
                    destinations.addAll(pagedResults.next())
                }
                _uiState.value = _uiState.value.copy(
                    destinations = destinations,
                    isLoading = false,
                )
            } catch (e: RichPresenceException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message,
                )
            }
        }
    }

    fun selectDestination(destination: Destination) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val options = RichPresenceOptions.builder()
                    .withApiName(destination.apiName)
                    .withIsJoinable(true)
                    .build()
                client.set(options)
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    isPresenceSet = true,
                )
            } catch (e: RichPresenceException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message,
                )
            }
        }
    }

    fun clearPresence() {
        viewModelScope.launch {
            try {
                client.clear()
                _uiState.value = _uiState.value.copy(isPresenceSet = false)
            } catch (e: RichPresenceException) {
                _uiState.value = _uiState.value.copy(error = e.message)
            }
        }
    }
}
```

### Example 5: Repository Pattern with Error Mapping

```kotlin
import horizon.platform.richpresence.RichPresence
import horizon.platform.richpresence.RichPresenceException
import horizon.platform.richpresence.models.Destination
import horizon.platform.richpresence.options.RichPresenceOptions
import kotlinx.coroutines.CoroutineScope

sealed class RichPresenceResult<out T> {
    data class Success<T>(val data: T) : RichPresenceResult<T>()
    data class Error(val message: String, val isRetryable: Boolean) : RichPresenceResult<Nothing>()
}

class RichPresenceRepository {
    private val client = RichPresence()

    suspend fun setPresence(
        apiName: String,
        deeplinkMessage: String = "",
        isJoinable: Boolean = false
    ): RichPresenceResult<Unit> {
        return try {
            val options = RichPresenceOptions.builder()
                .withApiName(apiName)
                .withDeeplinkMessageOverride(deeplinkMessage)
                .withIsJoinable(isJoinable)
                .build()
            client.set(options)
            RichPresenceResult.Success(Unit)
        } catch (e: RichPresenceException) {
            val isRetryable = e.message?.let { msg ->
                msg.contains("1") || msg.contains("4") || msg.contains("6")
            } ?: false
            RichPresenceResult.Error(
                e.message ?: "Failed to set presence",
                isRetryable,
            )
        }
    }

    suspend fun clearPresence(): RichPresenceResult<Unit> {
        return try {
            client.clear()
            RichPresenceResult.Success(Unit)
        } catch (e: RichPresenceException) {
            RichPresenceResult.Error(
                e.message ?: "Failed to clear presence",
                isRetryable = true,
            )
        }
    }

    fun getDestinations(
        coroutineScope: CoroutineScope
    ): RichPresenceResult<List<Destination>> {
        return try {
            val pagedResults = client.getDestinations(coroutineScope)
            val allDestinations = mutableListOf<Destination>()
            // Note: Caller must iterate pages in a coroutine
            RichPresenceResult.Success(allDestinations)
        } catch (e: RichPresenceException) {
            RichPresenceResult.Error(
                e.message ?: "Failed to get destinations",
                isRetryable = true,
            )
        }
    }
}
```

## Important Notes

1. **Rich Presence is deprecated** -- this entire API has been deprecated in favor of [Group Presence](https://developers.meta.com/horizon/documentation/android-apps/ps-group-presence-overview). New integrations should use `GroupPresence` instead. Each Rich Presence method has a direct Group Presence equivalent: `clear()` -> `GroupPresence.clear()`, `getDestinations()` -> `GroupPresence.getDestinations()`, `set()` -> `GroupPresence.set()`.

2. **`getDestinations()` is not a `suspend` function** -- it takes a `CoroutineScope` parameter and returns `PagedResults<Destination>`. The returned `PagedResults` object handles pagination internally using the provided scope. Call `hasNext()` and `next()` to iterate through pages.

3. **`RichPresenceOptions` uses the builder pattern** -- create options via `RichPresenceOptions.builder().withApiName(...).withDeeplinkMessageOverride(...).withIsJoinable(...).build()`. All fields have defaults (empty string for strings, `false` for booleans), so you only need to set the fields you want to change.

4. **Destinations are paginated** -- `getDestinations()` returns `PagedResults<Destination>`, which provides page-by-page access. Use `hasNext()` to check for more pages and `next()` to fetch the next page of results.

5. **Requires HzOS v85+** -- all Rich Presence methods require HzOS v85 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

6. **Migration path to Group Presence** -- when migrating, replace `RichPresence` with `GroupPresence`, `RichPresenceOptions` with `GroupPresenceOptions`, and `RichPresenceException` with `GroupPresenceException`. The destination model is shared between both APIs.
