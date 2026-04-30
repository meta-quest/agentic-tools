# Users API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.users` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-presence/#user-and-friends |
| **Minimum OS** | HzOS v78 |
| **Maven Artifact** | `horizon-platform-sdk-users-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Overview

The Users API provides methods to access user information and perform identity verification on Meta Quest Android applications. Key capabilities include:

1. **`get(userId)`** -- Retrieve a user by their app-scoped ID
2. **`getLoggedInUser()`** -- Get the currently signed-in user (available offline)
3. **`getLoggedInUserFriends()`** -- Get the logged-in user's bidirectional followers
4. **`getAccessToken()`** -- Get an access token for REST API calls
5. **`getUserProof()`** -- Get a nonce for server-side user identity verification
6. **`getOrgScopedId(userId)`** -- Get an org-scoped ID for cross-app user identification

## API Usage

### Get the Logged-In User

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

val users = Users()

try {
    val user = users.getLoggedInUser()

    val userId = user.id                // App-scoped user ID
    val oculusId = user.oculusId        // Oculus ID (alias)
    val displayName = user.displayName  // Display name (may be null)
    val imageUrl = user.imageUrl        // Profile picture URL (may be null)

} catch (e: UsersException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `User` -- an immutable object containing user profile information.

**Note**: `getLoggedInUser()` only returns alias (Oculus ID), ID (app-scoped ID), and profile URL. It does not return presence information. For presence details, use the returned ID with `get(userId)`.

### Get a User by ID

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

val users = Users()

try {
    val user = users.get(userId)

    val displayName = user.displayName
    val presenceStatus = user.presenceStatus   // ONLINE, OFFLINE, or UNKNOWN
    val presence = user.presence               // Human-readable presence string
    val destinationApiName = user.presenceDestinationApiName

} catch (e: UsersException) {
    // Handle error -- user may not exist or may be blocked
}
```

### Get the Logged-In User's Friends

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

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

### Get an Access Token

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

val users = Users()

try {
    val accessToken: String = users.getAccessToken()
    // Use accessToken for REST calls to graph.oculus.com

} catch (e: UsersException) {
    // Handle error
}
```

### Verify User Identity (User Proof)

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

val users = Users()

try {
    val proof = users.getUserProof()
    val nonce = proof.nonce

    // Send nonce + user ID to your backend for verification via:
    // https://graph.oculus.com/user_nonce_validate?nonce=NONCE&user_id=USER_ID&access_token=ACCESS_TOKEN

} catch (e: UsersException) {
    // Handle error
}
```

**Note**: The nonce is single-use. Once validated, it is invalidated.

### Get Org-Scoped ID

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

val users = Users()

try {
    val orgScoped = users.getOrgScopedId(userId)
    val orgScopedId = orgScoped.id  // Unique per organization, shared across apps

} catch (e: UsersException) {
    // Handle error
}
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

### Enums

#### `UserPresenceStatus`

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

### Example 1: Basic User Profile Retrieval

Retrieve the logged-in user and display their profile information.

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

suspend fun getLoggedInUserProfile(): Map<String, String> {
    val users = Users()
    return try {
        val user = users.getLoggedInUser()
        mapOf(
            "id" to user.id,
            "name" to (user.displayName ?: user.oculusId ?: "Unknown"),
            "imageUrl" to (user.imageUrl ?: ""),
        )
    } catch (e: UsersException) {
        mapOf("error" to (e.message ?: "Failed to get user"))
    }
}
```

### Example 2: Friends List with Presence

Retrieve the user's friends and show who is currently online.

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException
import horizon.platform.users.enums.UserPresenceStatus

data class FriendInfo(
    val id: String,
    val name: String,
    val isOnline: Boolean,
    val currentActivity: String?,
)

suspend fun getOnlineFriends(): List<FriendInfo> {
    val users = Users()
    return try {
        val friends = users.getLoggedInUserFriends()
        friends.map { friend ->
            FriendInfo(
                id = friend.id,
                name = friend.displayName ?: friend.oculusId ?: "Unknown",
                isOnline = friend.presenceStatus == UserPresenceStatus.ONLINE,
                currentActivity = friend.presence,
            )
        }
    } catch (e: UsersException) {
        emptyList()
    }
}
```

### Example 3: Server-Side User Verification

Use user proof to verify identity on your backend server.

```kotlin
import horizon.platform.users.Users
import horizon.platform.users.UsersException

sealed class VerificationResult {
    data class Success(val userId: String, val nonce: String) : VerificationResult()
    data class Error(val message: String) : VerificationResult()
}

suspend fun getUserVerificationData(): VerificationResult {
    val users = Users()
    return try {
        val loggedInUser = users.getLoggedInUser()
        val proof = users.getUserProof()
        VerificationResult.Success(
            userId = loggedInUser.id,
            nonce = proof.nonce,
        )
        // Send userId and nonce to your backend to verify via:
        // GET https://graph.oculus.com/user_nonce_validate?nonce=NONCE&user_id=USER_ID&access_token=ACCESS_TOKEN
    } catch (e: UsersException) {
        VerificationResult.Error(e.message ?: "Verification failed")
    }
}
```

### Example 4: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.users.Users
import horizon.platform.users.UsersException
import horizon.platform.users.enums.UserPresenceStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class UserProfileUiState(
    val userId: String = "",
    val displayName: String = "",
    val imageUrl: String = "",
    val friends: List<FriendItem> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null,
)

data class FriendItem(
    val id: String,
    val name: String,
    val isOnline: Boolean,
)

class UserProfileViewModel : ViewModel() {
    private val users = Users()
    private val _uiState = MutableStateFlow(UserProfileUiState())
    val uiState: StateFlow<UserProfileUiState> = _uiState

    fun loadUserProfile() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val user = users.getLoggedInUser()
                _uiState.value = _uiState.value.copy(
                    userId = user.id,
                    displayName = user.displayName ?: user.oculusId ?: "Unknown",
                    imageUrl = user.imageUrl ?: "",
                )
            } catch (e: UsersException) {
                _uiState.value = _uiState.value.copy(error = e.message)
            }

            try {
                val friends = users.getLoggedInUserFriends()
                _uiState.value = _uiState.value.copy(
                    friends = friends.map { friend ->
                        FriendItem(
                            id = friend.id,
                            name = friend.displayName ?: friend.oculusId ?: "Unknown",
                            isOnline = friend.presenceStatus == UserPresenceStatus.ONLINE,
                        )
                    },
                )
            } catch (e: UsersException) {
                // Friends list failed but profile may still be valid
            }

            _uiState.value = _uiState.value.copy(isLoading = false)
        }
    }
}
```

## Important Notes

1. **User IDs are app-scoped** -- each user has a unique ID per application. The same person will have different IDs in different apps. Use `getOrgScopedId()` to identify users across apps within the same Developer Center organization.

2. **`getLoggedInUser()` has limited data** -- it only returns the alias (Oculus ID), app-scoped ID, and profile URL. It does not return presence information. To get presence details, use the returned ID with `get(userId)`.

3. **`getLoggedInUser()` is available offline** -- unlike most other methods, this call works without network connectivity.

4. **User proof nonces are single-use** -- the nonce returned by `getUserProof()` can only be validated once against the Graph API endpoint. After validation, it is invalidated. Request a new nonce for each verification attempt.

5. **Data Use Checkup (DUC) required** -- you must complete a DUC in the Meta Developer Dashboard to access user platform features. Without DUC, API calls may fail with entitlement errors.

6. **Requires HzOS v78+** -- the Users API requires HzOS v78 or later. On older OS versions, methods return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

7. **No events or pagination** -- the Users API is a request/response API. Each call is independent and stateless. Friends lists are returned as complete lists, not paginated streams.
