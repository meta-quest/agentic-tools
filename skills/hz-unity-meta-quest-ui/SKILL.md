---
name: hz-unity-meta-quest-ui
license: Apache-2.0
description: Configures Unity UI for Meta VR and Horizon OS VR development — world-space canvases, TextMesh Pro setup, comfortable sizing, viewing distances, and interaction readiness.
---

# Meta VR UI Setup

## When to use this skill

Use this skill automatically when:
- Setting up a Canvas for VR
- Creating UI text with TextMesh Pro in a VR project
- Adding buttons, sliders, or other interactive UI in VR
- User reports pink/magenta text, unclickable buttons, or UI sizing issues in VR
- Configuring VR interaction (ray or poke) on a Canvas

## Driving the Editor

Build the UI against an open Editor with `unity-cli` rather than hand-editing scene YAML — the Editor
applies changes to the actual active scene.

```bash
unity status --format json          # look for state "ready"
unity command run_script --file AgentScripts/BuildUI.cs --entry BuildUI.Run --format json
```

Canvas construction is multi-component work, so it belongs in a `run_script` `.cs` file (kept
**outside `Assets/`**, path relative to the project root) rather than a chain of single commands.
Add `--dry_run true` to compile-check a script before it mutates the scene — but read the verdict
from **`data.result.success`**: a compile failure still returns outer `success: true` and exit code
`0`, with the errors in `data.result.diagnostics`. Connecting, `--project-path`, and Safe Mode
recovery are in the **`unity-cli`** skill; MCP equivalents are in
[references/unity-mcp-fallback.md](references/unity-mcp-fallback.md).

Single-property reads and writes are cheaper as direct commands than as a script:

```bash
unity command get_component_properties --target MenuUI --type Canvas --format json
unity command set_transform --target MenuUI --position "[0,1.5,2]" --format json
unity command find_gameobjects --name MenuUI --format json
```

Array parameters take a **JSON array in one argument** (`--position "[0,1.5,2]"`). Space- or
comma-separated components are rejected with `INVALID_COMMAND_ARGS`. Note `set_transform` writes
**local** position — see the **unity-placement** skill for parented canvases.

## Prerequisite: TMP Essential Resources

Before creating ANY VR UI, verify TMP resources are imported:

```bash
unity command find_assets --name "LiberationSans SDF" --format json
```

A `data.result.count` of `0` means they are missing — use the **tmp-resources** skill before
proceeding.

## Step 1: Create World Space Canvas

```csharp
// AgentScripts/BuildUI.cs
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class BuildUI
{
    public static string Run()
    {
        // Adapt the name to match your canvas (e.g., "MainMenu", "SettingsUI")
        var go = new GameObject("MenuUI");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        go.AddComponent<GraphicRaycaster>();

        // Remove CanvasScaler — not appropriate for VR
        var scaler = go.GetComponent<CanvasScaler>();
        if (scaler != null)
            Object.DestroyImmediate(scaler);

        var rt = go.GetComponent<RectTransform>();
        rt.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        rt.sizeDelta = new Vector2(1920f, 1080f);
        rt.position = new Vector3(0f, 1.5f, 2f);

        Undo.RegisterCreatedObjectUndo(go, "Create VR Canvas");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return $"Created VR Canvas '{go.name}'. Scale: {rt.localScale}, Size: {rt.sizeDelta}, Position: {rt.position}";
    }
}
```

Persist it when the shape is right:

```bash
unity command save_scene --format json
```

### Canvas rules

- **Render Mode**: Always World Space. Screen Space modes break stereo rendering.
- **Scale**: 0.001 on all axes (1 unit in canvas = 1mm in world).
- **CanvasScaler**: Remove it. Physical size is controlled by world scale, not screen adaptation.
- **Distance**: Place 1.5-3m from user. Never closer than 0.5m. Max 5m for readable text.
- **Physical size formula**: `Canvas sizeDelta * scale = meters`. Example: 1920 * 0.001 = 1.92m wide.

## Step 2: Create child UI elements

All child elements (panels, buttons, text) must follow these rules:

- **localScale**: Always `[1, 1, 1]`. Never scale children to compensate for canvas scale.
- **localPosition.z**: Always `0`. Children must sit on the canvas plane.
- **Size control**: Use `RectTransform.sizeDelta` and anchors, never scale.

```csharp
// AgentScripts/BuildPanel.cs
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class BuildPanel
{
    public static string Run()
    {
        // Replace "MenuUI" with the actual canvas name used in Step 1
        var canvas = GameObject.Find("MenuUI");
        if (canvas == null) return "ERROR: Canvas 'MenuUI' not found.";

        // Panel
        var panel = new GameObject("ButtonPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.localScale = Vector3.one;
        panelRT.sizeDelta = new Vector2(800f, 600f);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        panelImg.raycastTarget = false;

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 50f;
        layout.padding = new RectOffset(80, 80, 100, 100);
        layout.childAlignment = TextAnchor.MiddleCenter;

        // Button
        var btnGO = new GameObject("StartButton");
        btnGO.transform.SetParent(panel.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.localScale = Vector3.one;
        btnRT.sizeDelta = new Vector2(400f, 120f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 1f, 1f);
        btnGO.AddComponent<Button>();

        // Button text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.localScale = Vector3.one;
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Start";
        tmp.fontSize = 48f;
        tmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        Undo.RegisterCreatedObjectUndo(panel, "Create VR Panel");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return $"Created panel with button. Panel scale: {panelRT.localScale}, Button scale: {btnRT.localScale}";
    }
}
```

## Step 3: Validate created UI

Spot-check the canvas itself with a direct command:

```bash
unity command get_component_properties --target MenuUI --type Canvas --format json
unity command get_scene_hierarchy --format json                # whole active scene, incl. children
```

`get_scene_hierarchy --path` takes an open **scene** path, not a GameObject — `--path MenuUI` fails
with *"Scene 'MenuUI' is not open."* Omit it and read the `MenuUI` subtree out of the active scene.

Editor checks only go so far — deploy the build and pull `metavr capture screenshot` from the headset to see the UI the way users will.

To sweep every child and auto-fix, use a script:

```csharp
// AgentScripts/ValidateUI.cs
using System.Text;
using UnityEngine;

public static class ValidateUI
{
    public static string Run()
    {
        // Replace "MenuUI" with the actual canvas name
        var canvas = GameObject.Find("MenuUI");
        if (canvas == null) return "ERROR: Canvas not found.";

        var crt = canvas.GetComponent<RectTransform>();
        var c = canvas.GetComponent<Canvas>();
        var sb = new StringBuilder();

        if (c.renderMode != RenderMode.WorldSpace)
            sb.AppendLine("FAIL: Canvas renderMode is not World Space.");

        if (Mathf.Abs(crt.localScale.x - 0.001f) > 0.0001f)
            sb.AppendLine($"FAIL: Canvas scale is {crt.localScale}, expected 0.001.");

        foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (child == canvas.transform) continue;
            var rt = child.GetComponent<RectTransform>();
            if (rt == null) continue;

            if (rt.localScale != Vector3.one)
            {
                sb.AppendLine($"FIXED: '{child.name}' had localScale {rt.localScale}, expected (1,1,1).");
                rt.localScale = Vector3.one;
            }
            if (Mathf.Abs(rt.localPosition.z) > 0.01f)
            {
                sb.AppendLine($"FIXED: '{child.name}' had localPosition.z={rt.localPosition.z}, expected 0.");
                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
            }
        }

        return sb.Length == 0 ? "Validation PASSED. All properties correct." : sb.ToString();
    }
}
```

Common issues this catches:
- Canvas scale stuck at 1,1,1 instead of 0.001
- Child elements auto-scaled to compensate for parent (e.g., 1000x)
- Child elements offset on Z axis (e.g., localPosition.z = -2500)

## Step 4: Add VR interaction (interactive UI only)

**Only add interaction when your Canvas has interactive elements** (buttons, sliders, dropdowns, toggles, input fields). Skip this for display-only UI (HUD, score, labels).

### Decision tree

```text
Does your Canvas have buttons, dropdowns, sliders, toggles, or input fields?
  YES -> Add VR interaction. Without it, interactive elements won't work in VR.
  NO  -> Skip. Display-only UI doesn't need interaction overhead.
```

### Wiring it — ISDK Quick Actions

The supported entry point is the ISDK Quick Actions API, called through `run_script`. See the
**`hz-unity-meta-interaction-sdk`** skill for signatures and the behavior these calls bake in.

```csharp
// AgentScripts/AddCanvasInteraction.cs
using Oculus.Interaction.Editor.QuickActions;   // Editor-only assembly
using UnityEngine;

public static class AddCanvasInteraction
{
    public static string Run()
    {
        var target = GameObject.Find("MenuUI");
        QuickActionsAPI.AddRayCanvasInteraction(target);    // or AddPokeCanvasInteraction
        return "ok";
    }
}
```

**These calls take the canvas's `GameObject`, not a `Canvas` component** — passing `Canvas` is a
compile error (`cannot convert from 'UnityEngine.Canvas' to 'UnityEngine.GameObject'`). Verified
signatures:

| Interaction | Signature | When to use |
|---|---|---|
| **Ray** (default) | `AddRayCanvasInteraction(GameObject target, bool fixPointableCanvasModule = true)` | Menus, settings panels, any UI beyond arm's reach |
| **Poke** (close range) | `AddPokeCanvasInteraction(GameObject target, bool fixPointableCanvasModule = true)` | Control panels on surfaces, virtual keyboards, diegetic UI on props (< 0.8m) |
| **Gaze** | `AddGazeCanvasInteraction(GameObject target, bool enableRayFallback = true, bool addToChildCanvases = true, bool fixPointableCanvasModule = true)` | Eye-tracked selection |
| **Both** (advanced) | Ray first, then poke | Hybrid UIs where users can point OR touch |

Ray adds `RayInteractable`, `PointableCanvas`, an `ISDK_RayCanvasInteraction` child, and a
scene-level `Pointable Canvas Module`. Poke adds `PokeInteractable`, `PointableCanvas`, and
close-range collision detection.

> Pass `fixPointableCanvasModule: false` when the `PointableCanvasModule` comes from a prefab or an
> unloaded additive scene, or you get a second one in the open scene.

If a Unity MCP server with the Meta Unity extension is what you have instead, the equivalent tools
are `meta_add_canvas_interaction_ray` / `meta_add_canvas_interaction_poke` —
see [references/unity-mcp-fallback.md](references/unity-mcp-fallback.md).

### Verify interaction was added

```bash
unity command get_component_properties --target MenuUI --type PointableCanvas --format json
unity command find_gameobjects --name "Pointable Canvas Module" --format json
```

`get_component_properties` needs `--type` whenever `--target` is a GameObject (without it the call
fails: *"'type' is required when 'target' is a GameObject."*), and it addresses **one** component
rather than listing them — so name the component you are asserting. Success means the component is
there; a failure naming the type means it is not.

`PointableCanvas` on the canvas plus a `Pointable Canvas Module` in the scene is the bar. Note that
`PointableElement` wiring is assigned in `Awake()`, so on a **prefab asset** it always reads null —
check the serialized field instead:

```bash
unity command get_serialized_fields --target <interactable> --field _pointableElement --format json
```

### If buttons work in Editor but not on Meta VR device

The scene needs an interaction rig at runtime:

```csharp
using Oculus.Interaction.OVR.Editor.QuickActions;
OVRQuickActionsAPI.AddOVRInteractionRig();      // also adds an OVRCameraRig if the scene has none
```

This provides all hand/controller interactors. See **`hz-unity-meta-interaction-sdk`** for the rig it
builds and the "interactable authored without a rig present" caveat.

## VR UI sizing reference

All sizes assume canvas scale of 0.001 (1 unit = 1mm).

### Element sizes (minimum)

| Element | sizeDelta (units) | Physical size |
|---|---|---|
| Button (small) | 250 x 100 | 25cm x 10cm |
| Button (standard) | 300 x 120 | 30cm x 12cm |
| Button (large) | 400 x 150 | 40cm x 15cm |
| Dropdown | 500 x 140 | 50cm x 14cm |
| Slider | 600 x 80 | 60cm x 8cm |
| Toggle | 120 x 120 | 12cm x 12cm |
| Input Field | 800 x 140 | 80cm x 14cm |
| Panel/Menu | 1200-2000 x 800-1400 | 1.2-2m x 0.8-1.4m |

### Button spacing

Minimum 50 units (5cm) between interactive elements to prevent mis-clicks.

### Font sizes (at 0.001 canvas scale)

| Distance | Minimum | Comfortable | Large/Title |
|---|---|---|---|
| 1.5m | 32pt | 40-48pt | 60-72pt |
| 2.0m | 36pt | 48-56pt | 72-84pt |
| 2.5m | 40pt | 52-64pt | 84-96pt |
| 3.0m | 48pt | 64-72pt | 96-120pt |

Never use auto-sizing in VR. Never use legacy Text components — always TextMeshPro.

### Colors

- **Text**: off-white (0.9, 0.9, 0.9) or dark gray (0.1, 0.1, 0.1). Avoid pure white/black.
- **Backgrounds**: dark (0.1, 0.1, 0.1, 0.95) or light (0.9, 0.9, 0.9, 0.95).
- **Contrast ratio**: minimum 4.5:1, prefer 7:1+.
- Prefer opaque backgrounds over transparent (cheaper to render).

## Performance tips

- Disable `raycastTarget` on non-interactive elements (labels, backgrounds, decorative images).
- Split static and dynamic content onto separate canvases to minimize rebuilds.
- Canvas rebuild cost should be < 1-2ms to maintain 72/90Hz.
- Share one SDF font asset per font family across all text elements.
- Disable canvases or GameObjects when not visible.

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Pink/magenta text | TMP resources not imported | Use **tmp-resources** skill |
| Buttons not clickable in VR | Missing VR interaction on Canvas | `QuickActionsAPI.AddRayCanvasInteraction(go)` |
| Buttons work in Editor, not on device | Missing interaction rig | `OVRQuickActionsAPI.AddOVRInteractionRig()` |
| UI too small/large | Wrong canvas scale | Set to 0.001, verify with the validation script |
| Children offset in Z | Auto-positioning bug | Set `localPosition.z = 0` on all children |
| Children scaled to 1000x | Auto-scale compensation | Set `localScale = Vector3.one` on all children |
| Text blurry | Low atlas resolution or small font | Use SDF 2048x2048+, font size 48+ |
| Frame drops | Canvas rebuilding too often | Split into static/dynamic canvases |
| `unity command` won't connect | Editor in Safe Mode from compile errors | `unity pipeline list`, fix the errors, restart Unity |

## Checklists

### Interactive UI (buttons, sliders, etc.)

```text
[ ] TMP Essential Resources imported
[ ] Canvas: renderMode = World Space
[ ] Canvas: localScale = [0.001, 0.001, 0.001]
[ ] Canvas: positioned 1.5-3m from user
[ ] All children: localScale = [1, 1, 1]
[ ] All children: localPosition.z = 0
[ ] Sizes via sizeDelta, not scale
[ ] AddRayCanvasInteraction called on Canvas
[ ] PointableCanvas component verified on Canvas
[ ] Pointable Canvas Module exists in scene
[ ] Interaction rig present (AddOVRInteractionRig)
[ ] Buttons >= 250x100 units
[ ] Button spacing >= 50 units
[ ] Text >= 48pt, raycastTarget = false on non-interactive text
[ ] Validation script run and passed
[ ] Scene saved (unity command save_scene)
```

### Display-only UI (HUD, score, labels)

```text
[ ] TMP Essential Resources imported
[ ] Canvas: renderMode = World Space
[ ] Canvas: localScale = [0.001, 0.001, 0.001]
[ ] All children: localScale = [1, 1, 1]
[ ] All children: localPosition.z = 0
[ ] Text >= 48pt
[ ] raycastTarget = false on all elements
[ ] NO interaction components needed
[ ] Scene saved (unity command save_scene)
```

## Integration with other skills

- **tmp-resources**: Use first to import TMP Essential Resources before creating any UI.
- **hz-unity-meta-interaction-sdk**: Owns the Quick Actions APIs used in Step 4.
- **unity-placement**: Use for positioning canvases relative to other scene objects.
- **unity-cli**: Connecting to an Editor, `--project-path`, Safe Mode recovery.
