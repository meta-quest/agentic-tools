# Rate and Review API

- **Unity Package**: `com.meta.xr.sdk.platform`
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-rate-and-review/
- **Namespace**: `Oculus.Platform`

## Overview

The Rate and Review API is part of the Horizon Platform SDK. It surfaces the Meta Horizon Store's rating dialog from inside your app -- no leaving the experience. It provides two operations:

1. **`RateAndReview.CanLaunchRateAndReview()`** -- Check if the user is eligible to be asked (hasn't recently reviewed, hasn't been asked too often)
2. **`RateAndReview.RateAndReviewLauncher()`** -- Show the system rating dialog

The platform tracks when the user last reviewed/dismissed and gates eligibility automatically. You don't need to track it yourself.

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## API Usage

#### Check Eligibility Before Asking

Always call `CanLaunchRateAndReview` before showing the prompt. The platform decides whether the user is eligible.

```csharp
public async Task<bool> CanAskForReview()
{
    if (!Core.IsInitialized()) return false;

    var msg = await RateAndReview.CanLaunchRateAndReview();
    if (msg.IsError) return false;
    return msg.Data.Result;
}
```

**Return type**: `Request<ApplicationCanViewerRateAndReview>`

#### Launch the Rating UI

```csharp
public async Task<bool> AskForReview()
{
    if (!Core.IsInitialized()) return false;

    if (!await CanAskForReview())
    {
        Debug.Log("User not eligible for review prompt right now");
        return false;
    }

    var msg = await RateAndReview.RateAndReviewLauncher();
    if (msg.IsError)
    {
        Debug.LogError($"RateAndReviewLauncher: {msg.GetError().Message}");
        return false;
    }
    Debug.Log("Rating UI launched");
    return true;
}
```

> **The platform handles the dialog** -- it shows the rating UI, captures the rating, and sends it to the Store. You don't get the result back.

#### Pick the Right Moment

Ask **after** positive moments:

- After completing a level / boss fight
- After unlocking an achievement
- After a successful multiplayer session
- After the user manually shares something
- After N successful sessions over time

Avoid:

- During gameplay
- After errors or crashes
- On first launch
- Multiple times in one session

## Complete Rate and Review Trigger

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ReviewPromptTrigger : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private int sessionsBeforeFirstAsk = 3;

    private bool isInitialized;
    private const string SessionCountKey = "review_session_count";
    private const string LastAskedKey = "review_last_asked_unix";

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        isInitialized = !msg.IsError;

        // Track session counts locally so we don't hit the platform's eligibility
        // endpoint every session
        int sessions = PlayerPrefs.GetInt(SessionCountKey, 0) + 1;
        PlayerPrefs.SetInt(SessionCountKey, sessions);
        PlayerPrefs.Save();
    }

    /// <summary>Call after a positive in-game moment (level complete, achievement, etc).</summary>
    public async Task TryPromptAfterPositiveMoment()
    {
        if (!isInitialized) return;

        int sessions = PlayerPrefs.GetInt(SessionCountKey, 0);
        if (sessions < sessionsBeforeFirstAsk) return;

        long lastAsked = long.Parse(PlayerPrefs.GetString(LastAskedKey, "0"));
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now - lastAsked < 60 * 60 * 24 * 30) return; // don't re-ask within 30 days

        var canMsg = await RateAndReview.CanLaunchRateAndReview();
        if (canMsg.IsError || !canMsg.Data.Result) return;

        var launchMsg = await RateAndReview.RateAndReviewLauncher();
        if (!launchMsg.IsError)
        {
            PlayerPrefs.SetString(LastAskedKey, now.ToString());
            PlayerPrefs.Save();
        }
    }
}
```

> **Why local throttling on top of platform throttling?** The platform throttle is conservative. Adding your own "ask after a positive moment, never within 30 days" gate makes the prompt feel earned rather than automated. If your local gate says "yes" but the platform says "no", the platform wins -- which is fine.

## Data Types

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `RateAndReview.CanLaunchRateAndReview()` | `Request<ApplicationCanViewerRateAndReview>` | Check if the user is eligible to be asked |
| `RateAndReview.RateAndReviewLauncher()` | `Request` | Show the system rating UI |

### Models

| Type | Key Fields |
|------|------------|
| `ApplicationCanViewerRateAndReview` | `Result` (bool) |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Skipping `CanLaunchRateAndReview` | Always check first. The platform throttles aggressively to avoid spamming users. |
| Asking on first launch | Bad UX. Wait for a positive moment after the user has invested some time. |
| Asking during a struggle (after a death, error, etc.) | Negative bias on the rating. Ask after wins, not losses. |
| Asking multiple times in one session | The platform will likely throttle, but even if it doesn't, the user will. Add your own per-session gate. |
| Treating the launcher result as the user's rating | You don't get the rating. The platform handles it. |
| Pre-checking and immediately launching without waiting for the right moment | Couple the eligibility check with a moment-detection trigger (level complete, achievement, etc.). |

## Important Notes

1. **Always call `CanLaunchRateAndReview` before launching.** The platform tracks when users last reviewed/dismissed and won't surface the dialog otherwise.

2. **Trigger after positive moments**: level complete, achievement unlock, win, share. Avoid: first launch, mid-task, after errors, after deaths.

3. **You don't get the user's rating from the API.** The platform handles the UI and submits to the Store. Treat `RateAndReviewLauncher` as fire-and-forget once you confirm eligibility.

4. **Add your own per-session and per-N-days gating** on top of platform throttling for a better user experience.

## Useful Links

- [Meta Quest Rate and Review Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-rate-and-review/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Platform SDK Overview](https://developer.oculus.com/documentation/unity/ps-platform-intro/)
