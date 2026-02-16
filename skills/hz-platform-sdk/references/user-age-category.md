# User Age Category API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.useragecategory` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-get-age-category-api |
| **Minimum OS** | HzOS v81 (`get()`), HzOS v85 (`report()`) |
| **Maven Artifact** | `horizon-platform-sdk-user-age-category-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Overview

The User Age Category API provides two operations for Meta Quest Android applications:

1. **`get()`** -- Retrieve the current user's age category from their Meta account
2. **`report()`** -- Report the app's own determination of the user's age category back to Meta

## API Usage

### Retrieve the User's Age Category

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AccountAgeCategory
import horizon.platform.useragecategory.models.UserAccountAgeCategory

val userAgeCategory = UserAgeCategory()

try {
    val result: UserAccountAgeCategory = userAgeCategory.get()

    when (result.ageCategory) {
        AccountAgeCategory.Ch -> {
            // Child: ages 10-12 (or applicable age in user's region)
        }
        AccountAgeCategory.Tn -> {
            // Teen: ages 13-17 (or applicable age in user's region)
        }
        AccountAgeCategory.Ad -> {
            // Adult: ages 18+ (or applicable age in user's region)
        }
        AccountAgeCategory.Unknown -> {
            // Age category could not be determined
            // Treat conservatively -- apply the most restrictive content policy
        }
    }
} catch (e: UserAgeCategoryException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `UserAccountAgeCategory` -- an immutable object with:
- `ageCategory: AccountAgeCategory` -- defaults to `AccountAgeCategory.Unknown`

**Available since**: SDK v0.1.3 (HzOS v81)

### Report the User's Age Category to Meta

Use this when your app independently verifies or determines a user's age group.

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AppAgeCategory

val userAgeCategory = UserAgeCategory()

try {
    // Report user as a child (ages 10-12)
    userAgeCategory.report(AppAgeCategory.Ch)

    // OR report user as non-child (ages 13+)
    userAgeCategory.report(AppAgeCategory.Nch)
} catch (e: UserAgeCategoryException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `ageCategory: AppAgeCategory`
**Returns**: `Unit` -- success is indicated by no exception being thrown

**Available since**: SDK v0.2.1 (HzOS v85)

## Data Types

### `AccountAgeCategory` Enum (returned by `get()`)

Three-tier classification from Meta's account data:

| Value | Code | Age Range | Description |
|-------|------|-----------|-------------|
| `Unknown` | 0 | N/A | Age category could not be determined |
| `Ch` | 1 | 10-12 | Child (or applicable age in user's region) |
| `Tn` | 2 | 13-17 | Teenager (or applicable age in user's region) |
| `Ad` | 3 | 18+ | Adult (or applicable age in user's region) |

### `AppAgeCategory` Enum (parameter for `report()`)

Simplified two-tier classification for app reporting:

| Value | Code | Age Range | Description |
|-------|------|-----------|-------------|
| `Unknown` | 0 | N/A | Unknown |
| `Ch` | 1 | 10-12 | Child (or applicable age in user's region) |
| `Nch` | 2 | 13+ | Non-child (or applicable age in user's region) |

**Key distinction**: `AccountAgeCategory` (from `get()`) has three groups (Child/Teen/Adult), while `AppAgeCategory` (for `report()`) has two groups (Child/Non-Child). The asymmetry is intentional -- apps only need to report whether a user is a child or not.

### `UserAccountAgeCategory` Model

Immutable model returned by `get()`:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ageCategory` | `AccountAgeCategory` | `AccountAgeCategory.Unknown` | The user's age category |

## Error Handling

Both `get()` and `report()` throw `UserAgeCategoryException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

This package does not define any package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Basic Age-Gating

Retrieve the user's age category and restrict content accordingly.

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AccountAgeCategory

suspend fun shouldShowMatureContent(): Boolean {
    val client = UserAgeCategory()
    return try {
        val result = client.get()
        result.ageCategory == AccountAgeCategory.Ad
    } catch (e: UserAgeCategoryException) {
        // On error, default to restricting content
        false
    }
}
```

### Example 2: Repository Pattern with Sealed Result

Wrap the API in a repository for clean architecture.

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.enums.AccountAgeCategory
import horizon.platform.useragecategory.enums.AppAgeCategory

sealed class AgeCategoryResult {
    data class Success(val data: String) : AgeCategoryResult()
    data class Error(val message: String) : AgeCategoryResult()
}

class UserAgeCategoryRepository {
    private val client = UserAgeCategory()

    suspend fun get(): AgeCategoryResult {
        return try {
            val result = client.get()
            val label = when (result.ageCategory) {
                AccountAgeCategory.Ch -> "Child"
                AccountAgeCategory.Tn -> "Teenager"
                AccountAgeCategory.Ad -> "Adult"
                AccountAgeCategory.Unknown -> "Unknown"
            }
            AgeCategoryResult.Success(label)
        } catch (e: Exception) {
            AgeCategoryResult.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun reportChild(): AgeCategoryResult {
        return try {
            client.report(AppAgeCategory.Ch)
            AgeCategoryResult.Success("Reported as child")
        } catch (e: Exception) {
            AgeCategoryResult.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun reportNonChild(): AgeCategoryResult {
        return try {
            client.report(AppAgeCategory.Nch)
            AgeCategoryResult.Success("Reported as non-child")
        } catch (e: Exception) {
            AgeCategoryResult.Error(e.message ?: "Unknown error")
        }
    }
}
```

### Example 3: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class AgeCategoryUiState(
    val ageCategory: String = "",
    val isLoading: Boolean = false,
    val error: String? = null,
)

class AgeCategoryViewModel(
    private val repository: UserAgeCategoryRepository = UserAgeCategoryRepository()
) : ViewModel() {
    private val _uiState = MutableStateFlow(AgeCategoryUiState())
    val uiState: StateFlow<AgeCategoryUiState> = _uiState

    fun fetchAgeCategory() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = repository.get()) {
                is AgeCategoryResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        ageCategory = result.data,
                        isLoading = false,
                    )
                }
                is AgeCategoryResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = result.message,
                    )
                }
            }
        }
    }

    fun reportChild() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = repository.reportChild()) {
                is AgeCategoryResult.Success -> {
                    _uiState.value = _uiState.value.copy(isLoading = false)
                }
                is AgeCategoryResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = result.message,
                    )
                }
            }
        }
    }

    fun reportNonChild() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = repository.reportNonChild()) {
                is AgeCategoryResult.Success -> {
                    _uiState.value = _uiState.value.copy(isLoading = false)
                }
                is AgeCategoryResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = result.message,
                    )
                }
            }
        }
    }
}
```

### Example 4: Handling OS Version Compatibility

Gracefully handle the case where `report()` is not available on older devices.

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AppAgeCategory

suspend fun safeReport(ageCategory: AppAgeCategory): Boolean {
    val client = UserAgeCategory()
    return try {
        client.report(ageCategory)
        true
    } catch (e: UserAgeCategoryException) {
        if (e.message?.contains("1003") == true) {
            // ProviderOperationNotSupported -- OS version too old for report()
            // Log and continue without reporting
            false
        } else {
            throw e
        }
    }
}
```

## Important Notes

1. **Age ranges are region-dependent** -- the exact age boundaries for Child, Teen, and Adult may vary based on the user's region. Do not hardcode specific age thresholds in your app logic.

2. **Handle `Unknown` conservatively** -- if `get()` returns `AccountAgeCategory.Unknown`, apply the most restrictive content policy appropriate for your app.

3. **`report()` requires HzOS v85+** -- on older OS versions, it returns status code 1003 (`ProviderOperationNotSupported`). The `get()` method is available since HzOS v81. You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

4. **No pagination, events, or sessions** -- this is a simple request/response API. Each call is independent and stateless.

5. **Two enum types serve different purposes** -- `AccountAgeCategory` (3 groups: CH/TN/AD) is what Meta tells the app; `AppAgeCategory` (2 groups: CH/NCH) is what the app tells Meta. Do not confuse them.
