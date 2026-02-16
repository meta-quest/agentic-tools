# Leaderboards API

- **Kotlin Package**: `horizon.platform.leaderboards`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-leaderboards
- **Minimum OS**: HzOS v83
- **Maven Artifact**: `horizon-platform-sdk-leaderboards-kotlin`

## Overview

The Leaderboards API is part of the Horizon Platform SDK. It provides six operations for Meta Quest Android applications:

1. **`get(leaderboardName)`** -- Retrieve detailed information about a single leaderboard by name
2. **`getEntries(leaderboardName, limit, filter, startAt)`** -- Retrieve leaderboard entries with filtering (all, friends, or by user IDs) and starting position options
3. **`getEntriesAfterRank(leaderboardName, limit, afterRank)`** -- Retrieve a block of leaderboard entries starting after a specific rank
4. **`getEntriesByIds(leaderboardName, limit, startAt, userIds)`** -- Retrieve leaderboard entries for specific user IDs
5. **`writeEntry(leaderboardName, score, extraData, forceUpdate)`** -- Write a score entry to a leaderboard
6. **`writeEntryWithSupplementaryMetric(leaderboardName, score, supplementaryMetric, extraData, forceUpdate)`** -- Write a score entry with a supplementary metric for tiebreaker scenarios

Leaderboard-integrated apps get Challenges for free, accessible through the Scoreboards UI.

## Setup

For setup, initialization, and common status codes, see the common-setup tool.

## API Usage

#### Retrieve Leaderboard Information

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.models.Leaderboard

val leaderboards = Leaderboards()

try {
    val result: List<Leaderboard> = leaderboards.get("my_leaderboard")

    for (leaderboard in result) {
        val apiName = leaderboard.apiName        // Unique API name string
        val id = leaderboard.id                   // Generated GUID
        val destination = leaderboard.destination // Optional deep link destination
    }

} catch (e: LeaderboardsException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `leaderboardName: String` -- the API name of the leaderboard to retrieve
**Return type**: `List<Leaderboard>` -- a list of Leaderboard objects matching the name

#### Retrieve Leaderboard Entries

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.enums.LeaderboardFilterType
import horizon.platform.leaderboards.enums.LeaderboardStartAt
import horizon.platform.leaderboards.models.LeaderboardEntry

val leaderboards = Leaderboards()

try {
    val entries: List<LeaderboardEntry> = leaderboards.getEntries(
        "my_leaderboard",               // leaderboard name
        10,                              // limit
        LeaderboardFilterType.NONE,      // filter (NONE, FRIENDS, or USER_IDS)
        LeaderboardStartAt.TOP,          // start position
    )

    for (entry in entries) {
        val rank = entry.rank                              // Position in leaderboard
        val score = entry.score                            // Raw score value
        val displayScore = entry.displayScore              // Formatted score string (nullable)
        val userName = entry.user.displayName               // User's display name (nullable)
        val userId = entry.user.id                          // User's unique ID
        val timestamp = entry.timestamp                     // When the entry was created
        val extraData = entry.extraData                     // Custom data (nullable ByteArray)
        val suppMetric = entry.supplementaryMetric?.metric  // Tiebreaker metric (nullable)
    }

} catch (e: LeaderboardsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `leaderboardName: String` -- the API name of the leaderboard
- `limit: Int` -- maximum number of entries to return
- `filter: LeaderboardFilterType` -- filter type (`NONE`, `FRIENDS`, `USER_IDS`, `UNKNOWN`)
- `startAt: LeaderboardStartAt` -- starting position (`TOP`, `CENTERED_ON_VIEWER`, `CENTERED_ON_VIEWER_OR_TOP`, `UNKNOWN`)

**Return type**: `List<LeaderboardEntry>` -- a list of leaderboard entry objects

#### Retrieve Entries After a Specific Rank

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.models.LeaderboardEntry

val leaderboards = Leaderboards()

try {
    // Get 10 entries starting after rank 50 (returns ranks 51-60)
    val entries: List<LeaderboardEntry> = leaderboards.getEntriesAfterRank(
        "my_leaderboard",  // leaderboard name
        10,                // limit
        50UL,              // after rank (ULong)
    )

    for (entry in entries) {
        println("Rank ${entry.rank}: ${entry.user.displayName} - ${entry.score}")
    }

} catch (e: LeaderboardsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `leaderboardName: String` -- the API name of the leaderboard
- `limit: Int` -- maximum number of entries to return
- `afterRank: ULong` -- the rank after which to start (e.g., 10 returns entries starting at rank 11)

**Return type**: `List<LeaderboardEntry>` -- a list of leaderboard entry objects

#### Retrieve Entries by User IDs

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.enums.LeaderboardStartAt
import horizon.platform.leaderboards.models.LeaderboardEntry

val leaderboards = Leaderboards()

try {
    val userIds = listOf("user_id_1", "user_id_2", "user_id_3")

    val entries: List<LeaderboardEntry> = leaderboards.getEntriesByIds(
        "my_leaderboard",                          // leaderboard name
        10,                                         // limit
        LeaderboardStartAt.CENTERED_ON_VIEWER,      // start position
        userIds,                                    // user IDs to look up
    )

    for (entry in entries) {
        println("${entry.user.displayName}: Rank ${entry.rank}, Score ${entry.score}")
    }

} catch (e: LeaderboardsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `leaderboardName: String` -- the API name of the leaderboard
- `limit: Int` -- maximum number of entries to return
- `startAt: LeaderboardStartAt` -- starting position; if `CENTERED_ON_VIEWER` or `CENTERED_ON_VIEWER_OR_TOP`, the current user's ID is automatically included
- `userIds: List<String>` -- list of user IDs to retrieve entries for

**Return type**: `List<LeaderboardEntry>` -- a list of leaderboard entry objects

#### Write a Leaderboard Entry

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.models.LeaderboardUpdateStatus

val leaderboards = Leaderboards()

try {
    val result: LeaderboardUpdateStatus = leaderboards.writeEntry(
        "my_leaderboard",  // leaderboard name
        1500L,             // score
        null,              // extra data (optional ByteArray)
        null,              // force update (optional Boolean)
    )

    val didUpdate = result.didUpdate                     // Whether the leaderboard was updated
    val challengeIds = result.updatedChallengeIds        // Updated challenge IDs (nullable)

} catch (e: LeaderboardsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `leaderboardName: String` -- the API name of the leaderboard
- `score: Long` -- the score to write
- `extraData: ByteArray?` -- optional 2KB custom data (e.g., game replay data)
- `forceUpdate: Boolean?` -- if `true`, always updates even if not the user's best score

**Return type**: `LeaderboardUpdateStatus` -- contains update status and affected challenge IDs

#### Write an Entry with Supplementary Metric

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.models.LeaderboardUpdateStatus

val leaderboards = Leaderboards()

try {
    val result: LeaderboardUpdateStatus = leaderboards.writeEntryWithSupplementaryMetric(
        "my_leaderboard",  // leaderboard name
        1500L,             // score
        300L,              // supplementary metric (for tiebreakers)
        null,              // extra data (optional ByteArray)
        null,              // force update (optional Boolean)
    )

    if (result.didUpdate) {
        println("Score submitted successfully!")
    }

} catch (e: LeaderboardsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `leaderboardName: String` -- the API name of the leaderboard
- `score: Long` -- the score to write
- `supplementaryMetric: Long` -- supplemental data for tiebreaker scenarios
- `extraData: ByteArray?` -- optional 2KB custom data
- `forceUpdate: Boolean?` -- if `true`, always updates even if not the user's best score

**Return type**: `LeaderboardUpdateStatus` -- contains update status and affected challenge IDs

## Data Types

### `Leaderboard` Model (returned by `get()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `apiName` | `String` | `""` | Unique API name that identifies this leaderboard |
| `destination` | `Destination?` | `null` | Optional deep link destination for the leaderboard |
| `id` | `String` | `""` | Generated GUID for this leaderboard |

### `LeaderboardEntry` Model (returned by `getEntries()`, `getEntriesAfterRank()`, `getEntriesByIds()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `displayScore` | `String?` | `null` | Formatted score string for display |
| `extraData` | `ByteArray?` | `null` | 2KB custom data field (e.g., game replay) |
| `id` | `String?` | `null` | Unique identifier for this leaderboard entry |
| `rank` | `Int` | `0` | Rank position in the leaderboard |
| `score` | `Long` | `0` | Raw score value |
| `supplementaryMetric` | `SupplementaryMetric?` | `null` | Supplemental tiebreaker data |
| `timestamp` | `Time` | -- | Timestamp when the entry was created |
| `user` | `User` | -- | The user who made this entry |

### `LeaderboardUpdateStatus` Model (returned by `writeEntry()`, `writeEntryWithSupplementaryMetric()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `didUpdate` | `Boolean` | `false` | Whether the leaderboard was updated |
| `updatedChallengeIds` | `List<String>?` | `null` | Challenge IDs that were updated as a result |

### `SupplementaryMetric` Model (nested in `LeaderboardEntry`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `id` | `String` | `""` | ID of the leaderboard this metric belongs to |
| `metric` | `Long` | `0` | The tiebreaker metric value |

### `LeaderboardFilterType` Enum

| Value | Integer | Description |
|-------|---------|-------------|
| `NONE` | 0 | No filter; returns all entries |
| `FRIENDS` | 1 | Filter to bidirectional followers only |
| `UNKNOWN` | 2 | Unknown filter type |
| `USER_IDS` | 3 | Filter to specific user IDs |

### `LeaderboardStartAt` Enum

| Value | Integer | Description |
|-------|---------|-------------|
| `TOP` | 0 | Start at the top of the leaderboard |
| `CENTERED_ON_VIEWER` | 1 | Center on the current user's position |
| `CENTERED_ON_VIEWER_OR_TOP` | 2 | Center on the current user, or top if user is not ranked |
| `UNKNOWN` | 3 | Unknown start position |

## Error Handling

All methods throw `LeaderboardsException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes

For common status codes (0-6, 190, 1001-1005), see the common-setup tool.

#### Leaderboards-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `InvalidUserId` | 2001 | The provided user ID is invalid or does not exist | Verify that user IDs are valid before passing to `getEntriesByIds()` |
| `UserNotRanked` | 2002 | The current user does not have a rank on the leaderboard | Use `LeaderboardStartAt.CENTERED_ON_VIEWER_OR_TOP` to fall back to top, or submit a score first |
| `LeaderboardNotConfigured` | 2003 | The leaderboard has not been configured for this application | Create and configure the leaderboard in the developer dashboard |

## Examples

### Example 1: Display Top 10 Leaderboard

Retrieve and display the top 10 entries from a leaderboard.

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.enums.LeaderboardFilterType
import horizon.platform.leaderboards.enums.LeaderboardStartAt

suspend fun getTopScores(leaderboardName: String): List<String> {
    val client = Leaderboards()
    return try {
        val entries = client.getEntries(
            leaderboardName,
            10,
            LeaderboardFilterType.NONE,
            LeaderboardStartAt.TOP,
        )
        entries.map { entry ->
            "#${entry.rank} ${entry.user.displayName ?: "Unknown"}: ${entry.displayScore ?: entry.score.toString()}"
        }
    } catch (e: LeaderboardsException) {
        listOf("Error loading leaderboard: ${e.message}")
    }
}
```

### Example 2: Submit a Score with Error Handling

Write a score and handle common error cases including unconfigured leaderboards.

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException

sealed class ScoreSubmitResult {
    data class Success(val didUpdate: Boolean, val challengeIds: List<String>?) : ScoreSubmitResult()
    data class NotConfigured(val leaderboardName: String) : ScoreSubmitResult()
    data class Error(val message: String) : ScoreSubmitResult()
}

suspend fun submitScore(leaderboardName: String, score: Long, forceUpdate: Boolean = false): ScoreSubmitResult {
    val client = Leaderboards()
    return try {
        val result = client.writeEntry(
            leaderboardName,
            score,
            null,
            if (forceUpdate) true else null,
        )
        ScoreSubmitResult.Success(result.didUpdate, result.updatedChallengeIds)
    } catch (e: LeaderboardsException) {
        when {
            e.message?.contains("2003") == true ->
                ScoreSubmitResult.NotConfigured(leaderboardName)
            else ->
                ScoreSubmitResult.Error(e.message ?: "Unknown error")
        }
    }
}
```

### Example 3: Friends-Only Leaderboard Centered on Viewer

Retrieve friend scores centered around the current user's position.

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.enums.LeaderboardFilterType
import horizon.platform.leaderboards.enums.LeaderboardStartAt
import horizon.platform.leaderboards.models.LeaderboardEntry

suspend fun getFriendScoresAroundMe(leaderboardName: String): List<LeaderboardEntry> {
    val client = Leaderboards()
    return try {
        client.getEntries(
            leaderboardName,
            20,
            LeaderboardFilterType.FRIENDS,
            LeaderboardStartAt.CENTERED_ON_VIEWER_OR_TOP,
        )
    } catch (e: LeaderboardsException) {
        when {
            e.message?.contains("2002") == true -> {
                // User not ranked -- fall back to top of friends leaderboard
                try {
                    client.getEntries(
                        leaderboardName,
                        20,
                        LeaderboardFilterType.FRIENDS,
                        LeaderboardStartAt.TOP,
                    )
                } catch (fallbackError: LeaderboardsException) {
                    emptyList()
                }
            }
            else -> emptyList()
        }
    }
}
```

### Example 4: Paginated Leaderboard Loading

Load leaderboard entries in pages using `getEntriesAfterRank()`.

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.models.LeaderboardEntry

class LeaderboardPager(
    private val leaderboardName: String,
    private val pageSize: Int = 25,
) {
    private val client = Leaderboards()
    private var currentRank: ULong = 0UL
    private val allEntries = mutableListOf<LeaderboardEntry>()

    suspend fun loadNextPage(): List<LeaderboardEntry> {
        return try {
            val entries = client.getEntriesAfterRank(
                leaderboardName,
                pageSize,
                currentRank,
            )
            if (entries.isNotEmpty()) {
                currentRank = entries.last().rank.toULong()
                allEntries.addAll(entries)
            }
            entries
        } catch (e: LeaderboardsException) {
            emptyList()
        }
    }

    fun getAllLoaded(): List<LeaderboardEntry> = allEntries.toList()

    fun reset() {
        currentRank = 0UL
        allEntries.clear()
    }
}
```

### Example 5: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.enums.LeaderboardFilterType
import horizon.platform.leaderboards.enums.LeaderboardStartAt
import horizon.platform.leaderboards.models.LeaderboardEntry
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class LeaderboardUiState(
    val entries: List<LeaderboardEntry> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null,
    val lastSubmitSuccess: Boolean? = null,
)

class LeaderboardViewModel(
    private val leaderboardName: String = "my_leaderboard",
) : ViewModel() {
    private val client = Leaderboards()
    private val _uiState = MutableStateFlow(LeaderboardUiState())
    val uiState: StateFlow<LeaderboardUiState> = _uiState

    fun loadEntries(
        filter: LeaderboardFilterType = LeaderboardFilterType.NONE,
        startAt: LeaderboardStartAt = LeaderboardStartAt.TOP,
        limit: Int = 25,
    ) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val entries = client.getEntries(leaderboardName, limit, filter, startAt)
                _uiState.value = _uiState.value.copy(
                    entries = entries,
                    isLoading = false,
                )
            } catch (e: LeaderboardsException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to load leaderboard",
                )
            }
        }
    }

    fun submitScore(score: Long, forceUpdate: Boolean = false) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val result = client.writeEntry(
                    leaderboardName,
                    score,
                    null,
                    if (forceUpdate) true else null,
                )
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    lastSubmitSuccess = result.didUpdate,
                )
                // Refresh the leaderboard after submitting
                loadEntries()
            } catch (e: LeaderboardsException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    lastSubmitSuccess = false,
                    error = e.message ?: "Failed to submit score",
                )
            }
        }
    }
}
```

## Important Notes

1. **Requires HzOS v83+** -- all Leaderboards API methods require HzOS v83 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

2. **Leaderboard names are API names** -- the `leaderboardName` parameter must match the API name configured in the Meta Developer Dashboard. This is a case-sensitive string identifier, not the display name.

3. **`writeEntry()` respects best score by default** -- unless `forceUpdate` is set to `true`, the score is only updated if it is better than the user's current best score. Set `forceUpdate = true` to always overwrite regardless of whether the new score is better.

4. **Supplementary metrics are for tiebreakers** -- use `writeEntryWithSupplementaryMetric()` when you need a secondary value to break ties between users with the same primary score. The supplementary metric is stored alongside the entry and returned in `LeaderboardEntry.supplementaryMetric`.

5. **Extra data is limited to 2KB** -- the `extraData` parameter accepts an optional `ByteArray` up to 2KB in size. Use it for game replays, additional context, or any metadata associated with the entry.

6. **Handle status code 2002 (`UserNotRanked`)** -- when using `LeaderboardStartAt.CENTERED_ON_VIEWER`, if the current user has no entry on the leaderboard, status code 2002 is returned. Use `CENTERED_ON_VIEWER_OR_TOP` to gracefully fall back to the top of the leaderboard, or handle the error and prompt the user to submit a score first.

7. **`getEntriesByIds()` auto-includes viewer** -- when `startAt` is `CENTERED_ON_VIEWER` or `CENTERED_ON_VIEWER_OR_TOP`, the current user's ID is automatically added to the query, even if not explicitly included in the `userIds` list.

8. **Challenges integration is automatic** -- leaderboard-integrated apps get Challenges for free. When a score is written via `writeEntry()` or `writeEntryWithSupplementaryMetric()`, the response includes `updatedChallengeIds` listing any challenges that were affected by the score submission.

9. **No event streams or sessions** -- this is a request/response API. Each call is independent and stateless. Use `getEntriesAfterRank()` with incrementing rank values to implement paginated loading.
