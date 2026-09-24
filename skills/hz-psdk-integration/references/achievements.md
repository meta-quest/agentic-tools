# Achievements API

- **Kotlin Package**: `horizon.platform.achievements`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-achievements
- **Minimum OS**: HzOS v85
- **Maven Artifact**: `horizon-platform-sdk-achievements-kotlin`

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

Part of the Horizon Platform SDK. Awards trophies/badges for reaching goals. Operations on `Achievements`:

1. `addCount(name, count)` -- Add a count value to a COUNT achievement
2. `addFields(name, fields)` -- Unlock bits in a BITFIELD achievement
3. `getAllDefinitions(coroutineScope)` -- Retrieve all achievement definitions for the app
4. `getAllProgress(coroutineScope)` -- Retrieve the user's progress on all achievements
5. `getDefinitionsByName(coroutineScope, names)` -- Retrieve specific definitions by API name
6. `getProgressByName(coroutineScope, names)` -- Retrieve progress on specific achievements by API name
7. `unlock(name)` -- Unlock an achievement of any type (simple, count, or bitfield)

Achievement types: **Simple** (all-or-nothing), **Count** (counter reaches target), **Bitfield** (target number of bits set).

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

Imports:

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementUpdate
import horizon.platform.achievements.models.AchievementDefinition
import horizon.platform.achievements.models.AchievementProgress
import horizon.platform.achievements.enums.AchievementType
import horizon.core.android.common.pagination.PagedResults
import kotlinx.coroutines.CoroutineScope
```

All calls throw `AchievementsException`; wrap in try/catch. Returned-object properties: see [Data Types](#data-types).

```kotlin
val achievements = Achievements()

// unlock() -- name: String (the api_name). Returns AchievementUpdate.
val u: AchievementUpdate = achievements.unlock("REACHED_LEVEL_10")

// addCount() -- name: String; count: ULong (max: signed 64-bit int max, clamped). Returns AchievementUpdate.
val c: AchievementUpdate = achievements.addCount("ENEMIES_DEFEATED", 5uL)

// addFields() -- name: String; fields: String of '0'/'1', each '1' unlocks that bit. Returns AchievementUpdate.
val f: AchievementUpdate = achievements.addFields("COLLECT_ALL_ITEMS", "101")
```

Paginated getters take a `CoroutineScope` and return `PagedResults<T>` (NOT suspend functions); iterate with `.collect {}`. Pagination is automatic. `names` are the `api_names` to retrieve.

```kotlin
// getAllDefinitions(coroutineScope) / getDefinitionsByName(coroutineScope, names: List<String>): PagedResults<AchievementDefinition>
val defs: PagedResults<AchievementDefinition> = achievements.getAllDefinitions(coroutineScope)
achievements.getDefinitionsByName(coroutineScope, listOf("REACHED_LEVEL_10", "ENEMIES_DEFEATED"))
defs.collect { list: List<AchievementDefinition> -> /* props: see Data Types */ }

// getAllProgress(coroutineScope) / getProgressByName(coroutineScope, names: List<String>): PagedResults<AchievementProgress>
val progress: PagedResults<AchievementProgress> = achievements.getAllProgress(coroutineScope)
achievements.getProgressByName(coroutineScope, listOf("REACHED_LEVEL_10", "ENEMIES_DEFEATED"))
```

## Data Types

### `AchievementDefinition` Interface (returned by `getAllDefinitions()` / `getDefinitionsByName()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `type` | `AchievementType` | `AchievementType.Unknown` | The type of achievement (Simple, Count, or Bitfield) |
| `name` | `String` | `""` | The API name of the achievement as set in the developer dashboard |
| `bitfieldLength` | `Long` | `0` | The size of the bitfield (required for BITFIELD achievements) |
| `target` | `ULong` | `0` | The target value to reach for unlocking (for COUNT and BITFIELD) |

### `AchievementProgress` Interface (returned by `getAllProgress()` / `getProgressByName()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `bitfield` | `String?` | `null` | Current bitfield state (for BITFIELD type achievements) |
| `count` | `ULong` | `0` | Current counter value (for COUNT type achievements) |
| `isUnlocked` | `Boolean` | `false` | Whether the user has unlocked this achievement |
| `name` | `String` | `""` | The API name of the achievement |
| `unlockTime` | `LocalDateTime` | -- | When the achievement was unlocked (if unlocked) |

### `AchievementUpdate` Interface (returned by `unlock()` / `addCount()` / `addFields()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `justUnlocked` | `Boolean` | `false` | Whether THIS call newly unlocked the achievement. `false` on a replay of an already-unlocked achievement — a replay is a **success**, not an error (see [Important Notes](#important-notes)) |
| `name` | `String` | `""` | The API name of the updated achievement |

### `AchievementType` Enum

| Value | Code | Description |
|-------|------|-------------|
| `Unknown` | 0 | The achievement type is unknown |
| `Simple` | 1 | Unlocked by a single event or objective completion |
| `Bitfield` | 2 | Unlocked when a target number of bits are set in a bitfield |
| `Count` | 3 | Unlocked when a counter reaches a defined target |

## Error Handling

`AchievementsException` extends `HzPlatformSdkException`. Beyond the common codes (see [common-setup.md](common-setup.md)), `AchievementsException.code` can return these **Achievements domain codes** (`AchievementsStatusCode`, `@Public`, range 2001-2005):

| Code | Name | Meaning |
|------|------|---------|
| 2001 | `INVALID_REQUEST` | A required parameter is missing (e.g. `api_name`) |
| 2002 | `PERMISSIONS_ERROR` | The user lacks permission for the operation |
| 2003 | `INVALID_FIELD_FOR_ACHIEVEMENT_TYPE` | Field invalid for the type (e.g. bitfield on a COUNT achievement) |
| 2004 | `BITFIELD_LENGTH_MISMATCH` | Bitfield length ≠ the achievement definition's length |
| 2005 | `ACHIEVEMENT_NOT_CONFIGURED` | The achievement isn't configured for this app |

> Note: on the **OVR→Evo forwarded** path these domain codes are normalized to OVR codes in the C++ layer; a **native Evo** consumer sees the 2001-2005 codes above.

## Important Notes

1. **Configure first** -- achievements must be configured in the Meta Horizon Developer Dashboard before use. `unlock()` also works on COUNT/BITFIELD types, immediately unlocking them regardless of progress.
2. **`addCount()` clamps to signed 64-bit max** -- `count` is `ULong`, but values above signed 64-bit max are clamped before being sent to the server.
3. **`addFields()`** -- the `fields` string length should match the achievement's configured `bitfieldLength`.
4. **Names are API names** -- all `name` parameters expect the `api_name` (not a display name); retrieve valid names via `getAllDefinitions()` / `getDefinitionsByName()`.
5. **OS requirement** -- requires HzOS v85+. Older OS returns code 1003 (`ProviderOperationNotSupported`). Require a min OS in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.
6. **`justUnlocked` and replays** -- `unlock()` / `addCount()` / `addFields()` all return an `AchievementUpdate` whose `justUnlocked` is `true` only when THIS call crossed the unlock threshold. Re-unlocking (or re-adding to) an already-unlocked achievement **succeeds** with `justUnlocked == false` -- it does NOT throw `AchievementsException`. So gate any "Achievement unlocked!" celebration UI on `justUnlocked == true`, not on the call merely returning without an exception, or it re-fires on every replay.
