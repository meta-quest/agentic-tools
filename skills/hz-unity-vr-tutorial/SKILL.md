---
name: hz-unity-vr-tutorial
description: Guided hands-on tutorial that teaches beginners to build a VR target shooter in Unity and play it on a Meta Quest headset via Quest Link in approximately 30 minutes. Covers scene setup, scripting, VR configuration, and deployment for Meta Quest and Horizon OS.
allowed-tools:
  - Bash(metavr:*)
  - Bash(npx -y metavr:*)
  - Bash(adb:*)
  - PowerShell
  - mcp__unity-mcp__Unity_ManageScene
  - mcp__unity-mcp__Unity_ManageGameObject
  - mcp__unity-mcp__Unity_ManageEditor
  - mcp__unity-mcp__Unity_ReadConsole
  - mcp__unity-mcp__Unity_GetConsoleLogs
  - mcp__unity-mcp__Unity_CreateScript
  - mcp__unity-mcp__Unity_ManageScript
  - mcp__unity-mcp__Unity_FindProjectAssets
  - mcp__unity-mcp__Unity_Camera_Capture
  - mcp__unity-mcp__Unity_SceneView_Capture2DScene
  - mcp__unity-mcp__Unity_PackageManager_GetData
  - mcp__unity-mcp__Unity_GetProjectData
  - mcp__unity-mcp__Unity_ManageAsset
  - mcp__unity-mcp__Unity_ValidateScript
---

# VR Target Shooter Tutorial

A guided, hands-on tutorial that takes a complete beginner from an empty Unity project to a working VR target shooter running on their Quest headset.

**The game:** Targets appear around the player. The player aims with their head, pulls the right trigger to shoot, and scores points for each hit. Left thumbstick moves around the space.

**Time:** ~30 minutes with all prerequisites met.

## When to Use This Skill

Use this skill when you need to:

- Teach someone new to Unity how to build a simple VR game
- Walk through a complete Unity → Quest pipeline end to end
- Demonstrate Unity MCP, Quest Link, and VR development workflow
- Create a starter project that can be extended into a more complex game

## Teaching Approach

You are a hands-on teacher. Follow these principles:

- **One step at a time.** Give 1–3 actions, then verify before moving on.
- **MCP first.** Use Unity MCP tools to verify and act — do not ask the user to describe their screen. When the Unity MCP server is connected, use `Unity_ManageScene`, `Unity_ManageGameObject`, and `Unity_ReadConsole` to inspect and modify the project directly.
- **Explain the why.** One sentence per step on why it matters.
- **Skip past blockers.** If a UI step is confusing, do it via MCP or file edits.
- **Celebrate milestones.** Acknowledge progress at each checkpoint.

## Prerequisites (~5 min)

Run all checks automatically — do not ask the user, just verify.

**For a comprehensive prerequisites check, refer to `skills/hz-unity-quest-setup/`** which covers all required packages, settings, and configurations. The essential checks for this tutorial:

### Unity Project

Use Unity MCP to verify a project is open:
```
Unity_ManageScene(Action: "GetActive")
```
If MCP is not connected, check that Unity is running.

### Input System

```powershell
Select-String "activeInputHandler" "PROJECT_PATH\ProjectSettings\ProjectSettings.asset"
```
Must be `2` (Both). If `1`, change it and restart Unity. See `skills/hz-unity-quest-setup/` Check 2.

### Quest + Link

```powershell
# Quest connected?
metavr device list

# Link services running?
Get-Process | Where-Object { $_.ProcessName -match "OVR|Oculus" } | Select-Object ProcessName

# OpenXR runtime correct?
Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime"
```

If any check fails, see `skills/hz-quest-link/` for setup and troubleshooting.

## Phase 1: Build the Scene (~5 min)

### 1.1 Create ground

```
Unity_ManageGameObject(action: "create", name: "Ground", primitive_type: "Plane", position: [0,0,0], scale: [3,1,3])
```

> "This is your floor — everything stands on it."

### 1.2 Create test targets

```
Unity_ManageGameObject(action: "create", name: "Target1", primitive_type: "Cube", position: [0,1,5], tag: "Target")
Unity_ManageGameObject(action: "create", name: "Target2", primitive_type: "Cube", position: [-3,1,8], tag: "Target")
Unity_ManageGameObject(action: "create", name: "Target3", primitive_type: "Cube", position: [3,1.5,10], tag: "Target")
```

If the "Target" tag does not exist, tell the user to create it: Inspector → Tag → Add Tag → "Target".

> "These cubes are your targets. You'll aim and shoot to destroy them."

### 1.3 Verify

```
Unity_ManageScene(Action: "GetHierarchy")
```

Confirm: Ground, Target1, Target2, Target3, Main Camera, Directional Light.

## Phase 2: Game Scripts (~10 min)

Write four scripts to `Assets/Scripts/`. Use the file system (Write tool) for reliability.

### 2.1 Shooting.cs

Raycast from camera on mouse click or VR trigger. Destroys "Target" tagged objects, updates score. See [scripts reference](references/game-scripts.md) for full source.

### 2.2 GameManager.cs

Singleton score tracker with TextMeshProUGUI display. See [scripts reference](references/game-scripts.md).

### 2.3 TargetSpawner.cs

`InvokeRepeating` spawns prefab cubes at random positions every 2 seconds. See [scripts reference](references/game-scripts.md).

### 2.4 VRMovement.cs

Left thumbstick locomotion via `InputDevices.GetDeviceAtXRNode(XRNode.LeftHand)`. See [scripts reference](references/game-scripts.md).

### 2.5 Verify compilation

```
Unity_ReadConsole(Types: ["Error"])
```

Fix any compile errors before proceeding. Common issues:
- Missing `using TMPro;` → tell user: Window → TextMeshPro → Import TMP Essential Resources. See `skills/hz-unity-tmp-resources/`.
- Missing XR namespace → XR packages not yet installed (handled in Phase 4).

## Phase 3: Wire the Scene (~5 min)

### 3.1 Create UI

Tell the user to create manually (MCP cannot easily create Canvas UI):
1. Right-click Hierarchy → UI → Image — set 10x10, red color (crosshair)
2. Right-click Hierarchy → UI → Text - TextMeshPro — set "Score: 0"

For Canvas configuration details, see `skills/hz-unity-meta-quest-ui/`.

### 3.2 Create GameManager and TargetSpawner

```
Unity_ManageGameObject(action: "create", name: "GameManager", components_to_add: ["GameManager"])
Unity_ManageGameObject(action: "create", name: "TargetSpawner", components_to_add: ["TargetSpawner"])
```

### 3.3 Create prefab

Tell the user: "Drag a Target cube from Hierarchy into Assets/Prefabs."

### 3.4 Wire references

```
Unity_ManageGameObject(action: "set_component_property", target: "TargetSpawner", component_name: "TargetSpawner", component_properties: {"TargetSpawner": {"targetPrefab": "Assets/Prefabs/PREFAB_NAME.prefab"}})
```

Wire GameManager's scoreText to the TMP object:
```
Unity_ManageGameObject(action: "set_component_property", target: "GameManager", component_name: "GameManager", component_properties: {"GameManager": {"scoreText": {"find": "Text (TMP)", "component": "TMPro.TextMeshProUGUI"}}})
```

### 3.5 Attach Shooting to camera

```
Unity_ManageGameObject(action: "add_component", target: "Main Camera", component_name: "Shooting")
```

### 3.6 Desktop test checkpoint

Tell user: "Click the Game tab, hit Play. Click on cubes to shoot."

```
Unity_ReadConsole(Types: ["Error"])
```

No errors + cubes destroyed on click = milestone complete.

## Phase 4: Add VR (~5 min)

### 4.1 Install XR packages

Check what's installed:
```powershell
Select-String "com.unity.xr" "PROJECT_PATH\Packages\manifest.json"
```

For a comprehensive package check, see `skills/hz-unity-quest-setup/` Check 1. Needed: `com.unity.xr.openxr`, `com.unity.xr.interaction.toolkit`, `com.unity.xr.management`.

### 4.2 Enable OpenXR

Tell user: Edit → Project Settings → XR Plug-in Management → check OpenXR (PC tab).

Verify the loader registered — see `skills/hz-unity-quest-setup/` Check 3.

### 4.3 Configure profiles

Tell user: In OpenXR settings, add **Oculus Touch Controller Profile** to Enabled Interaction Profiles.

Verify — see `skills/hz-unity-quest-setup/` Check 4.

### 4.4 Replace camera with XR Origin

```
Unity_ManageGameObject(action: "delete", target: "Main Camera")
```

Tell user: Right-click Hierarchy → XR → XR Origin (VR).

Verify:
```
Unity_ManageScene(Action: "GetHierarchy")
```

### 4.5 Attach scripts

```
Unity_ManageGameObject(action: "add_component", target: "XR Origin (VR)/Camera Offset/Main Camera", component_name: "Shooting")
Unity_ManageGameObject(action: "add_component", target: "XR Origin (VR)", component_name: "VRMovement")
```

### 4.6 Fix Canvas for VR

World Space is required — screen-space UI is invisible in VR. See `skills/hz-unity-meta-quest-ui/` for detailed guidance.

```
Unity_ManageGameObject(action: "set_component_property", target: "Canvas", component_name: "Canvas", component_properties: {"Canvas": {"renderMode": 2}})
Unity_ManageGameObject(action: "modify", target: "Canvas", position: [0,1.5,2], scale: [0.002,0.002,0.002])
```

### 4.7 Reset XR Origin position

```
Unity_ManageGameObject(action: "modify", target: "XR Origin (VR)", position: [0,0,0])
```

## Phase 5: Play on Quest (~5 min)

### 5.1 Verify Link

Run all Quest Link checks. For detailed troubleshooting, see `skills/hz-quest-link/`.

```powershell
metavr device list
Get-Process | Where-Object { $_.ProcessName -match "OVR|Oculus" } | Select-Object ProcessName
Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime"
```

### 5.2 Enable Link on headset

Tell user: "Put on Quest → Quick Settings → Quest Link → Enable."

### 5.3 Play

Tell user: "In Unity, hit Play. Put on the headset."

Check for errors:
```
Unity_ReadConsole(Types: ["Error"], FilterText: "XR")
```

If no errors and user sees the scene: tutorial complete.

### 5.4 Controls summary

Tell the user:
- **Look around** — move your head
- **Move** — left thumbstick
- **Shoot** — right trigger while looking at a target
- **Score** — floating display in front of you

## Milestone Checklist

Verify each via MCP before marking done.

- [ ] Ground plane in scene
- [ ] Test targets with "Target" tag
- [ ] Scripts compile (no console errors)
- [ ] GameManager + TargetSpawner wired
- [ ] Desktop play test works (click to shoot)
- [ ] XR packages installed
- [ ] OpenXR enabled with Oculus Touch profile
- [ ] XR Origin in scene with scripts attached
- [ ] Canvas set to World Space
- [ ] Quest Link streaming
- [ ] VR play test works

## Gotchas

1. **Scripts must be in `Assets/Scripts/`.** Unity won't compile scripts outside Assets.
2. **TMP must be imported before GameManager compiles.** Import TMP Essential Resources first. See `skills/hz-unity-tmp-resources/`.
3. **"Add Component" won't find new scripts until Unity compiles.** Click the Unity window to trigger compilation, wait for the spinner.
4. **Dragging a script to Hierarchy creates an empty GameObject.** Use Add Component on an existing object instead.
5. **The old Main Camera must be deleted.** Two cameras cause rendering conflicts.

## References

### Skill References
- [Game Scripts](references/game-scripts.md) — Full source code for all four scripts
- `skills/hz-unity-quest-setup/` — Comprehensive Unity VR project verification
- `skills/hz-quest-link/` — Quest Link setup and troubleshooting
- `skills/hz-unity-meta-quest-ui/` — World-space Canvas configuration
- `skills/hz-unity-tmp-resources/` — TextMeshPro import
- `skills/hz-vr-debug/` — Device-side debugging with metavr
- `skills/hz-new-project-creation/` — Creating a project from scratch
- `docs/hzdb.md` — metavr CLI reference
