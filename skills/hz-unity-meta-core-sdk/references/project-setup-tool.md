# Project Setup Tool (OVRProjectSetup) Reference

The Unity Project Setup Tool (UPST) helps configure projects for Meta Quest development using a registry of **Configuration Tasks** that are checked and fixed automatically.

## CRITICAL: AndroidManifest Updates

**NEVER directly edit AndroidManifest.xml for features managed by OVRProjectConfig.** See [android-manifest.md](android-manifest.md) for the full workflow.

## Programmatic Access via Unity MCP

**Prefer this approach over the UI when available.** The UPST can be queried and controlled programmatically via Unity MCP RunCommand scripts. For general MCP reflection rules and the base template (type finding, method invocation, error handling), see **"Calling SDK Methods via Unity MCP"** in [SKILL.md](../SKILL.md).

This section covers UPST-specific patterns beyond the base template.

### Additional MCP Constraint: Private/Internal Field Access

The base template only covers **public** methods. UPST requires access to **private/internal** fields. Since `BindingFlags` overloads crash MCP, use `GetRuntimeFields()` instead:

```csharp
var getRuntimeFields = typeof(System.Reflection.RuntimeReflectionExtensions)
    .GetMethod("GetRuntimeFields");

// Returns ALL fields (private, internal, public, static, instance) without BindingFlags
var allFields = getRuntimeFields.Invoke(null, new object[] { someType })
    as System.Collections.IEnumerable;
```

Also note: `typeof(OVRProjectSetup)` does not compile in MCP — use the `FindType()` pattern from the base template.

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

```csharp
// Get _principalRegistry from OVRProjectSetup (use FindType from base template)
System.Reflection.FieldInfo registryField = null;
foreach (var f in (System.Collections.IEnumerable)getRuntimeFields
    .Invoke(null, new object[] { setupType }))
{
    var fi = f as System.Reflection.FieldInfo;
    if (fi.Name == "_principalRegistry") { registryField = fi; break; }
}
var registry = registryField.GetValue(null);

// Get _tasks from registry
System.Reflection.FieldInfo tasksField = null;
foreach (var f in (System.Collections.IEnumerable)getRuntimeFields
    .Invoke(null, new object[] { registry.GetType() }))
{
    var fi = f as System.Reflection.FieldInfo;
    if (fi.Name == "_tasks") { tasksField = fi; break; }
}
var tasksList = tasksField.GetValue(registry) as System.Collections.IList;
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

**OptionalLambdaType** properties (`Message`, `Level`, `Valid`, `FixMessage`, `ManualSetup`) require calling `.GetValue(targetGroup)`:

```csharp
var msgObj = messageProp.GetValue(task);
string message = msgObj.GetType().GetMethod("GetValue")
    .Invoke(msgObj, new object[] { targetGroup })?.ToString();
```

**Func delegate** properties (`IsDone`) require calling `.Invoke(targetGroup)`:

```csharp
var isDoneFunc = isDoneProp.GetValue(task);
bool isDone = (bool)isDoneFunc.GetType().GetMethod("Invoke")
    .Invoke(isDoneFunc, new object[] { targetGroup });
```

### Determining Fix Category

**Do NOT rely on `FixAutomatic` alone.** A task can have `FixAutomatic=True` but no `FixAction`. Determine category by checking what delegates exist:

- `FixAction != null` or `AsyncFixAction != null` → **Auto-fix**
- Neither fix action, but `ManualSetup.GetValue(targetGroup) != null` → **Manual (Guided Setup)**
- None of the above → **Manual**

### Listing Issues

When no platform is specified, default to `EditorUserBuildSettings.selectedBuildTargetGroup`, falling back to `BuildTargetGroup.Android`.

Filter each task by:
1. **Platform**: skip if `task.Platform != Unknown` and doesn't match target
2. **Validity**: skip if `task.Valid.GetValue(targetGroup)` is false
3. **isDone**: report tasks where `task.IsDone.Invoke(targetGroup)` is false

### Fixing Issues

#### Option 1: FixAllAsync (Preferred)

`FixAllAsync` is the only **public** fix method on `OVRProjectSetup`. Use the base template to invoke it:

```csharp
var fixAllMethod = setupType.GetMethod("FixAllAsync");
fixAllMethod.Invoke(null, new object[] { targetGroup });
```

**CRITICAL:** `FixAllAsync` processes asynchronously via `EditorApplication.update`. Fixes apply **after** the MCP command returns control to Unity. Verify results in a **separate** follow-up MCP command. Do NOT call `Task.Wait()` in the same command — it deadlocks the main thread.

#### Option 2: Direct FixAction Invocation

For individual tasks, invoke the `FixAction` delegate directly:

```csharp
var fixAction = fixActionProp.GetValue(task);
if (fixAction != null)
    fixAction.GetType().GetMethod("Invoke")
        .Invoke(fixAction, new object[] { targetGroup });
```

Executes synchronously — can verify in the same command, but bypasses the `ProcessorQueue`.

### Supported Platforms

- `BuildTargetGroup.Android` — Quest headsets
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
4. **Replicate programmatically**: Use the same APIs the fix delegate uses. When calling these via Unity MCP, follow the reflection pattern in "Calling SDK Methods via Unity MCP" in SKILL.md

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-upst-overview
