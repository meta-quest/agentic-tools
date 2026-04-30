# Rate and Review API

- **Kotlin Package**: `horizon.platform.rateandreview`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-rate-and-review
- **Minimum OS**: HzOS v201
- **Maven Artifact**: `horizon-platform-sdk-rate-and-review-kotlin`

## Overview

The Rate and Review API is part of the Horizon Platform SDK. It provides two operations for Meta Quest Android applications:

1. **`canLaunchRateAndReview()`** -- Check whether the current user is eligible to be shown the rating and review UI
2. **`rateAndReviewLauncher()`** -- Launch the system UI for soliciting a rating and review from the user

The eligibility check allows apps to conditionally show a "Rate this app" button or trigger only when the platform confirms the user can submit a review. The launcher opens a system-managed UI overlay where the user can rate and review the app.

> **Setup**: For dependency installation, service connection initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

#### Check Eligibility to Launch Rating UI

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException
import horizon.platform.rateandreview.models.ApplicationCanViewerRateAndReview

val rateAndReview = RateAndReview()

try {
    val result: ApplicationCanViewerRateAndReview = rateAndReview.canLaunchRateAndReview()

    if (result.canViewerRateAndReview) {
        // User is eligible -- show "Rate this app" button or launch the review UI
    } else {
        // User is not eligible -- hide the rating prompt
    }

} catch (e: RateAndReviewException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `ApplicationCanViewerRateAndReview` -- an immutable object containing the eligibility flag.

**Key properties of `ApplicationCanViewerRateAndReview`**:
- `canViewerRateAndReview: Boolean` -- Whether the user is eligible to launch the rating and review UI

#### Launch the Rating and Review UI

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

val rateAndReview = RateAndReview()

try {
    rateAndReview.rateAndReviewLauncher()
    // The system rating UI has been launched successfully

} catch (e: RateAndReviewException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `Unit` (Void) -- this method launches a system UI overlay and returns nothing on success.

## Data Types

### `ApplicationCanViewerRateAndReview` Model (returned by `canLaunchRateAndReview()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `canViewerRateAndReview` | `Boolean` | `false` | Whether the user is eligible to launch the rating and review UI |

## Error Handling

Both `canLaunchRateAndReview()` and `rateAndReviewLauncher()` throw `RateAndReviewException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes

This package does not define any package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Basic Eligibility Check

Check if the user can rate the app and log the result.

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

suspend fun checkRatingEligibility(): Boolean {
    val client = RateAndReview()
    return try {
        val result = client.canLaunchRateAndReview()
        result.canViewerRateAndReview
    } catch (e: RateAndReviewException) {
        false
    }
}
```

### Example 2: Conditional Rating Prompt with Error Handling

Only launch the rating UI if the user is eligible, and handle specific error cases.

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

sealed class RateAndReviewResult {
    data object Launched : RateAndReviewResult()
    data object NotEligible : RateAndReviewResult()
    data class Error(val message: String) : RateAndReviewResult()
}

suspend fun promptForRating(): RateAndReviewResult {
    val client = RateAndReview()
    return try {
        val eligibility = client.canLaunchRateAndReview()
        if (eligibility.canViewerRateAndReview) {
            client.rateAndReviewLauncher()
            RateAndReviewResult.Launched
        } else {
            RateAndReviewResult.NotEligible
        }
    } catch (e: RateAndReviewException) {
        RateAndReviewResult.Error(e.message ?: "Unknown error")
    }
}
```

### Example 3: Repository Pattern

Wrap the Rate and Review API in a repository for clean architecture.

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

sealed class RateReviewResult<out T> {
    data class Success<T>(val data: T) : RateReviewResult<T>()
    data class Error(val message: String) : RateReviewResult<Nothing>()
}

class RateAndReviewRepository {
    private val client = RateAndReview()

    suspend fun canLaunchRateAndReview(): RateReviewResult<Boolean> {
        return try {
            val result = client.canLaunchRateAndReview()
            RateReviewResult.Success(result.canViewerRateAndReview)
        } catch (e: RateAndReviewException) {
            RateReviewResult.Error(e.message ?: "Failed to check rating eligibility")
        }
    }

    suspend fun launchRateAndReview(): RateReviewResult<Unit> {
        return try {
            client.rateAndReviewLauncher()
            RateReviewResult.Success(Unit)
        } catch (e: RateAndReviewException) {
            RateReviewResult.Error(e.message ?: "Failed to launch rating UI")
        }
    }
}
```

### Example 4: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class RateAndReviewUiState(
    val isEligible: Boolean = false,
    val isLoading: Boolean = false,
    val hasLaunched: Boolean = false,
    val error: String? = null,
)

class RateAndReviewViewModel(
    private val repository: RateAndReviewRepository = RateAndReviewRepository()
) : ViewModel() {
    private val _uiState = MutableStateFlow(RateAndReviewUiState())
    val uiState: StateFlow<RateAndReviewUiState> = _uiState

    fun checkEligibility() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = repository.canLaunchRateAndReview()) {
                is RateReviewResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isEligible = result.data,
                        isLoading = false,
                    )
                }
                is RateReviewResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = result.message,
                    )
                }
            }
        }
    }

    fun launchRating() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = repository.launchRateAndReview()) {
                is RateReviewResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        hasLaunched = true,
                    )
                }
                is RateReviewResult.Error -> {
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

### Example 5: Rate After Session with Eligibility Gate

Prompt the user to rate the app after completing a meaningful session, checking eligibility first.

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

suspend fun promptRatingAfterSession(sessionCount: Int, minSessions: Int = 3) {
    // Only prompt after the user has completed enough sessions
    if (sessionCount < minSessions) return

    val client = RateAndReview()
    try {
        val eligibility = client.canLaunchRateAndReview()
        if (!eligibility.canViewerRateAndReview) {
            // User is not eligible (may have already rated, or platform restrictions apply)
            return
        }

        // User is eligible -- launch the rating UI
        client.rateAndReviewLauncher()
    } catch (e: RateAndReviewException) {
        // Silently fail -- rating prompts should not interrupt the user experience
    }
}
```

## Important Notes

1. **Requires HzOS v201+** -- both `canLaunchRateAndReview()` and `rateAndReviewLauncher()` require HzOS v201 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

2. **Always check eligibility before launching** -- call `canLaunchRateAndReview()` before `rateAndReviewLauncher()` to verify the user is eligible. The platform may restrict prompts based on user history, rate limits, or other policies.

3. **`rateAndReviewLauncher()` opens a system UI** -- this method launches a system-managed overlay. Your app does not control the UI or receive the rating result directly. The system handles the full rating and review flow.

4. **No return value from the launcher** -- `rateAndReviewLauncher()` returns `Unit`. You cannot determine what rating the user gave or whether they dismissed the dialog. Design your flow accordingly.

5. **Do not spam rating prompts** -- follow platform best practices for when to solicit ratings. Prompt at natural break points (after completing a level, finishing a session, achieving a milestone) rather than immediately on app launch or during active gameplay.

6. **No pagination, events, or sessions** -- this is a simple request/response API. Each call is independent and stateless. There are no Flow-based event streams or paginated results.
