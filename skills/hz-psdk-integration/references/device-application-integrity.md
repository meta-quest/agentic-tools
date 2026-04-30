# Device Application Integrity API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.deviceapplicationintegrity` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-device-application-integrity |
| **Minimum OS** | HzOS v85 |
| **Maven Artifact** | `horizon-platform-sdk-device-application-integrity-kotlin` |

> For initial setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Overview

The Device Application Integrity API provides a single operation for Meta Quest Android applications:

1. **`getIntegrityToken(challengeNonce)`** -- Obtain a signed JSON Web Token (JWT) that attests to the integrity of both the device and the application

The returned JWT contains a header, claims, and signature encoded in base64. The header specifies the algorithm type (PS256) and token type (JWT). Your backend server should verify this token to confirm the application and device have not been tampered with.

## API Usage

#### Obtain an Integrity Attestation Token

```kotlin
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrity
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrityException

val deviceAppIntegrity = DeviceApplicationIntegrity()

try {
    // Generate a unique nonce for this attestation request
    val nonce = UUID.randomUUID().toString()
    val token: String = deviceAppIntegrity.getIntegrityToken(nonce)

    // The token is a JWT in the format: header.claims.signature (base64-encoded)
    // Send this token to your backend server for verification

} catch (e: DeviceApplicationIntegrityException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `challengeNonce: String` -- A unique nonce value used to generate the attestation token, ensuring uniqueness and preventing replay attacks
**Return type**: `String` -- A signed JWT (JSON Web Token) in the format `header.claims.signature`, base64-encoded

## Data Types

This API uses simple types with no custom data models:

| Element | Type | Description |
|---------|------|-------------|
| `challengeNonce` (input) | `String` | Unique nonce for the attestation request |
| Return value | `String` | Signed JWT attestation token |

### JWT Token Structure

The returned token is a standard JWT with three base64-encoded segments separated by dots:

| Segment | Description |
|---------|-------------|
| Header | Contains algorithm type (`PS256`) and token type (`JWT`) |
| Claims | Contains attestation claims about device and application integrity |
| Signature | Cryptographic signature for verification |

## Error Handling

`getIntegrityToken()` throws `DeviceApplicationIntegrityException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes (`DeviceApplicationIntegrityStatusCode`)

This package does not define any package-specific status codes beyond the common ones. For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example 1: Basic Integrity Check

Obtain an attestation token and send it to your backend.

```kotlin
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrity
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrityException
import java.util.UUID

suspend fun getIntegrityToken(): String? {
    val client = DeviceApplicationIntegrity()
    return try {
        val nonce = UUID.randomUUID().toString()
        client.getIntegrityToken(nonce)
    } catch (e: DeviceApplicationIntegrityException) {
        Log.e("IntegrityCheck", "Failed: ${e.message}")
        null
    }
}
```

### Example 2: Server-Side Verification Flow

Obtain the token locally and send it to your backend for verification.

```kotlin
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrity
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrityException
import java.util.UUID

sealed class IntegrityResult {
    data class Verified(val token: String, val nonce: String) : IntegrityResult()
    data class Failed(val reason: String) : IntegrityResult()
}

suspend fun verifyIntegrity(): IntegrityResult {
    val client = DeviceApplicationIntegrity()
    val nonce = UUID.randomUUID().toString()

    return try {
        val token = client.getIntegrityToken(nonce)
        // Send token and nonce to your backend server for verification
        // The server should:
        // 1. Verify the JWT signature using Meta's public key
        // 2. Validate that the nonce in the claims matches the one sent
        // 3. Check the integrity claims in the token payload
        IntegrityResult.Verified(token, nonce)
    } catch (e: DeviceApplicationIntegrityException) {
        IntegrityResult.Failed(e.message ?: "Unknown integrity check failure")
    }
}
```

### Example 3: Retry with Exponential Backoff

Handle transient errors with automatic retries.

```kotlin
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrity
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrityException
import kotlinx.coroutines.delay
import java.util.UUID

suspend fun getIntegrityTokenWithRetry(
    maxRetries: Int = 3,
    initialDelayMs: Long = 1000L,
): String {
    val client = DeviceApplicationIntegrity()
    var lastException: DeviceApplicationIntegrityException? = null

    repeat(maxRetries) { attempt ->
        try {
            val nonce = UUID.randomUUID().toString()
            return client.getIntegrityToken(nonce)
        } catch (e: DeviceApplicationIntegrityException) {
            lastException = e
            // Only retry on transient errors
            val isRetryable = e.message?.let { msg ->
                msg.contains("1") || // InternalError
                msg.contains("4") || // RateLimitExceeded
                msg.contains("6") || // NetworkUnavailable
                msg.contains("1005") // ProviderGraphApiError
            } ?: false

            if (!isRetryable) throw e

            val delayMs = initialDelayMs * (1L shl attempt)
            delay(delayMs)
        }
    }

    throw lastException ?: IllegalStateException("Retry exhausted")
}
```

### Example 4: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import java.util.UUID

data class IntegrityUiState(
    val token: String? = null,
    val isLoading: Boolean = false,
    val error: String? = null,
    val isVerified: Boolean = false,
)

class IntegrityViewModel(
    private val client: DeviceApplicationIntegrity = DeviceApplicationIntegrity()
) : ViewModel() {
    private val _uiState = MutableStateFlow(IntegrityUiState())
    val uiState: StateFlow<IntegrityUiState> = _uiState

    fun checkIntegrity() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val nonce = UUID.randomUUID().toString()
                val token = client.getIntegrityToken(nonce)
                _uiState.value = _uiState.value.copy(
                    token = token,
                    isLoading = false,
                    isVerified = true,
                )
                // In production, send the token to your backend for verification
            } catch (e: DeviceApplicationIntegrityException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Integrity check failed",
                    isVerified = false,
                )
            }
        }
    }
}
```

### Example 5: Guarding a Sensitive Operation

Use integrity verification as a gate before performing a sensitive action.

```kotlin
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrity
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrityException
import java.util.UUID

class SecureActionExecutor(
    private val client: DeviceApplicationIntegrity = DeviceApplicationIntegrity(),
) {
    suspend fun <T> executeWithIntegrityCheck(action: suspend (integrityToken: String) -> T): T {
        val nonce = UUID.randomUUID().toString()
        val token = try {
            client.getIntegrityToken(nonce)
        } catch (e: DeviceApplicationIntegrityException) {
            throw SecurityException("Device integrity check failed: ${e.message}", e)
        }

        // Pass the token to the action so it can be forwarded to the backend
        return action(token)
    }
}

// Usage:
// val executor = SecureActionExecutor()
// val purchaseResult = executor.executeWithIntegrityCheck { token ->
//     backendApi.makePurchase(itemId, token)
// }
```

## Important Notes

1. **Use a unique nonce for every request** -- the `challengeNonce` parameter should be a unique, unpredictable value (e.g., `UUID.randomUUID().toString()`) for each attestation request. Reusing nonces makes the system vulnerable to replay attacks.

2. **Verify tokens on your backend server** -- the returned JWT should be sent to your backend server for verification. Do not trust the token locally on the device. The server should verify the JWT signature using Meta's public key, validate the nonce matches, and inspect the integrity claims.

3. **No custom data types** -- this is a simple string-in, string-out API. The input is a nonce string and the output is a JWT string. There are no custom models, enums, or options to manage.

4. **No package-specific status codes** -- this API uses only the common Platform SDK status codes (0-6, 190, 1001-1005). All error handling follows the standard pattern.

5. **Requires HzOS v85+** -- `getIntegrityToken()` requires HzOS v85 or later. On older OS versions, it returns status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

6. **Rate limiting** -- avoid calling `getIntegrityToken()` excessively. Use it at critical checkpoints (e.g., app launch, before purchases, before accessing sensitive content) rather than on every API call. If rate limited, you receive status code 4 (`RateLimitExceeded`).

7. **No pagination, events, or sessions** -- this is a simple request/response API. Each call is independent and stateless.
