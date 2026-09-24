# Consent API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.consent` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-consent |
| **Minimum OS** | HzOS v83 |
| **Maven Artifact** | `horizon-platform-sdk-consent-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Examples](#examples)
- [Important Notes](#important-notes)

## Overview

- **`getConsentStatus()`** -- check the current status of a specific consent for the user
- **`launchConsentIfRequired()`** -- launch a consent flow UI if the user has not yet completed it

## API Usage

#### Check Consent Status

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.models.ConsentStatusResult
import horizon.platform.consent.enums.ConsentStatus

val consent = Consent()

try {
    val results: List<ConsentStatusResult> = consent.getConsentStatus(
        consentFlowName = "tos_for_feature_x",
        version = null,       // Optional: specific consent version
        extraParams = null,   // Optional: additional parameters as Map<String, String>
    )

    for (result in results) {
        val status = result.status        // ConsentStatus enum value
        val type = result.consentType     // Type of the consent
        val time = result.decisionTime    // Timestamp of last status update
        val ver = result.version          // Optional consent version
    }

} catch (e: ConsentException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `consentFlowName: String` -- Name identifying the consent flow to check
- `version: String?` -- Optional consent version (some consents support multiple versions)
- `extraParams: Map<String, String>?` -- Optional extra parameters for consents that require additional context (e.g., target app)

**Return type**: `List<ConsentStatusResult>` -- a list of consent status results for the queried consent.

#### Launch Consent Flow If Required

Presents the consent UI if the user has not yet completed the consent flow.

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.models.ConsentLaunchResult
import horizon.platform.consent.enums.ConsentLaunchOutcome

val consent = Consent()

try {
    val result: ConsentLaunchResult = consent.launchConsentIfRequired(
        consentFlowName = "tos_for_feature_x",
        version = null,
        extraParams = null,
    )

    when (result.outcome) {
        ConsentLaunchOutcome.APPROVED -> { /* User agreed -- proceed with the feature */ }
        ConsentLaunchOutcome.DENIED -> { /* User declined -- do not enable the feature */ }
        ConsentLaunchOutcome.DISMISSED -> { /* Dismissed dialog without making a choice */ }
        ConsentLaunchOutcome.NOT_REQUIRED -> { /* Already completed -- no UI was shown */ }
        ConsentLaunchOutcome.UNKNOWN -> { /* Unknown outcome -- handle gracefully */ }
    }

} catch (e: ConsentException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**: same as `getConsentStatus()`. **Return type**: `ConsentLaunchResult` (contains `outcome`).

## Data Types

### `ConsentStatusResult` Model (returned by `getConsentStatus()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `status` | `ConsentStatus` | -- | Current status of the consent |
| `consentType` | `String` | `""` | Type identifier for the consent |
| `decisionTime` | `Long` | `0` | Timestamp of the last status update (epoch millis) |
| `version` | `String?` | `null` | Optional version of the consent |

### `ConsentLaunchResult` Model (returned by `launchConsentIfRequired()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `outcome` | `ConsentLaunchOutcome` | -- | Outcome of the consent launch request |

### `ConsentStatus` Enum

| Value | Ordinal | Description |
|-------|---------|-------------|
| `DEFAULT_NOT_SEEN` | 0 | User has not seen the consent yet |
| `SEEN` | 1 | User has seen the consent but has not approved or declined |
| `WITHDRAWN` | 2 | User declined or later withdrew consent |
| `CONSENTED` | 3 | User has agreed to the consent |

### `ConsentLaunchOutcome` Enum

| Value | Ordinal | Description |
|-------|---------|-------------|
| `NOT_REQUIRED` | 0 | Consent was already completed; no UI was shown |
| `DISMISSED` | 1 | User dismissed the consent dialog without choosing |
| `DENIED` | 2 | User declined the consent |
| `APPROVED` | 3 | User agreed to the consent |
| `UNKNOWN` | 4 | Unknown outcome |

## Error Handling

Both methods throw `ConsentException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Package-Specific Status Codes (`ConsentStatusCode`)

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `LaunchFailed` | 2001 | Consent flow launch failed | Retry the launch; check that HzPlatformService is running |
| `UnsupportedConsentFlowType` | 2002 | Consent flow name is not recognized | Verify the consent flow name is correct and supported |
| `NoConsentStatusFound` | 2004 | No consent status results returned | The consent may not exist or may not apply to this user |

## Examples

### Example: Ensure Consent (check, then launch if needed)

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.enums.ConsentStatus
import horizon.platform.consent.enums.ConsentLaunchOutcome

suspend fun ensureConsent(consentFlowName: String): Boolean {
    val consent = Consent()
    return try {
        val statuses = consent.getConsentStatus(consentFlowName, null, null)
        if (statuses.any { it.status == ConsentStatus.CONSENTED }) return true
        // Versioned/contextual flows: pass version + mapOf("target_app" to id) instead of nulls
        val result = consent.launchConsentIfRequired(consentFlowName, null, null)
        result.outcome == ConsentLaunchOutcome.APPROVED ||
            result.outcome == ConsentLaunchOutcome.NOT_REQUIRED
    } catch (e: ConsentException) {
        false
    }
}
```

## Important Notes

1. **`launchConsentIfRequired()` presents UI** -- may launch a system consent dialog and suspends until the user interacts or the flow determines consent is not required.
2. **`getConsentStatus()` returns a list** -- `List<ConsentStatusResult>`, not a single result, to allow flows with multiple consent types. Always iterate/query the list.
3. **Consent flow names must be valid** -- an invalid/unsupported `consentFlowName` returns status code 2002 (`UnsupportedConsentFlowType`).
4. **Handle `NOT_REQUIRED`** -- the user already completed the consent previously; no UI was shown. Not an error -- read prior state via `getConsentStatus()`.
5. **`version`/`extraParams` are optional** -- pass `null` unless the specific consent flow requires them.
6. **Requires HzOS v83+** -- on older OS versions both methods return status code 1003 (`ProviderOperationNotSupported`).
