---
name: hz-unity-meta-core-sdk
license: Apache-2.0
description: Meta XR Core SDK (com.meta.xr.sdk.core) for Unity XR development. Use when setting up VR/MR projects, configuring OVRManager, adding OVRCameraRig, enabling passthrough, hand tracking, spatial anchors, boundaryless mode, controller input, Scene API, or any Meta Quest XR feature. Covers OVRProjectSetup, AndroidManifest generation, and project configuration for Meta Quest headsets.
---

# Meta XR Core SDK (com.meta.xr.sdk.core)

The Meta XR Core SDK is the foundational Unity package for developing VR and MR applications targeting Meta Quest headsets. It provides the core XR rig, device management, input handling, and access to platform features like passthrough, hand tracking, spatial anchors, and scene understanding.

Package: `com.meta.xr.sdk.core`

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

- **Target Devices** - Which Quest headsets to target
- **Performance & Quality** - MSAA, adaptive resolution, dynamic resolution
- **Tracking** - Tracking origin type (Eye Level, Floor Level, Stage, Stationary)
- **Display** - Color gamut settings
- **Quest Features** - Hand tracking, passthrough, keyboard, focus awareness, security, experimental features
- **Mixed Reality Capture** - Real-world compositing

### Analyzing OVRManager for Features

To understand what features are available and how they're configured, **analyze the OVRManager component** on the OVRCameraRig in the scene. The OVRManager Inspector exposes all configurable XR features grouped into sections.

For the full settings reference (tracking origin, passthrough, boundary, hand tracking on OVRProjectConfig, etc.), see [references/ovr-manager.md](references/ovr-manager.md).

## OVRProjectSetup (UPST): Listing and Fixing Project Issues

The **Unity Project Setup Tool (UPST)** (`OVRProjectSetup`) is a Unity Editor extension that validates project configuration for Meta Quest. It maintains a registry of **Configuration Tasks** — each task checks a specific setting and reports whether it is satisfied or outstanding.

**Primary use: list all outstanding issues for the current platform, then fix them.**

- **List issues** — Query all tasks, filter by platform/validity, and report those where `IsDone` is false. Each issue has a level (Required, Recommended, Optional), a group (Compatibility, Rendering, Features, etc.), and a fix type (Auto-fix or Manual).
- **Fix issues** — Use `FixAllAsync(BuildTargetGroup)` to auto-fix all fixable issues, or invoke individual `FixAction` delegates directly.

**UPST can be driven programmatically via Unity MCP** using reflection (see [references/project-setup-tool.md](references/project-setup-tool.md) for the full API, task registry access, property reading, and fix invocation patterns).

Access via UI: **Meta > Tools > Project Setup Tool**.

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

## Calling SDK Methods via Unity MCP

SDK classes (e.g. `OVRManifestPreprocessor`, `OVRProjectSetup`) live in assemblies that are **not directly referenceable** from Unity MCP compiled scripts. You must use runtime reflection to call them. The Unity MCP compilation environment also has specific quirks that will cause silent crashes if not followed.

### Rules

1. **Never add `using System.Reflection;`** — it causes `UNEXPECTED_ERROR` crashes in the MCP framework. Fully qualify reflection types instead (e.g. `System.Reflection.TargetInvocationException`).
2. **Never use `BindingFlags` overloads** of `GetMethod` / `GetProperty` — they also trigger MCP crashes. Use the parameterless `GetMethod("MethodName")` overload (finds public methods by default).
3. **Always pass `silentMode: true`** (or equivalent) for any method that may call `EditorUtility.DisplayDialog` — dialogs block indefinitely in MCP context.
4. **Always catch `System.Reflection.TargetInvocationException`** and log `InnerException` — reflection wraps the real error.

### Template

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // 1. Find the type by name across all loaded assemblies
        System.Type t = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try { t = asm.GetType("CLASS_NAME_HERE"); } catch { }
            if (t != null) break;
        }
        if (t == null) { result.LogError("Type CLASS_NAME_HERE not found."); return; }

        // 2. Get the method (parameterless overload only — no BindingFlags)
        var m = t.GetMethod("METHOD_NAME_HERE");
        if (m == null) { result.LogError("Method METHOD_NAME_HERE not found."); return; }

        // 3. Invoke with error handling
        try
        {
            m.Invoke(null, new object[] { /* args */ });
            result.Log("Done.");
        }
        catch (System.Reflection.TargetInvocationException tie)
        {
            result.LogError("Error: " + tie.InnerException);
        }
    }
}
```

Replace `CLASS_NAME_HERE`, `METHOD_NAME_HERE`, and the args array as needed. For instance methods, pass the target object instead of `null`.

## Using metavr Tools for Latest Docs

If the `metavr` MCP server is available, use the `mcp__metavr__meta_docs_search` and `mcp__metavr__meta_docs_get_page` tools to verify current API details, as Meta SDK documentation updates frequently. Use `mcp__metavr__search_api_reference` with `engine='unity'` to look up exact method signatures for OVRManager, OVRCameraRig, OVRInput, and other classes.
