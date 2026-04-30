# Consent API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.consent` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-consent |
| **Minimum OS** | HzOS v83 |
| **Maven Artifact** | `horizon-platform-sdk-consent-kotlin` |

> For initial setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Overview

The Consent API provides two operations for Meta Quest Android applications:

1. **`getConsentStatus()`** -- Check the current status of a specific consent for the user
2. **`launchConsentIfRequired()`** -- Launch a consent flow UI if the user has not yet completed it

The Consent API enables applications to gate features behind user consent, check whether consent has been granted, and present the consent UI when needed.

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

Use this to present the consent UI to the user if they have not yet completed the consent flow.

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
        ConsentLaunchOutcome.APPROVED -> {
            // User agreed to the consent -- proceed with the feature
        }
        ConsentLaunchOutcome.DENIED -> {
            // User declined the consent -- do not enable the feature
        }
        ConsentLaunchOutcome.DISMISSED -> {
            // User dismissed the consent dialog without making a choice
        }
        ConsentLaunchOutcome.NOT_REQUIRED -> {
            // Consent was already completed -- no UI was shown
        }
        ConsentLaunchOutcome.UNKNOWN -> {
            // Unknown outcome -- handle gracefully
        }
    }

} catch (e: ConsentException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `consentFlowName: String` -- Name identifying the consent flow to launch
- `version: String?` -- Optional consent version
- `extraParams: Map<String, String>?` -- Optional extra parameters

**Return type**: `ConsentLaunchResult` -- contains the outcome of the consent launch request.

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

Both `getConsentStatus()` and `launchConsentIfRequired()` throw `ConsentException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Package-Specific Status Codes (`ConsentStatusCode`)

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `LaunchFailed` | 2001 | Consent flow launch failed | Retry the launch; check that HzPlatformService is running |
| `UnsupportedConsentFlowType` | 2002 | Consent flow name is not recognized | Verify the consent flow name is correct and supported |
| `NoConsentStatusFound` | 2004 | No consent status results returned | The consent may not exist or may not apply to this user |

## Examples

### Example 1: Basic Consent Check

Check if a user has consented before enabling a feature.

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.enums.ConsentStatus

suspend fun isFeatureConsented(consentFlowName: String): Boolean {
    val client = Consent()
    return try {
        val results = client.getConsentStatus(consentFlowName, null, null)
        results.any { it.status == ConsentStatus.CONSENTED }
    } catch (e: ConsentException) {
        false
    }
}
```

### Example 2: Launch Consent and Handle All Outcomes

Launch a consent flow and react to every possible outcome.

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.enums.ConsentLaunchOutcome

sealed class ConsentResult {
    data object Granted : ConsentResult()
    data object Denied : ConsentResult()
    data object Dismissed : ConsentResult()
    data object AlreadyCompleted : ConsentResult()
    data class Error(val message: String) : ConsentResult()
}

suspend fun requestConsent(consentFlowName: String): ConsentResult {
    val client = Consent()
    return try {
        val result = client.launchConsentIfRequired(consentFlowName, null, null)
        when (result.outcome) {
            ConsentLaunchOutcome.APPROVED -> ConsentResult.Granted
            ConsentLaunchOutcome.DENIED -> ConsentResult.Denied
            ConsentLaunchOutcome.DISMISSED -> ConsentResult.Dismissed
            ConsentLaunchOutcome.NOT_REQUIRED -> ConsentResult.AlreadyCompleted
            ConsentLaunchOutcome.UNKNOWN -> ConsentResult.Error("Unknown outcome")
        }
    } catch (e: ConsentException) {
        ConsentResult.Error(e.message ?: "Failed to launch consent")
    }
}
```

### Example 3: Repository Pattern with Consent Gating

Wrap the Consent API in a repository that combines status checking and consent launching.

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.enums.ConsentStatus
import horizon.platform.consent.enums.ConsentLaunchOutcome
import horizon.platform.consent.models.ConsentStatusResult

sealed class ConsentGateResult {
    data object Allowed : ConsentGateResult()
    data object Blocked : ConsentGateResult()
    data class Error(val message: String) : ConsentGateResult()
}

class ConsentRepository {
    private val consent = Consent()

    suspend fun getStatus(consentFlowName: String): List<ConsentStatusResult> {
        return try {
            consent.getConsentStatus(consentFlowName, null, null)
        } catch (e: ConsentException) {
            emptyList()
        }
    }

    suspend fun ensureConsent(consentFlowName: String): ConsentGateResult {
        return try {
            // First check if already consented
            val statuses = consent.getConsentStatus(consentFlowName, null, null)
            val alreadyConsented = statuses.any { it.status == ConsentStatus.CONSENTED }

            if (alreadyConsented) {
                return ConsentGateResult.Allowed
            }

            // Launch consent flow
            val result = consent.launchConsentIfRequired(consentFlowName, null, null)
            when (result.outcome) {
                ConsentLaunchOutcome.APPROVED,
                ConsentLaunchOutcome.NOT_REQUIRED -> ConsentGateResult.Allowed
                ConsentLaunchOutcome.DENIED,
                ConsentLaunchOutcome.DISMISSED -> ConsentGateResult.Blocked
                ConsentLaunchOutcome.UNKNOWN -> ConsentGateResult.Error("Unknown outcome")
            }
        } catch (e: ConsentException) {
            ConsentGateResult.Error(e.message ?: "Consent check failed")
        }
    }
}
```

### Example 4: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.consent.enums.ConsentStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class ConsentUiState(
    val isConsented: Boolean = false,
    val consentStatus: ConsentStatus? = null,
    val isLoading: Boolean = false,
    val isLaunchingConsent: Boolean = false,
    val error: String? = null,
)

class ConsentViewModel(
    private val repository: ConsentRepository = ConsentRepository()
) : ViewModel() {
    private val _uiState = MutableStateFlow(ConsentUiState())
    val uiState: StateFlow<ConsentUiState> = _uiState

    fun checkConsentStatus(consentFlowName: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            val statuses = repository.getStatus(consentFlowName)
            val currentStatus = statuses.firstOrNull()?.status
            _uiState.value = _uiState.value.copy(
                isConsented = currentStatus == ConsentStatus.CONSENTED,
                consentStatus = currentStatus,
                isLoading = false,
            )
        }
    }

    fun requestConsent(consentFlowName: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(
                isLaunchingConsent = true,
                error = null,
            )
            when (val result = repository.ensureConsent(consentFlowName)) {
                is ConsentGateResult.Allowed -> {
                    _uiState.value = _uiState.value.copy(
                        isConsented = true,
                        consentStatus = ConsentStatus.CONSENTED,
                        isLaunchingConsent = false,
                    )
                }
                is ConsentGateResult.Blocked -> {
                    _uiState.value = _uiState.value.copy(
                        isConsented = false,
                        consentStatus = ConsentStatus.WITHDRAWN,
                        isLaunchingConsent = false,
                    )
                }
                is ConsentGateResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLaunchingConsent = false,
                        error = result.message,
                    )
                }
            }
        }
    }
}
```

### Example 5: Consent Check with Version and Extra Parameters

Some consent flows support versioning and extra parameters for contextual consent.

```kotlin
import horizon.platform.consent.Consent
import horizon.platform.consent.ConsentException
import horizon.platform.consent.enums.ConsentStatus

suspend fun checkVersionedConsent(
    consentFlowName: String,
    version: String,
    targetAppId: String,
): ConsentStatus? {
    val client = Consent()
    return try {
        val extraParams = mapOf("target_app" to targetAppId)
        val results = client.getConsentStatus(consentFlowName, version, extraParams)
        results.firstOrNull()?.status
    } catch (e: ConsentException) {
        when {
            e.message?.contains("2002") == true -> {
                // Unsupported consent flow type -- the flow name is invalid
                null
            }
            e.message?.contains("2004") == true -> {
                // No consent status found -- consent may not apply
                null
            }
            else -> throw e
        }
    }
}
```

## Important Notes

1. **`launchConsentIfRequired()` presents UI** -- this method may launch a system consent dialog. It blocks (suspends) until the user interacts with the dialog or the flow determines consent is not required. Design your UX to account for this user interaction step.

2. **`getConsentStatus()` returns a list** -- the return type is `List<ConsentStatusResult>`, not a single result. This allows for consent flows that encompass multiple consent types. Always iterate through or query the list.

3. **Consent flow names must be valid** -- the `consentFlowName` parameter must match a recognized consent flow. Using an invalid or unsupported name returns status code 2002 (`UnsupportedConsentFlowType`).

4. **Handle `NOT_REQUIRED` outcome** -- when `launchConsentIfRequired()` returns `NOT_REQUIRED`, it means the user already completed the consent (approved, denied, or dismissed previously). No UI was shown. This is not an error -- treat it according to the previously stored consent status by calling `getConsentStatus()`.

5. **Version and extra parameters are optional** -- most consent flows do not require `version` or `extraParams`. Pass `null` for both unless the specific consent flow documentation indicates otherwise.

6. **Requires HzOS v83+** -- both `getConsentStatus()` and `launchConsentIfRequired()` require HzOS v83 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`).

7. **No pagination or events** -- this is a request/response API. Each call is independent. `getConsentStatus()` checks current state and `launchConsentIfRequired()` triggers a one-time UI flow.
