# Group Presence API -- Invites, Panels & Events

- **Kotlin Package**: `horizon.platform.grouppresence`
- Companion to [group-presence.md](group-presence.md) -- invite/roster/rejoin/error-dialog APIs + 2 Flow events. Status codes: [group-presence-errors.md](group-presence-errors.md).

## Contents
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Important Notes](#important-notes)

## API Usage

All suspend methods throw `GroupPresenceException`; wrap in try/catch.

#### Get Invitable Users

Returns users drawn from bidirectional followers and recently met users.

```kotlin
import horizon.platform.grouppresence.options.InviteOptions
import horizon.platform.users.models.User

val options = InviteOptions(suggestedUsers = listOf("user-id-1", "user-id-2"))
val invitableUsers: List<User> = groupPresence.getInvitableUsers(options)
for (user in invitableUsers) {
    val userId = user.id
    val displayName = user.displayName
    val presenceStatus = user.presenceStatus
}
```

#### Send Invites

```kotlin
import horizon.platform.grouppresence.models.SendInvitesResult

val result: SendInvitesResult = groupPresence.sendInvites(
    listOf("user-id-1", "user-id-2", "user-id-3")
)
for (invite in result.invites) {
    val inviteId = invite.id
    val recipient = invite.recipient
}
```

#### Launch Invite Panel

```kotlin
import horizon.platform.grouppresence.options.InviteOptions
import horizon.platform.grouppresence.models.InvitePanelResultInfo

val result: InvitePanelResultInfo =
    groupPresence.launchInvitePanel(InviteOptions(suggestedUsers = listOf("user-id-1")))
if (result.invitesSent) { /* sent */ }
```

#### Get Sent Invites

```kotlin
import horizon.platform.grouppresence.models.ApplicationInvite

val sentInvites: List<ApplicationInvite> = groupPresence.getSentInvites()
for (invite in sentInvites) {
    val inviteId = invite.id
    val destination = invite.destination
    val recipient = invite.recipient
    val isActive = invite.isActive
    val lobbySessionId = invite.lobbySessionId
    val matchSessionId = invite.matchSessionId
}
```

#### Launch Rejoin Dialog

```kotlin
import horizon.platform.grouppresence.models.RejoinDialogResult

val result: RejoinDialogResult = groupPresence.launchRejoinDialog(
    lobbySessionId = "lobby-abc-123",
    matchSessionId = "match-xyz-789",
    destinationApiName = "my_battle_arena",
)
if (result.rejoinSelected) { /* navigate to session */ }
```

#### Launch Multiplayer Error Dialog

```kotlin
import horizon.platform.grouppresence.options.MultiplayerErrorOptions
import horizon.platform.grouppresence.enums.MultiplayerErrorErrorKey

groupPresence.launchMultiplayerErrorDialog(
    MultiplayerErrorOptions(errorKey = MultiplayerErrorErrorKey.GROUP_FULL)
)
```

#### Launch Roster Panel

Not recommended for most use cases (current users already appear in the Destination UI on the Meta Quest button press).

```kotlin
import horizon.platform.grouppresence.options.RosterOptions

groupPresence.launchRosterPanel(
    RosterOptions(suggestedUsers = listOf("user-id-1", "user-id-2"))
)
```

#### Listen for Join Intent Events

```kotlin
import horizon.platform.grouppresence.models.GroupPresenceJoinIntent

groupPresence.joinIntentReceived().collect { intent: GroupPresenceJoinIntent ->
    val destination = intent.destinationApiName
    val lobbySession = intent.lobbySessionId
    val matchSession = intent.matchSessionId
    val deeplink = intent.deeplinkMessage
    // Navigate user to the requested destination/session
}
```

#### Listen for Invitations Sent Events

```kotlin
import horizon.platform.grouppresence.models.LaunchInvitePanelFlowResult

groupPresence.invitationsSent().collect { result: LaunchInvitePanelFlowResult ->
    val invitedUsers: List<User> = result.invitedUsers
    for (user in invitedUsers) { val userId = user.id }
}
```

## Data Types

### `InviteOptions` (for `getInvitableUsers()`, `launchInvitePanel()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `suggestedUsers` | `List<String>` | `[]` | Suggested invitable user IDs |

### `RosterOptions` (for `launchRosterPanel()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `suggestedUsers` | `List<String>` | `[]` | Suggested invitable user IDs |

### `MultiplayerErrorOptions` (for `launchMultiplayerErrorDialog()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `errorKey` | `MultiplayerErrorErrorKey` | `UNKNOWN` | Error message to display |

### `MultiplayerErrorErrorKey` Enum

| Value | Integer | Description |
|---|---|---|
| `UNKNOWN` | 0 | Unknown error |
| `DESTINATION_UNAVAILABLE` | 1 | Destination unavailable |
| `DLC_REQUIRED` | 2 | DLC required |
| `GENERAL` | 3 | General error |
| `GROUP_FULL` | 4 | Group/session full |
| `INVITER_NOT_JOINABLE` | 5 | Inviter not joinable |
| `LEVEL_NOT_HIGH_ENOUGH` | 6 | Level too low |
| `LEVEL_NOT_UNLOCKED` | 7 | Level not unlocked |
| `NETWORK_TIMEOUT` | 8 | Network timeout |
| `NO_LONGER_AVAILABLE` | 9 | No longer available |
| `UPDATE_REQUIRED` | 10 | Update required |
| `TUTORIAL_REQUIRED` | 11 | Tutorial required first |

### `ApplicationInvite` (from `getSentInvites()`, `SendInvitesResult`)

| Property | Type | Default | Description |
|---|---|---|---|
| `id` | `String` | `""` | Unique invite identifier |
| `destination` | `Destination?` | `null` | Invited destination |
| `recipient` | `User?` | `null` | Recipient user info |
| `isActive` | `Boolean?` | `null` | Still active? |
| `lobbySessionId` | `String?` | `null` | Invited lobby session |
| `matchSessionId` | `String?` | `null` | Invited match session |

### `GroupPresenceJoinIntent` (from `joinIntentReceived()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `destinationApiName` | `String?` | `null` | Destination to join |
| `lobbySessionId` | `String?` | `null` | Lobby session to join |
| `matchSessionId` | `String?` | `null` | Match session to join |
| `deeplinkMessage` | `String?` | `null` | Opaque deeplink navigation data |

### `InvitePanelResultInfo` (from `launchInvitePanel()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `invitesSent` | `Boolean` | `false` | Any invitations sent? |

### `LaunchInvitePanelFlowResult` (from `invitationsSent()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `invitedUsers` | `List<User>` | `[]` | Users sent an invitation |

### `RejoinDialogResult` (from `launchRejoinDialog()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `rejoinSelected` | `Boolean` | `false` | User chose to rejoin? |

### `SendInvitesResult` (from `sendInvites()`)

| Property | Type | Default | Description |
|---|---|---|---|
| `invites` | `List<ApplicationInvite>` | `[]` | Invites successfully sent |

### `User`

From `getInvitableUsers()` + invite events; Users package `horizon-platform-sdk-users-kotlin`. **Fields: see [users.md](users.md).**

## Important Notes

1. **Event methods return `Flow`** -- collect `joinIntentReceived()` / `invitationsSent()` in a coroutine scope; they do not throw directly.
2. **Respond to join intents immediately** -- on a `joinIntentReceived()` event, navigate the user to the destination as fast as possible.
3. **`launchInvitePanel()` preferred over `sendInvites()`** -- system panel gives better UX (visual roster); use `sendInvites()` only for programmatic control.
4. **`lobbySessionId` required for invites** -- the user must have `lobbySessionId` set and `isJoinable = true` for invites to work.
5. **Cross-package `User` dependency** -- `User` comes from the Users SDK package `horizon-platform-sdk-users-kotlin`; add it to access User properties.
