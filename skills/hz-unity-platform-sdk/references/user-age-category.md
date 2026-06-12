# User Age Category API

- **Unity Package**: `com.meta.xr.sdk.platform`
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-get-age-category-api/
- **Namespace**: `Oculus.Platform`

## Overview

The User Age Category API is part of the Horizon Platform SDK. It exposes the platform's age classification of the signed-in user and lets your app report back the age category your app determined (e.g., from an in-app birthday entry). It provides two operations:

1. **`UserAgeCategory.Get()`** -- Get the platform's classification of the signed-in user (Child, Teen, Adult)
2. **`UserAgeCategory.Report(category)`** -- Report your app's age determination back to the platform (Child, Non-child)

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Why This Matters

| | |
|---|---|
| **COPPA/COPRA compliance** | Apps must not collect data from children under 13 without verifiable parental consent. Knowing the user's age category lets you gate features automatically. |
| **Age-appropriate content** | Tune voice chat, social features, content rating based on age. |
| **Two-way exchange** | The platform tells you (`Get`) and you tell the platform (`Report`) so the platform can correlate. |

## API Usage

#### Get the User's Account Age Category

```csharp
public async Task<AccountAgeCategory> GetUserAgeCategory()
{
    if (!Core.IsInitialized()) return AccountAgeCategory.Unknown;

    var msg = await UserAgeCategory.Get();
    if (msg.IsError)
    {
        Debug.LogError($"UserAgeCategory.Get: {msg.GetError().Message}");
        return AccountAgeCategory.Unknown;
    }
    return msg.Data.AgeCategory;
}
```

**Return type**: `Request<UserAccountAgeCategory>`

#### Gate Features Based on Age

```csharp
public async Task ConfigureFeaturesByAge()
{
    var age = await GetUserAgeCategory();

    switch (age)
    {
        case AccountAgeCategory.Ch:
            DisableVoiceChat();
            DisableUserGeneratedContent();
            DisableTargetedAds();
            UseChildSafeContentFilter();
            break;
        case AccountAgeCategory.Tn:
            EnableVoiceChat();
            DisableUserGeneratedContent();   // or apply moderation
            EnableLimitedSocialFeatures();
            break;
        case AccountAgeCategory.Ad:
            EnableAllFeatures();
            break;
        case AccountAgeCategory.Unknown:
        default:
            // Default to most restrictive -- safer for unknown users
            UseConservativeDefaults();
            break;
    }
}
```

> **When `Unknown`, default to the most restrictive setting.** It's safer to assume "could be a minor" than to expose age-restricted features by accident.

#### Report Your App's Age Category Determination

If your app determines age independently (e.g., the user enters a birthday), report back so the platform has consistent data:

```csharp
public async Task ReportAge(bool isChild)
{
    if (!Core.IsInitialized()) return;

    var category = isChild ? AppAgeCategory.Ch : AppAgeCategory.Nch;
    var msg = await UserAgeCategory.Report(category);
    if (msg.IsError)
    {
        Debug.LogError($"UserAgeCategory.Report: {msg.GetError().Message}");
    }
}
```

**Parameters**: `category: AppAgeCategory` -- `Ch` (child) or `Nch` (non-child)

> Note `AppAgeCategory` only has child / non-child -- it's a coarser bucket than `AccountAgeCategory`. The platform takes care of finer breakdowns on its side.

## Complete Age Gate Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AgeGateManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    public AccountAgeCategory CurrentCategory { get; private set; } = AccountAgeCategory.Unknown;
    public bool IsChild => CurrentCategory == AccountAgeCategory.Ch;
    public bool IsAdult => CurrentCategory == AccountAgeCategory.Ad;

    public event Action<AccountAgeCategory> AgeCategoryDetermined;

    private bool isInitialized;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }
        isInitialized = true;

        await Refresh();
    }

    public async Task Refresh()
    {
        if (!isInitialized) return;
        var msg = await UserAgeCategory.Get();
        CurrentCategory = msg.IsError ? AccountAgeCategory.Unknown : msg.Data.AgeCategory;
        AgeCategoryDetermined?.Invoke(CurrentCategory);
    }

    public async Task ReportAppDeterminedAge(bool isChildPerApp)
    {
        if (!isInitialized) return;
        var category = isChildPerApp ? AppAgeCategory.Ch : AppAgeCategory.Nch;
        await UserAgeCategory.Report(category);
    }
}
```

## Data Types

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `UserAgeCategory.Get()` | `Request<UserAccountAgeCategory>` | Get the platform's classification of the signed-in user |
| `UserAgeCategory.Report(category)` | `Request` | Report your app's determination back to the platform |

### Models

| Type | Key Fields |
|------|------------|
| `UserAccountAgeCategory` | `AgeCategory` (`AccountAgeCategory`) |

### Enums

| Enum | Values |
|------|--------|
| `AccountAgeCategory` | `Ch`, `Tn`, `Ad`, `Unknown` |
| `AppAgeCategory` | `Ch`, `Nch`, `Unknown` |

### AccountAgeCategory Values (Platform to App)

| Value | Meaning |
|-------|---------|
| `Ch` | Child (10-12 in most regions) |
| `Tn` | Teen (13-17 in most regions) |
| `Ad` | Adult (18+ in most regions) |
| `Unknown` | Not determinable |

### AppAgeCategory Values (App to Platform)

| Value | Meaning |
|-------|---------|
| `Ch` | Child (your app determined the user is a child) |
| `Nch` | Non-child (13+) |
| `Unknown` | Reserved |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Defaulting to "Adult" when category is Unknown | Default to most restrictive. "Could be a minor" is the safe assumption. |
| Treating `AccountAgeCategory.Tn` like `Ad` | Teens get a different feature set per regulation. Don't enable adult-only features for them. |
| Using `AccountAgeCategory` values when calling `Report` | `Report` takes `AppAgeCategory` (Ch/Nch). Don't conflate the enums. |
| Storing the age category in PlayerPrefs and not refreshing | The user's age can change (birthday, account migration). Re-query at app start. |
| Skipping the report step when your app collects age | If you collect a birthday in-app, calling `Report` gives the platform consistent signals across apps. |

## Important Notes

1. **When category is `Unknown`, default to the most restrictive feature set.** Don't risk exposing age-restricted features. Re-query age category at every app start; don't cache long-term.

2. **Feature gating by category**: Use `Ch` to disable voice chat, UGC, targeted ads, and apply child-safe content filtering. Use `Tn` to enable a limited social feature set (per regulatory requirements in your region). Use `Ad` for the full feature surface.

3. **Don't conflate `AppAgeCategory` with `AccountAgeCategory`** -- `AppAgeCategory` (Ch/Nch) is what you tell the platform; `AccountAgeCategory` (Ch/Tn/Ad/Unknown) is what the platform tells you.

4. **COPPA/COPRA and similar regulations** require careful handling. Consult your legal/compliance team for your specific feature set. Always restrict, never enable, based on age signals.

5. **Often paired with the Consent API** -- age-gating and consent flows frequently work together.

## Useful Links

- [Meta Quest User Age Category Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-get-age-category-api/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Platform SDK Overview](https://developer.oculus.com/documentation/unity/ps-platform-intro/)
