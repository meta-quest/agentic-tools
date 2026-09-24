# Application API

- **Kotlin Package**: `horizon.platform.application`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-platform-sdk-application
- **Minimum OS**: HzOS v78 (core API); v85 for download/install APIs
- **Maven Artifact**: `horizon-platform-sdk-application-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

Operations for Meta VR Android apps to manage and interact with platform apps (methods under [API Usage](#api-usage)).

Typical self-update flow: `getVersion()` → `startAppDownload()` → poll `checkAppDownloadProgress()` → `installAppUpdateAndRelaunch()`.

All methods are on `Application()` and throw `ApplicationException`. Package paths: `Application`, `ApplicationException` in `horizon.platform.application`; `ApplicationVersion`, `AppDownloadResult`, `AppDownloadProgressResult` in `.models`; `ApplicationOptions` in `.options`; `AppInstallResult`, `AppStatus` in `.enums`.

## API Usage

Return-type properties: see [Data Types](#data-types).

#### Get Version Information
```kotlin
val version: ApplicationVersion = Application().getVersion()
val updateAvailable = version.latestCode > version.currentCode
```

#### Launch Another Application
Launches an app; if not installed, opens its store page.
```kotlin
val options = ApplicationOptions().apply {
    deeplinkMessage = "join-game-123"
    destinationApiName = "multiplayer_lobby"
    lobbySessionId = "lobby-456"
    matchSessionId = "match-789"
}
val result: String = Application().launchOtherApp("<target-app-id>", options)
// deeplinkOptions is optional: launchOtherApp("<target-app-id>") also works
```
Params: `appId: String`, `deeplinkOptions: ApplicationOptions?` = `null`. Returns `String`.

#### Start / Cancel App Download
```kotlin
val result: AppDownloadResult = Application().startAppDownload()
// val cancelled: AppDownloadResult = Application().cancelAppDownload()
```
Both return `AppDownloadResult` (`.appInstallResult`, `.timestamp`).

#### Check App Download Progress
Polling API -- call repeatedly with a delay.
```kotlin
val progress: AppDownloadProgressResult = Application().checkAppDownloadProgress()
val percent = if (progress.downloadBytes > 0)
    (progress.downloadedBytes * 100 / progress.downloadBytes).toInt() else 0
val status = progress.statusCode  // AppStatus enum
```

#### Install App Update and Relaunch
Installs a downloaded update; app exits during install, then relaunches (Note 1).
```kotlin
val result: AppDownloadResult = Application().installAppUpdateAndRelaunch()
// Optional deeplink options for the relaunch:
val result2 = Application().installAppUpdateAndRelaunch(
    ApplicationOptions().apply { destinationApiName = "home" })
```
Param: `deeplinkOptions: ApplicationOptions?` = `null`. Returns `AppDownloadResult`.

## Data Types

### `ApplicationVersion`

| Property | Type | Default | Description |
|---|---|---|---|
| `currentCode` | `Int` | `0` | Installed version code |
| `currentName` | `String` | `""` | Installed version name |
| `latestCode` | `Int` | `0` | Latest version code |
| `latestName` | `String` | `""` | Latest version name |
| `releaseDate` | `Long?` | `null` | Release (sec since epoch) |
| `size` | `String?` | `null` | Update size (bytes) |

### `AppDownloadResult`

| Property | Type | Default | Description |
|---|---|---|---|
| `appInstallResult` | `AppInstallResult` | `UNKNOWN` | Operation result |
| `timestamp` | `Long` | `0` | Finished time (ms) |

### `AppDownloadProgressResult`

| Property | Type | Default | Description |
|---|---|---|---|
| `downloadBytes` | `Long` | `0` | Total bytes |
| `downloadedBytes` | `Long` | `0` | Bytes downloaded |
| `statusCode` | `AppStatus` | `UNKNOWN` | Download/install status |

### `ApplicationOptions`

| Property | Type | Default | Description |
|---|---|---|---|
| `deeplinkMessage` | `String` | `""` | Message to app |
| `destinationApiName` | `String` | `""` | Intended destination |
| `lobbySessionId` | `String` | `""` | Lobby session ID |
| `matchSessionId` | `String` | `""` | Destination instance |
| `roomId` | `String?` | `null` | (Deprecated) Room ID |

### `AppStatus`

| Value | Code | Description |
|---|---|---|
| `UNKNOWN` | 0 | Unknown |
| `ENTITLED` | 1 | Entitled, not installed |
| `DOWNLOAD_QUEUED` | 2 | Download queued |
| `DOWNLOADING` | 3 | Downloading |
| `INSTALLING` | 4 | Installing |
| `INSTALLED` | 5 | Installed, ready |
| `UNINSTALLING` | 6 | Uninstalling |
| `INSTALL_QUEUED` | 7 | Install queued |

### `AppInstallResult`

| Value | Code | Description |
|---|---|---|
| `UNKNOWN` | 0 | Unknown |
| `LOW_STORAGE` | 1 | Low storage |
| `NETWORK_ERROR` | 2 | Network error |
| `DUPLICATE_REQUEST` | 3 | Install running |
| `INSTALLER_ERROR` | 4 | Installer error |
| `USER_CANCELLED` | 5 | User cancelled |
| `AUTHORIZATION_ERROR` | 6 | Auth error |
| `SUCCESS` | 7 | Succeeded |
| `NO_NEW_BINARIES_AVAILABLE` | 8 | Up to date |

## Error Handling

All methods throw `ApplicationException` (extends `HzPlatformSdkException`). Wrap in try/catch. Match app-specific codes against `e.message` (e.g., `e.message?.contains("2006")`).

### Application-Specific Status Codes

| Status Code | Value | Meaning & action |
|---|---|---|
| `MissingLaunchParameters` | 2001 | Params missing/invalid; verify app ID |
| `DeeplinkOptionsError` | 2002 | Deeplink validation failed |
| `CurrentAppBlocked` | 2003 | Current app blocked (policy) |
| `TargetAppBlocked` | 2004 | Target on receiving blocklist |
| `ApplabNotAllowed` | 2005 | AppLab launch blocked by config |
| `TargetAppNotFoundOrInstalled` | 2006 | Target not found/installed |
| `FailToLaunch` | 2007 | Launch failed; verify target installed |
| `GetVersionFailed` | 2009 | Version fetch failed; retry |
| `PackageNotFound` | 2012 | No active download; call `startAppDownload()` first |
| `CancelAppDownloadFailed` | 2013 | Cancel failed; no active download |
| `UnknownError` | 2014 | Unexpected error; retry |
| `LowStorage` | 2015 | Low storage; free space |
| `InstallTimeout` | 2016 | Install timed out; retry |
| `UserCancelled` | 2017 | User cancelled |
| `DuplicateRequest` | 2018 | Duplicate; wait for in-progress |
| `NetworkError` | 2019 | Network issue; retry |
| `AuthorizationError` | 2020 | Auth error; re-authenticate |
| `NoNewBinariesAvailable` | 2021 | No new binaries; up to date |
| `InstalledAppSignatureMismatch` | 2022 | Signature mismatch |
| `IoError` | 2023 | I/O error; retry |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Important Notes

1. **`installAppUpdateAndRelaunch()` exits the app** -- it exits once install begins, then relaunches with any deeplink options. Save state first.
2. **Download flow is sequential** -- call `startAppDownload()` before `checkAppDownloadProgress()`; polling with no active download returns 2012 (`PackageNotFound`).
3. **OS gating** -- `getVersion()` needs HzOS v78; download/install APIs need v85. On older OS they return 1003 (`ProviderOperationNotSupported`). Set a min OS in `AndroidManifest.xml` ([Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.
4. **`ApplicationOptions` for deeplinks** -- both `launchOtherApp()` and `installAppUpdateAndRelaunch()` accept optional `ApplicationOptions`; the launched app reads these via the Application Lifecycle API's `LaunchDetails`.
