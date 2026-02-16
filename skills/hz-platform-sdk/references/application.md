# Application API

- **Kotlin Package**: `horizon.platform.application`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-application
- **Minimum OS**: HzOS v78 (core API); v85 for download/install APIs
- **Maven Artifact**: `horizon-platform-sdk-application-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Overview

The Application API is part of the Horizon Platform SDK. It provides operations for Meta Quest Android applications to manage and interact with applications on the platform:

1. **`getVersion()`** -- Retrieve version information for the currently installed app and the latest available update
2. **`launchOtherApp(appId, deeplinkOptions)`** -- Launch another application or navigate to its store page
3. **`startAppDownload()`** -- Start downloading the app update
4. **`checkAppDownloadProgress()`** -- Check the progress of an ongoing app download
5. **`cancelAppDownload()`** -- Cancel an app download that is in progress
6. **`installAppUpdateAndRelaunch(deeplinkOptions)`** -- Install a previously downloaded update and relaunch the app

The typical self-update flow is: `getVersion()` to check for updates, `startAppDownload()` to begin downloading, `checkAppDownloadProgress()` to monitor progress, and `installAppUpdateAndRelaunch()` to apply the update.

## API Usage

#### Get Version Information

Retrieve the current and latest version information for the app:

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.models.ApplicationVersion

val application = Application()

try {
    val version: ApplicationVersion = application.getVersion()

    // Current installed version
    val currentCode = version.currentCode       // e.g., 12
    val currentName = version.currentName       // e.g., "1.2.0"

    // Latest available version
    val latestCode = version.latestCode         // e.g., 15
    val latestName = version.latestName         // e.g., "1.5.0"
    val size = version.size                     // e.g., "524288000" (bytes), nullable
    val releaseDate = version.releaseDate       // Seconds since epoch, nullable

    val updateAvailable = latestCode > currentCode

} catch (e: ApplicationException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `ApplicationVersion` -- an immutable object containing version metadata for the current install and the latest release.

#### Launch Another Application

Launch a different application in the user's library. If the user does not have that application installed, they will be taken to that app's page in the store:

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.options.ApplicationOptions

val application = Application()

try {
    // Basic launch
    val result: String = application.launchOtherApp("<target-app-id>")

    // Launch with deeplink options
    val options = ApplicationOptions().apply {
        deeplinkMessage = "join-game-123"
        destinationApiName = "multiplayer_lobby"
        lobbySessionId = "lobby-456"
        matchSessionId = "match-789"
    }
    val resultWithOptions: String = application.launchOtherApp("<target-app-id>", options)

} catch (e: ApplicationException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `appId: String` -- The unique ID of the app to launch
- `deeplinkOptions: ApplicationOptions?` -- Optional deeplink configuration (default: `null`)

**Return type**: `String` -- result of the launch operation

#### Start App Download

Start downloading the latest app update. Monitor progress with `checkAppDownloadProgress()`:

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.models.AppDownloadResult

val application = Application()

try {
    val result: AppDownloadResult = application.startAppDownload()

    val installResult = result.appInstallResult  // AppInstallResult enum
    val timestamp = result.timestamp             // Completion time in ms

} catch (e: ApplicationException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `AppDownloadResult` -- contains the install result status and completion timestamp.

#### Check App Download Progress

Track the progress of an ongoing download:

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.models.AppDownloadProgressResult

val application = Application()

try {
    val progress: AppDownloadProgressResult = application.checkAppDownloadProgress()

    val totalBytes = progress.downloadBytes       // Total bytes to download
    val downloadedBytes = progress.downloadedBytes // Bytes downloaded so far
    val status = progress.statusCode              // AppStatus enum

    val progressPercent = if (totalBytes > 0) {
        (downloadedBytes * 100 / totalBytes).toInt()
    } else {
        0
    }

} catch (e: ApplicationException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `AppDownloadProgressResult` -- contains byte counts and the current download status.

#### Cancel App Download

Cancel a download that is currently in progress:

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.models.AppDownloadResult

val application = Application()

try {
    val result: AppDownloadResult = application.cancelAppDownload()

    val installResult = result.appInstallResult
    val timestamp = result.timestamp

} catch (e: ApplicationException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `AppDownloadResult` -- contains the cancellation result and timestamp.

#### Install App Update and Relaunch

Install a previously downloaded update. The app will exit automatically during installation and relaunch when complete:

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.options.ApplicationOptions
import horizon.platform.application.models.AppDownloadResult

val application = Application()

try {
    // Install without deeplink options
    val result: AppDownloadResult = application.installAppUpdateAndRelaunch()

    // Install with deeplink options for relaunch
    val options = ApplicationOptions().apply {
        deeplinkMessage = "updated-successfully"
        destinationApiName = "home"
    }
    val resultWithOptions: AppDownloadResult = application.installAppUpdateAndRelaunch(options)

} catch (e: ApplicationException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**:
- `deeplinkOptions: ApplicationOptions?` -- Optional configuration for the relaunch after update (default: `null`)

**Return type**: `AppDownloadResult` -- contains the installation result and timestamp. Note that the app will exit during installation.

## Data Types

### `ApplicationVersion` Model (returned by `getVersion()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `currentCode` | `Int` | `0` | Version code of the currently installed app |
| `currentName` | `String` | `""` | Version name of the currently installed app |
| `latestCode` | `Int` | `0` | Version code of the latest available update |
| `latestName` | `String` | `""` | Version name of the latest available update |
| `releaseDate` | `Long?` | `null` | Seconds since epoch when the latest update was released |
| `size` | `String?` | `null` | Size of the latest update in bytes |

### `AppDownloadResult` Model (returned by `startAppDownload()`, `cancelAppDownload()`, `installAppUpdateAndRelaunch()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `appInstallResult` | `AppInstallResult` | `UNKNOWN` | Result of the install/download/cancel operation |
| `timestamp` | `Long` | `0` | Timestamp in milliseconds when the operation finished |

### `AppDownloadProgressResult` Model (returned by `checkAppDownloadProgress()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `downloadBytes` | `Long` | `0` | Total number of bytes to download |
| `downloadedBytes` | `Long` | `0` | Number of bytes already downloaded |
| `statusCode` | `AppStatus` | `UNKNOWN` | Current download/install status |

### `ApplicationOptions` Model (used for deeplink configuration)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `deeplinkMessage` | `String` | `""` | Message passed to the launched app |
| `destinationApiName` | `String` | `""` | Intended destination in the launched app |
| `lobbySessionId` | `String` | `""` | Lobby session ID for grouping users together |
| `matchSessionId` | `String` | `""` | Instance of the destination for the user |
| `roomId` | `String?` | `null` | (Deprecated) Room ID for the launched app |

### `AppStatus` Enum

| Value | Code | Description |
|-------|------|-------------|
| `UNKNOWN` | 0 | Status is unknown |
| `ENTITLED` | 1 | User has entitlement but app is not installed |
| `DOWNLOAD_QUEUED` | 2 | Download is queued, waiting for prior downloads |
| `DOWNLOADING` | 3 | App is currently being downloaded |
| `INSTALLING` | 4 | App is currently being installed |
| `INSTALLED` | 5 | App is installed and ready to use |
| `UNINSTALLING` | 6 | App is being uninstalled |
| `INSTALL_QUEUED` | 7 | Installation is queued, waiting for prior installs |

### `AppInstallResult` Enum

| Value | Code | Description |
|-------|------|-------------|
| `UNKNOWN` | 0 | Unknown result |
| `LOW_STORAGE` | 1 | Failed due to low storage |
| `NETWORK_ERROR` | 2 | Failed due to a network error |
| `DUPLICATE_REQUEST` | 3 | Another install request is already in progress |
| `INSTALLER_ERROR` | 4 | Internal installer error |
| `USER_CANCELLED` | 5 | User cancelled the operation |
| `AUTHORIZATION_ERROR` | 6 | User authorization error |
| `SUCCESS` | 7 | Operation succeeded |
| `NO_NEW_BINARIES_AVAILABLE` | 8 | App is already up to date |

## Error Handling

All API methods throw `ApplicationException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Application-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `MissingLaunchParameters` | 2001 | Required launch parameters are missing or invalid | Verify app ID is non-empty and parameters are correct |
| `DeeplinkOptionsError` | 2002 | Deeplink option validation failed | Check that deeplink options are properly formatted |
| `CurrentAppBlocked` | 2003 | Current app is blocked from launching other apps | Policy restriction; contact support |
| `TargetAppBlocked` | 2004 | Target app is on the receiving app blocklist | Target app cannot be launched due to policy |
| `ApplabNotAllowed` | 2005 | AppLab app launches are not permitted | Device configuration does not allow AppLab launches |
| `TargetAppNotFoundOrInstalled` | 2006 | Target app not found or not installed | Verify the target app ID is correct |
| `FailToLaunch` | 2007 | Launch activity failed | Verify the target app is properly installed |
| `GetVersionFailed` | 2009 | Version retrieval failed | Internal error; retry the operation |
| `PackageNotFound` | 2012 | No active download for the package | Ensure `startAppDownload()` was called first |
| `CancelAppDownloadFailed` | 2013 | Download cancellation failed | No active download to cancel or cancellation error |
| `UnknownError` | 2014 | Unexpected error during download/install/launch | Retry or show generic error |
| `LowStorage` | 2015 | Insufficient storage space | Prompt user to free storage |
| `InstallTimeout` | 2016 | Installation timed out | Retry the installation |
| `UserCancelled` | 2017 | User cancelled the download or installation | No action needed; user-initiated |
| `DuplicateRequest` | 2018 | Duplicate download or install request | A request is already in progress; wait for it |
| `NetworkError` | 2019 | Network issue prevented download | Prompt user to check connectivity and retry |
| `AuthorizationError` | 2020 | Authorization error during download/install | Re-authenticate or verify permissions |
| `NoNewBinariesAvailable` | 2021 | No new binaries available for installation | App is already up to date |
| `InstalledAppSignatureMismatch` | 2022 | Signature mismatch between installed and new app | Installation source mismatch; contact support |
| `IoError` | 2023 | I/O error during download or installation | Retry; check device storage health |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example 1: Check for Updates

Retrieve version information and determine if an update is available.

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException

data class UpdateInfo(
    val currentVersion: String,
    val latestVersion: String,
    val updateAvailable: Boolean,
    val updateSizeBytes: String?,
)

suspend fun checkForUpdates(): UpdateInfo {
    val application = Application()
    return try {
        val version = application.getVersion()
        UpdateInfo(
            currentVersion = version.currentName,
            latestVersion = version.latestName,
            updateAvailable = version.latestCode > version.currentCode,
            updateSizeBytes = version.size,
        )
    } catch (e: ApplicationException) {
        throw e
    }
}
```

### Example 2: Full Self-Update Flow with Progress Polling

Download and install an app update with periodic progress checks.

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.models.AppDownloadResult
import horizon.platform.application.models.AppDownloadProgressResult
import horizon.platform.application.enums.AppInstallResult
import horizon.platform.application.enums.AppStatus
import kotlinx.coroutines.delay

suspend fun performSelfUpdate(onProgress: (Int) -> Unit): Boolean {
    val application = Application()

    // Step 1: Check if update is available
    val version = application.getVersion()
    if (version.latestCode <= version.currentCode) {
        return false // Already up to date
    }

    // Step 2: Start download
    try {
        application.startAppDownload()
    } catch (e: ApplicationException) {
        throw e
    }

    // Step 3: Poll download progress
    var downloading = true
    while (downloading) {
        try {
            val progress: AppDownloadProgressResult = application.checkAppDownloadProgress()
            val percent = if (progress.downloadBytes > 0) {
                (progress.downloadedBytes * 100 / progress.downloadBytes).toInt()
            } else {
                0
            }
            onProgress(percent)

            when (progress.statusCode) {
                AppStatus.INSTALLED -> downloading = false
                AppStatus.DOWNLOADING, AppStatus.DOWNLOAD_QUEUED -> delay(1000)
                else -> delay(500)
            }
        } catch (e: ApplicationException) {
            throw e
        }
    }

    // Step 4: Install and relaunch (app will exit automatically)
    application.installAppUpdateAndRelaunch()
    return true
}
```

### Example 3: Launch Another App with Deeplink

Launch a multiplayer game session in another app with deeplink parameters.

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.options.ApplicationOptions

sealed class LaunchResult {
    data class Success(val result: String) : LaunchResult()
    data class AppBlocked(val reason: String) : LaunchResult()
    data class NotFound(val appId: String) : LaunchResult()
    data class Error(val message: String) : LaunchResult()
}

suspend fun launchMultiplayerSession(
    targetAppId: String,
    lobbyId: String,
    matchId: String,
    message: String,
): LaunchResult {
    val application = Application()
    val options = ApplicationOptions().apply {
        deeplinkMessage = message
        lobbySessionId = lobbyId
        matchSessionId = matchId
    }

    return try {
        val result = application.launchOtherApp(targetAppId, options)
        LaunchResult.Success(result)
    } catch (e: ApplicationException) {
        when {
            e.message?.contains("2003") == true ->
                LaunchResult.AppBlocked("Your app is blocked from launching other apps")
            e.message?.contains("2004") == true ->
                LaunchResult.AppBlocked("Target app is blocked")
            e.message?.contains("2006") == true ->
                LaunchResult.NotFound(targetAppId)
            e.message?.contains("2007") == true ->
                LaunchResult.Error("Failed to launch the target app")
            else ->
                LaunchResult.Error(e.message ?: "Unknown error")
        }
    }
}
```

### Example 4: MVVM Update Manager ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import horizon.platform.application.enums.AppStatus
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class UpdateUiState(
    val currentVersion: String = "",
    val latestVersion: String = "",
    val updateAvailable: Boolean = false,
    val isDownloading: Boolean = false,
    val downloadProgress: Int = -1,  // -1 = no active download, 0-100 = progress
    val error: String? = null,
)

class UpdateManagerViewModel : ViewModel() {
    private val application = Application()
    private val _uiState = MutableStateFlow(UpdateUiState())
    val uiState: StateFlow<UpdateUiState> = _uiState

    fun checkForUpdate() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(error = null)
            try {
                val version = application.getVersion()
                _uiState.value = _uiState.value.copy(
                    currentVersion = version.currentName,
                    latestVersion = version.latestName,
                    updateAvailable = version.latestCode > version.currentCode,
                )
            } catch (e: ApplicationException) {
                _uiState.value = _uiState.value.copy(error = e.message)
            }
        }
    }

    fun startUpdate() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(
                isDownloading = true,
                downloadProgress = 0,
                error = null,
            )

            try {
                application.startAppDownload()

                // Poll progress
                var completed = false
                while (!completed) {
                    val progress = application.checkAppDownloadProgress()
                    val percent = if (progress.downloadBytes > 0) {
                        (progress.downloadedBytes * 100 / progress.downloadBytes).toInt()
                    } else {
                        0
                    }
                    _uiState.value = _uiState.value.copy(downloadProgress = percent)

                    if (progress.statusCode == AppStatus.INSTALLED) {
                        completed = true
                    } else {
                        delay(1000)
                    }
                }

                // Install and relaunch
                application.installAppUpdateAndRelaunch()

            } catch (e: ApplicationException) {
                _uiState.value = _uiState.value.copy(
                    isDownloading = false,
                    downloadProgress = -1,
                    error = e.message,
                )
            }
        }
    }

    fun cancelUpdate() {
        viewModelScope.launch {
            try {
                application.cancelAppDownload()
                _uiState.value = _uiState.value.copy(
                    isDownloading = false,
                    downloadProgress = -1,
                )
            } catch (e: ApplicationException) {
                _uiState.value = _uiState.value.copy(error = e.message)
            }
        }
    }
}
```

### Example 5: Cancelling a Download with Retry Logic

Cancel an in-progress download and handle edge cases.

```kotlin
import horizon.platform.application.Application
import horizon.platform.application.ApplicationException
import kotlinx.coroutines.delay

suspend fun cancelDownloadWithRetry(maxRetries: Int = 3): Boolean {
    val application = Application()
    var attempt = 0

    while (attempt < maxRetries) {
        try {
            val result = application.cancelAppDownload()
            return true // Successfully cancelled
        } catch (e: ApplicationException) {
            when {
                e.message?.contains("2012") == true -> {
                    // PackageNotFound -- no active download to cancel
                    return true
                }
                e.message?.contains("2013") == true -> {
                    // CancelAppDownloadFailed -- retry
                    attempt++
                    if (attempt < maxRetries) {
                        delay(1000L * attempt)
                    }
                }
                else -> throw e
            }
        }
    }
    return false
}
```

## Important Notes

1. **`installAppUpdateAndRelaunch()` exits the app** -- once the install begins, the application will exit automatically. After installation completes, the app relaunches with any deeplink options provided. Save any necessary state before calling this method.

2. **Download flow is sequential** -- you must call `startAppDownload()` before `checkAppDownloadProgress()`. Calling `checkAppDownloadProgress()` without an active download returns status code 2012 (`PackageNotFound`).

3. **`checkAppDownloadProgress()` is a polling API** -- unlike event-based APIs, you need to call this method repeatedly to track download progress. Use a delay between polls (e.g., 1 second) to avoid excessive API calls.

4. **`getVersion()` is available from HzOS v78** -- this is the earliest API in the package. Download and install APIs (`startAppDownload`, `cancelAppDownload`, `checkAppDownloadProgress`, `installAppUpdateAndRelaunch`) require HzOS v85. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

5. **`launchOtherApp()` navigates to the store if not installed** -- if the target app is not installed on the user's device, the user will be taken to that app's page in the Meta Horizon Store rather than receiving an error.

6. **ApplicationOptions for deeplinks** -- both `launchOtherApp()` and `installAppUpdateAndRelaunch()` accept optional `ApplicationOptions` for deeplink configuration. The launched or relaunched app can retrieve these values through the Application Lifecycle API's `LaunchDetails`.
