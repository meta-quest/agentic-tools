# Group Presence API

- **Kotlin Package**: `horizon.platform.grouppresence`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-group-presence
- **Minimum OS**: HzOS v78 (invite/roster/rejoin/error dialogs require HzOS v83+)
- **Maven Artifact**: `horizon-platform-sdk-group-presence-kotlin`

> **Related:** invite/roster/rejoin APIs → [group-presence-invites.md](group-presence-invites.md); status codes → [group-presence-errors.md](group-presence-errors.md).
> For setup, initialization, and common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

Operations for Meta VR Android apps to manage multiplayer presence, invitations, and social interactions:

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

Methods 8-14 and both events are documented in [group-presence-invites.md](group-presence-invites.md); this file covers core presence (set/clear/setters).

Currently supported only for immersive mode; non-immersive (2D panel) apps are not yet supported.

## API Usage

All suspend methods throw `GroupPresenceException`; wrap in try/catch. Package paths: options in `horizon.platform.grouppresence.options.*`, models in `horizon.platform.grouppresence.models.*`, enums in `horizon.platform.grouppresence.enums.*`. `User` comes from `horizon.platform.users.models.User`.

#### Set Group Presence (recommended) / Clear

`set()` configures all parameters in a single call, avoiding inconsistent state. See `GroupPresenceOptions` under Data Types.

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
} catch (e: GroupPresenceException) { /* see Error Handling */ }

groupPresence.clear()
```

#### Individual setters

Use only when updating a single parameter (see Important Notes).

```kotlin
groupPresence.setDestination("my_battle_arena")
groupPresence.setIsJoinable(false)
groupPresence.setLobbySession("lobby-abc-123")
groupPresence.setMatchSession("match-xyz-789")
groupPresence.setDeeplinkMessageOverride("{\"level\":5}")
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

## Error Handling

All suspend methods throw `GroupPresenceException` (extends `HzPlatformSdkException`) on failure. Event methods (`joinIntentReceived()`, `invitationsSent()`) return `Flow` and do not throw -- errors are delivered through the Flow's error channel. Always wrap suspend calls in try/catch.

**Status codes:** see [group-presence-errors.md](group-presence-errors.md) (Group Presence-specific codes 2001-2205). For common codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Important Notes

1. **Use `set()` instead of individual setters** -- `set()` updates all parameters atomically; individual setters (`setDestination()`, etc.) can leave presence state inconsistent between calls. Use setters only to update a single parameter.
2. **Clear presence when leaving** -- always call `clear()` on exiting a multiplayer session or the app; stale presence confuses other users.
3. **`matchSessionId` vs `lobbySessionId`** -- lobby session IDs represent close groups (squad/party) where users can see/hear each other; match session IDs represent broader game instances (map, round). Same-lobby users appear in the roster; users sharing only a match session appear in "Recently Played With."
4. **Immersive apps only** -- currently supported only for immersive VR apps; 2D panel apps are not yet supported.
5. **OS version requirements** -- core set/clear require HzOS v78+; invite panel, roster panel, rejoin dialog, and error dialog require HzOS v83+. On older OS versions they return status code 1003 (`ProviderOperationNotSupported`).
6. **`setDeeplinkMessageOverride()` requires a destination** -- it can only be set if the destination is already set; otherwise use `set()` to configure destination and deeplink together.

_Invite/roster/event-specific notes are in [group-presence-invites.md](group-presence-invites.md)._
