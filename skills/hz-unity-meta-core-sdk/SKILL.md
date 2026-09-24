---
name: hz-unity-meta-core-sdk
license: Apache-2.0
description: Meta XR Core SDK (com.meta.xr.sdk.core) for Unity XR development. Use when setting up VR/MR projects, configuring OVRManager, adding OVRCameraRig, enabling passthrough, hand tracking, spatial anchors, boundaryless mode, controller input, Scene API, or any Meta VR feature. Covers OVRProjectSetup, AndroidManifest generation, and project configuration for Meta VR devices, driven against a live Editor with unity-cli.
---

# Meta XR Core SDK (com.meta.xr.sdk.core)

The Meta XR Core SDK is the foundational Unity package for developing VR and MR applications targeting Meta VR devices. It provides the core XR rig, device management, input handling, and access to platform features like passthrough, hand tracking, spatial anchors, and scene understanding.

Package: `com.meta.xr.sdk.core`

## Running SDK code — drive a live Editor with `unity-cli`

Everything here is **Editor-only** C#. Run it against an open Editor instead of hand-editing scene, manifest, or settings files. Connecting, `--project-path`, and Safe Mode recovery are in the **`unity-cli`** skill.

```bash
unity status --format json          # look for state "ready"
unity command run_script --project-path <proj> --file AgentScripts/Setup.cs --entry Setup.Run --format json
```

Prefer `run_script` over `eval` — put the `.cs` outside `Assets/` (path resolves against the project root) so writing it triggers no import or domain reload, and shell quoting can't mangle C# literals.

SDK-specific gotchas:

- **No type-discovery reflection needed.** `run_script` references every loaded assembly, so `OVRProjectSetup`, `OVRManifestPreprocessor` etc. compile directly; `using System.Reflection;` and `BindingFlags` work for the internal bits (`_principalRegistry`, `GetTasks`).
- **Pass `silentMode: true`** to anything that may open an `EditorUtility.DisplayDialog` — a modal blocks the request until it times out.
- **Async work lands after the script returns** (`FixAllAsync`, `Client.Add`) — verify in a separate follow-up `run_script`; never `Task.Wait()` on the main thread.
- Scene edits: `MarkSceneDirty` + `SaveScene` (+ `AssetDatabase.SaveAssets()`), then read the value back.
- Driving Unity **MCP** instead of the CLI? Its constraints break the above: [references/unity-mcp-fallback.md](references/unity-mcp-fallback.md).

## Finding the SDK Source

The package `com.meta.xr.sdk.core` may be located in different places depending on the project setup:
- `Library/PackageCache/com.meta.xr.sdk.core@<hash>/` (cached from registry)
- `Packages/com.meta.xr.sdk.core/` (local package reference)
- A custom on-disk path (embedded or local folder)

**Before searching for SDK source, first locate the package root** by searching for a known file by filename pattern:
```
**/com.meta.xr.sdk.core*/Scripts/OVRManager.cs
```
Then use the resolved parent path for all subsequent search operations.

## CRITICAL: After Any OVRProjectConfig or OVRManager Feature Change

**Any time you modify `OVRProjectConfig` or OVRManager feature settings (hand tracking, passthrough, boundaryless, target devices, etc.), you MUST call `GenerateOrUpdateAndroidManifest()` and verify the result.** See [references/android-manifest.md](references/android-manifest.md) for the full workflow.

## Quick Reference

| Component | Purpose |
|---|---|
| **OVRCameraRig** | Primary XR rig prefab replacing Unity's Main Camera |
| **OVRManager** | Singleton managing device state, features, and configuration |
| **OVRInput** | Unified API for controller input and tracking |
| **OVROverlay** | Compositor layers for sharper text, UI, and video |
| **OVRPassthroughLayer** | Enables passthrough visualization |
| **OVRSpatialAnchor** | World-locked spatial anchors |
| **OVRBoundary** | Guardian boundary system access |
| **OVRProjectSetup** | Project Setup Tool for configuration tasks |

## OVRCameraRig: The Primary XR Rig

The **OVRCameraRig** prefab is the primary GameObject to add to create a VR/MR scene. It replaces Unity's conventional Main Camera and provides:

- Stereo rendering for left/right eyes
- Head and positional tracking via the TrackingSpace hierarchy
- Anchors for eyes (CenterEyeAnchor, LeftEyeAnchor, RightEyeAnchor)
- Anchors for hands/controllers (LeftHandAnchor, RightHandAnchor)

### Hierarchy Structure

The OVRCameraRig hierarchy includes eye anchors, hand/controller anchors, and multimodal anchors. To see the current structure, inspect the OVRCameraRig prefab or instantiate it in a scene and examine the TrackingSpace children.

### Adding OVRCameraRig to a Scene

When working with a scene that needs XR support:

1. **Check if OVRCameraRig exists** in the current scene
2. If NOT found, the agent should:
   - **Option A**: Ask the user which scene contains the OVRCameraRig (it is part of the OVRCameraRig prefab instance)
   - **Option B**: Open a scene where the agent knows the OVRCameraRig exists
   - **Option C**: Ask the user if they want to add the OVRCameraRig prefab to the current scene
3. When adding OVRCameraRig, follow the setup steps in [references/ovr-camera-rig.md](references/ovr-camera-rig.md).

## OVRManager: Feature Configuration

**OVRManager** (`OVRManager.cs`) is the main interface to VR hardware. It is a **singleton** attached to the OVRCameraRig prefab that exposes the Meta XR SDK to Unity. It controls:

- **Target Devices** - Which Meta VR devices to target
- **Performance & Quality** - MSAA, adaptive resolution, dynamic resolution
- **Tracking** - Tracking origin type (Eye Level, Floor Level, Stage, Stationary)
- **Display** - Color gamut settings
- **Quest Features** - Hand tracking, passthrough, keyboard, focus awareness, security, experimental features
- **Mixed Reality Capture** - Real-world compositing

### Analyzing OVRManager for Features

To understand what features are available and how they're configured, **analyze the OVRManager component** on the OVRCameraRig in the scene. The OVRManager Inspector exposes all configurable XR features grouped into sections.

For the full settings reference (tracking origin, passthrough, boundary, hand tracking on OVRProjectConfig, etc.), see [references/ovr-manager.md](references/ovr-manager.md).

## CRITICAL: Changing Serialized Fields — Use SerializedObject, Not Reflection

To change a serialized field on any component, edit it via `SerializedObject` + `FindProperty(name)` + `ApplyModifiedProperties()` — never reflection (`FieldInfo`/`PropertyInfo.SetValue`). Reflection mutates the in-memory object but bypasses serialization: for a component on a **prefab instance** (e.g. OVRManager on OVRCameraRig), the change isn't recorded as a prefab override and is **silently dropped on save** (`SaveScene` returns `true`, but the field reverts to the prefab default).

- Get the component as `UnityEngine.Object` (`FindObjectOfType` / `GetComponent`); `SerializedObject`/`FindProperty` work by name, so no asmref is needed.
- The serialized name is the `[SerializeField]` backing field (often `_camelCase`), **not** the public property. If `FindProperty` returns null, discover names by iterating `so.GetIterator()` (`Next(true)` → log `propertyPath`).
- After `ApplyModifiedProperties()`, call `MarkSceneDirty` → `SaveScene` → `SaveAssets`, then read the field back to confirm it persisted.

```csharp
// component obtained as UnityEngine.Object (e.g. FindObjectOfType(type) as Object)
var so = new SerializedObject(component);
so.FindProperty("_SERIALIZED_FIELD_NAME").boolValue = true;   // typed accessor: boolValue/floatValue/intValue/...
so.ApplyModifiedProperties();                                 // records the prefab-instance override + marks dirty

var scene = EditorSceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();

// verify before declaring success
var check = new SerializedObject(component).FindProperty("_SERIALIZED_FIELD_NAME").boolValue;
```

## OVRProjectSetup (UPST): Listing and Fixing Project Issues

The **Unity Project Setup Tool (UPST)** (`OVRProjectSetup`) is a Unity Editor extension that validates project configuration for Meta VR. It maintains a registry of **Configuration Tasks** — each task checks a specific setting and reports whether it is satisfied or outstanding.

**Primary use: list all outstanding issues for the current platform, then fix them.**

- **List issues** — Query all tasks, filter by platform/validity, and report those where `IsDone` is false. Each issue has a level (Required, Recommended, Optional), a group (Compatibility, Rendering, Features, etc.), and a fix type (Auto-fix or Manual).
- **Fix issues** — Use `FixAllAsync(BuildTargetGroup)` to auto-fix all fixable issues, or invoke individual `FixAction` delegates directly.

**Drive UPST programmatically against a live Editor with `unity-cli`** (see [references/project-setup-tool.md](references/project-setup-tool.md) for the full API, task registry access, property reading, and fix invocation patterns).

Access via UI: **Meta > Tools > Project Setup Tool**.

After UPST is green, prove it on hardware: build an APK, `metavr app install <apk>`, and confirm the expected features initialized in a `metavr log --tag Unity` snapshot.

### CRITICAL: AndroidManifest Update

**NEVER directly edit AndroidManifest.xml for features managed by OVRProjectConfig.** See [references/android-manifest.md](references/android-manifest.md) for the full manifest update workflow and rules.

## Core Features

### Passthrough (Mixed Reality)

Passthrough provides real-time visualization of the physical world inside the headset, enabling mixed reality experiences. See [references/passthrough.md](references/passthrough.md) for setup steps and configuration.

### Hand Tracking

Hand tracking enables natural hand interaction without controllers. See [references/hand-tracking.md](references/hand-tracking.md) for setup and configuration.

### Spatial Anchors

Spatial anchors anchor virtual content to real-world locations that persist across sessions.

For spatial anchors details, see [references/spatial-anchors.md](references/spatial-anchors.md).

### Scene API

Scene provides access to the user's physical environment model (walls, floor, furniture) for scene-aware MR experiences.

For Scene API details, see [references/scene-api.md](references/scene-api.md).

### Boundaryless Mode

Boundaryless mode disables the Guardian boundary for MR experiences where the physical world is visible.

For boundaryless setup, see [references/boundaryless.md](references/boundaryless.md).

### Compositor Layers (OVROverlay)

OVROverlay renders textures directly via the VR compositor, bypassing eye buffer resampling for sharper text, UI, and video. Supports up to 15 overlay layers per scene with quad, cylinder, cubemap, equirect, and fisheye shapes.

For compositor layer details, see [references/ovr-overlay.md](references/ovr-overlay.md).

### Controller Input (OVRInput)

OVRInput provides unified access to controller buttons, triggers, thumbsticks, and tracking.

For controller input details, see [references/controller-input.md](references/controller-input.md).

## Project Setup Workflow

For the full project setup workflow (install SDK, set build platform, configure XR provider, add OVRCameraRig, configure OVRManager, generate AndroidManifest), see [references/project-setup-workflow.md](references/project-setup-workflow.md).

## Documentation Links

All documentation references point to the official Meta developer docs at developers.meta.com:

- [Meta XR Core SDK Overview](https://developers.meta.com/horizon/documentation/unity/book-unity-dg)
- [OVRCameraRig Configuration](https://developers.meta.com/horizon/documentation/unity/unity-ovrcamerarig)
- [Project Setup Tool](https://developers.meta.com/horizon/documentation/unity/unity-upst-overview)
- [Android Manifest Generation](https://developers.meta.com/horizon/documentation/unity/unity-android-manifest)
- [Passthrough API](https://developers.meta.com/horizon/documentation/unity/unity-passthrough)
- [Hand Tracking Overview](https://developers.meta.com/horizon/documentation/unity/unity-handtracking-overview)
- [Spatial Anchors Overview](https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-overview)
- [Scene Overview](https://developers.meta.com/horizon/documentation/unity/unity-scene-overview)
- [Compositor Layers (OVROverlay)](https://developers.meta.com/horizon/documentation/unity/unity-ovroverlay)
- [Controller Input](https://developers.meta.com/horizon/documentation/unity/unity-ovrinput)
- [Project Configuration](https://developers.meta.com/horizon/documentation/unity/unity-project-configuration)

## Using metavr Tools for Latest Docs

If the `metavr` MCP server is available, use the `mcp__metavr__vr_docs_search` and `mcp__metavr__vr_docs_get_page` tools to verify current API details, as Meta SDK documentation updates frequently. Use `mcp__metavr__vr_api_search` with `engine='unity'` to look up exact method signatures for OVRManager, OVRCameraRig, OVRInput, and other classes.

## Related skills

- **`unity-cli`** — drive a live Editor, install editors, build/test
- **`hz-unity-meta-interaction-sdk`** — interaction layer (grab, poke, teleport, interaction rig)
- **`hz-xr-simulator-install-and-configure`** / **`hz-vr-debug`** — run and debug beyond Play mode
