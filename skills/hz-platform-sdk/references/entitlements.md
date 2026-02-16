# Entitlements API

- **Kotlin Package**: `horizon.platform.entitlements`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-entitlements
- **Minimum OS**: HzOS v85
- **Maven Artifact**: `horizon-platform-sdk-entitlements-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Overview

The Entitlements API is part of the Horizon Platform SDK. It provides a single operation for Meta Quest Android applications:

1. **`getIsViewerEntitled()`** -- Verify that the current user has purchased or otherwise legitimately obtained the app

The entitlement check is a crucial component of the Meta Quest Store's app verification process. It must be called within 10 seconds of the user launching the app. The check does not require internet connectivity. If the check fails, developers are responsible for handling the error (e.g., displaying an error message and quitting the app).

## API Usage

#### Check Entitlement

```kotlin
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

val entitlements = Entitlements()

try {
    entitlements.getIsViewerEntitled()

    // If we reach here, the user is entitled to the app
    // Proceed with normal app flow

} catch (e: EntitlementsException) {
    // The user is NOT entitled -- handle accordingly
    // See Error Handling section for details
}
```

**Return type**: `Void` -- the method returns nothing on success. If the user is not entitled, it throws an `EntitlementsException`.

**Key behavior:**
- On success (user is entitled): the method returns normally with no value
- On failure (user is not entitled or error occurred): throws `EntitlementsException`
- Must be called within 10 seconds of app launch
- Does not require internet connectivity

## Data Types

The Entitlements API does not define any custom data types. The single method `getIsViewerEntitled()` returns `Void` on success and throws `EntitlementsException` on failure.

### `EntitlementsException`

Extends `HzPlatformSdkException`. Thrown when the entitlement check fails for any reason. Contains the status code and error message from the platform service.

## Error Handling

`getIsViewerEntitled()` throws `EntitlementsException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes (`EntitlementsStatusCode`)

This API uses only the common status codes. See [common-setup.md](common-setup.md) for the full common status codes table.

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

### Example 2: Entitlement Check with Detailed Error Handling

Handle different failure scenarios with specific user-facing messages.

```kotlin
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

sealed class EntitlementResult {
    object Entitled : EntitlementResult()
    data class NotEntitled(val reason: String) : EntitlementResult()
}

suspend fun checkEntitlement(): EntitlementResult {
    val client = Entitlements()
    return try {
        client.getIsViewerEntitled()
        EntitlementResult.Entitled
    } catch (e: EntitlementsException) {
        val reason = when {
            e.message?.contains("2") == true ->
                "Platform not initialized. Please restart the app."
            e.message?.contains("3") == true ->
                "You do not own this app. Please purchase it from the Meta Quest Store."
            e.message?.contains("1003") == true ->
                "Your device software is out of date. Please update your Quest."
            else ->
                "Entitlement check failed: ${e.message}"
        }
        EntitlementResult.NotEntitled(reason)
    }
}
```

### Example 3: Repository Pattern for Entitlement Checks

Wrap the Entitlements API in a repository for clean architecture.

```kotlin
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

class EntitlementRepository {
    private val entitlements = Entitlements()

    suspend fun isUserEntitled(): Boolean {
        return try {
            entitlements.getIsViewerEntitled()
            true
        } catch (e: EntitlementsException) {
            false
        }
    }

    suspend fun verifyEntitlementOrThrow() {
        try {
            entitlements.getIsViewerEntitled()
        } catch (e: EntitlementsException) {
            throw IllegalStateException(
                "Entitlement check failed. The user may not own this app.",
                e
            )
        }
    }
}
```

### Example 4: MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

data class EntitlementUiState(
    val isChecking: Boolean = true,
    val isEntitled: Boolean = false,
    val errorMessage: String? = null,
)

class EntitlementViewModel(
    private val entitlements: Entitlements = Entitlements()
) : ViewModel() {
    private val _uiState = MutableStateFlow(EntitlementUiState())
    val uiState: StateFlow<EntitlementUiState> = _uiState

    fun checkEntitlement() {
        viewModelScope.launch {
            _uiState.value = EntitlementUiState(isChecking = true)
            try {
                entitlements.getIsViewerEntitled()
                _uiState.value = EntitlementUiState(
                    isChecking = false,
                    isEntitled = true,
                )
            } catch (e: EntitlementsException) {
                _uiState.value = EntitlementUiState(
                    isChecking = false,
                    isEntitled = false,
                    errorMessage = "Entitlement check failed: ${e.message}",
                )
            }
        }
    }
}
```

### Example 5: Entitlement Gate with Retry Logic

Retry the entitlement check a limited number of times before giving up.

```kotlin
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException
import kotlinx.coroutines.delay

suspend fun checkEntitlementWithRetry(
    maxRetries: Int = 3,
    delayMs: Long = 1000L,
): Boolean {
    val client = Entitlements()
    var lastException: EntitlementsException? = null

    repeat(maxRetries) { attempt ->
        try {
            client.getIsViewerEntitled()
            return true
        } catch (e: EntitlementsException) {
            lastException = e
            // Only retry on transient errors (internal error, rate limit, network)
            val isTransient = e.message?.let { msg ->
                msg.contains("1") || msg.contains("4") || msg.contains("6")
            } ?: false

            if (!isTransient) {
                // Non-transient error (e.g., entitlement failure) -- do not retry
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

1. **Must be called within 10 seconds of app launch** -- the Meta Quest Store requires the entitlement check to happen promptly after the app starts. Call it in `onCreate` or as early as possible in your app lifecycle.

2. **Does not require internet connectivity** -- the entitlement check works offline. It verifies locally whether the user is authorized to use the app.

3. **Handle failure by quitting the app** -- if the entitlement check fails (status code 3, `EntitlementFailure`), the recommended behavior is to display an error message explaining that the app must be purchased from the Meta Quest Store, then close the app.

4. **Status code 3 means the user is not entitled** -- this is the primary failure case. The user has not purchased the app or does not have a valid license. Other status codes indicate infrastructure problems rather than entitlement issues.

5. **No return value on success** -- unlike most SDK methods, `getIsViewerEntitled()` returns `Void`. Success is indicated by the method returning normally without throwing an exception.

6. **Requires HzOS v85+** -- `getIsViewerEntitled()` requires HzOS v85 or later. On older OS versions, it returns status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

7. **No pagination, events, or sessions** -- this is a simple request/response API. Each call is independent and stateless. There is only one method in the entire API surface.

8. **Critical for app store compliance** -- failing to implement the entitlement check may result in your app being rejected from the Meta Quest Store or being flagged for non-compliance.
