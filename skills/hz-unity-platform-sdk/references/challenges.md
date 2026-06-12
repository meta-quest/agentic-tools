# Challenges API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-challenges/
- **Namespace**: Oculus.Platform

## Overview

1. Create time-bound challenges tied to existing leaderboards
2. List challenges with viewer filters (participating, invited, all visible)
3. Get challenge details and entries
4. Join, leave, and decline challenge invites
5. Invite users to challenges
6. Update or delete challenges
7. Integrate with leaderboard score submission (automatic)

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

Challenges require an existing **Leaderboard**. If you haven't set one up, see [leaderboards.md](leaderboards.md) first.

1. **Create a leaderboard** in the Developer Dashboard. Note its **API Name** (case-sensitive).
2. Any app that uses Leaderboards automatically gets Challenges. They appear in the Scoreboards UI on the Quest, and `Leaderboards.WriteEntry` returns affected challenge IDs in its response.

## API Usage

### Create a Challenge

A Challenge is built around an existing Leaderboard. Set the time window with `SetStartDate` / `SetEndDate` (both `DateTime`, UTC), and pick a visibility level.

```csharp
public async Task<ulong> CreateChallenge(string leaderboardName, string title, TimeSpan duration)
{
    if (!Core.IsInitialized()) return 0;

    var options = new ChallengeOptions();
    options.SetTitle(title);
    options.SetDescription("Highest score wins!");
    options.SetLeaderboardName(leaderboardName);
    options.SetVisibility(ChallengeVisibility.InviteOnly);
    var now = DateTime.UtcNow;
    options.SetStartDate(now);
    options.SetEndDate(now.Add(duration));

    try
    {
        Message<Challenge> msg = await Challenges.Create(leaderboardName, options);
        if (msg.IsError)
        {
            Debug.LogError($"Challenges.Create failed: {msg.GetError().Message}");
            return 0;
        }
        Challenge ch = msg.Data;
        Debug.Log($"Created challenge {ch.ID} ('{ch.Title}'), ends {ch.EndDate:u}");
        return ch.ID;
    }
    catch (Exception e)
    {
        Debug.LogException(e);
        return 0;
    }
}
```

### List Challenges

Use `ChallengeOptions` as a filter, then call `Challenges.GetList`.

```csharp
public async Task<List<Challenge>> ListMyChallenges(int limit = 25)
{
    if (!Core.IsInitialized()) return new();

    var filter = new ChallengeOptions();
    filter.SetViewerFilter(ChallengeViewerFilter.ParticipatingOrInvited);
    filter.SetIncludeActiveChallenges = true;
    filter.IncludeFutureChallenges = true;
    filter.IncludePastChallenges = false;

    var msg = await Challenges.GetList(filter, limit);
    if (msg.IsError) return new();
    return new List<Challenge>(msg.Data);
}
```

> **Pagination**: `ChallengeList` exposes `HasNextPage` / `HasPreviousPage`. Use `Challenges.GetNextChallenges(list)` / `Challenges.GetPreviousChallenges(list)` to walk pages.

### Get Challenge Details and Entries

```csharp
public async Task<Challenge> GetChallenge(ulong challengeId)
{
    var msg = await Challenges.Get(challengeId);
    if (msg.IsError) return null;
    return msg.Data;
}

public async Task<List<ChallengeEntry>> GetChallengeEntries(ulong challengeId, int limit = 25)
{
    var msg = await Challenges.GetEntries(
        challengeId,
        limit,
        LeaderboardFilterType.None,
        LeaderboardStartAt.Top);
    if (msg.IsError) return new();
    return new List<ChallengeEntry>(msg.Data);
}

public async Task<List<ChallengeEntry>> GetEntriesAfterRank(ulong challengeId, ulong afterRank, int limit = 25)
{
    var msg = await Challenges.GetEntriesAfterRank(challengeId, limit, afterRank);
    if (msg.IsError) return new();
    return new List<ChallengeEntry>(msg.Data);
}
```

`ChallengeEntry` mirrors `LeaderboardEntry`: it has `Rank`, `Score`, `DisplayScore`, `User`, `Timestamp`, etc.

### Join, Leave, Decline, Invite

```csharp
public async Task<bool> JoinChallenge(ulong challengeId)
{
    var msg = await Challenges.Join(challengeId);
    return !msg.IsError;
}

public async Task<bool> LeaveChallenge(ulong challengeId)
{
    var msg = await Challenges.Leave(challengeId);
    return !msg.IsError;
}

public async Task<bool> DeclineInvite(ulong challengeId)
{
    var msg = await Challenges.DeclineInvite(challengeId);
    return !msg.IsError;
}

public async Task<bool> InviteUsers(ulong challengeId, ulong[] userIds)
{
    var msg = await Challenges.InviteUsers(challengeId, userIds);
    return !msg.IsError;
}
```

`InviteUsers` requires user IDs you've already retrieved (e.g., via `GroupPresence.GetInvitableUsers`, `Users.GetLoggedInUserFriends`, or a roster you maintain).

### Update or Delete a Challenge

The user must have permission (typically the creator) to mutate the challenge.

```csharp
public async Task<bool> UpdateChallenge(ulong challengeId, string newTitle, string newDescription)
{
    var options = new ChallengeOptions();
    options.SetTitle(newTitle);
    options.SetDescription(newDescription);
    var msg = await Challenges.UpdateInfo(challengeId, options);
    return !msg.IsError;
}

public async Task<bool> DeleteChallenge(ulong challengeId)
{
    var msg = await Challenges.Delete(challengeId);
    return !msg.IsError;
}
```

### Hook into Leaderboard Score Submission

When `Leaderboards.WriteEntry` is called, the Platform automatically updates any Challenges tied to that leaderboard. Your code doesn't need to submit twice.

```csharp
public async Task SubmitScoreAndRefreshChallenges(string leaderboardName, long score)
{
    var writeMsg = await Leaderboards.WriteEntry(leaderboardName, score);
    if (writeMsg.IsError || !writeMsg.Data) return;

    var listMsg = await Challenges.GetList(BuildActiveChallengeFilter(leaderboardName), 25);
    if (!listMsg.IsError) RefreshChallengeUI(listMsg.Data);
}

private ChallengeOptions BuildActiveChallengeFilter(string leaderboardName)
{
    var f = new ChallengeOptions();
    f.SetLeaderboardName(leaderboardName);
    f.SetViewerFilter(ChallengeViewerFilter.Participating);
    f.IncludeActiveChallenges = true;
    return f;
}
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Challenges.Create(leaderboardName, options)` | `Request<Challenge>` | Create a new challenge bound to a leaderboard |
| `Challenges.Get(challengeId)` | `Request<Challenge>` | Fetch a single challenge by ID |
| `Challenges.GetList(options, limit)` | `Request<ChallengeList>` | List challenges matching filters |
| `Challenges.GetEntries(id, limit, filter, startAt)` | `Request<ChallengeEntryList>` | Fetch challenge entries |
| `Challenges.GetEntriesAfterRank(id, limit, afterRank)` | `Request<ChallengeEntryList>` | Page entries after a rank |
| `Challenges.GetEntriesByIds(id, limit, startAt, userIds)` | `Request<ChallengeEntryList>` | Entries for specific users |
| `Challenges.Join(id)` | `Request<Challenge>` | Join a challenge |
| `Challenges.Leave(id)` | `Request<Challenge>` | Leave a challenge |
| `Challenges.DeclineInvite(id)` | `Request<Challenge>` | Decline a challenge invite |
| `Challenges.InviteUsers(id, userIds)` | `Request<Challenge>` | Invite users by ID |
| `Challenges.UpdateInfo(id, options)` | `Request<Challenge>` | Update challenge metadata |
| `Challenges.Delete(id)` | `Request` | Delete a challenge |
| `Challenges.GetNextChallenges(list)` | `Request<ChallengeList>` | Next page of challenges |
| `Challenges.GetNextEntries(list)` | `Request<ChallengeEntryList>` | Next page of entries |

### Models

| Type | Key Fields |
|------|------------|
| `Challenge` | `ID`, `Title`, `Description`, `Leaderboard`, `StartDate`, `EndDate`, `Visibility`, `CreationType`, `ParticipantsOptional`, `InvitedUsersOptional` |
| `ChallengeEntry` | `Rank`, `Score`, `DisplayScore`, `User`, `Timestamp`, `ExtraData` |

### Enums

| Enum | Values |
|------|--------|
| `ChallengeVisibility` | `Public`, `InviteOnly`, `Private`, `Unknown` |
| `ChallengeViewerFilter` | `AllVisible`, `Participating`, `Invited`, `ParticipatingOrInvited`, `Unknown` |
| `ChallengeCreationType` | `UserCreated`, `DeveloperCreated`, `Unknown` |

### Visibility Options

| `ChallengeVisibility` | Meaning |
|------------------------|---------|
| `Public` | Anyone can see and join |
| `InviteOnly` | Anyone can see; only invited users can join |
| `Private` | Only invited users can see and join |

### Viewer Filter Options

| `ChallengeViewerFilter` | Meaning |
|--------------------------|---------|
| `AllVisible` | All public + invited |
| `Participating` | Challenges the user has joined |
| `Invited` | Challenges the user has been invited to |
| `ParticipatingOrInvited` | Union of the two above |

## Error Handling

| Mistake | Fix |
|---------|-----|
| Trying to create a Challenge without a Leaderboard | Challenges always reference an existing Leaderboard's API name. Set up the Leaderboard first. |
| Calling `Challenges.Create` before init | Always check `Core.IsInitialized()`. |
| Using local time instead of UTC for start/end dates | Use `DateTime.UtcNow` and add UTC offsets. The platform stores dates in Unix epoch seconds. |
| Submitting scores to a Challenge directly | Don't -- call `Leaderboards.WriteEntry` on the underlying leaderboard. Challenges are updated automatically. |
| Forgetting that `ParticipantsOptional` / `InvitedUsersOptional` can be null | They are nullable. Null-check before iterating. |
| Inviting users without their IDs | Get IDs first via `Users.GetLoggedInUserFriends` or `GroupPresence.GetInvitableUsers`. |
| Not checking visibility before showing a "Join" button | If `Visibility == Private`, only invited users can join. Hide the button otherwise. |
| Setting all three `IncludeActive/Future/Past` to false | Will return zero results. At least one must be true. |

## Examples

### Example 1: Complete Challenge Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private string leaderboardName = "high_scores";

    private bool isInitialized;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        isInitialized = !msg.IsError;
    }

    public async Task<Challenge> CreateWeeklyChallenge(string title)
    {
        if (!isInitialized) return null;
        var opts = new ChallengeOptions();
        opts.SetTitle(title);
        opts.SetDescription($"Compete this week on {leaderboardName}!");
        opts.SetLeaderboardName(leaderboardName);
        opts.SetVisibility(ChallengeVisibility.Public);
        opts.SetStartDate(DateTime.UtcNow);
        opts.SetEndDate(DateTime.UtcNow.AddDays(7));

        var msg = await Challenges.Create(leaderboardName, opts);
        if (msg.IsError)
        {
            Debug.LogError($"Create: {msg.GetError().Message}");
            return null;
        }
        return msg.Data;
    }

    public async Task<List<Challenge>> LoadMyChallenges()
    {
        if (!isInitialized) return new();
        var f = new ChallengeOptions();
        f.SetViewerFilter(ChallengeViewerFilter.ParticipatingOrInvited);
        f.IncludeActiveChallenges = true;
        f.IncludeFutureChallenges = true;
        var msg = await Challenges.GetList(f, 25);
        return msg.IsError ? new() : new List<Challenge>(msg.Data);
    }

    public async Task<List<ChallengeEntry>> LoadFriendsLeaderboard(ulong challengeId)
    {
        if (!isInitialized) return new();
        var msg = await Challenges.GetEntries(
            challengeId, 25, LeaderboardFilterType.Friends, LeaderboardStartAt.CenteredOnViewerOrTop);
        return msg.IsError ? new() : new List<ChallengeEntry>(msg.Data);
    }
}
```

### Example 2: Submit Score and Refresh Active Challenges

```csharp
public async Task SubmitAndRefresh(string leaderboardName, long score)
{
    // Score submission automatically fans out to all active challenges on this leaderboard
    var writeMsg = await Leaderboards.WriteEntry(leaderboardName, score);
    if (writeMsg.IsError || !writeMsg.Data) return;

    // Refresh the user's active challenges to pick up new ranking
    var filter = new ChallengeOptions();
    filter.SetLeaderboardName(leaderboardName);
    filter.SetViewerFilter(ChallengeViewerFilter.Participating);
    filter.IncludeActiveChallenges = true;

    var listMsg = await Challenges.GetList(filter, 25);
    if (!listMsg.IsError)
    {
        foreach (var challenge in listMsg.Data)
        {
            Debug.Log($"Challenge '{challenge.Title}' updated, ends {challenge.EndDate:u}");
        }
    }
}
```

## Important Notes

- **Challenges are a UI layer over Leaderboards**: Always submit scores via `Leaderboards.WriteEntry` -- the platform fans out to active challenges automatically. Your code never submits to a Challenge directly.
- **Use UTC for all dates**: `SetStartDate` / `SetEndDate` expect `DateTime` in UTC.
- **Default visibility**: Use `Public` for community challenges, `InviteOnly` for friend challenges, `Private` for closed groups.
- **Listing best practice**: Use `ChallengeViewerFilter.ParticipatingOrInvited` for a "My Challenges" UI. Always set at least one of `IncludeActiveChallenges`, `IncludeFutureChallenges`, `IncludePastChallenges` to `true`.
- **Resolve user IDs before inviting**: Get IDs via `Users.GetLoggedInUserFriends` or `GroupPresence.GetInvitableUsers`.
- **Sample tester**: `samples/unity/Baremetal/Assets/SamplesInternal/challenges/ChallengesTester.cs`
- **Related APIs**: [leaderboards.md](leaderboards.md), [users.md](users.md)
