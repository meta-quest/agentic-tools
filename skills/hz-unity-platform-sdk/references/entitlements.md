# Entitlements API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-entitlement-check/
- **Namespace**: Oculus.Platform

## Overview

1. Verify the user legitimately owns the app (anti-piracy check)
2. Required for every Quest app published to the Meta Horizon Store
3. Must complete within 10 seconds of app launch
4. Single API call: `Entitlements.GetIsViewerEntitled()`

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Why This Matters

| | |
|---|---|
| **Required by the Store** | Every Quest app submission must implement an entitlement check or it will fail VRC review. |
| **Anti-piracy** | Prevents sideloaded copies from running for users who didn't purchase. |
| **10-second SLA** | The check **must complete within 10 seconds** of app launch. |
| **Works offline** | The check does not require internet. The platform caches entitlement state locally. |
| **Single API call** | Only one method: `Entitlements.GetIsViewerEntitled()`. |

## API Usage

### Recommended: Standalone Bootstrap MonoBehaviour

The recommended pattern is to perform the entitlement check **in your very first `MonoBehaviour.Awake()`** (or `Start`), and quit the app immediately on failure.

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using UnityEngine;

public class EntitlementGate : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    async void Awake()
    {
        // 1) Initialize the platform
        try
        {
            Message<PlatformInitialize> initMsg = await Core.AsyncInitialize(appId);
            if (initMsg.IsError)
            {
                Debug.LogError($"Platform init failed: {initMsg.GetError().Message}");
                FailEntitlement("Platform init failed");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            FailEntitlement("Platform init threw");
            return;
        }

        // 2) Check entitlement (must complete within 10s of launch)
        try
        {
            Message msg = await Entitlements.GetIsViewerEntitled();
            if (msg.IsError)
            {
                Debug.LogError($"Entitlement check failed: {msg.GetError().Message}");
                FailEntitlement(msg.GetError().Message);
                return;
            }
            Debug.Log("User is entitled to this app.");
            // App is good to go -- load your real start scene here, or set a flag.
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            FailEntitlement("Entitlement check threw");
        }
    }

    private void FailEntitlement(string reason)
    {
        Debug.LogError($"Entitlement failure: {reason}. Quitting.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
```

Place this `EntitlementGate` on a GameObject in your **first loaded scene** (e.g., a "Boot" scene) so it runs before any gameplay code.

### Alternative: Callback Pattern

```csharp
void Start()
{
    Core.AsyncInitialize(appId).OnComplete(initMsg =>
    {
        if (initMsg.IsError) { FailEntitlement(initMsg.GetError().Message); return; }
        Entitlements.GetIsViewerEntitled().OnComplete(checkMsg =>
        {
            if (checkMsg.IsError) FailEntitlement(checkMsg.GetError().Message);
            else Debug.Log("Entitled");
        });
    });
}
```

### The 10-Second Rule

The Horizon Store requires that **the entitlement check complete within 10 seconds of app launch**. If your app takes longer to load (e.g., a heavy first scene), perform the check on a lightweight bootstrap scene first, then load gameplay assets.

> **Never block the main thread waiting for entitlement.** Use `async/await` (or `OnComplete`) so the Unity update loop continues. The 10-second budget is wall-clock from app launch, not from your code.

### Failure Handling

When the entitlement check fails, the recommended behavior is:

1. **Display a brief message** explaining that the user is not entitled (optional, but improves UX).
2. **Quit the app** with `Application.Quit()`.

> Don't try to "soft-fail" by hiding features -- the Store policy requires the app exit on entitlement failure. Soft-failures will fail VRC review.

```csharp
private void FailEntitlement(string reason)
{
    PlayerPrefs.SetString("LastEntitlementError", reason);
    PlayerPrefs.Save();
    Application.Quit();
}
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Entitlements.GetIsViewerEntitled()` | `Request` | Returns a non-error result if the user is entitled to the current app |

> **Note**: `Request` (no generic type parameter) -- the result has no payload. Success is signaled by `IsError == false`. Failure is signaled by `IsError == true` and an error message in `GetError()`.

## Error Handling

| Mistake | Fix |
|---------|-----|
| Skipping the entitlement check | **Required by Horizon Store**. Submission will fail VRC review without it. |
| Performing the check after gameplay loads | The 10-second budget is from app launch. Run the check from your bootstrap scene as early as possible (`Awake`/`Start` of the boot scene). |
| Soft-failing on entitlement failure (hiding features instead of quitting) | The Store policy requires `Application.Quit()`. Soft-fails fail VRC review. |
| Calling `Entitlements.GetIsViewerEntitled` before `Core.AsyncInitialize` | Always init first, then check entitlement. |
| Blocking the main thread waiting for the check | Use `async/await` or `OnComplete`. The 10s budget is wall-clock, not code-time. |
| Forgetting to handle init errors as entitlement failures | If `Core.AsyncInitialize` fails, treat it as an entitlement failure and quit. |
| Hardcoding the App ID without verifying it matches the dashboard | A wrong App ID always fails entitlement, even for legitimate users. |
| Testing only in the Editor | The Editor uses a test user. Always verify on a real device with a real signed-in account before submission. |

## Examples

### Example 1: Minimal Entitlement Gate

```csharp
using Oculus.Platform;
using UnityEngine;

public class MinimalEntitlementGate : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    async void Awake()
    {
        var initMsg = await Core.AsyncInitialize(appId);
        if (initMsg.IsError) { Application.Quit(); return; }

        var checkMsg = await Entitlements.GetIsViewerEntitled();
        if (checkMsg.IsError) { Application.Quit(); return; }

        Debug.Log("Entitled -- loading game...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
```

### Example 2: Entitlement with User Feedback

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using UnityEngine;
using UnityEngine.UI;

public class EntitlementWithFeedback : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private Text statusText;

    async void Awake()
    {
        if (statusText != null) statusText.text = "Verifying...";

        try
        {
            var initMsg = await Core.AsyncInitialize(appId);
            if (initMsg.IsError)
            {
                ShowErrorAndQuit("Could not connect to platform services.");
                return;
            }

            var checkMsg = await Entitlements.GetIsViewerEntitled();
            if (checkMsg.IsError)
            {
                ShowErrorAndQuit("You are not entitled to this app. Please purchase it from the Meta Horizon Store.");
                return;
            }

            if (statusText != null) statusText.text = "Verified!";
            Invoke(nameof(LoadMainMenu), 0.5f);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            ShowErrorAndQuit("An unexpected error occurred.");
        }
    }

    private void ShowErrorAndQuit(string message)
    {
        Debug.LogError(message);
        if (statusText != null) statusText.text = message;
        Invoke(nameof(QuitApp), 3f);
    }

    private void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
```

## Important Notes

- **Required for every Quest app**: Submitting without an entitlement check will fail VRC review.
- **10-second wall-clock deadline**: The check must complete within 10 seconds of app launch. Use a lightweight bootstrap scene.
- **Must quit on failure**: `Application.Quit()` is required. Do not soft-fail by hiding features.
- **Works offline**: The platform caches entitlement state locally. No need to special-case offline scenarios.
- **Editor behavior**: The check runs against the configured Standalone Platform test user. Configure via **Meta > Platform > Edit Settings**.
- **Sideloaded debug builds**: The check runs against the signed-in Quest account. Developer accounts are entitled automatically.
- **Store builds**: The check runs against the user's purchase records. Cached, so works offline.
- **Sample tester**: `samples/unity/Baremetal/Assets/SamplesInternal/entitlements/EntitlementsTester.cs`
- [Virtual Reality Checks (VRC)](https://developer.oculus.com/resources/publish-quest-req/)
