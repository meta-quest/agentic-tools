# Project Setup Tool (OVRProjectSetup) Reference

The Unity Project Setup Tool (UPST) helps configure projects for Meta VR development using a registry of **Configuration Tasks** that are checked and fixed automatically.

## CRITICAL: AndroidManifest Updates

**NEVER directly edit AndroidManifest.xml for features managed by OVRProjectConfig.** See [android-manifest.md](android-manifest.md) for the full workflow.

## Programmatic Access

**Prefer this over the UI.** Drive UPST against a live Editor with `unity-cli` — see "Running SDK code" in [SKILL.md](../SKILL.md) for the `run_script` invocation, and
[unity-mcp-fallback.md](unity-mcp-fallback.md) if you are stuck on a Unity MCP server instead.

Under `run_script`, `OVRProjectSetup` and `OVRManifestPreprocessor` compile directly and full
`System.Reflection` / `BindingFlags` work. Reflection is still needed for the pieces that are
genuinely non-public: `_principalRegistry`, `_tasks`, and `GetTasks(BuildTargetGroup)`.

### Architecture

```
OVRProjectSetup (public static, type name: "OVRProjectSetup")
├── _principalRegistry (private static OVRConfigurationTaskRegistry)
│   └── _tasks (private List<OVRConfigurationTask>)
├── ProcessorQueue (internal static OVRConfigurationTaskProcessorQueue)
├── FixAllAsync(BuildTargetGroup) — PUBLIC, queues fixes via ProcessorQueue
└── GetTasks(BuildTargetGroup) — internal, returns valid tasks for a platform
```

### Accessing the Task Registry

`GetTasks(BuildTargetGroup)` is internal but returns the valid tasks for a platform — the shortest
route, and enough for listing and fixing:

```csharp
var getTasks = typeof(OVRProjectSetup).GetMethod(
    "GetTasks", BindingFlags.NonPublic | BindingFlags.Static);
var tasks = (System.Collections.IEnumerable)getTasks.Invoke(null, new object[] { group });
```

To reach the registry itself (e.g. to enumerate tasks for *all* platforms):

```csharp
var registry = typeof(OVRProjectSetup)
    .GetField("_principalRegistry", BindingFlags.NonPublic | BindingFlags.Static)
    .GetValue(null);
var tasksList = registry.GetType()
    .GetField("_tasks", BindingFlags.NonPublic | BindingFlags.Instance)
    .GetValue(registry) as System.Collections.IList;
```

### OVRConfigurationTask Properties

All **public** — accessible via normal `taskType.GetProperty("Name")`:

| Property | Type | Description |
|---|---|---|
| `Message` | `OptionalLambdaType` | Task description |
| `Level` | `OptionalLambdaType` | Required / Recommended / Optional |
| `Group` | `TaskGroup` | Category (Compatibility, Rendering, Quality, etc.) |
| `Platform` | `BuildTargetGroup` | Target platform (`Unknown` = all platforms) |
| `Valid` | `OptionalLambdaType` | Whether task applies in current config |
| `IsDone` | `Func<BuildTargetGroup, bool>` | Whether the task is currently satisfied |
| `FixAction` | `Action<BuildTargetGroup>` | Synchronous fix delegate (null if no auto-fix) |
| `AsyncFixAction` | `Func<BuildTargetGroup, Task>` | Async fix delegate (null if no auto-fix) |
| `ManualSetup` | `OptionalLambdaType` | Guided setup (null if none) |
| `FixAutomatic` | `bool` | **Not reliable** for determining fix type — see below |

### Reading Property Values

`OVRConfigurationTask` is internal, so you hold each task as `object` — but its properties are
public, so plain `GetProperty(name)` works.

**OptionalLambdaType** properties (`Message`, `Level`, `Valid`, `FixMessage`, `ManualSetup`) require calling `.GetValue(targetGroup)`:

```csharp
var msgObj = task.GetType().GetProperty("Message").GetValue(task);
string message = msgObj?.GetType().GetMethod("GetValue")
    .Invoke(msgObj, new object[] { targetGroup })?.ToString();
```

**`IsDone`** is a `Func<BuildTargetGroup, bool>` — both type arguments are public, so cast and call it directly:

```csharp
var isDone = task.GetType().GetProperty("IsDone").GetValue(task) as Func<BuildTargetGroup, bool>;
bool done = isDone != null && isDone(targetGroup);
```

### Determining Fix Category

**Do NOT rely on `FixAutomatic` alone.** A task can have `FixAutomatic=True` but no `FixAction`. Determine category by checking what delegates exist:

- `FixAction != null` or `AsyncFixAction != null` → **Auto-fix**
- Neither fix action, but `ManualSetup.GetValue(targetGroup) != null` → **Manual (Guided Setup)**
- None of the above → **Manual**

### Listing Issues

When no platform is specified, default to `EditorUserBuildSettings.selectedBuildTargetGroup`, falling back to `BuildTargetGroup.Android`.

`GetTasks` already filters by platform and validity, so only `IsDone` remains. Complete script:

```csharp
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEditor;

public static class ListUpstIssues
{
    public static string Run()
    {
        var group = BuildTargetGroup.Android;

        var getTasks = typeof(OVRProjectSetup).GetMethod(
            "GetTasks", BindingFlags.NonPublic | BindingFlags.Static);
        var tasks = (IEnumerable)getTasks.Invoke(null, new object[] { group });

        var sb = new StringBuilder();
        foreach (var task in tasks)
        {
            var t = task.GetType();

            var isDone = t.GetProperty("IsDone").GetValue(task) as Func<BuildTargetGroup, bool>;
            if (isDone != null && isDone(group)) continue;

            var msgObj = t.GetProperty("Message").GetValue(task);
            string msg = msgObj?.GetType().GetMethod("GetValue")
                .Invoke(msgObj, new object[] { group })?.ToString();

            var lvlObj = t.GetProperty("Level").GetValue(task);
            string level = lvlObj?.GetType().GetMethod("GetValue")
                .Invoke(lvlObj, new object[] { group })?.ToString();

            bool auto = t.GetProperty("FixAction").GetValue(task) != null
                     || t.GetProperty("AsyncFixAction").GetValue(task) != null;

            sb.AppendLine($"[{level}] {(auto ? "auto" : "manual")} — {msg}");
        }
        return sb.ToString();
    }
}
```

If you enumerate `_tasks` directly instead of calling `GetTasks`, you must filter yourself:

1. **Platform**: skip if `task.Platform != Unknown` and doesn't match target
2. **Validity**: skip if `task.Valid.GetValue(targetGroup)` is false
3. **isDone**: report tasks where `task.IsDone.Invoke(targetGroup)` is false

### Fixing Issues

#### Option 1: FixAllAsync (Preferred)

`FixAllAsync` is the only **public** fix method on `OVRProjectSetup`, so call it directly:

```csharp
OVRProjectSetup.FixAllAsync(BuildTargetGroup.Android);
```

**CRITICAL:** `FixAllAsync` processes asynchronously via `EditorApplication.update`. Fixes apply **after** the script returns control to Unity. Verify results in a **separate** follow-up `run_script`. Do NOT call `Task.Wait()` in the same script — it deadlocks the main thread.

#### Option 2: Direct FixAction Invocation

For individual tasks, invoke the `FixAction` delegate directly:

```csharp
var fix = task.GetType().GetProperty("FixAction").GetValue(task) as Action<BuildTargetGroup>;
fix?.Invoke(targetGroup);
```

Executes synchronously — can verify in the same command, but bypasses the `ProcessorQueue`.

### Supported Platforms

- `BuildTargetGroup.Android` — Meta VR devices
- `BuildTargetGroup.Standalone` — PC VR (Link/Air Link)

## Editor UI Reference

### Opening the Tool

- **Menu**: Meta > Tools > Project Setup Tool
- **Alternative**: Edit > Project Settings > Meta XR

### Tool Interface

The main panel displays Configuration Tasks per target platform and per category/level:

#### Actions
- **Target Group**: Switch between build target groups
- **Filter by Group**: Filter tasks by group (packages, compatibility, features, rendering)
- **Fix All**: Fix all outstanding required settings
- **Apply All**: Apply all recommended settings

#### Cog Menu Options
- **Background Checks**: Toggle continuous background checks
- **Required throw errors**: Uncheck to ignore failing tasks when building
- **Log outstanding issues**: Uncheck to prevent console log spam
- **Show Status Icon**: Toggle status icon in editor bottom-right
- **Produce Report on Build**: Generate JSON report listing all rules and status

### Task Actions
- **Fix/Apply**: Manually call the fix delegate
- **Documentation**: Open related documentation URL
- **Ignore/Unignore**: Move task to ignored category

## Implementing Custom Configuration Tasks

To register custom tasks, use `OVRProjectSetup.AddTask()`. To find the current method signature and parameter options:
First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then:
- **Source file**: grep for `AddTask` in `Editor/OVRProjectSetup/OVRProjectSetup.cs`
- **Task groups**: grep for `enum TaskGroup` in the same directory
- **Task levels**: grep for `enum TaskLevel` (Required, Recommended, Optional)
- **Existing tasks as examples**: grep for `AddTask(` across `Editor/OVRProjectSetup/Tasks/Implementations/` to see how built-in tasks are defined

### Key Rules
- `message` or `conditionalMessage` must be unique (hashed for task UID)
- `isDone` and `fix` are required (ArgumentNullException if null)
- `group` cannot be "All"
- Call AddTask as early as possible for early detection
- Use `conditionalValidity` to skip tasks when preconditions aren't met
- Tasks cannot be removed once added

## Generated Report

A JSON report of project health can be generated (available from v52+). To find the current report format and CLI usage:
- **CLI entry point**: grep for `GenerateProjectSetupReport` in the SDK's `Editor/` directory

## Analyzing OVRProjectSetup for Feature Changes

To understand how any feature setting works programmatically:

1. **Find the source file**: Search for `OVRProjectSetup` in the package source
2. **Locate AddTask calls**: Each call defines one configuration task with its isDone check and fix action
3. **Understand the fix delegate**: This shows exactly what Unity settings or manifest entries are changed
4. **Replicate programmatically**: Use the same APIs the fix delegate uses, run through `unity command run_script` (see "Running SDK code" in [SKILL.md](../SKILL.md))

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-upst-overview
