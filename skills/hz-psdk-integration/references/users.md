# Users API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.users` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-presence/#user-and-friends |
| **Minimum OS** | HzOS v78 |
| **Maven Artifact** | `horizon-platform-sdk-users-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Examples](#examples)
- [Important Notes](#important-notes)

## Overview

- **`get(userId)`** -- Retrieve a user by their app-scoped ID
- **`getLoggedInUser()`** -- Get the currently signed-in user (available offline)
- **`getLoggedInUserFriends()`** -- Get the logged-in user's bidirectional followers
- **`getAccessToken()`** -- Get an access token for REST API calls
- **`getUserProof()`** -- Get a nonce for server-side user identity verification
- **`getOrgScopedId(userId)`** -- Get an org-scoped ID for cross-app user identification

## API Usage

### Get the Logged-In User

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

val users = Users()

try {
    val user = users.getLoggedInUser()
    // user.id (app-scoped), user.oculusId (alias), user.displayName, user.imageUrl
} catch (e: UsersException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `User` -- immutable. `getLoggedInUser()` returns only alias (Oculus ID), app-scoped ID, and profile URL -- no presence. For presence, use the returned ID with `get(userId)`.

### Get a User by ID

```kotlin
val users = Users()

try {
    val user = users.get(userId)
    // user.presenceStatus (ONLINE/OFFLINE/UNKNOWN), user.presence (human-readable),
    // user.presenceDestinationApiName, user.displayName
} catch (e: UsersException) {
    // Handle error -- user may not exist or may be blocked
}
```

### Get the Logged-In User's Friends

```kotlin
val users = Users()

try {
    val friends: List<User> = users.getLoggedInUserFriends()

    friends.forEach { friend ->
        val name = friend.displayName ?: friend.oculusId
        val status = friend.presenceStatus
    }

} catch (e: UsersException) {
    // Handle error
}
```

### Access Token, User Proof, Org-Scoped ID

All wrap in `try { ... } catch (e: UsersException) { ... }` as above.

```kotlin
val users = Users()
// Access token for REST calls to graph.oculus.com
val accessToken: String = users.getAccessToken()
// Single-use nonce for server-side verification (invalidated once validated). Verify via:
// https://graph.oculus.com/user_nonce_validate?nonce=NONCE&user_id=USER_ID&access_token=ACCESS_TOKEN
val nonce: String = users.getUserProof().nonce
// Org-scoped ID: unique per Developer Center organization, shared across that org's apps
val orgScopedId: String = users.getOrgScopedId(userId).id
```

## Data Types

### `User` Model

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `displayName` | `String?` | `null` | Non-unique displayable name chosen by the user |
| `id` | `String` | `""` | Unique app-scoped user ID |
| `imageUrl` | `String?` | `null` | URL of the user's profile picture |
| `oculusId` | `String?` | `null` | Unique Oculus ID used across developer dashboard |
| `presence` | `String?` | `null` | Human-readable description of current activity |
| `presenceDeeplinkMessage` | `String?` | `null` | Parseable deeplink message for app navigation |
| `presenceDestinationApiName` | `String?` | `null` | API name of the user's current destination |
| `presenceLobbySessionId` | `String?` | `null` | Current lobby session ID |
| `presenceMatchSessionId` | `String?` | `null` | Current match session ID |
| `presenceStatus` | `UserPresenceStatus?` | `null` | Current presence status (ONLINE, OFFLINE, UNKNOWN) |
| `smallImageUrl` | `String?` | `null` | URL of a smaller profile picture |

### `UserProof` Model

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `nonce` | `String` | `""` | Single-use nonce for server-side identity verification |

### `OrgScopedID` Model

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `id` | `String` | `""` | User ID unique per Developer Center organization |

### `UserPresenceStatus` Enum

| Value | Code | Description |
|-------|------|-------------|
| `UNKNOWN` | 0 | Presence status is unknown |
| `ONLINE` | 1 | User is currently online |
| `OFFLINE` | 2 | User is currently offline |

## Error Handling

All methods throw `UsersException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Users-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `QueryFailedError` | 2002 | Content provider query failed while retrieving user data | Retry; may be a database error or permission issue |
| `CursorNotFoundError` | 2003 | Content provider returned a null cursor | Content provider may be unavailable or query parameters invalid |
| `JsonParseError` | 2004 | Failed to parse JSON data during user operations | Retry; data may be malformed or a serialization issue occurred |
| `InvalidUrl` | 2005 | Provided URL is invalid or not a valid HTTPS URL | Verify the URL passed to `sendAuthUrl()` is a valid HTTPS URL |
| `UserAccessTokenNotAvailable` | 2006 | User access token is null or empty | Re-authenticate the user or check session state |
| `UserObjectNotFound` | 2008 | User object with the specified ID does not exist or cannot be loaded | Verify the user ID is valid and the user is not blocked |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example: Online Friends

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException
import horizon.platform.users.enums.UserPresenceStatus

suspend fun getOnlineFriends(): List<User> {
    val users = Users()
    return try {
        users.getLoggedInUserFriends()
            .filter { it.presenceStatus == UserPresenceStatus.ONLINE }
    } catch (e: UsersException) {
        emptyList()
    }
}
```

For server-side verification, combine `getLoggedInUser().id` with `getUserProof().nonce` and validate on your backend via the `user_nonce_validate` Graph endpoint (see the User Proof block above).

## Important Notes

1. **User IDs are app-scoped** -- unique per application. Use `getOrgScopedId()` to identify users across apps within the same Developer Center organization.
2. **`getLoggedInUser()` has limited data** -- only alias (Oculus ID), app-scoped ID, and profile URL; no presence. Use the returned ID with `get(userId)` for presence.
3. **`getLoggedInUser()` is available offline** -- works without network connectivity, unlike most other methods.
4. **User proof nonces are single-use** -- request a new nonce from `getUserProof()` per verification attempt.
5. **Data Use Checkup (DUC)** -- a missing grant fails in two ways, neither obvious. Without `user_id`/`user_profile`, the Graph-backed reads (`get`, `get_org_scoped_id`) **succeed and return wrong data**: IDs come back as the string `"0"` and profile fields are omitted, so a `"0"` user ID means a missing grant, not a bug. `get_logged_in_user` reads a device ContentProvider rather than the server and is not DUC-gated at all. Grants that do deny server-side reach the SDK as a generic `PROVIDER_ERROR` (10), indistinguishable from a transport failure -- there is no permission-specific status code to match on. Which grants gate what, provisional access during development, and what changes when you submit: [Complete data use checkup](https://developers.meta.com/horizon/resources/publish-data-use/).
6. **Requires HzOS v78+** -- older OS versions return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.
