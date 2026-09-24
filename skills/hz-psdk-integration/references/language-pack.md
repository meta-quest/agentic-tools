# Language Pack API

- **Kotlin Package**: `horizon.platform.languagepack`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-language-packs
- **Minimum OS**: HzOS v85
- **Maven Artifact**: `horizon-platform-sdk-language-pack-kotlin`

- [Overview](#overview)
- [Setup](#setup)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## Overview

The Language Pack API provides two operations for Meta VR Android apps:

1. **`getCurrent()`** -- Retrieve details about the currently installed language pack
2. **`setCurrent(tag)`** -- Set (download and install) a language pack by its BCP47 language tag

Setting a language pack triggers a download. The SDK provides a companion API (`AssetFile.downloadUpdate()`) to track download progress in real time.

## Setup

For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

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

    val languageTag = result.language?.tag          // BCP47 tag, e.g. "en"
    val englishName = result.language?.englishName  // e.g. "English"
    val nativeName = result.language?.nativeName    // e.g. "English"
    val filePath = result.filepath                  // Local file path
    val assetId = result.assetId                    // Unique asset identifier
    val version = result.versionCode                // Version code
} catch (e: LanguagePackException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `AssetDetails` -- an immutable object containing asset metadata and language information.

#### Set the Current Language Pack

Download and install a specific language pack by its BCP47 language tag.

```kotlin
import horizon.platform.languagepack.LanguagePack
import horizon.platform.languagepack.LanguagePackException
import horizon.platform.assetfile.models.AssetFileDownloadResult

val languagePack = LanguagePack()

try {
    val result: AssetFileDownloadResult = languagePack.setCurrent("fr")

    val assetId = result.assetId    // Use this to track download progress
    val filePath = result.filepath  // File path where the asset will be stored
} catch (e: LanguagePackException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `tag: String` -- A BCP47 language tag (e.g., `"en"`, `"fr"`, `"de"`, `"es"`, `"ja"`)
**Return type**: `AssetFileDownloadResult` -- contains the asset ID and file path for the initiated download

#### Track Download Progress (Optional)

After calling `setCurrent()`, track the download progress using the Asset File API:

```kotlin
import horizon.platform.assetfile.AssetFile
import horizon.platform.assetfile.models.AssetFileDownloadUpdate

val assetFile = AssetFile()

// Use the asset ID from setCurrent()
assetFile.downloadUpdate(assetId).collect { update: AssetFileDownloadUpdate ->
    val progressPercent =
        if (update.bytesTotal > 0) (update.bytesTransferred * 100 / update.bytesTotal) else 0
    val isComplete = update.completed
}
```

**Return type**: `Flow<AssetFileDownloadUpdate>` -- a Kotlin Flow that emits download progress updates

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

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

#### Language Pack-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `LanguagePackNotInstalled` | 2001 | No language pack is installed | Call `setCurrent()` to install one first |
| `LanguagePackNotSet` | 2002 | Language pack not set for this app | Call `setCurrent()` to set a language pack |
| `LanguagePackDuplicate` | 2003 | Requested language pack is already set | No action needed; current pack matches request |
| `LanguagePackInvalidTag` | 2004 | Invalid BCP47 language tag | Verify the language tag is valid and supported |
| `LanguagePackNotAvailable` | 2005 | Language pack not available | The requested language is not available for this app |

## Important Notes

1. **Requires HzOS v85+** -- both `getCurrent()` and `setCurrent()` require HzOS v85 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

2. **`setCurrent()` triggers a download** -- it returns immediately with an `AssetFileDownloadResult`; the download continues in the background. Use `AssetFile.downloadUpdate()` to track progress.

3. **BCP47 language tags** -- the `tag` parameter for `setCurrent()` must be a valid BCP47 language tag. Invalid tags return status code 2004 (`LanguagePackInvalidTag`).

4. **Handle status code 2002 on first use** -- if no language pack has been set, `getCurrent()` throws with 2002 (`LanguagePackNotSet`), expected on first launch. Call `setCurrent()` first.

5. **Duplicate set calls return 2003** -- calling `setCurrent()` with the already-installed tag throws 2003 (`LanguagePackDuplicate`); informational, not an error.

6. **Cross-package dependency for download tracking** -- requires the Asset File SDK (`horizon-platform-sdk-asset-file-kotlin`) in addition to the Language Pack SDK. `AssetFile.downloadUpdate()` returns a `Flow<AssetFileDownloadUpdate>`.
