# Abuse Report API

- **Kotlin Package**: `horizon.platform.abusereport`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-abuse-report
- **Minimum OS**: HzOS v85
- **Maven Artifact**: `horizon-platform-sdk-abuse-report-kotlin`

## Overview

The Abuse Report API is part of the Horizon Platform SDK. It provides one event for Meta Quest Android applications:

1. **`reportButtonPressed()`** -- Listen for the event when a user taps the report button in the system panel

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## API Usage

#### Listen for Report Button Press Events

Subscribe to the event fired when a user taps the report button in the system panel (after pressing the Oculus button).

```kotlin
import horizon.platform.abusereport.AbuseReport
import horizon.platform.abusereport.AbuseReportException

val abuseReport = AbuseReport()

abuseReport.reportButtonPressed().collect { reportId: String ->
    // The user pressed the report button
    // reportId contains the ID of the filed report
    // Launch your in-app reporting flow here
}
```

**Return type**: `Flow<String>` -- a Kotlin Flow that emits the report ID when the report button is pressed

## Error Handling

All methods throw `AbuseReportException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example 1: Listening for Report Button Events

Set up a listener for the system report button.

```kotlin
import horizon.platform.abusereport.AbuseReport
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.launch

fun setupReportButtonListener(scope: CoroutineScope) {
    val client = AbuseReport()

    scope.launch {
        client.reportButtonPressed().collect { reportId ->
            // User pressed the report button in the system panel
            // Show your in-app reporting UI
            showInAppReportingUI(reportId)
        }
    }
}

private fun showInAppReportingUI(reportId: String) {
    // Your in-app reporting UI implementation
}
```

## Important Notes

1. **`reportButtonPressed()` returns a Flow** -- it emits events when the user taps the report button in the system panel. Collect this Flow in a lifecycle-aware scope to avoid leaks.

2. **Requires HzOS v85+** -- all operations require HzOS v85 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`).
