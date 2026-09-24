# Unity MCP Fallback Reference

**`unity-cli` is the primary way to drive the Editor for this skill** — see [SKILL.md](../SKILL.md).
This file covers the Unity MCP route: the `Unity_RunCommand` harness, and the Meta Unity extension
tools that have no `unity-cli` equivalent.

## Harness differences

Under MCP every script is wrapped in the harness type, and two things that work fine under
`run_script` will crash the MCP framework:

```csharp
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result) { /* body */ }
}
```

- **Never add `using System.Reflection;`** — causes `UNEXPECTED_ERROR`.
- **Never use `BindingFlags` overloads** of `GetMethod` / `GetProperty`.
- Report through `result.Log` / `result.LogError` / `result.LogWarning` instead of returning a string.
- Register created objects with `result.RegisterObjectCreation(go)` instead of
  `Undo.RegisterCreatedObjectUndo(go, "…")`.

Everything else in the SKILL.md scripts transfers unchanged — translate the body and swap the
reporting calls.

## Meta Unity extension tools (no CLI equivalent)

These come from the Meta Unity extension's MCP server, not the `com.unity.pipeline` command catalog,
so they are reachable only over MCP. Prefer the ISDK Quick Actions calls in SKILL.md when you have a
CLI-driven Editor; use these when MCP is all you have.

| Tool | Parameters | Equivalent Quick Action |
|---|---|---|
| `meta_add_canvas_interaction_ray` | `NameOrID: "<canvas name>"` | `QuickActionsAPI.AddRayCanvasInteraction(go)` |
| `meta_add_canvas_interaction_poke` | `NameOrID: "<canvas name>"` | `QuickActionsAPI.AddPokeCanvasInteraction(go)` |
| `meta_add_interactionrig` | none | `OVRQuickActionsAPI.AddOVRInteractionRig()` |

`meta_add_interactionrig` adds `OVRInteractionComprehensive` as a child of the Camera Rig, providing
all hand/controller interactors. It requires `OVRCameraRig` to already be in the scene, whereas
`AddOVRInteractionRig()` creates one if the scene has none.

Call ray first, then poke, for hybrid UIs.

## Verifying interaction was added

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // Replace "MenuUI" with the actual canvas name
        var canvas = GameObject.Find("MenuUI");
        if (canvas == null) { result.LogError("Canvas not found."); return; }

        var components = canvas.GetComponents<Component>();
        bool hasPointable = false;
        foreach (var c in components)
        {
            if (c != null && c.GetType().Name.Contains("PointableCanvas"))
                hasPointable = true;
        }

        if (hasPointable)
            result.Log("VR interaction verified: PointableCanvas present on '{0}'.", canvas.name);
        else
            result.LogError("VR interaction MISSING on '{0}'.", canvas.name);

        // Check scene-level Pointable Canvas Module
        var module = GameObject.Find("Pointable Canvas Module");
        if (module != null)
            result.Log("Pointable Canvas Module found in scene.");
        else
            result.LogWarning("Pointable Canvas Module not found. It should be auto-created.");
    }
}
```

Under `unity-cli` this is two read commands instead of a script — but `get_component_properties`
addresses one named component rather than enumerating them, so assert the type directly:

```bash
unity command get_component_properties --target MenuUI --type PointableCanvas --format json
unity command find_gameobjects --name "Pointable Canvas Module" --format json
```

## Checking TMP resources

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        string fontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        var font = AssetDatabase.LoadAssetAtPath<Object>(fontPath);
        if (font != null)
            result.Log("TMP Essential Resources: IMPORTED. Default font present.");
        else
            result.LogError("TMP Essential Resources: NOT IMPORTED. Use tmp-resources skill first.");
    }
}
```

CLI equivalent: `unity command find_assets --name "LiberationSans SDF" --format json`.
