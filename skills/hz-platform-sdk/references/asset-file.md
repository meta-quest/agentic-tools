# Asset File API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.assetfile` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-asset-file |
| **Minimum OS** | HzOS v85 |
| **Maven Artifact** | `horizon-platform-sdk-asset-file-kotlin` |

> For initial setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Overview

The Asset File API provides methods for managing downloadable asset files in Meta Quest Android applications:

1. **`getList()`** -- Retrieve a list of all asset files with their details and download status
2. **`statusById(assetFileId)`** -- Get details for a single asset file by its ID
3. **`statusByName(assetFileName)`** -- Get details for a single asset file by its name
4. **`downloadById(assetFileId)`** -- Download an asset file by its ID
5. **`downloadByName(assetFileName)`** -- Download an asset file by its name
6. **`downloadByIdList(assetFileIdList)`** -- Batch download multiple asset files by their IDs
7. **`downloadByNameList(assetFileNameList)`** -- Batch download multiple asset files by their names
8. **`downloadCancelById(assetFileId)`** -- Cancel an in-progress download by asset file ID
9. **`downloadCancelByName(assetFileName)`** -- Cancel an in-progress download by asset file name
10. **`deleteById(assetFileId)`** -- Delete an installed asset file by its ID
11. **`deleteByName(assetFileName)`** -- Delete an installed asset file by its name
12. **`downloadUpdate()`** -- Subscribe to download progress events (returns a `Flow`)

## API Usage

#### List All Asset Files

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetDetails

val assetFile = AssetFile()

try {
    val assets: List<AssetDetails> = assetFile.getList()

    for (asset in assets) {
        val id = asset.assetId              // Unique asset identifier
        val type = asset.assetType          // e.g. "default", "store", "language_pack"
        val status = asset.downloadStatus   // "installed", "available", or "in-progress"
        val path = asset.filepath           // Local file path (if installed)
        val iap = asset.iapStatus           // "free", "entitled", or "not-entitled"
        val lang = asset.language           // LanguagePackInfo? (for language_pack type)
        val meta = asset.metadata           // Optional extra metadata
    }

} catch (e: AssetFileException) {
    // Handle error -- see Error Handling section
}
```

#### Check Asset Status by ID

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetDetails

val assetFile = AssetFile()

try {
    val details: AssetDetails = assetFile.statusById("asset-file-id")

    val isInstalled = details.downloadStatus == "installed"
    val isAvailable = details.downloadStatus == "available"
    val isInProgress = details.downloadStatus == "in-progress"

} catch (e: AssetFileException) {
    // Handle error -- see Error Handling section
}
```

#### Check Asset Status by Name

```kotlin
val details: AssetDetails = assetFile.statusByName("my-asset-file")
```

#### Download an Asset File by ID

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetFileDownloadResult

val assetFile = AssetFile()

try {
    val result: AssetFileDownloadResult = assetFile.downloadById("asset-file-id")

    val assetId = result.assetId   // Use this to track download progress
    val filePath = result.filepath // File path where asset will be stored

} catch (e: AssetFileException) {
    // Handle error -- see Error Handling section
}
```

#### Download an Asset File by Name

```kotlin
val result: AssetFileDownloadResult = assetFile.downloadByName("my-asset-file")
```

#### Batch Download by ID List

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException

val assetFile = AssetFile()

try {
    val sessionId: Int = assetFile.downloadByIdList(listOf("id-1", "id-2", "id-3"))

    // sessionId can be used to track the batch download
    // Returns -1 on failure
    if (sessionId == -1) {
        // Handle batch download initiation failure
    }

} catch (e: AssetFileException) {
    // Handle error -- see Error Handling section
}
```

#### Batch Download by Name List

```kotlin
val sessionId: Int = assetFile.downloadByNameList(listOf("asset-1", "asset-2"))
```

#### Cancel a Download by ID

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetFileDownloadCancelResult

val assetFile = AssetFile()

try {
    val result: AssetFileDownloadCancelResult = assetFile.downloadCancelById("asset-file-id")

    val wasSuccessful = result.success
    val assetId = result.assetId
    val filePath = result.filepath

} catch (e: AssetFileException) {
    // Handle error -- see Error Handling section
}
```

#### Cancel a Download by Name

```kotlin
val result: AssetFileDownloadCancelResult = assetFile.downloadCancelByName("my-asset-file")
```

#### Delete an Asset File by ID

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetFileDeleteResult

val assetFile = AssetFile()

try {
    val result: AssetFileDeleteResult = assetFile.deleteById("asset-file-id")

    val wasSuccessful = result.success
    val assetId = result.assetId
    val filePath = result.filepath

} catch (e: AssetFileException) {
    // Handle error -- see Error Handling section
}
```

#### Delete an Asset File by Name

```kotlin
val result: AssetFileDeleteResult = assetFile.deleteByName("my-asset-file")
```

#### Track Download Progress (Event)

Use `downloadUpdate()` to subscribe to real-time download progress updates. This method returns a `Flow` and does not need to be called as a suspend function.

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.models.AssetFileDownloadUpdate

val assetFile = AssetFile()

assetFile.downloadUpdate().collect { update: AssetFileDownloadUpdate ->
    val assetId = update.assetId
    val completed = update.bytesTransferred
    val total = update.bytesTotal
    val isComplete = update.completed

    // Update your UI with download progress
    val progressPercent = if (total > 0u) (completed * 100 / total.toLong()) else 0
}
```

**Note:** `completed == true` means the file is downloaded but may not yet be installed. After completion, call `statusById()` and poll until `downloadStatus` changes from `"available"` to `"installed"`.

## Data Types

### `AssetDetails` Model (returned by `getList()`, `statusById()`, `statusByName()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | Unique identifier for the asset file |
| `assetType` | `String` | `""` | One of `"default"`, `"store"`, `"shader_blob"`, `"shader_blob_final"`, `"dfm_apk"`, `"apk_v4_signature"`, or `"language_pack"` |
| `downloadStatus` | `String` | `""` | One of `"installed"`, `"available"`, or `"in-progress"` |
| `filepath` | `String` | `""` | Local file path of the asset file |
| `iapStatus` | `String` | `""` | IAP entitlement status: `"free"`, `"entitled"`, or `"not-entitled"` |
| `language` | `LanguagePackInfo?` | `null` | Language metadata (only for `language_pack` type assets) |
| `metadata` | `String?` | `null` | Optional extra metadata associated with the asset |

### `AssetFileDownloadResult` Model (returned by `downloadById()`, `downloadByName()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | Unique identifier for the downloading asset |
| `filepath` | `String` | `""` | Local file path where the asset will be stored |

### `AssetFileDownloadUpdate` Model (emitted by `downloadUpdate()` Flow)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | ID of the asset file being downloaded |
| `bytesTotal` | `ULong` | `0` | Total number of bytes to download |
| `bytesTransferred` | `Long` | `0` | Number of bytes downloaded so far (-1 if download has not started) |
| `completed` | `Boolean` | `false` | Whether the download has finished (may not yet be installed) |

### `AssetFileDownloadCancelResult` Model (returned by `downloadCancelById()`, `downloadCancelByName()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | ID of the asset file whose download was canceled |
| `filepath` | `String` | `""` | File path of the asset file |
| `success` | `Boolean` | `false` | Whether the cancel request succeeded |

### `AssetFileDeleteResult` Model (returned by `deleteById()`, `deleteByName()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | ID of the deleted asset file |
| `filepath` | `String` | `""` | File path of the deleted asset file |
| `success` | `Boolean` | `false` | Whether the delete operation succeeded |

### `LanguagePackInfo` Model (nested in `AssetDetails`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `englishName` | `String` | `""` | Language name in English (e.g., "German") |
| `nativeName` | `String` | `""` | Language name in its native form (e.g., "Deutsch") |
| `tag` | `String` | `""` | BCP47 language tag (e.g., "de") |

## Error Handling

All methods (except `downloadUpdate()` which returns a Flow) throw `AssetFileException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Package-Specific Status Codes (`AssetFileStatusCode`)

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `InvalidRequestFormat` | 2001 | Request data is null, empty, or malformed; asset file ID or name is invalid or blank | Verify the asset file ID or name is correct and non-empty |
| `NotEntitled` | 2002 | Asset not found or user is not entitled to access it | Verify the asset exists and the user has access |
| `DownloadFailed` | 2003 | Initiating the asset download failed due to an internal issue | Retry the download; check network connectivity |
| `DeleteFailed` | 2004 | Deleting the asset file failed due to an internal issue | Retry the deletion; ensure the asset is installed |
| `CancelFailed` | 2005 | Canceling the asset download failed due to an internal issue | Retry the cancel; ensure the download is still in progress |

## Examples

### Example 1: List and Filter Available Assets

Retrieve all asset files and filter to show only those available for download.

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetDetails

suspend fun getAvailableAssets(): List<AssetDetails> {
    val client = AssetFile()
    return try {
        val allAssets = client.getList()
        allAssets.filter { it.downloadStatus == "available" }
    } catch (e: AssetFileException) {
        emptyList()
    }
}
```

### Example 2: Download with Progress Tracking

Download an asset file by name and track progress in real time.

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetFileDownloadResult
import horizon.platform.assetfile.models.AssetFileDownloadUpdate
import kotlinx.coroutines.flow.Flow

suspend fun downloadAssetWithProgress(
    assetName: String,
    onProgress: (Int) -> Unit,
    onComplete: (String) -> Unit,
    onError: (String) -> Unit,
) {
    val client = AssetFile()

    try {
        val result: AssetFileDownloadResult = client.downloadByName(assetName)

        // Track download progress
        client.downloadUpdate().collect { update: AssetFileDownloadUpdate ->
            if (update.assetId == result.assetId) {
                val progress = if (update.bytesTotal > 0u) {
                    (update.bytesTransferred * 100 / update.bytesTotal.toLong()).toInt()
                } else {
                    0
                }
                onProgress(progress)

                if (update.completed) {
                    onComplete(result.filepath)
                    return@collect
                }
            }
        }

    } catch (e: AssetFileException) {
        onError(e.message ?: "Download failed")
    }
}
```

### Example 3: Asset File Manager with Error Handling

Manage assets with proper error handling for common failure scenarios.

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import horizon.platform.assetfile.models.AssetDetails

sealed class AssetResult<out T> {
    data class Success<T>(val data: T) : AssetResult<T>()
    data class NotFound(val id: String) : AssetResult<Nothing>()
    data class Error(val message: String) : AssetResult<Nothing>()
}

class AssetFileManager {
    private val client = AssetFile()

    suspend fun getAssetStatus(assetId: String): AssetResult<AssetDetails> {
        return try {
            val details = client.statusById(assetId)
            AssetResult.Success(details)
        } catch (e: AssetFileException) {
            when {
                e.message?.contains("2001") == true ->
                    AssetResult.NotFound(assetId)
                e.message?.contains("2002") == true ->
                    AssetResult.NotFound(assetId)
                else ->
                    AssetResult.Error(e.message ?: "Unknown error")
            }
        }
    }

    suspend fun downloadAsset(assetId: String): AssetResult<String> {
        return try {
            val result = client.downloadById(assetId)
            AssetResult.Success(result.filepath)
        } catch (e: AssetFileException) {
            when {
                e.message?.contains("2003") == true ->
                    AssetResult.Error("Download failed. Check network and retry.")
                else ->
                    AssetResult.Error(e.message ?: "Unknown error")
            }
        }
    }

    suspend fun deleteAsset(assetId: String): AssetResult<Boolean> {
        return try {
            val result = client.deleteById(assetId)
            AssetResult.Success(result.success)
        } catch (e: AssetFileException) {
            when {
                e.message?.contains("2004") == true ->
                    AssetResult.Error("Delete failed. Asset may not be installed.")
                else ->
                    AssetResult.Error(e.message ?: "Unknown error")
            }
        }
    }
}
```

### Example 4: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.models.AssetDetails
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class AssetFileUiState(
    val assets: List<AssetDetails> = emptyList(),
    val isLoading: Boolean = false,
    val downloadProgress: Map<String, Int> = emptyMap(),  // assetId -> progress %
    val error: String? = null,
)

class AssetFileViewModel : ViewModel() {
    private val client = AssetFile()
    private val _uiState = MutableStateFlow(AssetFileUiState())
    val uiState: StateFlow<AssetFileUiState> = _uiState

    init {
        // Start collecting download updates
        viewModelScope.launch {
            client.downloadUpdate().collect { update ->
                val progress = if (update.bytesTotal > 0u) {
                    (update.bytesTransferred * 100 / update.bytesTotal.toLong()).toInt()
                } else {
                    0
                }

                if (update.completed) {
                    // Remove from progress tracking and refresh asset list
                    _uiState.value = _uiState.value.copy(
                        downloadProgress = _uiState.value.downloadProgress - update.assetId,
                    )
                    fetchAssets()
                } else {
                    _uiState.value = _uiState.value.copy(
                        downloadProgress = _uiState.value.downloadProgress +
                            (update.assetId to progress),
                    )
                }
            }
        }
    }

    fun fetchAssets() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val assets = client.getList()
                _uiState.value = _uiState.value.copy(
                    assets = assets,
                    isLoading = false,
                )
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to fetch assets",
                )
            }
        }
    }

    fun downloadAsset(assetId: String) {
        viewModelScope.launch {
            try {
                client.downloadById(assetId)
                // Progress will be tracked via downloadUpdate() flow above
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    error = e.message ?: "Download failed",
                )
            }
        }
    }

    fun cancelDownload(assetId: String) {
        viewModelScope.launch {
            try {
                val result = client.downloadCancelById(assetId)
                if (result.success) {
                    _uiState.value = _uiState.value.copy(
                        downloadProgress = _uiState.value.downloadProgress - assetId,
                    )
                    fetchAssets()
                }
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    error = e.message ?: "Cancel failed",
                )
            }
        }
    }

    fun deleteAsset(assetId: String) {
        viewModelScope.launch {
            try {
                val result = client.deleteById(assetId)
                if (result.success) {
                    fetchAssets() // Refresh the list after deletion
                }
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    error = e.message ?: "Delete failed",
                )
            }
        }
    }
}
```

### Example 5: Batch Download with Completion Polling

Download multiple assets at once and poll for installation completion.

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.AssetFileException
import kotlinx.coroutines.delay

suspend fun batchDownloadAndWaitForInstall(assetNames: List<String>): Boolean {
    val client = AssetFile()

    try {
        // Initiate batch download
        val sessionId = client.downloadByNameList(assetNames)
        if (sessionId == -1) {
            return false // Batch download initiation failed
        }

        // Poll until all assets are installed
        var allInstalled = false
        while (!allInstalled) {
            delay(2000) // Poll every 2 seconds
            val assets = client.getList()
            allInstalled = assetNames.all { name ->
                assets.any { asset ->
                    asset.filepath.contains(name) && asset.downloadStatus == "installed"
                }
            }
        }

        return true

    } catch (e: AssetFileException) {
        return false
    }
}
```

## Important Notes

1. **`downloadUpdate()` returns a `Flow`** -- unlike other methods, `downloadUpdate()` is not a suspend function. It returns a `Flow<AssetFileDownloadUpdate>` that emits progress updates for all active downloads. Collect it in a coroutine scope to receive updates.

2. **`completed` does not mean installed** -- when `AssetFileDownloadUpdate.completed` is `true`, the file has been downloaded but may not yet be installed. After receiving a completion event, call `statusById()` and poll until `downloadStatus` changes from `"available"` to `"installed"`.

3. **Batch downloads are all-or-nothing** -- `downloadByIdList()` and `downloadByNameList()` download all specified assets as a batch. All assets must succeed or fail together. They return a session ID (`Int`) for tracking, or `-1` on failure.

4. **By-ID vs by-name methods** -- most operations have both `ById` and `ByName` variants. Use `ById` when you have the asset's unique identifier (from `AssetDetails.assetId`). Use `ByName` when referencing assets by their human-readable name. Both variants produce the same results.

5. **Asset types** -- the `assetType` field on `AssetDetails` indicates the purpose of the asset. Common types are `"default"` (app-controlled extra content), `"store"` (shown in Store), and `"language_pack"` (localization data). Other types (`"shader_blob"`, `"dfm_apk"`, etc.) are for specialized platform use.

6. **IAP status checking** -- check `AssetDetails.iapStatus` before downloading paid assets. Assets with `"not-entitled"` IAP status require the user to purchase them first through the in-app purchase flow.

7. **Requires HzOS v85+** -- all Asset File API methods require HzOS v85 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.
