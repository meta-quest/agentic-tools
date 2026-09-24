# Leaderboards API

- **Kotlin Package**: `horizon.platform.leaderboards`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-leaderboards
- **Minimum OS**: HzOS v83
- **Maven Artifact**: `horizon-platform-sdk-leaderboards-kotlin`

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

Operations on `Leaderboards` (typed signatures in [API Usage](#api-usage)):

1. `get()` -- info about a single leaderboard by name
2. `getEntries()` -- entries with filtering (all/friends/by user IDs) + start position
3. `getEntriesAfterRank()` -- a block of entries starting after a specific rank
4. `getEntriesByIds()` -- entries for specific user IDs
5. `writeEntry()` -- write a score entry
6. `writeEntryWithSupplementaryMetric()` -- write a score entry with a supplementary metric for tiebreakers

Leaderboard-integrated apps get Challenges for free, accessible through the Scoreboards UI.

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## API Usage

Imports:

```kotlin
import horizon.platform.leaderboards.Leaderboards
import horizon.platform.leaderboards.LeaderboardsException
import horizon.platform.leaderboards.models.Leaderboard
import horizon.platform.leaderboards.models.LeaderboardEntry
import horizon.platform.leaderboards.models.LeaderboardUpdateStatus
import horizon.platform.leaderboards.enums.LeaderboardFilterType
import horizon.platform.leaderboards.enums.LeaderboardStartAt
```

All calls throw `LeaderboardsException`; wrap in try/catch. Returned-object properties and enum values: see [Data Types](#data-types).

```kotlin
val leaderboards = Leaderboards()

// get(leaderboardName: String): List<Leaderboard>
val info = leaderboards.get("my_leaderboard")

// getEntries(leaderboardName: String, limit: Int, filter: LeaderboardFilterType, startAt: LeaderboardStartAt): List<LeaderboardEntry>
val entries = leaderboards.getEntries("my_leaderboard", 10, LeaderboardFilterType.NONE, LeaderboardStartAt.TOP)

// getEntriesAfterRank(leaderboardName: String, limit: Int, afterRank: ULong): List<LeaderboardEntry> -- afterRank=50UL returns ranks 51-60
val block = leaderboards.getEntriesAfterRank("my_leaderboard", 10, 50UL)

// getEntriesByIds(leaderboardName: String, limit: Int, startAt: LeaderboardStartAt, userIds: List<String>): List<LeaderboardEntry>
val byId = leaderboards.getEntriesByIds("my_leaderboard", 10, LeaderboardStartAt.CENTERED_ON_VIEWER, listOf("user_id_1", "user_id_2"))

// writeEntry(leaderboardName: String, score: Long, extraData: ByteArray?, forceUpdate: Boolean?): LeaderboardUpdateStatus
val w = leaderboards.writeEntry("my_leaderboard", 1500L, null, null)

// writeEntryWithSupplementaryMetric(leaderboardName: String, score: Long, supplementaryMetric: Long, extraData: ByteArray?, forceUpdate: Boolean?): LeaderboardUpdateStatus
val ws = leaderboards.writeEntryWithSupplementaryMetric("my_leaderboard", 1500L, 300L, null, null)
```

## Data Types

### `Leaderboard` (from `get()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `apiName` | `String` | `""` | Unique API name that identifies this leaderboard |
| `destination` | `Destination?` | `null` | Optional deep link destination for the leaderboard |
| `id` | `String` | `""` | Generated GUID for this leaderboard |

### `LeaderboardEntry` (from `getEntries()`, `getEntriesAfterRank()`, `getEntriesByIds()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `displayScore` | `String?` | `null` | Formatted score string for display |
| `extraData` | `ByteArray?` | `null` | 2KB custom data field (e.g., game replay) |
| `id` | `String?` | `null` | Unique identifier for this leaderboard entry |
| `rank` | `Int` | `0` | Rank position in the leaderboard |
| `score` | `Long` | `0` | Raw score value |
| `supplementaryMetric` | `SupplementaryMetric?` | `null` | Supplemental tiebreaker data |
| `timestamp` | `Time` | -- | Timestamp when the entry was created |
| `user` | `User` | -- | The user who made this entry (`user.displayName` nullable, `user.id` = ID) |

### `LeaderboardUpdateStatus` (from `writeEntry()`, `writeEntryWithSupplementaryMetric()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `didUpdate` | `Boolean` | `false` | Whether the leaderboard was updated |
| `updatedChallengeIds` | `List<String>?` | `null` | Challenge IDs that were updated as a result |

### `SupplementaryMetric` (nested in `LeaderboardEntry`)

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

`LeaderboardsException` extends `HzPlatformSdkException`. For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

#### Leaderboards-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `InvalidUserId` | 2001 | The provided user ID is invalid or does not exist | Verify user IDs before `getEntriesByIds()` |
| `UserNotRanked` | 2002 | The current user does not have a rank on the leaderboard | Use `CENTERED_ON_VIEWER_OR_TOP` to fall back to top, or submit a score first |
| `LeaderboardNotConfigured` | 2003 | The leaderboard has not been configured for this application | Configure the leaderboard in the developer dashboard |

## Important Notes

1. **OS** -- requires HzOS v83+; older OS returns 1003 (`ProviderOperationNotSupported`). Require a min OS in `AndroidManifest.xml` ([Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003.
2. **Names are API names** -- `leaderboardName` must match the case-sensitive API name in the Developer Dashboard, not the display name.
3. **Best score by default** -- `writeEntry()` updates only if the score beats the user's best, unless `forceUpdate = true`.
4. **Supplementary metrics** -- tiebreakers via `writeEntryWithSupplementaryMetric()`; returned in `LeaderboardEntry.supplementaryMetric`.
5. **`getEntriesByIds()` auto-includes viewer** -- when `startAt` is `CENTERED_ON_VIEWER`/`CENTERED_ON_VIEWER_OR_TOP`, the current user's ID is added even if absent from `userIds`.
6. **Challenges** -- automatic for leaderboard-integrated apps; `writeEntry()` / `writeEntryWithSupplementaryMetric()` return `updatedChallengeIds` for affected challenges.
7. **Stateless** -- request/response API, no event streams or sessions; paginate via `getEntriesAfterRank()` with incrementing ranks.
