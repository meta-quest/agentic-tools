# Achievements API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-achievements/
- **Namespace**: Oculus.Platform

## Overview

1. Unlock simple (one-shot) achievements
2. Increment count-based achievements
3. Set bits on bitfield (collect-them-all) achievements
4. Fetch achievement definitions and user progress
5. Join definitions and progress for UI display

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Define your achievements** in the Developer Dashboard under your app's "Platform Services > Achievements" section. For each achievement decide:
   - **API Name** (case-sensitive, used in code)
   - **Type**: `Simple`, `Count`, or `Bitfield`
   - For `Count`: the **target value** the counter must reach to unlock
   - For `Bitfield`: the **bitfield length** and the **target** number of bits that must be set

### Achievement Types -- Cheat Sheet

| Type | When to use | API to call |
|------|-------------|-------------|
| `Simple` | One-shot events ("Reached the boss", "Completed tutorial") | `Achievements.Unlock(name)` |
| `Count` | Cumulative progress ("Defeat 100 enemies") | `Achievements.AddCount(name, count)` -- counter is monotonically increasing on the server |
| `Bitfield` | Collect-them-all sets ("Find all 7 hidden gems") | `Achievements.AddFields(name, "0010001")` |

## API Usage

### Unlock a Simple Achievement

```csharp
public async Task UnlockAchievement(string apiName)
{
    if (!Core.IsInitialized()) return;

    try
    {
        Message<AchievementUpdate> msg = await Achievements.Unlock(apiName);
        if (msg.IsError)
        {
            Debug.LogError($"Achievements.Unlock({apiName}) failed: {msg.GetError().Message}");
            return;
        }
        AchievementUpdate update = msg.Data;
        if (update.JustUnlocked)
        {
            Debug.Log($"Just unlocked '{apiName}' for the first time!");
            ShowUnlockToast(apiName);
        }
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

`Unlock` is **idempotent** -- calling it on an already-unlocked achievement is safe and just returns `JustUnlocked = false`. You don't need to track unlock state locally to avoid duplicate calls.

### Increment a Count Achievement

`AddCount` adds to the **server-side** running counter. The achievement unlocks automatically when the counter reaches the target you defined in the dashboard.

```csharp
public async Task AddProgress(string apiName, ulong increment = 1)
{
    if (!Core.IsInitialized()) return;

    var msg = await Achievements.AddCount(apiName, increment);
    if (msg.IsError) return;

    AchievementUpdate update = msg.Data;
    if (update.JustUnlocked)
    {
        Debug.Log($"Reached count target on '{apiName}'!");
    }
}
```

> **Don't pass the cumulative total** -- the API adds to the existing server value. Pass only the delta.

### Set Bitfield Achievement Bits

`AddFields` accepts a string of `'0'` and `'1'` characters representing which bits to set. The string length must equal the achievement's `BitfieldLength`. The platform OR's the bits with the existing value.

```csharp
public async Task CollectItem(int itemIndex, int bitfieldLength = 7)
{
    var bits = new char[bitfieldLength];
    for (int i = 0; i < bitfieldLength; i++) bits[i] = '0';
    bits[itemIndex] = '1';
    string fields = new string(bits);

    var msg = await Achievements.AddFields("collect_all_gems", fields);
    if (!msg.IsError && msg.Data.JustUnlocked)
    {
        Debug.Log("Found all the gems!");
    }
}
```

### Fetch All Definitions

```csharp
public async Task LoadAllDefinitions()
{
    var msg = await Achievements.GetAllDefinitions();
    if (msg.IsError) return;

    foreach (AchievementDefinition def in msg.Data)
    {
        Debug.Log($"{def.Name} ({def.Type}) target={def.Target}");
    }

    if (msg.Data.HasNextPage)
    {
        var nextMsg = await Achievements.GetNextAchievementDefinitionListPage(msg.Data);
    }
}
```

### Fetch Specific Definitions by Name

```csharp
string[] names = { "first_kill", "collect_all_gems", "defeat_100_enemies" };
var msg = await Achievements.GetDefinitionsByName(names);
```

### Fetch All User Progress

```csharp
var msg = await Achievements.GetAllProgress();
foreach (AchievementProgress p in msg.Data)
{
    if (p.IsUnlocked)
        Debug.Log($"{p.Name}: unlocked at {p.UnlockTime:u}");
    else
        Debug.Log($"{p.Name}: count={p.Count}, bitfield={p.Bitfield}");
}
```

### Fetch Progress for Specific Achievements

```csharp
var msg = await Achievements.GetProgressByName(new[] { "first_kill", "defeat_100_enemies" });
```

### Join Definitions and Progress for UI

A common UI pattern: show every achievement's name, target, and the current player's progress. Fetch both in parallel, then join by `Name`.

```csharp
public async Task<List<(AchievementDefinition def, AchievementProgress progress)>> LoadAchievementUiData()
{
    var defsTask = Achievements.GetAllDefinitions();
    var progressTask = Achievements.GetAllProgress();

    var defsMsg = await defsTask;
    var progressMsg = await progressTask;

    if (defsMsg.IsError || progressMsg.IsError) return new();

    var progressByName = new Dictionary<string, AchievementProgress>();
    foreach (var p in progressMsg.Data) progressByName[p.Name] = p;

    var result = new List<(AchievementDefinition, AchievementProgress)>();
    foreach (var def in defsMsg.Data)
    {
        progressByName.TryGetValue(def.Name, out var prog);
        result.Add((def, prog)); // prog may be null if user has no progress yet
    }
    return result;
}
```

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Achievements.Unlock(name)` | `Request<AchievementUpdate>` | Unlock a Simple achievement (or any type) |
| `Achievements.AddCount(name, count)` | `Request<AchievementUpdate>` | Increment a Count achievement |
| `Achievements.AddFields(name, fields)` | `Request<AchievementUpdate>` | Set bits on a Bitfield achievement |
| `Achievements.GetAllDefinitions()` | `Request<AchievementDefinitionList>` | List all achievement definitions |
| `Achievements.GetDefinitionsByName(names)` | `Request<AchievementDefinitionList>` | Definitions for specific names |
| `Achievements.GetAllProgress()` | `Request<AchievementProgressList>` | Current user's progress on all achievements |
| `Achievements.GetProgressByName(names)` | `Request<AchievementProgressList>` | Progress for specific achievements |
| `Achievements.GetNextAchievementDefinitionListPage(list)` | `Request<AchievementDefinitionList>` | Next page of definitions |
| `Achievements.GetNextAchievementProgressListPage(list)` | `Request<AchievementProgressList>` | Next page of progress |

### Models

| Type | Key Fields |
|------|------------|
| `AchievementDefinition` | `Name`, `Type`, `Target`, `BitfieldLength` |
| `AchievementProgress` | `Name`, `IsUnlocked`, `UnlockTime`, `Count`, `Bitfield` |
| `AchievementUpdate` | `Name`, `JustUnlocked` |

### Enums

| Enum | Values |
|------|--------|
| `AchievementType` | `Simple`, `Count`, `Bitfield`, `Unknown` |

## Error Handling

| Mistake | Fix |
|---------|-----|
| Passing the cumulative total to `AddCount` | Pass only the delta. The platform tracks the running total server-side. |
| Calling `Unlock` defensively to prevent dupes | `Unlock` is idempotent. Just call it; check `JustUnlocked` if you want to fire UI only on first unlock. |
| Bitfield string length mismatch | The `fields` string must be exactly `BitfieldLength` characters of `'0'` / `'1'`. |
| Calling Achievements before init | Always check `Core.IsInitialized()`. |
| Tracking unlock state locally | The server is authoritative. Fetch progress with `GetAllProgress` on app start. |
| Confusing display name with API name | Always use the **API Name** from the dashboard. Case-sensitive. |
| Ignoring nullability of `AchievementProgress.UnlockTime` | If `IsUnlocked == false`, treat `UnlockTime` as meaningless. |
| Spamming `AddCount` per frame | Batch increments client-side and submit periodically (e.g., every 5 seconds or at checkpoints) to reduce API churn. |

## Examples

### Example 1: Complete Achievements Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AchievementsManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";

    private bool isInitialized;
    private readonly Dictionary<string, AchievementDefinition> defsByName = new();

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }
        isInitialized = true;
        await LoadDefinitions();
    }

    private async Task LoadDefinitions()
    {
        var msg = await Achievements.GetAllDefinitions();
        if (msg.IsError) return;
        foreach (var d in msg.Data) defsByName[d.Name] = d;
    }

    public async Task UnlockSimple(string name)
    {
        if (!isInitialized) return;
        var msg = await Achievements.Unlock(name);
        if (!msg.IsError && msg.Data.JustUnlocked) ShowToast(name);
    }

    public async Task IncrementCount(string name, ulong delta = 1)
    {
        if (!isInitialized) return;
        var msg = await Achievements.AddCount(name, delta);
        if (!msg.IsError && msg.Data.JustUnlocked) ShowToast(name);
    }

    public async Task SetBitfieldBit(string name, int bitIndex)
    {
        if (!isInitialized || !defsByName.TryGetValue(name, out var def)) return;
        if (def.Type != AchievementType.Bitfield) return;

        char[] bits = new char[def.BitfieldLength];
        for (int i = 0; i < bits.Length; i++) bits[i] = '0';
        if (bitIndex >= 0 && bitIndex < bits.Length) bits[bitIndex] = '1';

        var msg = await Achievements.AddFields(name, new string(bits));
        if (!msg.IsError && msg.Data.JustUnlocked) ShowToast(name);
    }

    private void ShowToast(string name)
    {
        Debug.Log($"Unlocked: {name}");
    }
}
```

### Example 2: Progress Display UI Data Loader

```csharp
// Fetch definitions and progress in parallel, then join for UI rendering
public async Task DisplayAllAchievements()
{
    var defsTask = Achievements.GetAllDefinitions();
    var progressTask = Achievements.GetAllProgress();

    var defsMsg = await defsTask;
    var progressMsg = await progressTask;
    if (defsMsg.IsError || progressMsg.IsError) return;

    var progressByName = new Dictionary<string, AchievementProgress>();
    foreach (var p in progressMsg.Data) progressByName[p.Name] = p;

    foreach (var def in defsMsg.Data)
    {
        progressByName.TryGetValue(def.Name, out var prog);
        string status = prog?.IsUnlocked == true
            ? $"Unlocked at {prog.UnlockTime:u}"
            : def.Type == AchievementType.Count
                ? $"{prog?.Count ?? 0} / {def.Target}"
                : $"Locked";
        Debug.Log($"{def.Name}: {status}");
    }
}
```

## Important Notes

- **Quest Home integration**: Achievements automatically appear in Meta Quest Home. No extra integration needed.
- **Server is authoritative**: Don't store unlock state locally. Fetch progress with `GetAllProgress` on app start.
- **Cache definitions at startup**: Call `GetAllDefinitions` once and cache so you can look up `BitfieldLength` and `Target` without re-querying.
- **Batch count increments**: Don't call `AddCount` every frame. Accumulate deltas client-side and submit periodically.
- **Bitfield string must match length**: The string passed to `AddFields` must be exactly `BitfieldLength` characters.
- **Sample tester**: `samples/unity/Baremetal/Assets/SamplesInternal/achievements/AchievementsTester.cs`
