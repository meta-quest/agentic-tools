# Group Presence API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-group-presence-overview/
- **Namespace**: Oculus.Platform

## Overview

The Group Presence API is part of the Horizon Platform SDK Unity package. It provides operations for Meta Quest Unity applications to manage multiplayer presence, invitations, and social interactions:

1. **`GroupPresence.Set(options)`** -- Set all group presence parameters at once (recommended)
2. **`GroupPresence.Clear()`** -- Clear the current group presence
3. **`GroupPresence.SetDestination(name)`** -- Set the user's current destination (prefer `Set()`)
4. **`GroupPresence.SetIsJoinable(bool)`** -- Set whether the user is joinable (prefer `Set()`)
5. **`GroupPresence.SetLobbySession(id)`** -- Set the user's lobby session ID (prefer `Set()`)
6. **`GroupPresence.SetMatchSession(id)`** -- Set the user's match session ID (prefer `Set()`)
7. **`GroupPresence.SetDeeplinkMessageOverride(msg)`** -- Override the deeplink message (prefer `Set()`)
8. **`GroupPresence.GetInvitableUsers(options)`** -- Get users who can be invited to the current lobby
9. **`GroupPresence.GetSentInvites()`** -- Get invites previously sent by the user
10. **`GroupPresence.SendInvites(userIds)`** -- Send invites to specific users
11. **`GroupPresence.LaunchInvitePanel(options)`** -- Launch the system invite dialog
12. **`GroupPresence.LaunchRosterPanel(options)`** -- Launch the roster/party panel
13. **`GroupPresence.LaunchRejoinDialog(lobby, match, dest)`** -- Launch the rejoin dialog
14. **`GroupPresence.LaunchMultiplayerErrorDialog(options)`** -- Launch a predefined error dialog
15. **`GroupPresence.GetDestinations()`** -- List configured Destinations

**Notification Callbacks (register immediately after init):**
1. **`SetJoinIntentReceivedNotificationCallback(cb)`** -- Fires when a user accepts an invite or taps "Join"
2. **`SetInvitationsSentNotificationCallback(cb)`** -- Fires when the user finishes the invite panel
3. **`SetLeaveIntentReceivedNotificationCallback(cb)`** -- Fires when the user leaves a session via the platform UI

**Note:** These APIs are currently supported only for immersive mode. Non-immersive apps (regular Android panel apps or 2D experiences) are not yet supported.

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Register your app** at [developer.oculus.com/manage](https://developer.oculus.com/manage/)
2. **Create at least one Destination** in the Developer Dashboard. A Destination is a named, deep-linkable location in your app (e.g., `lobby`, `boss_arena`, `tutorial`). Note the **API Name**.
3. **Note your App ID**

**Concept**: A Destination is a top-level location. A `lobby_session_id` is a specific session at that destination (e.g., a specific lobby instance). A `match_session_id` further narrows users into the same gameplay instance (e.g., a match within a lobby).

## API Usage

### Register Notification Callbacks (Immediately After Init)

Notification callbacks **must be registered immediately after init** (before the platform delivers any pending events):

```csharp
async void Start()
{
    var msg = await Core.AsyncInitialize(appId);
    if (msg.IsError) return;

    GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntent);
    GroupPresence.SetInvitationsSentNotificationCallback(OnInvitationsSent);
    GroupPresence.SetLeaveIntentReceivedNotificationCallback(OnLeaveIntent);
}

private void OnJoinIntent(Message<GroupPresenceJoinIntent> msg)
{
    if (msg.IsError) return;
    var intent = msg.Data;
    Debug.Log($"User wants to join: dest={intent.DestinationApiName}, lobby={intent.LobbySessionId}, match={intent.MatchSessionId}");
    TravelTo(intent.DestinationApiName, intent.LobbySessionId, intent.MatchSessionId, intent.DeeplinkMessage);
}
```

### Set Group Presence (Recommended)

Use `GroupPresence.Set` with **all fields at once** (atomic update). Avoid the individual setters -- they can produce inconsistent intermediate states.

```csharp
public async Task EnterLobby(string lobbyId, bool isJoinable = true)
{
    var options = new GroupPresenceOptions();
    options.SetDestinationApiName("main_lobby");
    options.SetLobbySessionId(lobbyId);
    options.SetIsJoinable(isJoinable);
    options.SetDeeplinkMessageOverride($"lobby={lobbyId}");

    var msg = await GroupPresence.Set(options);
    if (msg.IsError)
    {
        Debug.LogError($"GroupPresence.Set failed: {msg.GetError().Message}");
    }
}

public async Task EnterMatch(string lobbyId, string matchId)
{
    var options = new GroupPresenceOptions();
    options.SetDestinationApiName("boss_arena");
    options.SetLobbySessionId(lobbyId);
    options.SetMatchSessionId(matchId);
    options.SetIsJoinable(false); // match in progress, not joinable mid-fight
    await GroupPresence.Set(options);
}
```

### Clear Group Presence

```csharp
public async Task GoIdle()
{
    await GroupPresence.Clear();
}
```

### Launch the Invite Panel

```csharp
public async Task OpenInvitePanel()
{
    var options = new InviteOptions();
    var msg = await GroupPresence.LaunchInvitePanel(options);
    if (msg.IsError)
    {
        Debug.LogError($"LaunchInvitePanel: {msg.GetError().Message}");
    }
}

private void OnInvitationsSent(Message<LaunchInvitePanelFlowResult> msg)
{
    if (msg.IsError) return;
    foreach (var user in msg.Data.InvitedUsers)
    {
        Debug.Log($"Invited: {user.DisplayName}");
    }
}
```

### Direct-Send Invites (Programmatic)

```csharp
public async Task DirectInvite(ulong[] userIds)
{
    var msg = await GroupPresence.SendInvites(userIds);
    if (!msg.IsError)
    {
        Debug.Log($"Sent {msg.Data.InvitedUsers.Count} invites");
    }
}
```

Prefer `LaunchInvitePanel` -- it surfaces suggested friends and Recently Played With.

### Get Invitable Users

```csharp
var msg = await GroupPresence.GetInvitableUsers(new InviteOptions());
foreach (var user in msg.Data) Debug.Log(user.DisplayName);
```

### Multiplayer Error Dialog

```csharp
public async Task ShowLobbyFullError()
{
    var opts = new MultiplayerErrorOptions();
    opts.SetErrorKey(MultiplayerErrorErrorKey.DestinationUnavailable);
    await GroupPresence.LaunchMultiplayerErrorDialog(opts);
}
```

### Rejoin Dialog

```csharp
public async Task OfferRejoin(string lobbyId, string matchId, string destination)
{
    var msg = await GroupPresence.LaunchRejoinDialog(lobbyId, matchId, destination);
    if (!msg.IsError && msg.Data.RejoinSelected)
    {
        // User chose to rejoin
    }
}
```

### List Configured Destinations

```csharp
var msg = await GroupPresence.GetDestinations();
foreach (var dest in msg.Data) Debug.Log($"{dest.ApiName}: {dest.DisplayName}");
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GroupPresence.Set(options)` | `Request` | Set all presence fields atomically (recommended) |
| `GroupPresence.Clear()` | `Request` | Clear current presence |
| `GroupPresence.LaunchInvitePanel(options)` | `Request<InvitePanelResultInfo>` | Open system invite UI |
| `GroupPresence.GetInvitableUsers(options)` | `Request<UserList>` | Friends + recently met, eligible to invite |
| `GroupPresence.SendInvites(userIds)` | `Request<SendInvitesResult>` | Programmatic invite (skip panel) |
| `GroupPresence.GetSentInvites()` | `Request<ApplicationInviteList>` | Invites the user has sent |
| `GroupPresence.LaunchMultiplayerErrorDialog(options)` | `Request` | System-localized error dialog |
| `GroupPresence.LaunchRejoinDialog(lobby, match, dest)` | `Request<RejoinDialogResult>` | "Rejoin?" prompt |
| `GroupPresence.LaunchRosterPanel(options)` | `Request` | Roster UI (rarely needed; system handles it) |
| `GroupPresence.GetDestinations()` | `Request<DestinationList>` | List configured Destinations |
| `GroupPresence.SetDestination(name)` | `Request` | Individual setter -- prefer `Set()` |
| `GroupPresence.SetLobbySession(id)` | `Request` | Individual setter -- prefer `Set()` |
| `GroupPresence.SetMatchSession(id)` | `Request` | Individual setter -- prefer `Set()` |
| `GroupPresence.SetIsJoinable(bool)` | `Request` | Individual setter -- prefer `Set()` |
| `GroupPresence.SetDeeplinkMessageOverride(msg)` | `Request` | Individual setter -- prefer `Set()` |

### `GroupPresenceOptions` (passed to `Set()`)

| Field | Setter | Description |
|-------|--------|-------------|
| `DestinationApiName` | `SetDestinationApiName` | Top-level location (must match a Destination configured in the Dashboard) |
| `LobbySessionId` | `SetLobbySessionId` | Identifies a specific lobby instance -- same ID = same lobby visible to each other and "Recently Played With" |
| `MatchSessionId` | `SetMatchSessionId` | Identifies a specific match instance -- same ID = playing together right now (does NOT show in roster) |
| `IsJoinable` | `SetIsJoinable` | If false, others cannot invite the user. Set false when full / private. |
| `DeeplinkMessageOverride` | `SetDeeplinkMessageOverride` | Opaque string your app understands. Use it to pass extra context for join intent. |

### `GroupPresenceJoinIntent` Model (from `SetJoinIntentReceivedNotificationCallback`)

| Field | Type | Description |
|-------|------|-------------|
| `DestinationApiName` | `string` | The destination the user wants to join |
| `LobbySessionId` | `string` | The lobby session the user wants to join |
| `MatchSessionId` | `string` | The match session the user wants to join |
| `DeeplinkMessage` | `string` | Opaque deeplink data to help navigate the user |

### `Destination` Model

| Field | Type | Description |
|-------|------|-------------|
| `ApiName` | `string` | API name (matches Dashboard configuration) |
| `DisplayName` | `string` | Human-readable display name |
| `DeeplinkMessage` | `string` | Default deeplink message for the destination |
| `ShareableUri` | `string` | Shareable URI for the destination |

### `ApplicationInvite` Model (from `GetSentInvites()`)

| Field | Type | Description |
|-------|------|-------------|
| `ID` | `ulong` | Unique invite identifier |
| `Recipient` | `User` | The recipient's user information |
| `IsActive` | `bool` | Whether the invite is still active |
| `LobbySessionId` | `string` | Lobby session the recipient is invited to |
| `MatchSessionId` | `string` | Match session the recipient is invited to |
| `DestinationOptional` | `Destination` | The destination the recipient is invited to (nullable) |

### `MultiplayerErrorErrorKey` Enum

| Value | Description |
|-------|-------------|
| `DestinationUnavailable` | Travel destination is no longer available |
| `DlcRequired` | Downloadable content is required |
| `General` | General error not covered by other keys |
| `GroupFull` | The group/session is full |
| `InviterNotJoinable` | The inviter's presence is not set to joinable |
| `LevelNotHighEnough` | User's level is not high enough |
| `LevelNotUnlocked` | Required level has not been unlocked |
| `NetworkTimeout` | Network timeout occurred |
| `NoLongerAvailable` | Content is no longer available |
| `UpdateRequired` | An update is required |
| `TutorialRequired` | A tutorial must be completed first |
| `Unknown` | Unknown error |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Using individual setters (`SetDestination`, `SetLobbySession`, ...) for a multi-field update | Use `GroupPresence.Set(options)` with all fields at once for atomic updates. |
| Not registering `OnJoinIntent` immediately after init | Register all notification callbacks right after init, before any pending events deliver. |
| Treating `match_session_id` like `lobby_session_id` | They're distinct. Lobby drives roster/recently-played-with; match drives gameplay grouping. |
| Setting `IsJoinable=true` when the lobby is full | Update presence to `IsJoinable=false` as soon as the lobby fills. |
| Using a Destination API name that isn't in the Dashboard | Will silently fail. Always verify with `GetDestinations()` during dev. |
| Silently ignoring `OnJoinIntent` errors (lobby full, etc.) | Show a clear error via `LaunchMultiplayerErrorDialog` so the user knows why the join failed. |
| Calling Group Presence methods before init | Always check `Core.IsInitialized()`. |
| Forgetting to `Clear()` when the user goes idle/menu | Stale presence shows the user still "in lobby" to friends. Clear on menu/idle. |

## Examples

### Complete Group Presence Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class GroupPresenceManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    private bool isInitialized;
    public event Action<string, string, string, string> JoinIntentReceived; // (dest, lobby, match, deeplink)

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }

        GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntent);
        GroupPresence.SetInvitationsSentNotificationCallback(OnInvitationsSent);
        GroupPresence.SetLeaveIntentReceivedNotificationCallback(OnLeaveIntent);
        isInitialized = true;
    }

    public async Task EnterLobby(string destinationApiName, string lobbyId, bool joinable = true)
    {
        if (!isInitialized) return;
        var opts = new GroupPresenceOptions();
        opts.SetDestinationApiName(destinationApiName);
        opts.SetLobbySessionId(lobbyId);
        opts.SetIsJoinable(joinable);
        await GroupPresence.Set(opts);
    }

    public async Task EnterMatch(string destinationApiName, string lobbyId, string matchId)
    {
        var opts = new GroupPresenceOptions();
        opts.SetDestinationApiName(destinationApiName);
        opts.SetLobbySessionId(lobbyId);
        opts.SetMatchSessionId(matchId);
        opts.SetIsJoinable(false);
        await GroupPresence.Set(opts);
    }

    public async Task ClearPresence() => await GroupPresence.Clear();

    public async Task LaunchInvitePanel() =>
        await GroupPresence.LaunchInvitePanel(new InviteOptions());

    private void OnJoinIntent(Message<GroupPresenceJoinIntent> msg)
    {
        if (msg.IsError) return;
        var i = msg.Data;
        JoinIntentReceived?.Invoke(i.DestinationApiName, i.LobbySessionId, i.MatchSessionId, i.DeeplinkMessage);
    }

    private void OnInvitationsSent(Message<LaunchInvitePanelFlowResult> msg)
    {
        if (msg.IsError) return;
        Debug.Log($"Sent {msg.Data.InvitedUsers.Count} invites");
    }

    private void OnLeaveIntent(Message<GroupPresenceLeaveIntent> msg)
    {
        if (msg.IsError) return;
        Debug.Log("User wants to leave the current session");
    }
}
```

## Important Notes

1. **Use `Set()` instead of individual setters** -- the `Set()` method updates all group presence parameters atomically. Using individual setters like `SetDestination()`, `SetIsJoinable()`, etc. can lead to inconsistent presence state between calls.

2. **Respond to join intents immediately** -- when a `SetJoinIntentReceivedNotificationCallback` event fires, navigate the user to the requested destination as quickly as possible. If the user can't be taken there (full, ended, etc.), use `LaunchMultiplayerErrorDialog` for system-localized messaging.

3. **Clear presence when leaving** -- always call `Clear()` when the user exits a multiplayer session or your app. Stale presence data causes confusion for other users.

4. **`LaunchInvitePanel()` is preferred over `SendInvites()`** -- the system invite panel provides a better user experience with a visual roster.

5. **`lobbySessionId` is required for invites** -- a user must have `lobbySessionId` set and `isJoinable` set to `true` in their group presence for the invite system to work.

6. **`matchSessionId` vs `lobbySessionId`** -- lobby session IDs represent close groups (squad/party) where users can see/hear each other. Match session IDs represent broader game instances (map, round). Users with the same lobby session ID appear in the roster; users with only the same match session ID appear in "Recently Played With."

7. **Immersive apps only** -- the Group Presence API is currently supported only for immersive VR apps. Regular Android panel apps and 2D experiences are not yet supported.

## Useful Links

- [Meta Quest Group Presence Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-group-presence-overview/)
- [Destinations Overview](https://developer.oculus.com/documentation/unity/ps-destinations-overview/)
- [Invokable Error Dialogs](https://developer.oculus.com/documentation/unity/ps-multiplayer-error-dialog/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
