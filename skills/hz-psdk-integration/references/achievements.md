# Achievements API

- **Kotlin Package**: `horizon.platform.achievements`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-achievements
- **Minimum OS**: HzOS v85 (public), HzOS v83 (partner)
- **Maven Artifact**: `horizon-platform-sdk-achievements-kotlin`

## Overview

The Achievements API is part of the Horizon Platform SDK. It enables developers to create engaging experiences by awarding trophies, badges, and awards for reaching goals. The API provides six operations:

1. **`addCount(name, count)`** -- Add a count value to a COUNT achievement
2. **`addFields(name, fields)`** -- Unlock bits in a BITFIELD achievement
3. **`getAllDefinitions(coroutineScope)`** -- Retrieve all achievement definitions for the app
4. **`getAllProgress(coroutineScope)`** -- Retrieve the user's progress on all achievements
5. **`getDefinitionsByName(coroutineScope, names)`** -- Retrieve specific achievement definitions by API name
6. **`getProgressByName(coroutineScope, names)`** -- Retrieve the user's progress on specific achievements by API name
7. **`unlock(name)`** -- Unlock an achievement of any type (simple, count, or bitfield)

The Meta Quest Platform supports three types of achievements:
- **Simple** -- All-or-nothing; unlocked by a single event or objective completion
- **Count** -- Unlocked when a counter reaches a defined target
- **Bitfield** -- Unlocked when a target number of bits in a bitfield are set

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

#### Unlock a Simple Achievement

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementUpdate

val achievements = Achievements()

try {
    val result: AchievementUpdate = achievements.unlock("REACHED_LEVEL_10")

    val didJustUnlock = result.justUnlocked  // true if this call triggered the unlock
    val achievementName = result.name        // API name of the achievement

} catch (e: AchievementsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**: `name: String` -- The `api_name` of the achievement as configured in the developer dashboard
**Return type**: `AchievementUpdate` -- indicates whether the achievement was just unlocked

#### Add Count to a COUNT Achievement

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementUpdate

val achievements = Achievements()

try {
    val result: AchievementUpdate = achievements.addCount("ENEMIES_DEFEATED", 5uL)

    val didJustUnlock = result.justUnlocked  // true if count reached the target
    val achievementName = result.name

} catch (e: AchievementsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `name: String` -- The `api_name` of the COUNT achievement
- `count: ULong` -- The value to add to the counter (max: signed 64-bit integer max value)

**Return type**: `AchievementUpdate`

#### Unlock Fields of a BITFIELD Achievement

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementUpdate

val achievements = Achievements()

try {
    // Unlock the 1st and 3rd fields of a bitfield achievement
    val result: AchievementUpdate = achievements.addFields("COLLECT_ALL_ITEMS", "101")

    val didJustUnlock = result.justUnlocked  // true if target bits reached
    val achievementName = result.name

} catch (e: AchievementsException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `name: String` -- The `api_name` of the BITFIELD achievement
- `fields: String` -- A string of '0' and '1' characters; each '1' unlocks the bit at that position

**Return type**: `AchievementUpdate`

#### Retrieve All Achievement Definitions

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementDefinition
import horizon.core.android.common.pagination.PagedResults
import kotlinx.coroutines.CoroutineScope

val achievements = Achievements()

val pagedResults: PagedResults<AchievementDefinition> =
    achievements.getAllDefinitions(coroutineScope)

// Iterate through pages
pagedResults.collect { definitions: List<AchievementDefinition> ->
    for (def in definitions) {
        val name = def.name                // API name
        val type = def.type                // AchievementType enum
        val target = def.target            // Target value for count/bitfield
        val bitfieldLength = def.bitfieldLength  // Bitfield size (for BITFIELD type)
    }
}
```

**Parameters**: `coroutineScope: CoroutineScope` -- The coroutine scope for pagination
**Return type**: `PagedResults<AchievementDefinition>` -- a paginated result set

#### Retrieve All Achievement Progress

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementProgress
import horizon.core.android.common.pagination.PagedResults

val achievements = Achievements()

val pagedResults: PagedResults<AchievementProgress> =
    achievements.getAllProgress(coroutineScope)

pagedResults.collect { progressList: List<AchievementProgress> ->
    for (progress in progressList) {
        val name = progress.name              // API name
        val isUnlocked = progress.isUnlocked  // Whether achievement is unlocked
        val count = progress.count            // Current count (for COUNT type)
        val bitfield = progress.bitfield      // Current bitfield state (for BITFIELD type)
        val unlockTime = progress.unlockTime  // When it was unlocked (LocalDateTime)
    }
}
```

**Return type**: `PagedResults<AchievementProgress>` -- a paginated result set

#### Retrieve Definitions by Name

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.models.AchievementDefinition
import horizon.core.android.common.pagination.PagedResults

val achievements = Achievements()

val pagedResults: PagedResults<AchievementDefinition> =
    achievements.getDefinitionsByName(
        coroutineScope,
        listOf("REACHED_LEVEL_10", "ENEMIES_DEFEATED", "COLLECT_ALL_ITEMS")
    )

pagedResults.collect { definitions ->
    for (def in definitions) {
        println("${def.name}: type=${def.type}, target=${def.target}")
    }
}
```

**Parameters**:
- `coroutineScope: CoroutineScope` -- The coroutine scope for pagination
- `names: List<String>` -- The `api_names` of the achievements to retrieve

#### Retrieve Progress by Name

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.models.AchievementProgress
import horizon.core.android.common.pagination.PagedResults

val achievements = Achievements()

val pagedResults: PagedResults<AchievementProgress> =
    achievements.getProgressByName(
        coroutineScope,
        listOf("REACHED_LEVEL_10", "ENEMIES_DEFEATED")
    )

pagedResults.collect { progressList ->
    for (progress in progressList) {
        println("${progress.name}: unlocked=${progress.isUnlocked}, count=${progress.count}")
    }
}
```

**Parameters**:
- `coroutineScope: CoroutineScope` -- The coroutine scope for pagination
- `names: List<String>` -- The `api_names` of the achievements to retrieve progress for

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
| `justUnlocked` | `Boolean` | `false` | Whether this update caused the achievement to unlock |
| `name` | `String` | `""` | The API name of the updated achievement |

### `AchievementType` Enum

| Value | Code | Description |
|-------|------|-------------|
| `Unknown` | 0 | The achievement type is unknown |
| `Simple` | 1 | Unlocked by a single event or objective completion |
| `Bitfield` | 2 | Unlocked when a target number of bits are set in a bitfield |
| `Count` | 3 | Unlocked when a counter reaches a defined target |

## Error Handling

All methods throw `AchievementsException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

This package does not define package-specific status codes beyond the common ones. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Unlock a Simple Achievement on Game Event

Unlock a simple achievement when the player completes a specific milestone.

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException

suspend fun onPlayerReachedGoal(goalName: String) {
    val achievements = Achievements()
    try {
        val result = achievements.unlock(goalName)
        if (result.justUnlocked) {
            // Show unlock notification to the user
            showAchievementUnlockedUI(result.name)
        }
    } catch (e: AchievementsException) {
        // Log error, do not block gameplay
        Log.e("Achievements", "Failed to unlock $goalName: ${e.message}")
    }
}
```

### Example 2: Incrementing a Count Achievement

Track incremental progress toward a count-based achievement.

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException

suspend fun onEnemyDefeated(enemiesDefeatedThisSession: Int) {
    val achievements = Achievements()
    try {
        val result = achievements.addCount(
            "ENEMIES_DEFEATED",
            enemiesDefeatedThisSession.toULong()
        )
        if (result.justUnlocked) {
            showAchievementUnlockedUI("Enemies Defeated")
        }
    } catch (e: AchievementsException) {
        Log.e("Achievements", "Failed to add count: ${e.message}")
    }
}
```

### Example 3: Display All Achievements with Progress

Fetch definitions and progress, then combine them for display.

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.models.AchievementDefinition
import horizon.platform.achievements.models.AchievementProgress
import horizon.platform.achievements.enums.AchievementType
import kotlinx.coroutines.CoroutineScope

data class AchievementDisplayItem(
    val name: String,
    val type: AchievementType,
    val isUnlocked: Boolean,
    val progressText: String,
)

suspend fun loadAchievementsForDisplay(
    coroutineScope: CoroutineScope
): List<AchievementDisplayItem> {
    val achievements = Achievements()
    val definitions = mutableListOf<AchievementDefinition>()
    val progressMap = mutableMapOf<String, AchievementProgress>()

    // Collect all definitions
    achievements.getAllDefinitions(coroutineScope).collect { page ->
        definitions.addAll(page)
    }

    // Collect all progress
    achievements.getAllProgress(coroutineScope).collect { page ->
        for (p in page) {
            progressMap[p.name] = p
        }
    }

    // Combine definitions with progress
    return definitions.map { def ->
        val progress = progressMap[def.name]
        val progressText = when (def.type) {
            AchievementType.Simple ->
                if (progress?.isUnlocked == true) "Unlocked" else "Locked"
            AchievementType.Count ->
                "${progress?.count ?: 0u} / ${def.target}"
            AchievementType.Bitfield ->
                "${progress?.bitfield ?: "0".repeat(def.bitfieldLength.toInt())} (target: ${def.target})"
            AchievementType.Unknown -> "Unknown"
        }
        AchievementDisplayItem(
            name = def.name,
            type = def.type,
            isUnlocked = progress?.isUnlocked == true,
            progressText = progressText,
        )
    }
}
```

### Example 4: Repository Pattern with Error Handling

Wrap the Achievements API in a repository for clean architecture.

```kotlin
import horizon.platform.achievements.Achievements
import horizon.platform.achievements.AchievementsException
import horizon.platform.achievements.models.AchievementDefinition
import horizon.platform.achievements.models.AchievementProgress
import horizon.platform.achievements.models.AchievementUpdate
import kotlinx.coroutines.CoroutineScope

sealed class AchievementResult<out T> {
    data class Success<T>(val data: T) : AchievementResult<T>()
    data class Error(val message: String, val statusCode: Int) : AchievementResult<Nothing>()
}

class AchievementsRepository {
    private val achievements = Achievements()

    suspend fun unlock(name: String): AchievementResult<AchievementUpdate> {
        return try {
            val result = achievements.unlock(name)
            AchievementResult.Success(result)
        } catch (e: AchievementsException) {
            AchievementResult.Error(
                e.message ?: "Failed to unlock achievement",
                e.statusCode
            )
        }
    }

    suspend fun addCount(name: String, count: ULong): AchievementResult<AchievementUpdate> {
        return try {
            val result = achievements.addCount(name, count)
            AchievementResult.Success(result)
        } catch (e: AchievementsException) {
            AchievementResult.Error(
                e.message ?: "Failed to add count",
                e.statusCode
            )
        }
    }

    suspend fun addFields(name: String, fields: String): AchievementResult<AchievementUpdate> {
        return try {
            val result = achievements.addFields(name, fields)
            AchievementResult.Success(result)
        } catch (e: AchievementsException) {
            AchievementResult.Error(
                e.message ?: "Failed to add fields",
                e.statusCode
            )
        }
    }

    suspend fun getAllDefinitions(
        coroutineScope: CoroutineScope
    ): AchievementResult<List<AchievementDefinition>> {
        return try {
            val allDefs = mutableListOf<AchievementDefinition>()
            achievements.getAllDefinitions(coroutineScope).collect { page ->
                allDefs.addAll(page)
            }
            AchievementResult.Success(allDefs)
        } catch (e: AchievementsException) {
            AchievementResult.Error(
                e.message ?: "Failed to get definitions",
                e.statusCode
            )
        }
    }

    suspend fun getAllProgress(
        coroutineScope: CoroutineScope
    ): AchievementResult<List<AchievementProgress>> {
        return try {
            val allProgress = mutableListOf<AchievementProgress>()
            achievements.getAllProgress(coroutineScope).collect { page ->
                allProgress.addAll(page)
            }
            AchievementResult.Success(allProgress)
        } catch (e: AchievementsException) {
            AchievementResult.Error(
                e.message ?: "Failed to get progress",
                e.statusCode
            )
        }
    }
}
```

### Example 5: MVVM ViewModel for Achievements Screen

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class AchievementsUiState(
    val achievements: List<AchievementDisplayItem> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null,
)

class AchievementsViewModel(
    private val repository: AchievementsRepository = AchievementsRepository()
) : ViewModel() {
    private val _uiState = MutableStateFlow(AchievementsUiState())
    val uiState: StateFlow<AchievementsUiState> = _uiState

    fun loadAchievements() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)

            val defsResult = repository.getAllDefinitions(viewModelScope)
            val progressResult = repository.getAllProgress(viewModelScope)

            if (defsResult is AchievementResult.Success && progressResult is AchievementResult.Success) {
                val progressMap = progressResult.data.associateBy { it.name }
                val items = defsResult.data.map { def ->
                    val progress = progressMap[def.name]
                    AchievementDisplayItem(
                        name = def.name,
                        type = def.type,
                        isUnlocked = progress?.isUnlocked == true,
                        progressText = formatProgress(def, progress),
                    )
                }
                _uiState.value = _uiState.value.copy(
                    achievements = items,
                    isLoading = false,
                )
            } else {
                val errorMsg = when {
                    defsResult is AchievementResult.Error -> defsResult.message
                    progressResult is AchievementResult.Error -> progressResult.message
                    else -> "Unknown error"
                }
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = errorMsg,
                )
            }
        }
    }

    fun unlockAchievement(name: String) {
        viewModelScope.launch {
            when (val result = repository.unlock(name)) {
                is AchievementResult.Success -> {
                    if (result.data.justUnlocked) {
                        // Refresh the list to show updated state
                        loadAchievements()
                    }
                }
                is AchievementResult.Error -> {
                    _uiState.value = _uiState.value.copy(error = result.message)
                }
            }
        }
    }

    private fun formatProgress(
        def: AchievementDefinition,
        progress: AchievementProgress?
    ): String {
        return when (def.type) {
            AchievementType.Simple ->
                if (progress?.isUnlocked == true) "Unlocked" else "Locked"
            AchievementType.Count ->
                "${progress?.count ?: 0u} / ${def.target}"
            AchievementType.Bitfield ->
                "${progress?.bitfield ?: "0".repeat(def.bitfieldLength.toInt())} (target: ${def.target})"
            AchievementType.Unknown -> "Unknown"
        }
    }
}
```

## Important Notes

1. **`getAllDefinitions()`, `getAllProgress()`, `getDefinitionsByName()`, and `getProgressByName()` return `PagedResults`** -- these are not suspend functions. They take a `CoroutineScope` parameter and return a `PagedResults<T>` object. Use `.collect { }` to iterate through pages. Pagination is handled automatically.

2. **Three achievement types** -- achievements must be configured in the Meta Quest Developer Dashboard before they can be used. Each type has a different unlock mechanism: `Simple` uses `unlock()`, `Count` uses `addCount()`, and `Bitfield` uses `addFields()`. You can also use `unlock()` directly on COUNT and BITFIELD achievements to immediately unlock them regardless of progress.

3. **`addCount()` clamps to signed 64-bit max** -- the `count` parameter is `ULong`, but the largest supported value is the max of a signed 64-bit integer. Values larger than that are clamped before being sent to the server.

4. **`addFields()` uses a binary string** -- the `fields` parameter is a string of '0' and '1' characters. Each '1' at position N unlocks the bit at position N. The string length should match the `bitfieldLength` configured for the achievement.

5. **Achievement names are API names** -- all methods that accept a `name` parameter expect the `api_name` as configured in the developer dashboard, not a display name. Retrieve valid API names via `getAllDefinitions()` or `getDefinitionsByName()`.

6. **Requires HzOS v85+ (public) or v83+ (partner)** -- on older OS versions, API calls return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

7. **No package-specific status codes** -- the Achievements API uses only common status codes (0-6, 190, 1001-1005). There are no achievement-specific error codes in the 2001+ range.
