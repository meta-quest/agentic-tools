# Asset File API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.assetfile` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-sdk-sample-assetfile |
| **Minimum OS** | HzOS v85 |
| **Maven Artifact** | `horizon-platform-sdk-asset-file-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Contents
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## API Usage

All methods are on `horizon.platform.assetfile.AssetFile`. All are suspend functions except `downloadUpdate()` (returns a `Flow`). All except `downloadUpdate()` throw `AssetFileException` on failure -- wrap in `try/catch` (see Error Handling). Most operations have both `ById` and `ByName` variants producing the same result; use `ById` with `AssetDetails.assetId`, `ByName` with the human-readable name.

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.*

val assetFile = AssetFile()
```

#### `getList(): List<AssetDetails>`

Retrieve all asset files (see `AssetDetails` for fields).

```kotlin
val assets: List<AssetDetails> = assetFile.getList()
val available = assets.filter { it.downloadStatus == "available" }
```

#### `statusById(id: String): AssetDetails` / `statusByName(name: String): AssetDetails`

```kotlin
val details: AssetDetails = assetFile.statusById("asset-file-id")   // or statusByName("my-asset-file")
val isInstalled = details.downloadStatus == "installed"   // or "available" / "in-progress"
```

#### `downloadById(id: String): AssetFileDownloadResult` / `downloadByName(name: String): AssetFileDownloadResult`

Download an asset file. Use `result.assetId` to track progress via `downloadUpdate()`.

```kotlin
val result: AssetFileDownloadResult = assetFile.downloadById("asset-file-id")   // or downloadByName(...)
val assetId = result.assetId
val filePath = result.filepath
```

#### `downloadByIdList(ids: List<String>): Int` / `downloadByNameList(names: List<String>): Int`

Batch download. Returns a session ID for tracking, or `-1` on failure. Batches are all-or-nothing.

```kotlin
val sessionId: Int = assetFile.downloadByIdList(listOf("id-1", "id-2", "id-3"))   // or downloadByNameList(...)
if (sessionId == -1) { /* Handle batch download initiation failure */ }
```

#### `downloadCancelById(id: String): AssetFileDownloadCancelResult` / `downloadCancelByName(name: String): AssetFileDownloadCancelResult`

```kotlin
val result: AssetFileDownloadCancelResult = assetFile.downloadCancelById("asset-file-id")   // or downloadCancelByName(...)
val wasSuccessful = result.success
```

#### `deleteById(id: String): AssetFileDeleteResult` / `deleteByName(name: String): AssetFileDeleteResult`

```kotlin
val result: AssetFileDeleteResult = assetFile.deleteById("asset-file-id")   // or deleteByName(...)
val wasSuccessful = result.success
```

#### `downloadUpdate(): Flow<AssetFileDownloadUpdate>`

Returns a `Flow` (not a suspend function). Emits progress for all active downloads; collect it in a coroutine scope.

```kotlin
assetFile.downloadUpdate().collect { update: AssetFileDownloadUpdate ->
    val progressPercent = if (update.bytesTotal > 0u)
        (update.bytesTransferred * 100 / update.bytesTotal.toLong()) else 0
    if (update.completed) { /* downloaded; see note below */ }
}
```

**Note:** `completed == true` means the file is downloaded but may not yet be installed. After completion, call `statusById()` and poll until `downloadStatus` changes from `"available"` to `"installed"`.

## Data Types

### `AssetDetails` (returned by `getList()`, `statusById()`, `statusByName()`)

`assetId: String` (`""`); `assetType: String` (`""`; one of `"default"`, `"store"`, `"shader_blob"`, `"shader_blob_final"`, `"dfm_apk"`, `"apk_v4_signature"`, `"language_pack"`); `downloadStatus: String` (`""`; one of `"installed"`, `"available"`, `"in-progress"`); `filepath: String` (`""`, local path); `iapStatus: String` (`""`; IAP entitlement `"free"`, `"entitled"`, or `"not-entitled"`); `language: LanguagePackInfo?` (`null`, `language_pack` assets only); `metadata: String?` (`null`, optional extra metadata).

### `AssetFileDownloadUpdate` (emitted by `downloadUpdate()` Flow)

`assetId: String` (`""`); `bytesTotal: ULong` (`0`, total bytes to download); `bytesTransferred: Long` (`0`, bytes downloaded so far, -1 if not started); `completed: Boolean` (`false`, download finished but may not yet be installed).

### Result models

All default to `""` (String) / `false` (Boolean).

- **`AssetFileDownloadResult`** (from `downloadById()`, `downloadByName()`): `assetId: String`, `filepath: String` (where the asset will be stored).
- **`AssetFileDownloadCancelResult`** (from `downloadCancelById()`, `downloadCancelByName()`): `assetId: String`, `filepath: String`, `success: Boolean` (whether the cancel succeeded).
- **`AssetFileDeleteResult`** (from `deleteById()`, `deleteByName()`): `assetId: String`, `filepath: String`, `success: Boolean` (whether the delete succeeded).

### `LanguagePackInfo` (nested in `AssetDetails`, only for `language_pack` assets)

`englishName: String` (`""`, e.g. "German"), `nativeName: String` (`""`, e.g. "Deutsch"), `tag: String` (`""`, BCP47 tag e.g. "de").

## Error Handling

All methods (except `downloadUpdate()`, which returns a Flow) throw `AssetFileException` (extends `HzPlatformSdkException`) on failure.

### Package-Specific Status Codes (`AssetFileStatusCode`)

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `InvalidRequestFormat` | 2001 | Request data is null, empty, or malformed; asset file ID or name is invalid or blank | Verify the asset file ID or name is correct and non-empty |
| `NotEntitled` | 2002 | Asset not found or user is not entitled to access it | Verify the asset exists and the user has access |
| `DownloadFailed` | 2003 | Initiating the asset download failed due to an internal issue | Retry the download; check network connectivity |
| `DeleteFailed` | 2004 | Deleting the asset file failed due to an internal issue | Retry the deletion; ensure the asset is installed |
| `CancelFailed` | 2005 | Canceling the asset download failed due to an internal issue | Retry the cancel; ensure the download is still in progress |

## Important Notes

- **Batch downloads are all-or-nothing** -- `downloadByIdList()` / `downloadByNameList()` download all specified assets together (all succeed or fail). They return a session `Int`, or `-1` on failure.
- **`completed` does not mean installed** -- after an `AssetFileDownloadUpdate.completed == true`, call `statusById()` and poll until `downloadStatus` changes from `"available"` to `"installed"`.
- **Check IAP entitlement before downloading paid assets** -- assets with `iapStatus == "not-entitled"` require the user to purchase them through the in-app purchase flow first.
- **Requires HzOS v85+** -- on older OS versions all methods return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.
