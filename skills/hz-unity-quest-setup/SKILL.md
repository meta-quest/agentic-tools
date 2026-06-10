---
name: hz-unity-quest-setup
description: Verifies and configures Unity projects for Meta Quest and Horizon OS VR development — checks required packages, OpenXR settings, XR loader registration, input system, and project settings.
allowed-tools:
  - Bash(metavr:*)
  - Bash(npx -y metavr:*)
  - Bash(adb:*)
  - PowerShell
  - mcp__unity-mcp__Unity_ManageScene
  - mcp__unity-mcp__Unity_ManageGameObject
  - mcp__unity-mcp__Unity_ReadConsole
  - mcp__unity-mcp__Unity_GetConsoleLogs
  - mcp__unity-mcp__Unity_PackageManager_GetData
  - mcp__unity-mcp__Unity_GetProjectData
---

# Unity Quest Setup Skill

Verify and configure an existing Unity project for Meta Quest VR development. This skill checks that all required packages, settings, and configurations are in place before building or testing.

For **creating** a new project from scratch, see `skills/hz-new-project-creation/`. This skill is for **verifying and fixing** an existing project's Quest-readiness.

## When to Use This Skill

Use this skill when you need to:

- Verify a Unity project has all required VR packages installed
- Check that OpenXR is configured correctly for Quest
- Diagnose why a project won't run in VR or won't connect to a Quest headset
- Fix common Unity VR configuration issues (wrong input system, missing XR loader, wrong OpenXR runtime)
- Prepare an existing non-VR Unity project for Quest VR

## Prerequisites

1. **Unity 6 or Unity 2022.3 LTS+** with a project open
2. **Unity MCP server** connected (if available) — enables direct project inspection via MCP tools
3. **metavr CLI** available via `npx -y metavr`

## Verification Checklist

Run through these checks in order. If any check fails, apply the fix before continuing.

### Check 1: Required Packages

**Via file system:**
```powershell
$manifest = Get-Content "PROJECT_PATH\Packages\manifest.json" -Raw
@("com.unity.xr.management", "com.unity.xr.openxr", "com.unity.xr.interaction.toolkit") | ForEach-Object {
    $found = $manifest -match $_
    Write-Output "$_ : $(if ($found) {'INSTALLED'} else {'MISSING'})"
}
```

**Via Unity MCP:**
```
Unity_PackageManager_GetData(action: "list")
```

**Required packages:**
| Package | ID | Purpose |
|---------|-----|---------|
| XR Plugin Management | `com.unity.xr.management` | Manages XR plugin loading |
| OpenXR Plugin | `com.unity.xr.openxr` | Cross-platform VR standard |
| XR Interaction Toolkit | `com.unity.xr.interaction.toolkit` | VR interaction system |

**Optional but recommended:**
| Package | ID | Purpose |
|---------|-----|---------|
| Input System | `com.unity.inputsystem` | Modern input handling |
| TextMeshPro | `com.unity.textmeshpro` | UI text rendering |

If packages are missing, add them to `Packages/manifest.json` or tell the user: "Window → Package Manager → Unity Registry → search and install."

### Check 2: Input System Mode

```powershell
Select-String "activeInputHandler" "PROJECT_PATH\ProjectSettings\ProjectSettings.asset"
```

| Value | Meaning | Legacy Input.* works? |
|-------|---------|----------------------|
| `0` | Old Input Manager only | Yes |
| `1` | New Input System only | **No — breaks legacy code** |
| `2` | Both | Yes |

Unity 6 URP template defaults to `1`. If the project uses `Input.GetAxis()`, `Input.GetMouseButtonDown()`, or similar legacy calls, change to `2`. This requires a Unity restart.

### Check 3: OpenXR Enabled

```powershell
Select-String "m_Loaders|m_AutomaticLoading|m_AutomaticRunning" "PROJECT_PATH\Assets\XR\XRGeneralSettingsPerBuildTarget.asset"
```

**Healthy values:**
- `m_AutomaticLoading: 1`
- `m_AutomaticRunning: 1`
- `m_Loaders:` contains at least one entry (not empty `[]`)

**If `m_Loaders: []` is empty:** The OpenXR loader is not registered. Tell the user to go to Edit → Project Settings → XR Plug-in Management → uncheck OpenXR → wait → re-check OpenXR. This forces Unity to re-register the loader.

### Check 4: OpenXR Interaction Profiles

```powershell
Select-String "OculusTouchControllerProfile|m_enabled: 1" "PROJECT_PATH\Assets\XR\Settings\OpenXR Package Settings.asset"
```

The **Oculus Touch Controller Profile** must be enabled (`m_enabled: 1`) for Quest controllers to work.

If missing: Edit → Project Settings → XR Plug-in Management → OpenXR → Enabled Interaction Profiles → click **+** → add **Oculus Touch Controller Profile**.

### Check 5: XR Origin in Scene

**Via Unity MCP:**
```
Unity_ManageScene(Action: "GetHierarchy")
```

Look for `XR Origin (VR)` or `OVRCameraRig` in the hierarchy. One of these must exist for VR to work. A bare `Main Camera` without a tracked pose driver will not track the headset.

**If missing:** The user needs to add an XR rig. For OpenXR projects: Right-click Hierarchy → XR → XR Origin (VR).

### Check 6: OpenXR Runtime (PC-side)

```powershell
Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime" | Select-Object -ExpandProperty ActiveRuntime
```

Must point to `oculus_openxr_64.json` for Quest Link. See `skills/hz-quest-link/` for detailed runtime troubleshooting.

### Check 7: Console Errors

**Via Unity MCP:**
```
Unity_ReadConsole(Types: ["Error"])
```

Fix any compile errors before attempting to run. Common issues:
- Missing `using TMPro;` — TextMeshPro not imported
- Missing XR namespaces — XR packages not installed
- `Input.GetAxis` errors — input system mode is wrong (Check 2)

## Quick Fix Reference

| Problem | Symptom | Fix |
|---------|---------|-----|
| Legacy input fails | WASD/mouse ignored in Play | `activeInputHandler` → `2` in ProjectSettings.asset, restart Unity |
| XR doesn't initialize | No VR output on Play | Uncheck/recheck OpenXR in XR Plug-in Management |
| Wrong OpenXR runtime | Scene on screen, not headset | Set Quest Link as active runtime, restart Unity |
| No controller input | Can't interact in VR | Add Oculus Touch Controller Profile in OpenXR settings |
| Canvas invisible in VR | UI not visible | Change Canvas Render Mode to World Space |
| TMP not imported | Text rendering errors | Window → TextMeshPro → Import TMP Essential Resources |

## Gotchas

1. **Unity 6 URP defaults to New Input System only.** This silently breaks all `Input.*` calls. Always check `activeInputHandler` on new projects.
2. **Toggling OpenXR off/on is the fix for empty loaders.** This is a known Unity issue where the XR loader doesn't get written to the settings asset on first enable.
3. **OpenXR runtime changes require editor restart.** The runtime path is cached at Unity startup.
4. **World Space Canvas scale.** In VR, a Canvas at scale (1,1,1) is enormous. Use (0.002, 0.002, 0.002) for readable text at arm's length.
5. **The "Oculus" checkbox in XR Plug-in Management is legacy.** Use the "OpenXR" checkbox for new projects.

## References

- `skills/hz-new-project-creation/` — Creating a new Quest project from scratch
- `skills/hz-new-project-creation/references/unity-project.md` — Detailed Unity project setup via metavr
- `skills/hz-quest-link/` — Quest Link setup and troubleshooting
- `skills/hz-unity-meta-quest-ui/` — World-space Canvas and UI configuration for Quest
- `skills/hz-unity-tmp-resources/` — TextMeshPro import and verification
- `skills/hz-unity-code-review/` — Code review for Quest Unity projects
- `docs/hzdb.md` — Full metavr CLI reference
