# Entitlements API

- **Kotlin Package**: `horizon.platform.entitlements`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-entitlement-check
- **Minimum OS**: HzOS v85
- **Maven Artifact**: `horizon-platform-sdk-entitlements-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Examples](#examples)
- [Important Notes](#important-notes)

## Overview

Part of the Horizon Platform SDK. Single operation:

1. **`getIsViewerEntitled()`** -- Verify that the current user has purchased or otherwise legitimately obtained the app.

Must be called within 10 seconds of app launch. Does not require internet connectivity. If the check fails, the developer is responsible for handling the error (e.g. showing a message and quitting the app).

## API Usage

#### Check Entitlement

```kotlin
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

val entitlements = Entitlements()

try {
    entitlements.getIsViewerEntitled()
    // Reached here => user is entitled; proceed with normal app flow
} catch (e: EntitlementsException) {
    // User is NOT entitled -- handle accordingly (see Error Handling)
}
```

**Return type**: `Void` -- returns nothing on success; throws `EntitlementsException` if the user is not entitled. Success is indicated by the method returning normally without throwing.

## Data Types

The Entitlements API defines no custom data types.

### `EntitlementsException`

Extends `HzPlatformSdkException`. Thrown when the entitlement check fails for any reason. Contains the status code and error message from the platform service.

## Error Handling

`getIsViewerEntitled()` throws `EntitlementsException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes (`EntitlementsStatusCode`)

This API uses only the common status codes. See [common-setup.md](common-setup.md) for the full table. Key cases:
- Status code `3` (`EntitlementFailure`) -- primary failure: user has not purchased the app / has no valid license. Recommended: show a "purchase from the Meta Horizon Store" message, then quit.
- Status code `1003` (`ProviderOperationNotSupported`) -- returned on OS versions below HzOS v85.
- Other status codes indicate infrastructure problems rather than entitlement issues.

## Examples

### Example 1: Basic Entitlement Check at App Launch

Verify the user is entitled immediately after connecting to the platform service.

```kotlin
import horizon.core.android.driver.coroutines.HorizonServiceConnection
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

class MainActivity : ComponentActivity() {
    private val APPLICATION_ID = "<your-app-id>"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        HorizonServiceConnection.connect(
            APPLICATION_ID,
            applicationContext,
            lifecycleScope,
        )

        lifecycleScope.launch {
            val entitlements = Entitlements()
            try {
                entitlements.getIsViewerEntitled()
                // User is entitled -- proceed with normal app flow
                loadMainContent()
            } catch (e: EntitlementsException) {
                // User is NOT entitled -- show error and quit
                showEntitlementError()
                finish()
            }
        }
    }
}
```

### Example 2: Entitlement Check with Retry Logic

Retry a limited number of times on transient errors before giving up.

```kotlin
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException
import kotlinx.coroutines.delay

suspend fun checkEntitlementWithRetry(
    maxRetries: Int = 3,
    delayMs: Long = 1000L,
): Boolean {
    val client = Entitlements()

    repeat(maxRetries) { attempt ->
        try {
            client.getIsViewerEntitled()
            return true
        } catch (e: EntitlementsException) {
            // Only retry on transient errors (internal error, rate limit, network)
            val isTransient = e.message?.let { msg ->
                msg.contains("1") || msg.contains("4") || msg.contains("6")
            } ?: false

            if (!isTransient) {
                // Non-transient (e.g. entitlement failure) -- do not retry
                return false
            }
            if (attempt < maxRetries - 1) {
                delay(delayMs * (attempt + 1)) // Linear backoff
            }
        }
    }
    return false
}
```

## Important Notes

1. **Call within 10 seconds of app launch** -- the Meta Horizon Store requires the check promptly after start. Call it in `onCreate` or as early as possible.
2. **Works offline** -- verifies locally whether the user is authorized; no internet required.
3. **Status code 3 means the user is not entitled** -- the primary failure case; recommended behavior is to show a "purchase from the Meta Horizon Store" message and close the app.
4. **No return value on success** -- `getIsViewerEntitled()` returns `Void`; success = returning normally without throwing.
5. **Requires HzOS v85+** -- older OS versions return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.
6. **Simple request/response API** -- no pagination, events, or sessions; each call is independent and stateless. One method in the entire API surface.
7. **Critical for app store compliance** -- failing to implement the check may result in rejection from or non-compliance flags in the Meta Horizon Store.
