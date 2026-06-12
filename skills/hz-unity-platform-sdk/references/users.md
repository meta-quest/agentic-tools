# Users API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-presence/#user-and-friends
- **Namespace**: Oculus.Platform

## Overview

1. Get the logged-in user's profile (ID, alias, avatar URL)
2. Look up any user by ID (including presence data)
3. Fetch the friends (bidirectional followers) list with pagination
4. Obtain access tokens for server-to-server REST calls
5. Generate identity proof nonces for backend verification
6. Retrieve org-scoped IDs for cross-app user identity
7. Launch system Block / Unblock / Friend Request flows

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Complete Data Use Checkup (DUC)** -- required to access user platform features (friends, presence, etc.). See [DUC documentation](https://developer.oculus.com/resources/publish-data-use/).

> **Important**: User IDs are **app-scoped**. The same physical user has a different `User.ID` in different apps. To identify a user across apps within the same org, use `Users.GetOrgScopedID(userID)`.

## API Usage

### Get the Logged-In User

`GetLoggedInUser` is **available offline** and is the single most common call -- use it to get the player's app-scoped ID, Oculus ID alias, and profile picture URL.

```csharp
public async Task<User> GetCurrentUser()
{
    if (!Core.IsInitialized()) return null;

    var msg = await Users.GetLoggedInUser();
    if (msg.IsError)
    {
        Debug.LogError($"GetLoggedInUser failed: {msg.GetError().Message}");
        return null;
    }
    User me = msg.Data;
    Debug.Log($"Signed in as {me.OculusID} (ID: {me.ID})");
    return me;
}
```

> **`GetLoggedInUser` returns limited data**: `OculusID` (alias), `ID` (app-scoped), `ImageURL`. It does **not** return presence info. To get presence, pass the `ID` to `Users.Get(userId)`.

### Get a User by ID

Use this for any user other than the current player, or to get the current player's full presence data.

```csharp
public async Task<User> GetUserById(ulong userId)
{
    if (!Core.IsInitialized()) return null;

    var msg = await Users.Get(userId);
    if (msg.IsError) return null;

    User u = msg.Data;
    Debug.Log($"User: {u.DisplayName ?? u.OculusID}, status: {u.PresenceStatus}, doing: {u.Presence}");
    return u;
}
```

`User.PresenceStatus` is `Online`, `Offline`, or `Unknown`. The human-readable `Presence` string is locale-dependent -- display it as-is, don't parse it.

### Friends List

```csharp
public async Task<List<User>> GetFriends()
{
    if (!Core.IsInitialized()) return new();

    var msg = await Users.GetLoggedInUserFriends();
    if (msg.IsError) return new();

    var friends = new List<User>(msg.Data);

    var page = msg.Data;
    while (page.HasNextPage)
    {
        var nextMsg = await Users.GetNextUserListPage(page);
        if (nextMsg.IsError) break;
        friends.AddRange(nextMsg.Data);
        page = nextMsg.Data;
    }
    return friends;
}
```

> "Friends" means **bidirectional followers** -- both users must follow each other. One-way follows are not returned here.

### Access Token (REST API Calls)

For server-to-server calls to `graph.oculus.com`, fetch an access token. Pass it as a Bearer token in your REST requests.

```csharp
public async Task<string> GetAccessToken()
{
    if (!Core.IsInitialized()) return null;
    var msg = await Users.GetAccessToken();
    if (msg.IsError) return null;
    return msg.Data;
}
```

> **Never log or persist** the access token. Treat it like a session credential -- fetch fresh each time you need it.

### Server-Side Identity Verification (User Proof)

Use this when your backend needs to confirm the player's identity:

1. Client calls `Users.GetUserProof()` to get a one-time `nonce`
2. Client sends `nonce` + `userID` to your backend
3. Backend calls `https://graph.oculus.com/user_nonce_validate?nonce=NONCE&user_id=USER_ID&access_token=APP_ACCESS_TOKEN` to verify
4. Backend stores the verified user mapping

```csharp
public async Task<(string nonce, ulong userId)?> GetIdentityProof()
{
    var meMsg = await Users.GetLoggedInUser();
    if (meMsg.IsError) return null;

    var proofMsg = await Users.GetUserProof();
    if (proofMsg.IsError) return null;

    return (proofMsg.Data.Value, meMsg.Data.ID);
}
```

> The nonce is **single-use**. Each call to `GetUserProof` returns a fresh one. The platform invalidates it after one validation attempt.

### Org-Scoped IDs (Cross-App Within Same Org)

If your org publishes multiple apps and you want to recognize the same user across them, use the org-scoped ID instead of the app-scoped ID.

```csharp
public async Task<string> GetOrgScopedId(ulong appScopedUserId)
{
    var msg = await Users.GetOrgScopedID(appScopedUserId);
    if (msg.IsError) return null;
    return msg.Data.ID;
}
```

### Launch Block / Unblock / Friend-Request Flows

These open the system UI for the user to confirm -- your app can't block someone silently.

```csharp
await Users.LaunchBlockFlow(userId);
await Users.LaunchUnblockFlow(userId);
await Users.LaunchFriendRequestFlow(userId);

// List users blocked by the signed-in user
var blockedMsg = await Users.GetBlockedUsers();
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Users.GetLoggedInUser()` | `Request<User>` | Current player (limited fields, available offline) |
| `Users.Get(userId)` | `Request<User>` | Full user record including presence |
| `Users.GetLoggedInUserFriends()` | `Request<UserList>` | Bidirectional followers |
| `Users.GetAccessToken()` | `Request<string>` | OAuth-style access token for REST calls |
| `Users.GetUserProof()` | `Request<UserProof>` | One-time nonce for backend identity verification |
| `Users.GetOrgScopedID(userId)` | `Request<OrgScopedID>` | Cross-app ID within same org |
| `Users.GetSdkAccounts()` | `Request<SdkAccountList>` | All accounts (Oculus + linked x-users) |
| `Users.GetLinkedAccounts(options)` | `Request<LinkedAccountList>` | Linked external service accounts |
| `Users.GetBlockedUsers()` | `Request<BlockedUserList>` | Users blocked by the signed-in user |
| `Users.LaunchBlockFlow(userId)` | `Request<LaunchBlockFlowResult>` | System UI for blocking a user |
| `Users.LaunchUnblockFlow(userId)` | `Request<LaunchUnblockFlowResult>` | System UI for unblocking |
| `Users.LaunchFriendRequestFlow(userId)` | `Request<LaunchFriendRequestFlowResult>` | System UI for sending follow request |
| `Users.GetLoggedInUserManagedInfo()` | `Request<User>` | MMA-only managed-account info |
| `Users.GetNextUserListPage(list)` | `Request<UserList>` | Paginate friends/users list |

### Models

| Type | Key Fields |
|------|------------|
| `User` | `ID`, `OculusID`, `DisplayName`, `ImageURL`, `SmallImageUrl`, `Presence`, `PresenceStatus`, `PresenceDestinationApiName`, `PresenceLobbySessionId`, `PresenceMatchSessionId`, `ManagedInfoOptional` |
| `UserProof` | `Value` (the nonce) |
| `OrgScopedID` | `ID` |

### Enums

| Enum | Values |
|------|--------|
| `UserPresenceStatus` | `Online`, `Offline`, `Unknown` |

## Error Handling

| Mistake | Fix |
|---------|-----|
| Sharing `User.ID` across apps and expecting the same value | IDs are **app-scoped**. Use `GetOrgScopedID` for cross-app identity within the same org. |
| Calling `GetLoggedInUser` and expecting presence data | Use `Users.Get(loggedInUser.ID)` after `GetLoggedInUser` to get presence fields. |
| Persisting access tokens to disk | Treat tokens as session credentials. Fetch fresh each time you need to call REST. |
| Re-using a `UserProof` nonce | Nonces are single-use. Each backend verification needs a fresh `GetUserProof` call. |
| Skipping DUC | Many user APIs require Data Use Checkup approval. Without it you'll get permission errors. |
| Treating one-way followers as friends | `GetLoggedInUserFriends` returns bidirectional followers only. |
| Parsing `Presence` strings | `Presence` is locale-dependent and may change at any time. Display as-is. |
| Forgetting nullability of `DisplayName`, `ManagedInfoOptional` | Both are nullable. Fall back to `OculusID` for display name. |
| Calling Users methods before init | Always check `Core.IsInitialized()`. |

## Examples

### Example 1: Complete Users Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UsersManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    private bool isInitialized;
    private User cachedMe;
    private List<User> cachedFriends = new();

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }
        isInitialized = true;
        await LoadProfileAndFriends();
    }

    private async Task LoadProfileAndFriends()
    {
        var meMsg = await Users.GetLoggedInUser();
        if (!meMsg.IsError) cachedMe = meMsg.Data;

        var friendsMsg = await Users.GetLoggedInUserFriends();
        if (!friendsMsg.IsError)
        {
            cachedFriends = new List<User>(friendsMsg.Data);
            var page = friendsMsg.Data;
            while (page.HasNextPage)
            {
                var nextMsg = await Users.GetNextUserListPage(page);
                if (nextMsg.IsError) break;
                cachedFriends.AddRange(nextMsg.Data);
                page = nextMsg.Data;
            }
        }
    }

    public User Me => cachedMe;
    public IReadOnlyList<User> Friends => cachedFriends;

    public async Task<string> RequestServerIdentityProof()
    {
        if (!isInitialized) return null;
        var msg = await Users.GetUserProof();
        return msg.IsError ? null : msg.Data.Value;
    }

    public async Task SendFriendRequest(ulong userId)
    {
        if (!isInitialized) return;
        await Users.LaunchFriendRequestFlow(userId);
    }
}
```

### Example 2: Backend Identity Verification Flow

```csharp
// Client-side: get proof and send to backend
public async Task<bool> AuthenticateWithBackend(string backendUrl)
{
    var meMsg = await Users.GetLoggedInUser();
    if (meMsg.IsError) return false;

    var proofMsg = await Users.GetUserProof();
    if (proofMsg.IsError) return false;

    // Send nonce + userId to your backend for validation
    // Backend calls: https://graph.oculus.com/user_nonce_validate?nonce=NONCE&user_id=USER_ID&access_token=APP_ACCESS_TOKEN
    string nonce = proofMsg.Data.Value;
    ulong userId = meMsg.Data.ID;
    Debug.Log($"Authenticating user {userId} with nonce");

    // Your HTTP POST to backend here
    return true;
}
```

## Important Notes

- **`GetLoggedInUser` is available offline** -- safe to call at app start before network is up.
- **IDs are app-scoped**: Don't share `User.ID` with other apps in the same org without going through `GetOrgScopedID`.
- **Cache the friends list at app start**: Refresh sparingly since it changes infrequently.
- **Prefer `DisplayName ?? OculusID`** for user-facing labels.
- **Display the `Presence` string verbatim** -- do not parse it, as it is locale-dependent.
- **Use the system Block/Unblock/FriendRequest flows** -- never silently mutate relationships.
- **Treat access tokens and user proofs as ephemeral credentials**: Never log them.
- **Sample tester**: `samples/unity/Baremetal/Assets/SamplesInternal/users/UsersTester.cs`
- [Data Use Checkup (DUC)](https://developer.oculus.com/resources/publish-data-use/)
