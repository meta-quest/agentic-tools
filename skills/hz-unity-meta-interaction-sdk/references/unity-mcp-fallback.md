# Unity MCP Fallback Reference

**`unity-cli` is the primary way to run the Quick Actions calls** — see "Running the calls" in
[SKILL.md](../SKILL.md). This file is for projects still driven through a **Unity MCP** server.

Nothing here applies to `unity-cli`: its Roslyn `run_script` compiles against **every loaded
assembly**, so `using Oculus.Interaction.Editor.QuickActions;` and
`using Oculus.Interaction.OVR.Editor.QuickActions;` resolve directly and full `System.Reflection` /
`BindingFlags` work.

## Harness constraints

Under MCP every script is wrapped in the harness type, and two things that are fine under
`run_script` will crash the MCP framework:

```csharp
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result) { /* body */ }
}
```

- **Never add `using System.Reflection;`** — causes `UNEXPECTED_ERROR`. Fully qualify inline instead
  (`System.Reflection.MethodInfo`).
- **Never use `BindingFlags` overloads** of `GetMethod` / `GetProperty`. The parameterless
  `GetMethod("Name")` finds public methods.
- Report through `result.Log` / `result.LogError` / `result.LogWarning` rather than returning a string.
- Register created objects with `result.RegisterObjectCreation(go)` rather than
  `Undo.RegisterCreatedObjectUndo(go, "…")`.

## Reaching the Quick Actions APIs

The MCP harness wraps scripts in `Unity.AI.Assistant.Agent.Dynamic.Extension.Editor` and does not
reference package **Editor** assemblies — the documented behavior for the sibling Meta SDKs
(`Meta.*` in Movement SDK, `meta.xr.mrutilitykit` in MRUK). Expect the same for
`Oculus.Interaction.Editor` and `Oculus.Interaction.OVR.Editor`, so a top-level `using` will not
compile and you need the type by name.

> Not verified against a live MCP server for these two assemblies specifically — confirm before
> relying on it. If the `using` compiles, use the typed call and ignore the rest of this section.

```csharp
// QuickActionsAPI lives in Oculus.Interaction.Editor; OVRQuickActionsAPI in Oculus.Interaction.OVR.Editor
var qa = System.Type.GetType(
    "Oculus.Interaction.Editor.QuickActions.QuickActionsAPI, Oculus.Interaction.Editor");
var ovrQa = System.Type.GetType(
    "Oculus.Interaction.OVR.Editor.QuickActions.OVRQuickActionsAPI, Oculus.Interaction.OVR.Editor");

// Every method takes a GameObject target — see the signature table in SKILL.md
qa.GetMethod("AddGrabInteraction").Invoke(null, new object[] { go });

// AddOVRInteractionRig(bool generateAsEditableCopy = true) — the default does NOT apply
// through reflection, so pass it explicitly
try { ovrQa.GetMethod("AddOVRInteractionRig").Invoke(null, new object[] { true }); }
catch (System.Reflection.TargetInvocationException tie)
{
    result.LogError("AddOVRInteractionRig failed: " + tie.InnerException);
}
```

Always catch `System.Reflection.TargetInvocationException` and log `InnerException` — reflection wraps
the real error.

**Optional parameters do not apply through reflection.** `Invoke` needs the full argument list, so
pass the defaults explicitly. Both verified signatures that bite here:

- `AddOVRInteractionRig(Boolean generateAsEditableCopy = True)` — pass `true`, not `null`
- `AddRayCanvasInteraction(GameObject target, Boolean fixPointableCanvasModule = True)` — pass both

## Prefer the menu items over reflection here

**Reflect only into the two public API classes, never into the wizards.** SKILL.md's rule stands: if
there is no API method for what you want, use the menu item or build it from components. Reflecting
into wizard internals is unsupported and breaks across SDK versions.

The same actions exist under **`GameObject > Interaction SDK`** as blocking wizard windows. Under MCP
a modal wizard blocks the request until it times out, so drive those through a project Editor script
with a `[MenuItem]` that a human triggers — option 2 in SKILL.md — rather than from MCP.

## Meta Unity extension tools

A Unity MCP server with the Meta Unity extension exposes a few tools that cover the common rig and
canvas setup without any reflection:

| Tool | Parameters | Equivalent Quick Action |
|---|---|---|
| `meta_add_interactionrig` | none | `OVRQuickActionsAPI.AddOVRInteractionRig()` |
| `meta_add_canvas_interaction_ray` | `NameOrID: "<canvas name>"` | `QuickActionsAPI.AddRayCanvasInteraction(go)` |
| `meta_add_canvas_interaction_poke` | `NameOrID: "<canvas name>"` | `QuickActionsAPI.AddPokeCanvasInteraction(go)` |

`meta_add_interactionrig` requires `OVRCameraRig` to already be in the scene and adds
`OVRInteractionComprehensive` under it, whereas `AddOVRInteractionRig()` creates a camera rig if the
scene has none. These have no command-catalog equivalent, so they are reachable only over MCP.

## Command equivalences

| `unity-cli` | MCP |
|---|---|
| `unity command clear_console` | `Unity_ClearConsole` |
| `unity command get_console_logs --severity error` | `Unity_GetConsoleLogs` with `logTypes: "Error"` |
| `unity command editor_play` / `editor_stop` | the harness's play-mode tools |
| `unity command package_add --identifier <id> --confirm true --wait true` | `Client.Add(id)` in a script, polling `request.IsCompleted` |
| `unity command save_scene` | `EditorSceneManager.MarkSceneDirty` + `SaveScene` in a script |

The Play-mode verification loop in [verification.md](verification.md) is unchanged in substance —
clear the console, enter Play mode, read the log, exit. Only the transport differs.

**A package install still forces a recompile and domain reload**, so never install a package and use
its API in the same script, on either transport.
