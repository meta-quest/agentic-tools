# Device Application Integrity API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.deviceapplicationintegrity` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-sdk-sample-deviceintegrity |
| **Minimum OS** | HzOS v85 |
| **Maven Artifact** | `horizon-platform-sdk-device-application-integrity-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Examples](#examples)
- [Important Notes](#important-notes)

## Overview

A single operation:

1. **`getIntegrityToken(challengeNonce)`** -- Obtain a signed JSON Web Token (JWT) that attests to the integrity of both the device and the application

The returned JWT contains a header, claims, and signature encoded in base64. The header specifies the algorithm type (PS256) and token type (JWT). Verify this token on your backend server to confirm the application and device have not been tampered with.

## API Usage

#### Obtain an Integrity Attestation Token

```kotlin
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrity
import horizon.platform.deviceapplicationintegrity.DeviceApplicationIntegrityException
import java.util.UUID

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
**Return type**: `String` -- A signed JWT in the format `header.claims.signature`, base64-encoded

## Data Types

Simple types, no custom data models:

| Element | Type | Description |
|---------|------|-------------|
| `challengeNonce` (input) | `String` | Unique nonce for the attestation request |
| Return value | `String` | Signed JWT attestation token |

### JWT Token Structure

Standard JWT with three base64-encoded segments separated by dots:

| Segment | Description |
|---------|-------------|
| Header | Contains algorithm type (`PS256`) and token type (`JWT`) |
| Claims | Contains attestation claims about device and application integrity |
| Signature | Cryptographic signature for verification |

## Error Handling

`getIntegrityToken()` throws `DeviceApplicationIntegrityException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

This package (`DeviceApplicationIntegrityStatusCode`) defines no package-specific status codes beyond the common ones. For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example 1: Basic Integrity Check

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

### Example 2: Retry with Exponential Backoff

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

### Example 3: Guarding a Sensitive Operation

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
```

## Important Notes

1. **Use a unique nonce for every request** -- `challengeNonce` should be a unique, unpredictable value (e.g., `UUID.randomUUID().toString()`) per request. Reusing nonces enables replay attacks.

2. **Verify tokens on your backend server** -- do not trust the token locally. The server should verify the JWT signature using Meta's public key, validate the nonce matches, and inspect the integrity claims.

3. **Simple string-in, string-out API** -- input is a nonce string, output is a JWT string. No custom models, enums, or options; only the common Platform SDK status codes (0-6, 190, 1001-1005).

4. **Requires HzOS v85+** -- on older OS versions it returns status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.

5. **Rate limiting** -- avoid calling `getIntegrityToken()` excessively. Use it at critical checkpoints (app launch, before purchases, before accessing sensitive content) rather than on every API call. If rate limited, you receive status code 4 (`RateLimitExceeded`).
