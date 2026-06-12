# Common Setup and Shared Reference

This document covers setup steps, initialization code, Editor testing, and common patterns shared across all Horizon Platform SDK Unity packages. Read this first before reading any specific API reference file.

## Step 1: Install the Package

Install `com.meta.xr.sdk.platform` via the Unity Package Manager (UPM). This is the single package that provides all platform APIs.

- **UPM Package**: `com.meta.xr.sdk.platform`
- **Setup documentation**: https://developers.meta.com/horizon/documentation/unity/ps-platform-intro/

## Step 2: Namespace and Imports

All Platform SDK types live under two namespaces:

```csharp
using Oculus.Platform;        // Static API classes (Leaderboards, IAP, Users, etc.), enums, options
using Oculus.Platform.Models;  // Response data models (User, Product, LeaderboardEntry, etc.)
```

The `Oculus.Platform` namespace is maintained for backward compatibility with the legacy LibOVRPlatform SDK.

## Step 3: Initialize the Platform

Before calling any SDK API, you **must** initialize the platform. `Core.IsInitialized()` gates every API call — calling any method before init returns `null`.

### Async/Await (Recommended)

```csharp
[SerializeField] private string appId = "YOUR_APP_ID";

async void Start()
{
    try
    {
        Message<PlatformInitialize> msg = await Core.AsyncInitialize(appId);
        if (msg.IsError)
        {
            Debug.LogError($"Platform init failed: {msg.GetError().Message}");
            return;
        }
        Debug.Log("Platform initialized");
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

### Callback Pattern

```csharp
void Start()
{
    Core.AsyncInitialize(appId).OnComplete(msg =>
    {
        if (msg.IsError)
        {
            Debug.LogError("Platform init failed");
            return;
        }
        Debug.Log("Platform initialized");
    });
}
```

### Editor Testing (Standalone Platform Mode)

In the Unity Editor, the SDK routes through `WindowsClient` (P/Invoke to a native DLL) instead of `AndroidClient` (JNI). To test:

1. Open **Meta > Platform > Edit Settings**
2. Check **Use Standalone Platform**
3. Enter test user email/password and click Login
4. Optionally check "Use Meta Quest App ID over Rift App ID in Editor"

Or initialize with a runtime mode parameter:

```csharp
#if UNITY_EDITOR
    var msg = await Core.AsyncInitialize(appId, "standalone");
#else
    var msg = await Core.AsyncInitialize(appId);
#endif
```

## Step 4: Request Handling Patterns

All SDK methods return `Request` or `Request<T>`. Both `async/await` and callback patterns are supported.

### Async/Await (Recommended for New Code)

```csharp
var msg = await Leaderboards.GetEntries("my_leaderboard", 10, LeaderboardFilterType.None, LeaderboardStartAt.Top);
if (msg.IsError)
{
    Debug.LogError($"Failed: {msg.GetError().Message}");
    return;
}
foreach (var entry in msg.Data) { /* ... */ }
```

### Callback Pattern

```csharp
Leaderboards.GetEntries("my_leaderboard", 10, LeaderboardFilterType.None, LeaderboardStartAt.Top)
    .OnComplete(msg =>
    {
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }
        foreach (var entry in msg.Data) { /* ... */ }
    });
```

### Error Checking

Always check `msg.IsError` before accessing `msg.Data`. Wrap async calls in `try/catch`:

```csharp
try
{
    var msg = await SomeApi.SomeMethod();
    if (msg.IsError) { /* handle error */ return; }
    // use msg.Data
}
catch (Exception e) { Debug.LogException(e); }
```

### Pagination

Many list APIs return paginated results. Check `HasNextPage` / `HasPreviousPage` and use the corresponding `GetNext*Page` / `GetPrevious*Page` methods:

```csharp
var msg = await SomeApi.GetList();
var page = msg.Data;
while (page.HasNextPage)
{
    var nextMsg = await SomeApi.GetNextPage(page);
    if (nextMsg.IsError) break;
    page = nextMsg.Data;
}
```

### Notification Callbacks (Event Streams)

Some APIs provide notification callbacks for platform events (join intents, report button presses, download progress). These must be registered **immediately after init**, before any pending events deliver:

```csharp
async void Start()
{
    var msg = await Core.AsyncInitialize(appId);
    if (msg.IsError) return;

    // Register callbacks immediately
    GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntent);
    ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(OnIntentChanged);
    AssetFile.SetDownloadUpdateNotificationCallback(OnDownloadUpdate);
}
```

## Async Return Type Rules

The SDK's `Request<T>` is natively awaitable (it implements `GetAwaiter()` via `TaskCompletionSource`). The continuation after `await` runs on the Unity main thread — no `SynchronizationContext` worries.

Use these rules when writing async code with the Platform SDK:

| Method type | Return type | Why |
|------------|-------------|-----|
| Unity lifecycle (`Start`, `Awake`) | `async void` | Unity calls these — the signature is fixed |
| `UnityEvent` button handlers (`OnClick`) | `async void` | Unity's event system requires void |
| Reusable public methods (called by other scripts) | `async Task` or `async Task<T>` | Callers need to `await`, observe errors, chain calls |
| Notification callbacks (platform events) | `void` (not async) | Platform calls these with a `Message<T>` argument |

**Example — the right structure for a real app:**

```csharp
public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private string leaderboardName = "high_scores";

    private bool isInitialized;

    // Unity lifecycle — async void is correct here
    async void Start()
    {
        isInitialized = await InitializeAsync();
    }

    // Reusable method — returns Task<bool> so callers can await and check the result
    private async Task<bool> InitializeAsync()
    {
        try
        {
            var msg = await Core.AsyncInitialize(appId);
            return !msg.IsError;
        }
        catch (Exception e) { Debug.LogException(e); return false; }
    }

    // Public API — returns Task<T> so game code can: var entries = await mgr.GetTopEntriesAsync();
    public async Task<List<LeaderboardEntry>> GetTopEntriesAsync(int limit = 25)
    {
        if (!isInitialized) return new();
        var msg = await Leaderboards.GetEntries(leaderboardName, limit, LeaderboardFilterType.None, LeaderboardStartAt.Top);
        if (msg.IsError) return new();
        return new List<LeaderboardEntry>(msg.Data);
    }

    // Button handler — async void is OK since UnityEvent requires void
    public async void OnSubmitScoreClicked()
    {
        bool success = await SubmitScoreAsync(currentScore);
        resultText.text = success ? "Submitted!" : "Failed";
    }

    // Reusable method backing the button handler
    public async Task<bool> SubmitScoreAsync(long score)
    {
        if (!isInitialized) return false;
        var msg = await Leaderboards.WriteEntry(leaderboardName, score);
        return !msg.IsError && msg.Data;
    }
}
```

**Why this matters:** `async void` swallows exceptions silently — if a `Task`-returning method throws, the caller sees it. If an `async void` method throws, the exception goes to the `UnityEngine` unhandled exception handler and is easy to miss. In a real app, your game logic (`GameManager`, `SessionController`, etc.) will `await` the platform manager methods and handle errors, so those methods must return `Task`.

## Common Coding Rules

1. **Always init first**: `Core.AsyncInitialize(appId)` before any API call. Gate with `Core.IsInitialized()`.
2. **Prefer async/await** over callbacks for new code.
3. **Always check `msg.IsError`** before accessing `msg.Data`.
4. **Return `Task`/`Task<T>` from reusable methods** — only use `async void` for Unity lifecycle methods and button handlers.
5. **API names are case-sensitive**: leaderboard names, achievement names, product SKUs — must match the Developer Dashboard exactly.
6. **IDs are app-scoped**: User IDs are unique per app. Use `Users.GetOrgScopedID` for cross-app identity within the same org.
7. **Register notification callbacks immediately after init**: Don't await other work first.
8. **Handle nullability**: Many model fields are nullable (`DisplayName`, `DisplayScore`, optional model fields ending in `Optional`).

## Common Error Patterns

| Symptom | Cause | Fix |
|---------|-------|-----|
| API returns `null` | `Core.IsInitialized()` is false | Call `Core.AsyncInitialize` first |
| `msg.IsError == true` with no details | Platform service not connected | Ensure HzPlatformService is installed on device |
| Editor tests fail silently | Standalone mode not configured | Set up test user in **Meta > Platform > Edit Settings** |
| Method works on device but not in Editor | Editor uses WindowsClient, not AndroidClient | Some features may not be available in Editor |

## Useful Links

- [Platform SDK Overview (Unity)](https://developer.oculus.com/documentation/unity/ps-platform-intro/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Data Use Checkup (DUC)](https://developer.oculus.com/resources/publish-data-use/)
- [Virtual Reality Checks (VRC)](https://developer.oculus.com/resources/publish-quest-req/)
