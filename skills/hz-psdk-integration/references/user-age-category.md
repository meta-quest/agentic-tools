# User Age Category API

| Field | Value |
|-------|-------|
| **Kotlin Package** | `horizon.platform.useragecategory` |
| **Documentation** | https://developers.meta.com/horizon/documentation/android-apps/ps-get-age-category-api |
| **Minimum OS** | HzOS v81 (`get()`), HzOS v85 (`report()`) |
| **Maven Artifact** | `horizon-platform-sdk-user-age-category-kotlin` |

> For setup, initialization, and client instantiation, see [common-setup.md](common-setup.md).

## Contents
- [Overview](#overview)
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Examples](#examples)
- [Important Notes](#important-notes)

## Overview

Two operations:

1. **`get()`** -- Retrieve the current user's age category from their Meta account
2. **`report()`** -- Report the app's own determination of the user's age category back to Meta

## API Usage

### Retrieve the User's Age Category

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AccountAgeCategory
import horizon.platform.useragecategory.models.UserAccountAgeCategory

val userAgeCategory = UserAgeCategory()

try {
    val result: UserAccountAgeCategory = userAgeCategory.get()

    when (result.ageCategory) {
        AccountAgeCategory.Ch -> { /* Child: ages 10-12 (or applicable age in region) */ }
        AccountAgeCategory.Tn -> { /* Teen: ages 13-17 (or applicable age in region) */ }
        AccountAgeCategory.Ad -> { /* Adult: ages 18+ (or applicable age in region) */ }
        AccountAgeCategory.Unknown -> {
            // Could not be determined -- treat conservatively (most restrictive policy)
        }
    }
} catch (e: UserAgeCategoryException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `UserAccountAgeCategory` -- immutable, with `ageCategory: AccountAgeCategory` (defaults to `AccountAgeCategory.Unknown`).

**Available since**: SDK v0.1.3 (HzOS v81)

### Report the User's Age Category to Meta

Use when your app independently verifies or determines a user's age group.

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AppAgeCategory

val userAgeCategory = UserAgeCategory()

try {
    userAgeCategory.report(AppAgeCategory.Ch)   // child (ages 10-12)
    // OR
    userAgeCategory.report(AppAgeCategory.Nch)  // non-child (ages 13+)
} catch (e: UserAgeCategoryException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `ageCategory: AppAgeCategory`
**Returns**: `Unit` -- success is indicated by no exception being thrown

**Available since**: SDK v0.2.1 (HzOS v85)

## Data Types

### `AccountAgeCategory` Enum (returned by `get()`)

Three-tier classification from Meta's account data:

| Value | Code | Age Range | Description |
|-------|------|-----------|-------------|
| `Unknown` | 0 | N/A | Age category could not be determined |
| `Ch` | 1 | 10-12 | Child (or applicable age in user's region) |
| `Tn` | 2 | 13-17 | Teenager (or applicable age in user's region) |
| `Ad` | 3 | 18+ | Adult (or applicable age in user's region) |

### `AppAgeCategory` Enum (parameter for `report()`)

Simplified two-tier classification for app reporting:

| Value | Code | Age Range | Description |
|-------|------|-----------|-------------|
| `Unknown` | 0 | N/A | Unknown |
| `Ch` | 1 | 10-12 | Child (or applicable age in user's region) |
| `Nch` | 2 | 13+ | Non-child (or applicable age in user's region) |

**Key distinction**: `AccountAgeCategory` (from `get()`) has three groups (Child/Teen/Adult); `AppAgeCategory` (for `report()`) has two (Child/Non-Child). The asymmetry is intentional -- apps only need to report whether a user is a child or not.

### `UserAccountAgeCategory` Model

Immutable model returned by `get()`:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ageCategory` | `AccountAgeCategory` | `AccountAgeCategory.Unknown` | The user's age category |

## Error Handling

Both `get()` and `report()` throw `UserAgeCategoryException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

This package defines no package-specific status codes beyond the common set. See [common-setup.md](common-setup.md) for the full common status codes table.

## Examples

### Example 1: Basic Age-Gating

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AccountAgeCategory

suspend fun shouldShowMatureContent(): Boolean {
    val client = UserAgeCategory()
    return try {
        client.get().ageCategory == AccountAgeCategory.Ad
    } catch (e: UserAgeCategoryException) {
        false  // On error, default to restricting content
    }
}
```

### Example 2: Handling OS Version Compatibility for `report()`

Gracefully handle the case where `report()` is not available on older devices.

```kotlin
import horizon.platform.useragecategory.UserAgeCategory
import horizon.platform.useragecategory.UserAgeCategoryException
import horizon.platform.useragecategory.enums.AppAgeCategory

suspend fun safeReport(ageCategory: AppAgeCategory): Boolean {
    val client = UserAgeCategory()
    return try {
        client.report(ageCategory)
        true
    } catch (e: UserAgeCategoryException) {
        if (e.message?.contains("1003") == true) {
            // ProviderOperationNotSupported -- OS version too old for report()
            false
        } else {
            throw e
        }
    }
}
```

## Important Notes

1. **Age ranges are region-dependent** -- exact age boundaries for Child, Teen, and Adult may vary by region. Do not hardcode age thresholds in app logic.

2. **Handle `Unknown` conservatively** -- if `get()` returns `AccountAgeCategory.Unknown`, apply the most restrictive content policy appropriate for your app.

3. **`report()` requires HzOS v85+**, **`get()` requires HzOS v81+** -- on older OS versions the unsupported method returns status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle 1003 at runtime.

4. **Two enum types serve different purposes** -- `AccountAgeCategory` (3 groups: CH/TN/AD) is what Meta tells the app; `AppAgeCategory` (2 groups: CH/NCH) is what the app tells Meta. Do not confuse them.
