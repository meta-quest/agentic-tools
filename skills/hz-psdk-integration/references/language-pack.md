# Language Pack API

- **Kotlin Package**: `horizon.platform.languagepack`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-language-packs
- **Minimum OS**: HzOS v85
- **Maven Artifact**: `horizon-platform-sdk-language-pack-kotlin`

## Overview

The Language Pack API is part of the Horizon Platform SDK. It provides two operations for Meta Quest Android applications:

1. **`getCurrent()`** -- Retrieve details about the currently installed language pack
2. **`setCurrent(tag)`** -- Set (download and install) a language pack by its BCP47 language tag

Setting a language pack triggers a download. The SDK provides a companion API (`AssetFile.downloadUpdate()`) to track download progress in real time.

## Setup

For setup, initialization, and common status codes, see the common-setup tool.

If you need download progress tracking, also add the Asset File package:

```text
horizon-platform-sdk-asset-file-kotlin
```

## API Usage

#### Retrieve the Current Language Pack

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.languagepack.LanguagePackException
import horizon.platform.assetfile.models.AssetDetails

val languagePack = LanguagePack()

try {
    val result: AssetDetails = languagePack.getCurrent()

    // Access language pack details
    val languageTag = result.language?.tag          // BCP47 tag, e.g. "en"
    val englishName = result.language?.englishName   // e.g. "English"
    val nativeName = result.language?.nativeName     // e.g. "English"
    val filePath = result.filepath                   // Local file path of the asset
    val assetId = result.assetId                     // Unique asset identifier
    val version = result.versionCode                 // Version code of the asset

} catch (e: LanguagePackException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `AssetDetails` -- an immutable object containing asset metadata and language information.

**Key properties of `AssetDetails`**:
- `assetId: String` -- Unique identifier for the asset (default: `""`)
- `filepath: String` -- Local file path of the downloaded asset (default: `""`)
- `language: LanguagePackInfo?` -- Language metadata (may be `null`)
- `versionCode: Long` -- Version code of the asset (default: `0`)
- `metadata: String` -- Additional metadata (default: `""`)

**Key properties of `LanguagePackInfo`**:
- `englishName: String` -- Language name in English (e.g., "French")
- `nativeName: String` -- Language name in its native form (e.g., "Francais")
- `tag: String` -- BCP47 language tag (e.g., "fr")

#### Set the Current Language Pack

Use this to download and install a specific language pack by its BCP47 language tag.

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.languagepack.LanguagePackException
import horizon.platform.assetfile.models.AssetFileDownloadResult

val languagePack = LanguagePack()

try {
    val result: AssetFileDownloadResult = languagePack.setCurrent("fr")

    // Download initiated successfully
    val assetId = result.assetId    // Use this to track download progress
    val filePath = result.filepath  // File path where the asset will be stored

} catch (e: LanguagePackException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `tag: String` -- A BCP47 language tag (e.g., `"en"`, `"fr"`, `"de"`, `"es"`, `"ja"`)
**Return type**: `AssetFileDownloadResult` -- contains the asset ID and file path for the initiated download

**Key properties of `AssetFileDownloadResult`**:
- `assetId: String` -- Unique identifier for the downloading asset (default: `""`)
- `filepath: String` -- Local file path where the asset will be stored (default: `""`)

#### Track Download Progress (Optional)

After calling `setCurrent()`, you can track the download progress using the Asset File API:

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.models.AssetFileDownloadUpdate

val assetFile = AssetFile()

// Register for download updates using the asset ID from setCurrent()
assetFile.downloadUpdate(assetId).collect { update: AssetFileDownloadUpdate ->
    val completed = update.bytesTransferred
    val total = update.bytesTotal
    val isComplete = update.completed

    // Update your UI with download progress
    val progressPercent = if (total > 0) (completed * 100 / total) else 0
}
```

**Return type**: `Flow<AssetFileDownloadUpdate>` -- a Kotlin Flow that emits download progress updates

**Key properties of `AssetFileDownloadUpdate`**:
- `bytesTransferred: Long` -- Number of bytes downloaded so far
- `bytesTotal: Long` -- Total number of bytes to download
- `completed: Boolean` -- Whether the download has finished

## Data Types

### `AssetDetails` Model (returned by `getCurrent()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | Unique identifier for the asset |
| `filepath` | `String` | `""` | Local file path of the downloaded asset |
| `language` | `LanguagePackInfo?` | `null` | Language metadata (name, tag) |
| `versionCode` | `Long` | `0` | Version code of the asset |
| `metadata` | `String` | `""` | Additional metadata |

### `LanguagePackInfo` Model (nested in `AssetDetails`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `englishName` | `String` | `""` | Language name in English (e.g., "French") |
| `nativeName` | `String` | `""` | Language name in native form (e.g., "Francais") |
| `tag` | `String` | `""` | BCP47 language tag (e.g., "fr") |

### `AssetFileDownloadResult` Model (returned by `setCurrent()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `assetId` | `String` | `""` | Unique identifier for the downloading asset |
| `filepath` | `String` | `""` | Local file path where asset will be stored |

### `AssetFileDownloadUpdate` Model (emitted by download tracking Flow)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `bytesTransferred` | `Long` | `0` | Bytes downloaded so far |
| `bytesTotal` | `Long` | `0` | Total bytes to download |
| `completed` | `Boolean` | `false` | Whether the download has finished |

## Error Handling

Both `getCurrent()` and `setCurrent()` throw `LanguagePackException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### Status Codes

For common status codes (0-6, 190, 1001-1005), see the common-setup tool.

#### Language Pack-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `LanguagePackNotInstalled` | 2001 | No language pack is installed | Call `setCurrent()` to install one first |
| `LanguagePackNotSet` | 2002 | Language pack not set for this app | Call `setCurrent()` to set a language pack |
| `LanguagePackDuplicate` | 2003 | Requested language pack is already set | No action needed; current pack matches request |
| `LanguagePackInvalidTag` | 2004 | Invalid BCP47 language tag | Verify the language tag is valid and supported |
| `LanguagePackNotAvailable` | 2005 | Language pack not available | The requested language is not available for this app |

## Examples

### Example 1: Basic Language Check

Retrieve the current language pack and display its name.

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.languagepack.LanguagePackException

suspend fun getCurrentLanguageName(): String {
    val client = LanguagePack()
    return try {
        val result = client.getCurrent()
        result.language?.englishName ?: "Unknown"
    } catch (e: LanguagePackException) {
        "Error: ${e.message}"
    }
}
```

### Example 2: Language Switching with Error Handling

Switch to a new language pack and handle common error cases.

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.languagepack.LanguagePackException

sealed class LanguageSwitchResult {
    data class Success(val assetId: String) : LanguageSwitchResult()
    data class AlreadySet(val message: String) : LanguageSwitchResult()
    data class NotAvailable(val tag: String) : LanguageSwitchResult()
    data class Error(val message: String) : LanguageSwitchResult()
}

suspend fun switchLanguage(tag: String): LanguageSwitchResult {
    val client = LanguagePack()
    return try {
        val result = client.setCurrent(tag)
        LanguageSwitchResult.Success(result.assetId)
    } catch (e: LanguagePackException) {
        when {
            e.message?.contains("2003") == true ->
                LanguageSwitchResult.AlreadySet("Language '$tag' is already the current pack")
            e.message?.contains("2004") == true ->
                LanguageSwitchResult.NotAvailable(tag)
            e.message?.contains("2005") == true ->
                LanguageSwitchResult.NotAvailable(tag)
            else ->
                LanguageSwitchResult.Error(e.message ?: "Unknown error")
        }
    }
}
```

### Example 3: Repository Pattern with Download Tracking

Wrap the Language Pack and Asset File APIs in a repository for clean architecture.

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.models.AssetDetails
import horizon.platform.assetfile.models.AssetFileDownloadResult
import horizon.platform.assetfile.models.AssetFileDownloadUpdate
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.emptyFlow

sealed class LanguagePackResult<out T> {
    data class Success<T>(val data: T) : LanguagePackResult<T>()
    data class Error(val message: String) : LanguagePackResult<Nothing>()
}

class LanguagePackRepository {
    private val languagePack = LanguagePack()
    private val assetFile = AssetFile()

    suspend fun getCurrent(): LanguagePackResult<AssetDetails> {
        return try {
            val result = languagePack.getCurrent()
            LanguagePackResult.Success(result)
        } catch (e: Exception) {
            LanguagePackResult.Error(e.message ?: "Failed to get current language pack")
        }
    }

    suspend fun setCurrent(tag: String): LanguagePackResult<AssetFileDownloadResult> {
        return try {
            val result = languagePack.setCurrent(tag)
            LanguagePackResult.Success(result)
        } catch (e: Exception) {
            LanguagePackResult.Error(e.message ?: "Failed to set language pack")
        }
    }

    fun trackDownload(assetId: String): Flow<AssetFileDownloadUpdate> {
        return try {
            assetFile.downloadUpdate(assetId)
        } catch (e: Exception) {
            emptyFlow()
        }
    }
}
```

### Example 4: Full MVVM Integration with ViewModel and Download Progress

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class LanguagePackUiState(
    val currentLanguage: String = "",
    val currentTag: String = "",
    val isLoading: Boolean = false,
    val downloadProgress: Int = -1,  // -1 = no active download, 0-100 = progress
    val error: String? = null,
)

class LanguagePackViewModel(
    private val repository: LanguagePackRepository = LanguagePackRepository()
) : ViewModel() {
    private val _uiState = MutableStateFlow(LanguagePackUiState())
    val uiState: StateFlow<LanguagePackUiState> = _uiState

    fun fetchCurrentLanguage() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = repository.getCurrent()) {
                is LanguagePackResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        currentLanguage = result.data.language?.englishName ?: "Unknown",
                        currentTag = result.data.language?.tag ?: "",
                        isLoading = false,
                    )
                }
                is LanguagePackResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = result.message,
                    )
                }
            }
        }
    }

    fun setLanguage(tag: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(
                isLoading = true,
                error = null,
                downloadProgress = 0,
            )

            when (val result = repository.setCurrent(tag)) {
                is LanguagePackResult.Success -> {
                    // Start tracking download progress
                    repository.trackDownload(result.data.assetId).collect { update ->
                        val progress = if (update.bytesTotal > 0) {
                            (update.bytesTransferred * 100 / update.bytesTotal).toInt()
                        } else {
                            0
                        }
                        _uiState.value = _uiState.value.copy(
                            downloadProgress = progress,
                            isLoading = !update.completed,
                        )
                        if (update.completed) {
                            _uiState.value = _uiState.value.copy(downloadProgress = -1)
                            fetchCurrentLanguage() // Refresh the current language display
                        }
                    }
                }
                is LanguagePackResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        downloadProgress = -1,
                        error = result.message,
                    )
                }
            }
        }
    }
}
```

### Example 5: Handling "No Language Pack Set" Gracefully

When no language pack has been set yet, `getCurrent()` returns status code 2002. Handle this to provide a setup flow.

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.languagepack.LanguagePackException

suspend fun getOrSetDefaultLanguage(): String {
    val client = LanguagePack()
    return try {
        val current = client.getCurrent()
        current.language?.tag ?: "en"
    } catch (e: LanguagePackException) {
        if (e.message?.contains("2002") == true) {
            // No language pack set -- install English as default
            try {
                client.setCurrent("en")
                "en"
            } catch (setError: LanguagePackException) {
                throw setError
            }
        } else {
            throw e
        }
    }
}
```

## Important Notes

1. **Requires HzOS v85+** -- both `getCurrent()` and `setCurrent()` require HzOS v85 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

2. **`setCurrent()` triggers a download** -- calling `setCurrent()` initiates a file download. The method returns immediately with an `AssetFileDownloadResult`, but the actual download continues in the background. Use `AssetFile.downloadUpdate()` to track progress if needed.

3. **BCP47 language tags** -- the `tag` parameter for `setCurrent()` must be a valid BCP47 language tag (e.g., `"en"`, `"fr"`, `"de"`, `"ja"`, `"es"`). Invalid tags return status code 2004 (`LanguagePackInvalidTag`).

4. **Handle status code 2002 on first use** -- if no language pack has been set for the app, `getCurrent()` throws with status code 2002 (`LanguagePackNotSet`). This is expected on first launch. Call `setCurrent()` to install a language pack before calling `getCurrent()`.

5. **Duplicate set calls return 2003** -- calling `setCurrent()` with the language tag that is already installed throws with status code 2003 (`LanguagePackDuplicate`). This is informational, not an error -- the desired language is already active.

6. **Cross-package dependency for download tracking** -- tracking download progress requires the Asset File SDK (`horizon-platform-sdk-asset-file-kotlin`) in addition to the Language Pack SDK. The `AssetFile.downloadUpdate()` method returns a `Flow<AssetFileDownloadUpdate>` that emits progress updates.

7. **No pagination, events, or sessions** -- this is a simple request/response API. Each call is independent and stateless. Download progress tracking is the only stateful interaction, and it uses a separate `AssetFile` client.
