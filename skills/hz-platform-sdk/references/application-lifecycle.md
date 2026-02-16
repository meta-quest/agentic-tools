# Application Lifecycle API

- **Kotlin Package**: `horizon.platform.applicationlifecycle`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-application-lifecycle
- **Minimum OS**: HzOS v78
- **Maven Artifact**: `horizon-platform-sdk-application-lifecycle-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Overview

The Application Lifecycle API is part of the Horizon Platform SDK. It provides three operations for Meta Quest Android applications:

1. **`launchIntentChanged()`** -- Subscribe to an event that fires when a launch intent is received (cold or warm start)
2. **`getLaunchDetails()`** -- Retrieve details about how the application was started, including launch type, deeplink message, destination, and session IDs
3. **`logDeeplinkResult(trackingId, result)`** -- Log whether a deeplink attempt was successful or failed, and the reason for failure

These methods enable developers to handle app-to-app travel, deeplinks from invites and rich presence, and track the effectiveness of deeplinking flows.

## API Usage

#### Retrieve Launch Details

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.models.LaunchDetails
import horizon.platform.applicationlifecycle.enums.LaunchType

val applicationLifecycle = ApplicationLifecycle()

try {
    val details: LaunchDetails = applicationLifecycle.getLaunchDetails()

    // Check how the app was launched
    val launchType = details.launchType           // LaunchType enum
    val deeplinkMsg = details.deeplinkMessage     // Opaque deeplink string, nullable
    val destination = details.destinationApiName  // Destination API name, nullable
    val launchSource = details.launchSource       // Where the deeplink came from, nullable
    val lobbyId = details.lobbySessionId          // Lobby session ID, nullable
    val matchId = details.matchSessionId          // Match session ID, nullable
    val trackingId = details.trackingId           // Deeplink tracking ID, nullable
    val users = details.users                     // List of users, nullable

} catch (e: ApplicationLifecycleException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `LaunchDetails` -- an immutable object containing information about how the application was launched.

#### Listen for Launch Intent Changes

Use this to detect when a new launch intent is received while the app is already running (warm start).

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException

val applicationLifecycle = ApplicationLifecycle()

applicationLifecycle.launchIntentChanged().collect { intentType: String ->
    // A new launch intent was received
    // Call getLaunchDetails() to get the full details
    try {
        val details = applicationLifecycle.getLaunchDetails()
        // Handle the new launch intent
    } catch (e: ApplicationLifecycleException) {
        // Handle error
    }
}
```

**Return type**: `Flow<String>` -- a Kotlin Flow that emits the type of launch intent received.

#### Log Deeplink Result

Use this to report whether a deeplink attempt succeeded or failed.

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.enums.LaunchResult

val applicationLifecycle = ApplicationLifecycle()

try {
    // Get the tracking ID from launch details
    val details = applicationLifecycle.getLaunchDetails()
    val trackingId = details.trackingId

    if (trackingId != null) {
        // Log that the deeplink was handled successfully
        applicationLifecycle.logDeeplinkResult(trackingId, LaunchResult.Success)
    }

} catch (e: ApplicationLifecycleException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `trackingId: String` -- The unique tracking ID from the deeplink attempt (from `LaunchDetails.trackingId`)
- `result: LaunchResult` -- An enum indicating whether the deeplink succeeded or failed, and the failure reason

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

All methods throw `ApplicationLifecycleException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch. The `launchIntentChanged()` Flow throws the exception from within the `collect` block.

### Status Codes (`ApplicationLifecycleStatusCode`)

This API uses only the common status codes. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Basic Launch Type Detection

Detect how the application was launched and take action accordingly.

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.enums.LaunchType

suspend fun handleAppLaunch() {
    val client = ApplicationLifecycle()
    try {
        val details = client.getLaunchDetails()
        when (details.launchType) {
            LaunchType.Normal -> {
                // Standard launch -- show home screen
            }
            LaunchType.Invite -> {
                // Launched from an invite -- join the session
                val lobbyId = details.lobbySessionId
                val matchId = details.matchSessionId
                if (lobbyId != null) {
                    joinLobby(lobbyId)
                } else if (matchId != null) {
                    joinMatch(matchId)
                }
            }
            LaunchType.Deeplink -> {
                // Launched from a deeplink -- navigate to content
                val message = details.deeplinkMessage
                if (message != null) {
                    navigateToContent(message)
                }
            }
            else -> {
                // Unknown or deprecated launch type
            }
        }
    } catch (e: ApplicationLifecycleException) {
        Log.e("App", "Failed to get launch details: ${e.message}")
    }
}
```

### Example 2: Full Deeplink Flow with Result Logging

Handle a deeplink and log whether it succeeded or failed.

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.enums.LaunchResult
import horizon.platform.applicationlifecycle.enums.LaunchType

suspend fun handleDeeplink() {
    val client = ApplicationLifecycle()
    try {
        val details = client.getLaunchDetails()

        if (details.launchType != LaunchType.Deeplink) return

        val trackingId = details.trackingId ?: return
        val deeplinkMessage = details.deeplinkMessage

        try {
            // Attempt to navigate to the deeplinked content
            val success = navigateToDeeplink(deeplinkMessage)
            if (success) {
                client.logDeeplinkResult(trackingId, LaunchResult.Success)
            } else {
                client.logDeeplinkResult(trackingId, LaunchResult.FailedOtherReason)
            }
        } catch (e: Exception) {
            client.logDeeplinkResult(trackingId, LaunchResult.FailedOtherReason)
        }
    } catch (e: ApplicationLifecycleException) {
        Log.e("App", "Deeplink handling error: ${e.message}")
    }
}
```

### Example 3: Listening for Warm Start Launch Intents

Subscribe to launch intent changes to handle app-to-app travel while the app is already running.

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.enums.LaunchType
import kotlinx.coroutines.flow.catch

fun observeLaunchIntents(scope: CoroutineScope) {
    val client = ApplicationLifecycle()

    scope.launch {
        client.launchIntentChanged()
            .catch { e ->
                Log.e("App", "Launch intent stream error: ${e.message}")
            }
            .collect { intentType ->
                // New intent received -- fetch full details
                try {
                    val details = client.getLaunchDetails()
                    when (details.launchType) {
                        LaunchType.Deeplink -> {
                            val message = details.deeplinkMessage
                            if (message != null) {
                                navigateToContent(message)
                            }
                        }
                        LaunchType.Invite -> {
                            val lobbyId = details.lobbySessionId
                            if (lobbyId != null) {
                                joinLobby(lobbyId)
                            }
                        }
                        else -> { /* handle other types */ }
                    }
                } catch (e: ApplicationLifecycleException) {
                    Log.e("App", "Failed to get launch details: ${e.message}")
                }
            }
    }
}
```

### Example 4: MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.enums.LaunchType
import horizon.platform.applicationlifecycle.models.LaunchDetails
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.catch
import kotlinx.coroutines.launch

data class LaunchUiState(
    val launchType: LaunchType = LaunchType.Unknown,
    val deeplinkMessage: String? = null,
    val destinationApiName: String? = null,
    val isLoading: Boolean = false,
    val error: String? = null,
)

class LaunchViewModel : ViewModel() {
    private val client = ApplicationLifecycle()
    private val _uiState = MutableStateFlow(LaunchUiState())
    val uiState: StateFlow<LaunchUiState> = _uiState

    fun fetchLaunchDetails() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val details = client.getLaunchDetails()
                _uiState.value = _uiState.value.copy(
                    launchType = details.launchType,
                    deeplinkMessage = details.deeplinkMessage,
                    destinationApiName = details.destinationApiName,
                    isLoading = false,
                )
            } catch (e: ApplicationLifecycleException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message,
                )
            }
        }
    }

    fun observeLaunchIntentChanges() {
        viewModelScope.launch {
            client.launchIntentChanged()
                .catch { e ->
                    _uiState.value = _uiState.value.copy(error = e.message)
                }
                .collect {
                    fetchLaunchDetails()
                }
        }
    }
}
```

### Example 5: Invite Handling with Session Joining

Handle invite-based launches with lobby and match session joining.

```kotlin
import horizon.platform.applicationlifecycle.ApplicationLifecycle
import horizon.platform.applicationlifecycle.ApplicationLifecycleException
import horizon.platform.applicationlifecycle.enums.LaunchResult
import horizon.platform.applicationlifecycle.enums.LaunchType

sealed class InviteResult {
    data class JoinedLobby(val lobbyId: String) : InviteResult()
    data class JoinedMatch(val matchId: String) : InviteResult()
    data class NavigatedToDestination(val destination: String) : InviteResult()
    data class Failed(val reason: String) : InviteResult()
    data object NotAnInvite : InviteResult()
}

suspend fun handleInviteLaunch(): InviteResult {
    val client = ApplicationLifecycle()
    return try {
        val details = client.getLaunchDetails()

        if (details.launchType != LaunchType.Invite) {
            return InviteResult.NotAnInvite
        }

        val trackingId = details.trackingId

        val result = when {
            details.lobbySessionId != null -> {
                joinLobby(details.lobbySessionId!!)
                InviteResult.JoinedLobby(details.lobbySessionId!!)
            }
            details.matchSessionId != null -> {
                joinMatch(details.matchSessionId!!)
                InviteResult.JoinedMatch(details.matchSessionId!!)
            }
            details.destinationApiName != null -> {
                navigateToDestination(details.destinationApiName!!)
                InviteResult.NavigatedToDestination(details.destinationApiName!!)
            }
            else -> {
                InviteResult.Failed("No session or destination in invite")
            }
        }

        // Log the deeplink result if we have a tracking ID
        if (trackingId != null) {
            val launchResult = if (result is InviteResult.Failed) {
                LaunchResult.FailedOtherReason
            } else {
                LaunchResult.Success
            }
            client.logDeeplinkResult(trackingId, launchResult)
        }

        result
    } catch (e: ApplicationLifecycleException) {
        InviteResult.Failed(e.message ?: "Unknown error")
    }
}
```

## Important Notes

1. **`launchIntentChanged()` returns a `Flow<String>`** -- it is not a suspend function. Collect the Flow within a coroutine scope. The Flow emits whenever a new launch intent is received, including during warm starts.

2. **Call `getLaunchDetails()` after receiving a `launchIntentChanged()` event** -- the event only provides the intent type as a string. To get the full launch details (deeplink message, destination, session IDs, users), call `getLaunchDetails()` in response to the event.

3. **Always log deeplink results** -- when your app is launched via a deeplink (LaunchType.Deeplink) or invite (LaunchType.Invite), use `logDeeplinkResult()` with the `trackingId` from `LaunchDetails` to report whether the deeplink was handled successfully. This helps Meta track deeplink effectiveness.

4. **`LaunchType.Coordinated` is deprecated** -- do not use this launch type. Handle it as a fallback case but do not design new flows around it.

5. **The `users` field requires the Users SDK** -- if you need to access `LaunchDetails.users`, add the `horizon-platform-sdk-users-kotlin` dependency. Each `User` object contains `id`, `displayName`, `imageUrl`, and `oculusId`.

6. **Requires HzOS v78+** -- `getLaunchDetails()` and `launchIntentChanged()` require HzOS v78 or later. `logDeeplinkResult()` requires HzOS v83 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

7. **No pagination or sessions** -- this is a simple request/response and event-based API. `getLaunchDetails()` and `logDeeplinkResult()` are one-shot calls. `launchIntentChanged()` is a long-lived event stream.
