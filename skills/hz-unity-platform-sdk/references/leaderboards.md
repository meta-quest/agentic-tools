# Leaderboards API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-leaderboards/
- **Namespace**: Oculus.Platform

## Overview

1. Retrieve leaderboard info by API name
2. Write scores (best-only or forced update)
3. Write scores with supplementary tiebreaker metrics
4. Fetch entries (top, friends, centered on viewer, after rank, by user IDs)
5. Paginate entry lists
6. Render entries in Unity UI

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Create one or more leaderboards** in the Developer Dashboard under your app's "Platform Services > Leaderboards" section. Note the **API Name** (case-sensitive) -- this is what your code will reference, not the display name.
2. **Pick a sort order** (`HIGH_IS_BEST` or `LOW_IS_BEST`) and a **score type** (e.g., `NUMERIC`, `TIME`, `MILLISECONDS`). The score type only affects display formatting -- `Score` is always a `long`.

## API Usage

### Retrieve Leaderboard Info

Look up a leaderboard by its API name. Useful to confirm the leaderboard exists and grab its destination for deep-linking.

```csharp
public async Task GetLeaderboardInfo(string leaderboardName)
{
    if (!Core.IsInitialized()) return;

    try
    {
        Message<LeaderboardList> msg = await Leaderboards.Get(leaderboardName);
        if (msg.IsError)
        {
            Debug.LogError($"Leaderboards.Get failed: {msg.GetError().Message}");
            return;
        }
        foreach (Leaderboard lb in msg.Data)
        {
            Debug.Log($"Leaderboard: {lb.ApiName}, ID: {lb.ID}");
        }
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

### Write a Score

`WriteEntry` only updates the user's entry if the new score beats their best, unless `forceUpdate` is `true`. The response indicates whether the leaderboard was updated.

```csharp
public async Task SubmitScore(string leaderboardName, long score)
{
    if (!Core.IsInitialized()) return;

    try
    {
        Message<bool> msg = await Leaderboards.WriteEntry(leaderboardName, score, extraData: null, forceUpdate: null);
        if (msg.IsError)
        {
            Debug.LogError($"WriteEntry failed: {msg.GetError().Message}");
            return;
        }
        bool didUpdate = msg.Data;
        Debug.Log(didUpdate ? "New high score!" : "Score not better than current best.");
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

### Score with Extra Data

Attach a `byte[]` (max 2KB) to the entry -- useful for replay data, ghost recordings, or contextual metadata.

```csharp
byte[] replayData = SerializeGhostReplay(); // must be <= 2048 bytes
await Leaderboards.WriteEntry(leaderboardName, score, replayData, forceUpdate: false);
```

### Score with Supplementary Metric (Tiebreaker)

When two players tie on the primary score, the supplementary metric breaks the tie (e.g., time taken, items collected).

```csharp
public async Task SubmitScoreWithTiebreaker(string leaderboardName, long score, long tiebreakerMetric)
{
    var msg = await Leaderboards.WriteEntryWithSupplementaryMetric(
        leaderboardName,
        score,
        tiebreakerMetric,
        extraData: null,
        forceUpdate: null);
    if (!msg.IsError && msg.Data) Debug.Log("Score updated.");
}
```

### Fetch Top N Globally

```csharp
public async Task LoadTopScores(string leaderboardName, int limit = 25)
{
    if (!Core.IsInitialized()) return;

    var msg = await Leaderboards.GetEntries(
        leaderboardName,
        limit,
        LeaderboardFilterType.None,
        LeaderboardStartAt.Top);

    if (msg.IsError)
    {
        Debug.LogError($"GetEntries failed: {msg.GetError().Message}");
        return;
    }

    foreach (LeaderboardEntry entry in msg.Data)
    {
        Debug.Log($"#{entry.Rank} {entry.User.DisplayName}: {entry.DisplayScore ?? entry.Score.ToString()}");
    }
}
```

### Fetch Centered on Current User

`CenteredOnViewerOrTop` is the safe default -- if the user has no entry yet, it falls back to the top instead of returning an error.

```csharp
var msg = await Leaderboards.GetEntries(
    leaderboardName,
    limit: 20,
    filter: LeaderboardFilterType.None,
    startAt: LeaderboardStartAt.CenteredOnViewerOrTop);
```

### Fetch Friends Only

Returns entries from bidirectional followers only.

```csharp
var msg = await Leaderboards.GetEntries(
    leaderboardName,
    limit: 25,
    filter: LeaderboardFilterType.Friends,
    startAt: LeaderboardStartAt.CenteredOnViewerOrTop);
```

### Fetch After a Specific Rank (Pagination)

Use this for "load next page" buttons or infinite scroll. Pass the highest rank from the previous page.

```csharp
ulong lastRank = 0;
public async Task LoadNextPage(string leaderboardName, int pageSize = 25)
{
    var msg = await Leaderboards.GetEntriesAfterRank(leaderboardName, pageSize, lastRank);
    if (msg.IsError) return;
    if (msg.Data.Count > 0)
    {
        lastRank = (ulong)msg.Data[msg.Data.Count - 1].Rank;
    }
    foreach (var entry in msg.Data) RenderEntry(entry);
}
```

### Fetch by Specific User IDs

```csharp
ulong[] userIds = new ulong[] { 12345UL, 67890UL };
var msg = await Leaderboards.GetEntriesByIds(
    leaderboardName,
    limit: 10,
    startAt: LeaderboardStartAt.CenteredOnViewer,
    userIds);
```

> **Note**: When `startAt` is `CenteredOnViewer` or `CenteredOnViewerOrTop`, the current user is automatically included in the results, even if their ID isn't in `userIds`.

### Pagination via HasNextPage / HasPreviousPage

`LeaderboardEntryList` exposes `HasNextPage` and `HasPreviousPage`. Use the helper methods to fetch the next page without managing rank cursors yourself.

```csharp
var firstPage = await Leaderboards.GetEntries(name, 25, LeaderboardFilterType.None, LeaderboardStartAt.Top);
if (!firstPage.IsError && firstPage.Data.HasNextPage)
{
    var nextPage = await Leaderboards.GetNextEntries(firstPage.Data);
}
```

### Render Entries in Unity UI

Pattern: instantiate row prefabs into a `ScrollRect`'s content container, then force a layout rebuild.

```csharp
[SerializeField] private GameObject rowPrefab;
[SerializeField] private ScrollRect leaderboardScrollView;
[SerializeField] private Transform leaderboardContent;

private void RenderEntries(LeaderboardEntryList entries)
{
    foreach (var entry in entries)
    {
        GameObject row = Instantiate(rowPrefab, leaderboardContent);
        Text[] textComponents = row.GetComponentsInChildren<Text>();
        if (textComponents.Length >= 2)
        {
            textComponents[0].text = entry.User?.DisplayName ?? "Unknown";
            textComponents[1].text = entry.DisplayScore ?? entry.Score.ToString();
        }
    }
    LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardScrollView.content);
}
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Leaderboards.Get(name)` | `Request<LeaderboardList>` | Look up a leaderboard by API name |
| `Leaderboards.GetEntries(name, limit, filter, startAt)` | `Request<LeaderboardEntryList>` | Fetch entries with filter/start position |
| `Leaderboards.GetEntriesAfterRank(name, limit, afterRank)` | `Request<LeaderboardEntryList>` | Fetch a page of entries after a rank |
| `Leaderboards.GetEntriesByIds(name, limit, startAt, userIds)` | `Request<LeaderboardEntryList>` | Fetch entries for specific user IDs |
| `Leaderboards.WriteEntry(name, score, extraData, forceUpdate)` | `Request<bool>` | Submit a score (best-only by default) |
| `Leaderboards.WriteEntryWithSupplementaryMetric(name, score, suppMetric, extraData, forceUpdate)` | `Request<bool>` | Submit a score with a tiebreaker metric |
| `Leaderboards.GetNextEntries(list)` | `Request<LeaderboardEntryList>` | Next page of entries |
| `Leaderboards.GetPreviousEntries(list)` | `Request<LeaderboardEntryList>` | Previous page of entries |

### Models

| Type | Key Fields |
|------|------------|
| `Leaderboard` | `ApiName`, `ID`, `Destination` |
| `LeaderboardEntry` | `Rank`, `Score`, `DisplayScore`, `ExtraData`, `User`, `Timestamp`, `SupplementaryMetricOptional` |
| `SupplementaryMetric` | `ID`, `Metric` |

### Enums

| Enum | Values |
|------|--------|
| `LeaderboardFilterType` | `None`, `Friends`, `UserIds`, `Unknown` |
| `LeaderboardStartAt` | `Top`, `CenteredOnViewer`, `CenteredOnViewerOrTop`, `Unknown` |

## Error Handling

| Mistake | Fix |
|---------|-----|
| Using leaderboard **display name** instead of **API name** | Always use the API name from the Developer Dashboard. It's case-sensitive. |
| Calling `WriteEntry` and expecting it to always update | By default it only updates if the new score is better. Pass `forceUpdate: true` to overwrite. |
| Calling Leaderboards before init | Always check `Core.IsInitialized()` first. |
| Using `CenteredOnViewer` and crashing on a new user | Use `CenteredOnViewerOrTop` to gracefully fall back to the top of the leaderboard when the user has no entry. |
| Ignoring `extraData` size limit | `extraData` must be <= 2KB. Larger payloads will be rejected. |
| Reading `DisplayScore` without null check | `DisplayScore` is `null` if the leaderboard isn't configured with a score type. Fall back to `Score.ToString()`. |
| Manually managing pagination cursors when not needed | Prefer `GetNextEntries(list)` / `GetPreviousEntries(list)` over re-running `GetEntriesAfterRank`. |
| Skipping the `User` null check | `entry.User` and `entry.User.DisplayName` can be null for guest accounts or users who blocked you. |

## Examples

### Example 1: Complete Leaderboard Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private string leaderboardName = "high_scores";

    private bool isInitialized;
    private List<LeaderboardEntry> cachedTopEntries = new();

    async void Start()
    {
        await InitializePlatform();
    }

    private async Task InitializePlatform()
    {
        try
        {
#if UNITY_EDITOR
            var msg = await Core.AsyncInitialize(appId, "standalone");
#else
            var msg = await Core.AsyncInitialize(appId);
#endif
            if (msg.IsError)
            {
                Debug.LogError($"Platform init failed: {msg.GetError().Message}");
                return;
            }
            isInitialized = true;
        }
        catch (Exception e) { Debug.LogException(e); }
    }

    public async Task<bool> SubmitScoreAsync(long score, byte[] extraData = null, bool forceUpdate = false)
    {
        if (!isInitialized || !Core.IsInitialized()) return false;
        try
        {
            var msg = await Leaderboards.WriteEntry(leaderboardName, score, extraData, forceUpdate ? true : (bool?)null);
            if (msg.IsError)
            {
                Debug.LogError($"SubmitScore: {msg.GetError().Message}");
                return false;
            }
            return msg.Data;
        }
        catch (Exception e) { Debug.LogException(e); return false; }
    }

    public async Task<List<LeaderboardEntry>> LoadTopEntriesAsync(int limit = 25)
    {
        if (!isInitialized || !Core.IsInitialized()) return new();
        try
        {
            var msg = await Leaderboards.GetEntries(
                leaderboardName, limit, LeaderboardFilterType.None, LeaderboardStartAt.Top);
            if (msg.IsError)
            {
                Debug.LogError($"LoadTopEntries: {msg.GetError().Message}");
                return new();
            }
            cachedTopEntries = new List<LeaderboardEntry>(msg.Data);
            return cachedTopEntries;
        }
        catch (Exception e) { Debug.LogException(e); return new(); }
    }

    public async Task<List<LeaderboardEntry>> LoadFriendEntriesAsync(int limit = 25)
    {
        if (!isInitialized || !Core.IsInitialized()) return new();
        try
        {
            var msg = await Leaderboards.GetEntries(
                leaderboardName, limit, LeaderboardFilterType.Friends, LeaderboardStartAt.CenteredOnViewerOrTop);
            if (msg.IsError) return new();
            return new List<LeaderboardEntry>(msg.Data);
        }
        catch (Exception e) { Debug.LogException(e); return new(); }
    }
}
```

### Example 2: Score with Tiebreaker and Friends Filter

```csharp
// Submit a score with a tiebreaker metric, then show friends-only leaderboard
public async Task SubmitAndShowFriends(string leaderboardName, long score, long timeTaken)
{
    var writeMsg = await Leaderboards.WriteEntryWithSupplementaryMetric(
        leaderboardName, score, timeTaken, extraData: null, forceUpdate: null);
    if (writeMsg.IsError || !writeMsg.Data) return;

    var friendsMsg = await Leaderboards.GetEntries(
        leaderboardName, 25, LeaderboardFilterType.Friends, LeaderboardStartAt.CenteredOnViewerOrTop);
    if (friendsMsg.IsError) return;

    foreach (var entry in friendsMsg.Data)
    {
        string tiebreaker = entry.SupplementaryMetricOptional != null
            ? $" (tiebreaker: {entry.SupplementaryMetricOptional.Metric})"
            : "";
        Debug.Log($"#{entry.Rank} {entry.User?.DisplayName ?? "Unknown"}: {entry.DisplayScore ?? entry.Score.ToString()}{tiebreaker}");
    }
}
```

## Important Notes

- **Challenges integration**: Leaderboard-integrated apps automatically get Challenges. When `WriteEntry` returns, the response includes any Challenge IDs that were affected -- surface this in your UI.
- **Score submission is best-effort by default**: `WriteEntry` only updates if the new score is better. Check `msg.Data` (the `didUpdate` flag) to know if the leaderboard changed.
- **Extra data limit**: `extraData` attached to entries must be <= 2KB.
- **Default to `CenteredOnViewerOrTop`** for "around me" views -- it gracefully handles unranked users.
- **Use `LeaderboardFilterType.Friends`** for social leaderboards (bidirectional followers only).
- **Sample tester**: `samples/unity/Baremetal/Assets/SamplesInternal/leaderboards/LeaderboardsTester.cs`
- [Server-to-Server Leaderboard API](https://developer.oculus.com/documentation/unity/ps-leaderboards-s2s/)
