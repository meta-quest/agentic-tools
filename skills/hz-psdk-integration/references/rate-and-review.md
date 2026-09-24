# Rate and Review API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.rateandreview` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-sdk-sample-rateandreview |
| **Minimum OS** | HzOS v201 |
| **Maven Artifact** | `horizon-platform-sdk-rate-and-review-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Examples](#examples)
- [Important Notes](#important-notes)

## Overview

Two operations:

1. **`canLaunchRateAndReview()`** -- Check whether the current user is eligible to be shown the rating and review UI
2. **`rateAndReviewLauncher()`** -- Launch the system UI for soliciting a rating and review from the user

Use the eligibility check to conditionally show a "Rate this app" button or trigger only when the platform confirms the user can submit a review. The launcher opens a system-managed UI overlay.

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
        // Eligible -- show "Rate this app" button or launch the review UI
    } else {
        // Not eligible -- hide the rating prompt
    }

} catch (e: RateAndReviewException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `ApplicationCanViewerRateAndReview` -- immutable object containing:
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

**Return type**: `Unit` (Void) -- launches a system UI overlay and returns nothing on success.

## Data Types

### `ApplicationCanViewerRateAndReview` Model (returned by `canLaunchRateAndReview()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `canViewerRateAndReview` | `Boolean` | `false` | Whether the user is eligible to launch the rating and review UI |

## Error Handling

Both `canLaunchRateAndReview()` and `rateAndReviewLauncher()` throw `RateAndReviewException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

This package defines no package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Basic Eligibility Check

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

suspend fun checkRatingEligibility(): Boolean {
    val client = RateAndReview()
    return try {
        client.canLaunchRateAndReview().canViewerRateAndReview
    } catch (e: RateAndReviewException) {
        false
    }
}
```

### Example 2: Conditional Rating Prompt with Error Handling

Only launch the rating UI if the user is eligible.

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

### Example 3: Rate After Session with Eligibility Gate

Prompt after a meaningful session, checking eligibility first.

```kotlin
import horizon.platform.rateandreview.RateAndReview
import horizon.platform.rateandreview.RateAndReviewException

suspend fun promptRatingAfterSession(sessionCount: Int, minSessions: Int = 3) {
    if (sessionCount < minSessions) return

    val client = RateAndReview()
    try {
        val eligibility = client.canLaunchRateAndReview()
        if (!eligibility.canViewerRateAndReview) return  // already rated, or restricted
        client.rateAndReviewLauncher()
    } catch (e: RateAndReviewException) {
        // Silently fail -- rating prompts should not interrupt the user experience
    }
}
```

## Important Notes

1. **Requires HzOS v201+** -- on older OS versions both methods return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.

2. **Always check eligibility before launching** -- call `canLaunchRateAndReview()` before `rateAndReviewLauncher()`. The platform may restrict prompts based on user history, rate limits, or other policies.

3. **`rateAndReviewLauncher()` opens a system UI** -- a system-managed overlay. Your app does not control the UI or receive the rating result directly.

4. **No return value from the launcher** -- `rateAndReviewLauncher()` returns `Unit`; you cannot determine the rating given or whether the user dismissed the dialog.

5. **Do not spam rating prompts** -- prompt at natural break points (completing a level, finishing a session, achieving a milestone), not on app launch or during active gameplay.
