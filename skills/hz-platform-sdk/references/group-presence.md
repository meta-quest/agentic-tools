# Group Presence API

- **Kotlin Package**: `horizon.platform.grouppresence`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-group-presence
- **Minimum OS**: HzOS v78
- **Maven Artifact**: `horizon-platform-sdk-group-presence-kotlin`

## Overview

The Group Presence API is part of the Horizon Platform SDK. It provides operations for Meta Quest Android applications to manage multiplayer presence, invitations, and social interactions:

1. **`set(options)`** -- Set all group presence parameters at once (recommended)
2. **`clear()`** -- Clear the current group presence
3. **`setDestination(apiName)`** -- Set the user's current destination
4. **`setIsJoinable(isJoinable)`** -- Set whether the user is joinable
5. **`setLobbySession(id)`** -- Set the user's lobby session ID
6. **`setMatchSession(id)`** -- Set the user's match session ID
7. **`setDeeplinkMessageOverride(deeplinkMessage)`** -- Override the deeplink message
8. **`getInvitableUsers(options)`** -- Get users who can be invited to the current lobby
9. **`getSentInvites()`** -- Get invites previously sent by the user
10. **`sendInvites(userIds)`** -- Send invites to specific users
11. **`launchInvitePanel(options)`** -- Launch the system invite dialog
12. **`launchRosterPanel(options)`** -- Launch the roster/party panel
13. **`launchRejoinDialog(lobbySessionId, matchSessionId, destinationApiName)`** -- Launch the rejoin dialog
14. **`launchMultiplayerErrorDialog(options)`** -- Launch a predefined error dialog

**Events (Flow-based):**
1. **`joinIntentReceived()`** -- Emitted when a user chooses to join a destination/lobby/match
2. **`invitationsSent()`** -- Emitted when the user finishes sending invitations from the invite panel

**Note:** These APIs are currently supported only for immersive mode. Non-immersive apps (regular Android panel apps or 2D experiences) are not yet supported.

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## API Usage

#### Set Group Presence (Recommended)

Use `set()` to configure all group presence parameters in a single call. This is the recommended approach to avoid inconsistent state.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.GroupPresenceOptions

val groupPresence = GroupPresence()

try {
    groupPresence.set(
        GroupPresenceOptions(
            destinationApiName = "my_battle_arena",
            lobbySessionId = "lobby-abc-123",
            matchSessionId = "match-xyz-789",
            isJoinable = true,
            deeplinkMessageOverride = "{\"level\":5,\"mode\":\"ranked\"}",
        )
    )
    // Group presence set successfully
} catch (e: GroupPresenceException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `GroupPresenceOptions` -- an options object with the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `destinationApiName` | `String` | `""` | Unique API name of the in-app destination |
| `lobbySessionId` | `String` | `""` | Session ID for the user's lobby/squad/party |
| `matchSessionId` | `String` | `""` | Session ID for the specific match/game instance |
| `isJoinable` | `Boolean` | `false` | Whether others can join this user |
| `deeplinkMessageOverride` | `String` | `""` | Custom deeplink data to override the destination default |

#### Clear Group Presence

```kotlin
try {
    groupPresence.clear()
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Set Individual Presence Parameters

While `set()` is recommended, you can update individual parameters:

```kotlin
// Set destination only
groupPresence.setDestination("my_battle_arena")

// Set joinability only
groupPresence.setIsJoinable(false)

// Set lobby session only
groupPresence.setLobbySession("lobby-abc-123")

// Set match session only
groupPresence.setMatchSession("match-xyz-789")

// Set deeplink message override only
groupPresence.setDeeplinkMessageOverride("{\"level\":5}")
```

#### Get Invitable Users

Retrieve users who can be invited to the current lobby, drawn from bidirectional followers and recently met users.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.InviteOptions
import horizon.platform.users.models.User

val groupPresence = GroupPresence()

try {
    val options = InviteOptions(
        suggestedUsers = listOf("user-id-1", "user-id-2"),
    )
    val invitableUsers: List<User> = groupPresence.getInvitableUsers(options)

    for (user in invitableUsers) {
        val userId = user.id
        val displayName = user.displayName       // Nullable
        val imageUrl = user.imageUrl             // Nullable
        val presenceStatus = user.presenceStatus // Nullable
    }
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Send Invites

Send invites programmatically to a list of user IDs.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.models.SendInvitesResult

val groupPresence = GroupPresence()

try {
    val result: SendInvitesResult = groupPresence.sendInvites(
        listOf("user-id-1", "user-id-2", "user-id-3")
    )

    // Access the list of successfully sent invites
    for (invite in result.invites) {
        val inviteId = invite.id
        val recipient = invite.recipient  // Nullable User
    }
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Launch Invite Panel

Launch the system invite dialog for the user to select who to invite.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.InviteOptions
import horizon.platform.grouppresence.models.InvitePanelResultInfo

val groupPresence = GroupPresence()

try {
    val options = InviteOptions(
        suggestedUsers = listOf("user-id-1"),
    )
    val result: InvitePanelResultInfo = groupPresence.launchInvitePanel(options)

    if (result.invitesSent) {
        // Invitations were successfully sent
    }
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Get Sent Invites

Retrieve invites that the current user has already sent.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.models.ApplicationInvite

val groupPresence = GroupPresence()

try {
    val sentInvites: List<ApplicationInvite> = groupPresence.getSentInvites()

    for (invite in sentInvites) {
        val inviteId = invite.id
        val destination = invite.destination     // Nullable Destination
        val recipient = invite.recipient         // Nullable User
        val isActive = invite.isActive           // Nullable Boolean
        val lobbySessionId = invite.lobbySessionId   // Nullable
        val matchSessionId = invite.matchSessionId   // Nullable
    }
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Launch Rejoin Dialog

Launch a dialog asking the user if they want to rejoin a previous lobby or match.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.models.RejoinDialogResult

val groupPresence = GroupPresence()

try {
    val result: RejoinDialogResult = groupPresence.launchRejoinDialog(
        lobbySessionId = "lobby-abc-123",
        matchSessionId = "match-xyz-789",
        destinationApiName = "my_battle_arena",
    )

    if (result.rejoinSelected) {
        // User chose to rejoin -- navigate them to the session
    } else {
        // User declined to rejoin
    }
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Launch Multiplayer Error Dialog

Display a system error dialog with a predefined multiplayer error message.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.MultiplayerErrorOptions
import horizon.platform.grouppresence.enums.MultiplayerErrorErrorKey

val groupPresence = GroupPresence()

try {
    groupPresence.launchMultiplayerErrorDialog(
        MultiplayerErrorOptions(
            errorKey = MultiplayerErrorErrorKey.GROUP_FULL,
        )
    )
} catch (e: GroupPresenceException) {
    // Handle error
}
```

#### Launch Roster Panel

Display the panel showing current users in the roster/party.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.RosterOptions

val groupPresence = GroupPresence()

try {
    groupPresence.launchRosterPanel(
        RosterOptions(
            suggestedUsers = listOf("user-id-1", "user-id-2"),
        )
    )
} catch (e: GroupPresenceException) {
    // Handle error
}
```

**Note:** The roster panel is not recommended for most use cases because the list of current users is already surfaced in the Destination UI when the Meta Quest button is pressed.

#### Listen for Join Intent Events

Listen for events when a user accepts an invitation to join a destination/lobby/match.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.models.GroupPresenceJoinIntent

val groupPresence = GroupPresence()

groupPresence.joinIntentReceived().collect { intent: GroupPresenceJoinIntent ->
    val destination = intent.destinationApiName   // Nullable
    val lobbySession = intent.lobbySessionId      // Nullable
    val matchSession = intent.matchSessionId      // Nullable
    val deeplink = intent.deeplinkMessage          // Nullable

    // Navigate the user to the requested destination/session
}
```

#### Listen for Invitations Sent Events

Listen for events when the user finishes sending invitations from the invite panel.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.models.LaunchInvitePanelFlowResult

val groupPresence = GroupPresence()

groupPresence.invitationsSent().collect { result: LaunchInvitePanelFlowResult ->
    val invitedUsers = result.invitedUsers  // List<User>

    for (user in invitedUsers) {
        // Process each invited user
        val userId = user.id
        val displayName = user.displayName
    }
}
```

## Data Types

### `GroupPresenceOptions` (passed to `set()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `destinationApiName` | `String` | `""` | Unique API name of the in-app destination |
| `lobbySessionId` | `String` | `""` | Session ID for the user's lobby/squad/party |
| `matchSessionId` | `String` | `""` | Session ID for the specific match/game instance |
| `isJoinable` | `Boolean` | `false` | Whether other users can join this user |
| `deeplinkMessageOverride` | `String` | `""` | Custom deeplink data to override destination default |

### `InviteOptions` (passed to `getInvitableUsers()` and `launchInvitePanel()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `suggestedUsers` | `List<String>` | `[]` | User IDs to add to the suggested invitable users list |

### `RosterOptions` (passed to `launchRosterPanel()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `suggestedUsers` | `List<String>` | `[]` | User IDs to add to the suggested invitable users list |

### `MultiplayerErrorOptions` (passed to `launchMultiplayerErrorDialog()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `errorKey` | `MultiplayerErrorErrorKey` | `UNKNOWN` | Predefined error key for the error message to display |

### `MultiplayerErrorErrorKey` Enum

| Value | Integer | Description |
|-------|---------|-------------|
| `UNKNOWN` | 0 | Unknown error |
| `DESTINATION_UNAVAILABLE` | 1 | Travel destination is no longer available |
| `DLC_REQUIRED` | 2 | Downloadable content is required |
| `GENERAL` | 3 | General error not covered by other keys |
| `GROUP_FULL` | 4 | The group/session is full |
| `INVITER_NOT_JOINABLE` | 5 | The inviter's presence is not set to joinable |
| `LEVEL_NOT_HIGH_ENOUGH` | 6 | User's level is not high enough |
| `LEVEL_NOT_UNLOCKED` | 7 | Required level has not been unlocked |
| `NETWORK_TIMEOUT` | 8 | Network timeout occurred |
| `NO_LONGER_AVAILABLE` | 9 | Content is no longer available |
| `UPDATE_REQUIRED` | 10 | An update is required |
| `TUTORIAL_REQUIRED` | 11 | A tutorial must be completed first |

### `ApplicationInvite` Model (returned by `getSentInvites()`, nested in `SendInvitesResult`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `id` | `String` | `""` | Unique identifier for the invite |
| `destination` | `Destination?` | `null` | The destination the recipient is invited to |
| `recipient` | `User?` | `null` | The recipient's user information |
| `isActive` | `Boolean?` | `null` | Whether the invite is still active |
| `lobbySessionId` | `String?` | `null` | The lobby session the recipient is invited to |
| `matchSessionId` | `String?` | `null` | The match session the recipient is invited to |

### `GroupPresenceJoinIntent` Model (emitted by `joinIntentReceived()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `destinationApiName` | `String?` | `null` | The destination the user wants to join |
| `lobbySessionId` | `String?` | `null` | The lobby session the user wants to join |
| `matchSessionId` | `String?` | `null` | The match session the user wants to join |
| `deeplinkMessage` | `String?` | `null` | Opaque deeplink data to help navigate the user |

### `InvitePanelResultInfo` Model (returned by `launchInvitePanel()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `invitesSent` | `Boolean` | `false` | Whether any invitations were successfully sent |

### `LaunchInvitePanelFlowResult` Model (emitted by `invitationsSent()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `invitedUsers` | `List<User>` | `[]` | List of users who were sent an invitation |

### `RejoinDialogResult` Model (returned by `launchRejoinDialog()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `rejoinSelected` | `Boolean` | `false` | Whether the user chose to rejoin |

### `SendInvitesResult` Model (returned by `sendInvites()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `invites` | `List<ApplicationInvite>` | `[]` | List of invites that were successfully sent |

### `User` Model (from Users package, returned by invite APIs)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `id` | `String` | `""` | Unique user identifier |
| `displayName` | `String?` | `null` | User's display name |
| `imageUrl` | `String?` | `null` | URL of the user's profile picture |
| `oculusId` | `String?` | `null` | User's Oculus ID |
| `presence` | `String?` | `null` | Human-readable presence string |
| `presenceDeeplinkMessage` | `String?` | `null` | Deeplink message for the user's presence |
| `presenceDestinationApiName` | `String?` | `null` | API name of the user's current destination |
| `presenceLobbySessionId` | `String?` | `null` | User's current lobby session ID |
| `presenceMatchSessionId` | `String?` | `null` | User's current match session ID |
| `presenceStatus` | `UserPresenceStatus?` | `null` | User's online/offline status |
| `smallImageUrl` | `String?` | `null` | URL of the user's smaller profile picture |

## Error Handling

All suspend methods throw `GroupPresenceException` (extends `HzPlatformSdkException`) on failure. Event methods (`joinIntentReceived()`, `invitationsSent()`) return `Flow` and do not throw -- errors are delivered through the Flow's error channel. Always wrap suspend calls in try/catch.

### Group Presence-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `LoggedInUserManagerBuildPresenceError` | 2001 | Failed to build presence data for the logged-in user | Verify presence parameters are valid |
| `OvrServiceError` | 2002 | General error communicating with the OVR service | Retry the operation |
| `OvrServiceUnknownError` | 2003 | Unknown error communicating with the OVR service | Retry or contact support |
| `ClientUsageError` | 2004 | Client usage error (e.g., app not registered, not foreground app, invalid parameters) | Check error message for details; verify app registration and foreground state |
| `SetUnknownError` | 2010 | Unknown error during a set operation | Retry the operation |
| `PresenceApiUnavailable` | 2011 | Presence API is not available | Check OS version and service status |
| `SetDeeplinkMessageOverrideUnknownError` | 2020 | Unknown error setting deeplink message override | Ensure destination is set first, then retry |
| `SetDestinationUnknownError` | 2030 | Unknown error setting destination | Retry the operation |
| `SetIsJoinableUnknownError` | 2040 | Unknown error setting joinable state | Retry the operation |
| `SetLobbySessionUnknownError` | 2050 | Unknown error setting lobby session | Retry the operation |
| `SetMatchSessionUnknownError` | 2060 | Unknown error setting match session | Retry the operation |
| `LaunchRosterPanelUnknownError` | 2070 | Unknown error launching the roster panel | Retry the operation |
| `LaunchRosterPanelPayloadError` | 2071 | Payload error launching the roster panel | Verify roster options |
| `LaunchDeeplinkIntentError` | 2072 | Error launching the deeplink intent | Verify deeplink configuration |
| `LaunchRosterPanelRetrievePresenceError` | 2073 | Error retrieving presence data for the roster panel | Retry the operation |
| `LaunchRosterPanelClientUsageError` | 2074 | Client usage error launching the roster panel | Verify app is in foreground |
| `LaunchMultiplayerErrorDialogUnknownError` | 2080 | Unknown error launching the multiplayer error dialog | Retry the operation |
| `LaunchInvitePanelUnknownError` | 2090 | Unknown error launching the invite panel | Retry the operation |
| `LaunchInvitePanelPayloadError` | 2091 | Payload error launching the invite panel | Verify invite options |
| `LaunchInvitePanelRetrievePresenceError` | 2093 | Error retrieving presence data for the invite panel | Retry the operation |
| `LaunchInvitePanelClientUsageError` | 2094 | Client usage error launching the invite panel | Verify app is in foreground |
| `LaunchInvitePanelTravelInviteActivityNotFoundError` | 2095 | Travel invite activity not found | Check HzPlatformService installation |
| `LaunchInvitePanelTravelInviteActivityLaunchError` | 2096 | Error launching travel invite activity | Retry the operation |
| `LaunchRejoinDialogUnknownError` | 2100 | Unknown error launching the rejoin dialog | Retry the operation |
| `LaunchRejoinDialogClientUsageError` | 2101 | Client usage error launching the rejoin dialog | Verify lobby/match session IDs are valid |
| `SetRichPresenceUnknownError` | 2110 | Unknown error setting rich presence | Retry the operation |
| `SetRichPresenceNotConnected` | 2111 | Service not connected when setting rich presence | Ensure HorizonServiceConnection is connected |
| `SetRichPresenceInvalidInput` | 2112 | Invalid input for rich presence | Check presence parameters |
| `SetRichPresenceSerializationError` | 2113 | Serialization error setting rich presence | Check data format |
| `SetRichPresenceServerError` | 2114 | Server error setting rich presence | Retry later |
| `SetRichPresenceTimeout` | 2115 | Timeout setting rich presence | Check connectivity and retry |
| `SetRichPresenceCancelled` | 2116 | Rich presence operation cancelled | Retry if needed |
| `SetRichPresenceGraphqlTimeout` | 2200 | GraphQL timeout setting rich presence | Check connectivity and retry later |
| `SetRichPresenceGraphqlExecutionException` | 2201 | GraphQL execution exception | Server-side issue; retry later |
| `SetRichPresenceInterruptedException` | 2202 | Interrupted exception setting rich presence | Retry the operation |
| `SetRichPresencePreferencesManagerNull` | 2203 | Preferences manager is null | Check service initialization |
| `SetRichPresencePushTokenNull` | 2204 | Push token is null | Check push notification setup |
| `SetRichPresenceHeartbeatDisabled` | 2205 | Heartbeat is disabled | Check service configuration |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example 1: Setting Up Group Presence When Entering a Multiplayer Session

Set the user's full group presence when they join a multiplayer session.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.GroupPresenceOptions

suspend fun enterMultiplayerSession(
    destinationApiName: String,
    lobbySessionId: String,
    matchSessionId: String,
) {
    val client = GroupPresence()
    try {
        client.set(
            GroupPresenceOptions(
                destinationApiName = destinationApiName,
                lobbySessionId = lobbySessionId,
                matchSessionId = matchSessionId,
                isJoinable = true,
            )
        )
    } catch (e: GroupPresenceException) {
        // Log and handle error
    }
}
```

### Example 2: Handling Join Intents

Listen for join intents and navigate the user accordingly.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.models.GroupPresenceJoinIntent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.launch

class MultiplayerSessionManager(private val scope: CoroutineScope) {
    private val groupPresence = GroupPresence()

    fun startListening() {
        scope.launch {
            groupPresence.joinIntentReceived().collect { intent ->
                handleJoinIntent(intent)
            }
        }
    }

    private suspend fun handleJoinIntent(intent: GroupPresenceJoinIntent) {
        val destination = intent.destinationApiName ?: return
        val lobbyId = intent.lobbySessionId
        val matchId = intent.matchSessionId

        // Navigate the user to the requested destination
        // Then update their group presence
        try {
            groupPresence.set(
                horizon.platform.grouppresence.options.GroupPresenceOptions(
                    destinationApiName = destination,
                    lobbySessionId = lobbyId ?: "",
                    matchSessionId = matchId ?: "",
                    isJoinable = true,
                )
            )
        } catch (e: Exception) {
            // Handle error
        }
    }
}
```

### Example 3: Invite Flow with Fallback

Attempt to launch the invite panel; fall back to programmatic invites if the panel fails.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.InviteOptions
import horizon.platform.grouppresence.models.InvitePanelResultInfo

sealed class InviteResult {
    data class PanelSuccess(val invitesSent: Boolean) : InviteResult()
    data class DirectSuccess(val inviteCount: Int) : InviteResult()
    data class Error(val message: String) : InviteResult()
}

suspend fun inviteUsers(suggestedUserIds: List<String>): InviteResult {
    val client = GroupPresence()
    val options = InviteOptions(suggestedUsers = suggestedUserIds)

    // Try the system invite panel first (recommended)
    return try {
        val result: InvitePanelResultInfo = client.launchInvitePanel(options)
        InviteResult.PanelSuccess(result.invitesSent)
    } catch (e: GroupPresenceException) {
        // Fall back to programmatic invites
        try {
            val invitableUsers = client.getInvitableUsers(options)
            val userIds = invitableUsers.map { it.id }
            if (userIds.isNotEmpty()) {
                val sendResult = client.sendInvites(userIds)
                InviteResult.DirectSuccess(sendResult.invites.size)
            } else {
                InviteResult.Error("No invitable users found")
            }
        } catch (fallbackError: GroupPresenceException) {
            InviteResult.Error(fallbackError.message ?: "Failed to send invites")
        }
    }
}
```

### Example 4: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.GroupPresenceOptions
import horizon.platform.grouppresence.models.GroupPresenceJoinIntent
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class GroupPresenceUiState(
    val isPresenceSet: Boolean = false,
    val currentDestination: String = "",
    val isJoinable: Boolean = false,
    val lobbySessionId: String = "",
    val matchSessionId: String = "",
    val pendingJoinIntent: GroupPresenceJoinIntent? = null,
    val isLoading: Boolean = false,
    val error: String? = null,
)

class GroupPresenceViewModel : ViewModel() {
    private val groupPresence = GroupPresence()
    private val _uiState = MutableStateFlow(GroupPresenceUiState())
    val uiState: StateFlow<GroupPresenceUiState> = _uiState

    init {
        listenForJoinIntents()
    }

    fun setPresence(
        destinationApiName: String,
        lobbySessionId: String,
        matchSessionId: String,
        isJoinable: Boolean,
    ) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                groupPresence.set(
                    GroupPresenceOptions(
                        destinationApiName = destinationApiName,
                        lobbySessionId = lobbySessionId,
                        matchSessionId = matchSessionId,
                        isJoinable = isJoinable,
                    )
                )
                _uiState.value = _uiState.value.copy(
                    isPresenceSet = true,
                    currentDestination = destinationApiName,
                    isJoinable = isJoinable,
                    lobbySessionId = lobbySessionId,
                    matchSessionId = matchSessionId,
                    isLoading = false,
                )
            } catch (e: GroupPresenceException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to set presence",
                )
            }
        }
    }

    fun clearPresence() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                groupPresence.clear()
                _uiState.value = GroupPresenceUiState()
            } catch (e: GroupPresenceException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to clear presence",
                )
            }
        }
    }

    private fun listenForJoinIntents() {
        viewModelScope.launch {
            groupPresence.joinIntentReceived().collect { intent ->
                _uiState.value = _uiState.value.copy(
                    pendingJoinIntent = intent,
                )
            }
        }
    }

    fun dismissJoinIntent() {
        _uiState.value = _uiState.value.copy(pendingJoinIntent = null)
    }
}
```

### Example 5: Multiplayer Error Dialog with Contextual Error Keys

Show the appropriate system error dialog based on the type of multiplayer failure.

```kotlin
import horizon.platform.grouppresence.GroupPresence
import horizon.platform.grouppresence.GroupPresenceException
import horizon.platform.grouppresence.options.MultiplayerErrorOptions
import horizon.platform.grouppresence.enums.MultiplayerErrorErrorKey

suspend fun showMultiplayerError(errorType: String) {
    val client = GroupPresence()

    val errorKey = when (errorType) {
        "full" -> MultiplayerErrorErrorKey.GROUP_FULL
        "unavailable" -> MultiplayerErrorErrorKey.DESTINATION_UNAVAILABLE
        "dlc" -> MultiplayerErrorErrorKey.DLC_REQUIRED
        "timeout" -> MultiplayerErrorErrorKey.NETWORK_TIMEOUT
        "update" -> MultiplayerErrorErrorKey.UPDATE_REQUIRED
        "not_joinable" -> MultiplayerErrorErrorKey.INVITER_NOT_JOINABLE
        else -> MultiplayerErrorErrorKey.GENERAL
    }

    try {
        client.launchMultiplayerErrorDialog(
            MultiplayerErrorOptions(errorKey = errorKey)
        )
    } catch (e: GroupPresenceException) {
        // Fall back to custom in-app error UI
    }
}
```

## Important Notes

1. **Use `set()` instead of individual setters** -- the `set()` method updates all group presence parameters atomically. Using individual setters like `setDestination()`, `setIsJoinable()`, etc. can lead to inconsistent presence state between calls. Only use individual setters when you specifically need to update a single parameter.

2. **Event methods return `Flow`** -- `joinIntentReceived()` and `invitationsSent()` return `Flow` objects. Collect them in a coroutine scope to receive events. These do not throw exceptions directly.

3. **Respond to join intents immediately** -- when a `joinIntentReceived()` event fires, navigate the user to the requested destination as quickly as possible. Users expect immediate feedback when accepting an invitation.

4. **Clear presence when leaving** -- always call `clear()` when the user exits a multiplayer session or your app. Stale presence data causes confusion for other users.

5. **`launchInvitePanel()` is preferred over `sendInvites()`** -- the system invite panel provides a better user experience with a visual roster. Use `sendInvites()` only when you need programmatic control over the invite flow.

6. **`lobbySessionId` is required for invites** -- a user must have `lobbySessionId` set and `isJoinable` set to `true` in their group presence for the invite system to work.

7. **`matchSessionId` vs `lobbySessionId`** -- lobby session IDs represent close groups (squad/party) where users can see/hear each other. Match session IDs represent broader game instances (map, round). Users with the same lobby session ID appear in the roster; users with only the same match session ID appear in "Recently Played With."

8. **Immersive apps only** -- the Group Presence API is currently supported only for immersive VR apps. Regular Android panel apps and 2D experiences are not yet supported.

9. **Requires HzOS v78+** -- the core set/clear operations require HzOS v78 or later. Invite panel, roster panel, rejoin dialog, and error dialog operations require HzOS v83+. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`).

10. **`setDeeplinkMessageOverride()` requires a destination** -- the deeplink message override can only be set if the user's destination is already set. If no destination has been set, use `set()` to configure the destination and deeplink message together.

11. **Cross-package dependency for User model** -- the `User` model returned by `getInvitableUsers()` and event flows comes from the Users SDK package (`horizon-platform-sdk-users-kotlin`). Add this dependency if you need to access User properties.
