# Common Setup and Shared Reference

This document covers setup steps, initialization code, and common status codes shared across all Horizon Platform SDK packages. Read this first before reading any specific API tool file.

## Step 1: Initial App Setup

If the developer has not yet integrated the Horizon Platform SDK, direct them to the setup guide:
- **Setup documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-setup-kotlin

This covers adding Meta's Maven repository, configuring the Android project, and obtaining an app ID from the Meta Quest Developer Dashboard.

## Step 2: Add the SDK Dependency

Each package has a Maven artifact following this naming convention:

```text
horizon-platform-sdk-<package-name>-kotlin
```

For example:
- `horizon-platform-sdk-achievements-kotlin`
- `horizon-platform-sdk-iap-kotlin`
- `horizon-platform-sdk-users-kotlin`

Browse all available artifacts at: https://central.sonatype.com/search?namespace=com.meta.horizon.platform.sdk

Add the artifact from Meta's Maven repository as configured in your project's dependency management.

## Step 3: Initialize HorizonServiceConnection

Before calling any SDK API, connect to the platform service in your Activity's `onCreate`:

```kotlin
import horizon.core.android.driver.coroutines.HorizonServiceConnection

class MainActivity : ComponentActivity() {
    private val APPLICATION_ID = "<your-app-id>"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        HorizonServiceConnection.connect(
            APPLICATION_ID,
            applicationContext,
            lifecycleScope,
        )
    }
}
```

**Universal prerequisites:**
- HzPlatformService must be installed and running on the device
- Your app must have a valid entitlement (otherwise status code 3 is returned)

## Step 4: Instantiate the Client

Each package provides a lightweight client class that can be instantiated wherever needed:

```kotlin
val client = <ClientClass>()
```

For example: `val achievements = Achievements()`, `val iap = Iap()`, `val users = Users()`.

## Common Status Codes

All SDK packages share these common status codes. When an API call fails, the exception's status code will be one of these (or a package-specific code in the 2001+ range documented in each tool file).

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `Success` | 0 | Operation completed successfully | N/A (no exception thrown) |
| `InternalError` | 1 | Internal error | Retry or show generic error |
| `NotInitialized` | 2 | Platform not initialized | Call `HorizonServiceConnection.connect()` first |
| `EntitlementFailure` | 3 | Entitlement check failed | Verify app ID and entitlement |
| `RateLimitExceeded` | 4 | Rate limit exceeded | Back off and retry |
| `Forbidden` | 5 | Feature disabled for user | Feature not available for this user |
| `NetworkUnavailable` | 6 | No network connection | Prompt user to check connectivity |
| `InvalidAuthAccessToken` | 190 | Invalid auth token | Re-authenticate the user |
| `ProviderUnknownError` | 1001 | Unknown provider error | Retry or contact support |
| `ProviderFeatureNotEnabled` | 1002 | Feature disabled | Feature gated server-side |
| `ProviderOperationNotSupported` | 1003 | Operation not supported | OS version too old; update device |
| `ProviderInitializationFailed` | 1004 | Provider init failed | Check HzPlatformService installation |
| `ProviderGraphApiError` | 1005 | Graph API error | Server-side issue; retry later |

## Common Important Notes

1. **Suspend functions require coroutine scope** -- most SDK methods are `suspend` functions and must be called from a coroutine scope (e.g., `viewModelScope.launch {}`, `lifecycleScope.launch {}`). Do not call them from the main thread without a coroutine.

2. **HzPlatformService must be running** -- the SDK communicates via IPC to the platform service on the device. Ensure it is installed and connected before making API calls.

3. **Network required** -- most operations require network connectivity to communicate with the platform service and backend servers. Handle `NetworkUnavailable` (status code 6) appropriately.

4. **Minimum OS version handling** -- on older OS versions that don't support a particular API, calls return status code 1003 (`ProviderOperationNotSupported`). You can either require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

5. **Exception hierarchy** -- each package defines its own exception class (e.g., `AchievementsException`, `IapException`) that extends `HzPlatformSdkException`. Always wrap API calls in try/catch.
