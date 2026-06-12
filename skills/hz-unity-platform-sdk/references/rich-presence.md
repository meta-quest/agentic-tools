# Rich Presence API (Deprecated)

- **Unity Package**: `com.meta.xr.sdk.platform`
- **Documentation**: https://developers.meta.com/horizon/documentation/unity/ps-group-presence-overview/
- **Namespace**: `Oculus.Platform`

> **DEPRECATED** -- Rich Presence is deprecated in favor of **Group Presence**. For all new code, use the Group Presence API (`references/group-presence.md`).
>
> Use this reference only if you're maintaining a legacy app that already uses Rich Presence and need to understand or migrate the existing calls.

## Overview

The Rich Presence API is part of the Horizon Platform SDK. The `RichPresence` class in Unity is now a thin shim that forwards to the underlying `group_presence` module. It provides three legacy operations:

1. **`RichPresence.Set(opts)`** -- Set rich presence fields (destination, etc.)
2. **`RichPresence.Clear()`** -- Clear presence
3. **`RichPresence.GetDestinations()`** -- List configured destinations

New code should call `GroupPresence.*` directly with `GroupPresenceOptions`.

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Why Rich Presence Was Deprecated

Group Presence supersedes it because:

| Capability | Rich Presence (legacy) | Group Presence (recommended) |
|------------|------------------------|-------------------------------|
| Set destination | `Set(RichPresenceOptions)` | `Set(GroupPresenceOptions)` |
| Lobby/match session IDs | Limited | Full support (`LobbySessionId`, `MatchSessionId`) |
| Joinability flag | Limited | `IsJoinable` field |
| Invite panel | Not supported | `LaunchInvitePanel` |
| Join intent callback | Not supported | `SetJoinIntentReceivedNotificationCallback` |
| Multiplayer error dialog | Not supported | `LaunchMultiplayerErrorDialog` |
| Rejoin dialog | Not supported | `LaunchRejoinDialog` |

## Legacy API Usage

#### Set Rich Presence

```csharp
public async Task SetRichPresence(string destinationApiName)
{
    if (!Core.IsInitialized()) return;

    var opts = new RichPresenceOptions();
    opts.SetDestinationApiName(destinationApiName);

    var msg = await RichPresence.Set(opts);
    if (msg.IsError)
    {
        Debug.LogError($"RichPresence.Set: {msg.GetError().Message}");
    }
}
```

#### Clear Rich Presence

```csharp
await RichPresence.Clear();
```

#### List Destinations

```csharp
var msg = await RichPresence.GetDestinations();
if (!msg.IsError)
{
    foreach (var dest in msg.Data) Debug.Log($"{dest.ApiName}: {dest.DisplayName}");
}
```

## Migration to Group Presence

The migration is mostly a rename + builder swap. The semantic meaning of "destination" is preserved.

### Before (Rich Presence)

```csharp
var opts = new RichPresenceOptions();
opts.SetDestinationApiName("main_lobby");
await RichPresence.Set(opts);
```

### After (Group Presence)

```csharp
var opts = new GroupPresenceOptions();
opts.SetDestinationApiName("main_lobby");
opts.SetIsJoinable(true);                  // new: explicit joinability
opts.SetLobbySessionId("abc123");          // new: lobby grouping
// opts.SetMatchSessionId("xyz");          // optional: gameplay grouping
await GroupPresence.Set(opts);
```

### Step-by-Step Migration

1. Find all uses of `RichPresence.Set` / `Clear` / `GetDestinations` in your codebase
2. Replace `RichPresenceOptions` with `GroupPresenceOptions`
3. Replace `RichPresence.X(...)` with `GroupPresence.X(...)`
4. Add explicit `SetIsJoinable(...)` and (if you have multiplayer) `SetLobbySessionId(...)`
5. Subscribe to `GroupPresence.SetJoinIntentReceivedNotificationCallback` to handle accepted invites -- this didn't exist on Rich Presence
6. Test the invite flow end-to-end with another account

## Complete Migration Helper (Legacy to Modern)

If you have a wrapper that touches `RichPresence`, consolidate it:

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class PresenceManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    private bool isInitialized;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) return;

        // New: register the join-intent callback (required for invites to work)
        GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntent);
        isInitialized = true;
    }

    public async Task SetPresence(string destinationApiName, string lobbyId = null, bool joinable = true)
    {
        if (!isInitialized) return;

        var opts = new GroupPresenceOptions();
        opts.SetDestinationApiName(destinationApiName);
        opts.SetIsJoinable(joinable);
        if (!string.IsNullOrEmpty(lobbyId)) opts.SetLobbySessionId(lobbyId);

        await GroupPresence.Set(opts);
    }

    public async Task ClearPresence() => await GroupPresence.Clear();

    private void OnJoinIntent(Message<GroupPresenceJoinIntent> msg)
    {
        if (msg.IsError) return;
        TravelTo(msg.Data.DestinationApiName, msg.Data.LobbySessionId);
    }
}
```

## Data Types

### Methods (Legacy)

| Method | Returns | Description | Replacement |
|--------|---------|-------------|-------------|
| `RichPresence.Set(opts)` | `Request` | Set rich presence fields | `GroupPresence.Set(GroupPresenceOptions)` |
| `RichPresence.Clear()` | `Request` | Clear presence | `GroupPresence.Clear()` |
| `RichPresence.GetDestinations()` | `Request<DestinationList>` | List configured destinations | `GroupPresence.GetDestinations()` |
| `RichPresence.GetNextDestinationListPage(list)` | `Request<DestinationList>` | Pagination | `GroupPresence.GetNextDestinationListPage(list)` |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Using Rich Presence for new code | Use Group Presence instead. |
| Migrating `Set` but not setting `IsJoinable` / `LobbySessionId` | Group Presence requires explicit values for these. Defaults can break the invite flow. |
| Migrating but not adding `SetJoinIntentReceivedNotificationCallback` | Without this, accepted invites do nothing. |

## Important Notes

1. **Don't use Rich Presence for new code.** Use Group Presence instead.

2. **Plan a migration to Group Presence** -- the surface area is small. Add `IsJoinable` and `LobbySessionId` explicitly during migration since Group Presence needs these for the invite flow.

3. **Subscribe to `GroupPresence.SetJoinIntentReceivedNotificationCallback`** after migration so accepted invites actually do something.

## Useful Links

- [Group Presence Documentation (recommended replacement)](https://developers.meta.com/horizon/documentation/unity/ps-group-presence-overview/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Platform SDK Overview](https://developer.oculus.com/documentation/unity/ps-platform-intro/)
