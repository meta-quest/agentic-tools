---
name: hz-unity-meta-quest-ui
description: Configures Unity UI for Meta Quest and Horizon OS VR development — world-space canvases, TextMesh Pro setup, comfortable sizing, viewing distances, and interaction readiness.
---

# Meta Quest VR UI Setup

## When to use this skill

Use this skill automatically when:
- Setting up a Canvas for VR
- Creating UI text with TextMesh Pro in a VR project
- Adding buttons, sliders, or other interactive UI in VR
- User reports pink/magenta text, unclickable buttons, or UI sizing issues in VR
- Configuring VR interaction (ray or poke) on a Canvas

## Prerequisite: TMP Essential Resources

Before creating ANY VR UI, verify TMP resources are imported. Use `Unity_RunCommand`:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

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

If not imported, use the **tmp-resources** skill before proceeding.

## Step 1: Create World Space Canvas

Use `Unity_RunCommand` to create and configure the canvas:

```csharp
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
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

        result.RegisterObjectCreation(go);
        result.Log("Created VR Canvas '{0}'. Scale: {1}, Size: {2}, Position: {3}",
            go.name, rt.localScale, rt.sizeDelta, rt.position);
    }
}
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
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // Replace "MenuUI" with the actual canvas name used in Step 1
        var canvas = GameObject.Find("MenuUI");
        if (canvas == null) { result.LogError("Canvas 'MenuUI' not found."); return; }

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

        result.RegisterObjectCreation(panel);
        result.Log("Created panel with button. Panel scale: {0}, Button scale: {1}",
            panelRT.localScale, btnRT.localScale);
    }
}
```

## Step 3: Validate created UI

After creating UI, verify critical properties with `Unity_RunCommand`:

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

        var crt = canvas.GetComponent<RectTransform>();
        var c = canvas.GetComponent<Canvas>();

        bool pass = true;

        if (c.renderMode != RenderMode.WorldSpace)
        { result.LogError("Canvas renderMode is not World Space."); pass = false; }

        if (Mathf.Abs(crt.localScale.x - 0.001f) > 0.0001f)
        { result.LogError("Canvas scale is {0}, expected 0.001.", crt.localScale); pass = false; }

        // Check all children
        foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (child == canvas.transform) continue;
            var rt = child.GetComponent<RectTransform>();
            if (rt == null) continue;

            if (rt.localScale != Vector3.one)
            {
                result.LogError("Child '{0}' has localScale {1}, expected (1,1,1).", child.name, rt.localScale);
                rt.localScale = Vector3.one;
                pass = false;
            }
            if (Mathf.Abs(rt.localPosition.z) > 0.01f)
            {
                result.LogError("Child '{0}' has localPosition.z={1}, expected 0.", child.name, rt.localPosition.z);
                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
                pass = false;
            }
        }

        if (pass)
            result.Log("Validation PASSED. All properties correct.");
        else
            result.LogWarning("Validation found issues. Auto-fixed child scale/position where possible.");
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

```
Does your Canvas have buttons, dropdowns, sliders, toggles, or input fields?
  YES -> Add VR interaction. Without it, interactive elements won't work in VR.
  NO  -> Skip. Display-only UI doesn't need interaction overhead.
```

### Ray interaction (default choice)

Use for most VR UI — lets users point at UI with controller or hand rays:

Call the Meta Unity extension MCP tool `meta_add_canvas_interaction_ray` with the canvas name:

```
meta_add_canvas_interaction_ray  with  NameOrID: "<your canvas name>"
```

This adds: `RayInteractable`, `PointableCanvas`, `ISDK_RayCanvasInteraction` child, and a scene-level `Pointable Canvas Module`.

**When to use**: menus, settings panels, any UI beyond arm's reach.

### Poke interaction (close-range only)

Use for physical touch-style interaction within arm's reach (< 0.8m):

Call the Meta Unity extension MCP tool `meta_add_canvas_interaction_poke` with the canvas name:

```
meta_add_canvas_interaction_poke  with  NameOrID: "<your canvas name>"
```

This adds: `PokeInteractable`, `PointableCanvas`, close-range collision detection.

**When to use**: control panels on surfaces, virtual keyboards, diegetic UI on props.

### Both (advanced)

Call ray first, then poke, for hybrid UIs where users can point OR touch.

### Verify interaction was added

After calling the Meta Unity extension MCP tool, verify with `Unity_RunCommand`:

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
            result.LogError("VR interaction MISSING on '{0}'. Call meta_add_canvas_interaction_ray.", canvas.name);

        // Check scene-level Pointable Canvas Module
        var module = GameObject.Find("Pointable Canvas Module");
        if (module != null)
            result.Log("Pointable Canvas Module found in scene.");
        else
            result.LogWarning("Pointable Canvas Module not found. It should be auto-created by the Meta Unity extension MCP tool.");
    }
}
```

### If buttons work in Editor but not on Quest device

Ensure the interaction rig exists in the scene:

```
meta_add_interactionrig  (no parameters)
```

This adds `OVRInteractionComprehensive` as a child of the Camera Rig, providing all hand/controller interactors. Requires `OVRCameraRig` to be in the scene first.

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
| Buttons not clickable in VR | Missing VR interaction on Canvas | Call `meta_add_canvas_interaction_ray` |
| Buttons work in Editor, not on device | Missing interaction rig | Call `meta_add_interactionrig` |
| UI too small/large | Wrong canvas scale | Set to 0.001, verify with validation script |
| Children offset in Z | Auto-positioning bug | Set `localPosition.z = 0` on all children |
| Children scaled to 1000x | Auto-scale compensation | Set `localScale = Vector3.one` on all children |
| Text blurry | Low atlas resolution or small font | Use SDF 2048x2048+, font size 48+ |
| Frame drops | Canvas rebuilding too often | Split into static/dynamic canvases |

## Checklists

### Interactive UI (buttons, sliders, etc.)

```
[ ] TMP Essential Resources imported
[ ] Canvas: renderMode = World Space
[ ] Canvas: localScale = [0.001, 0.001, 0.001]
[ ] Canvas: positioned 1.5-3m from user
[ ] All children: localScale = [1, 1, 1]
[ ] All children: localPosition.z = 0
[ ] Sizes via sizeDelta, not scale
[ ] meta_add_canvas_interaction_ray called on Canvas
[ ] PointableCanvas component verified on Canvas
[ ] Pointable Canvas Module exists in scene
[ ] Interaction rig present (meta_add_interactionrig)
[ ] Buttons >= 250x100 units
[ ] Button spacing >= 50 units
[ ] Text >= 48pt, raycastTarget = false on non-interactive text
[ ] Validation script run and passed
```

### Display-only UI (HUD, score, labels)

```
[ ] TMP Essential Resources imported
[ ] Canvas: renderMode = World Space
[ ] Canvas: localScale = [0.001, 0.001, 0.001]
[ ] All children: localScale = [1, 1, 1]
[ ] All children: localPosition.z = 0
[ ] Text >= 48pt
[ ] raycastTarget = false on all elements
[ ] NO interaction components needed
```

## Integration with other skills

- **tmp-resources**: Use first to import TMP Essential Resources before creating any UI.
- **unity-placement**: Use for positioning canvases relative to other scene objects.
