# Unity Project Setup for Meta Quest — Manual Reference

> **You probably don't need this file.** The primary [unity-project.md](unity-project.md)
> reference uses `metavr_unity_setup` to do everything below in one call. This
> manual reference exists for two situations:
>
> 1. You can't use the MCP tool (e.g., shared/non-MCP environment, debugging a
>    setup failure, restricted network access to packages.unity.com).
> 2. You want to understand exactly what the automated setup does to your
>    project, step by step.
>
> If neither applies, go back to [unity-project.md](unity-project.md) — calling
> `metavr_unity_setup(action="inject", project_path="<absolute>")` does all of
> this in <30s and a single Unity reload.

The following steps are equivalent to what `metavr_unity_setup` does
automatically. Use them only if the MCP tool isn't available or if you need
to debug a specific configuration step.

## Step 1: Create the Unity Project

1. Open Unity Hub and click **New Project**.
2. Select the **3D (URP)** template. The Universal Render Pipeline is recommended for Quest because it provides the best balance of visual quality and performance on mobile hardware.
3. Name your project and choose a location.
4. Click **Create project**.

If you prefer the command line:

```bash
# Create a URP project via Unity CLI (adjust Unity path as needed)
/Applications/Unity/Hub/Editor/2022.3.*/Unity.app/Contents/MacOS/Unity \
  -createProject ./MyQuestApp \
  -quit -batchmode
```

## Step 2: Install the Meta XR SDK

1. Open **Window > Package Manager**.
2. Click the **+** button and select **Add package by name**.
3. Enter `com.meta.xr.sdk.all` and click **Add**.

This meta-package includes all Meta XR packages: OVR Plugin, Interaction SDK, Audio SDK, Platform SDK, and more.

Alternatively, add only the packages you need:

| Package | Name | Purpose |
|---------|------|---------|
| Meta XR Core SDK | `com.meta.xr.sdk.core` | Core VR runtime, OVRManager, OVRCameraRig |
| Meta XR Interaction SDK | `com.meta.xr.sdk.interaction` | Hand tracking, controller interaction |
| Meta XR Platform SDK | `com.meta.xr.sdk.platform` | Entitlements, achievements, multiplayer |
| Meta XR Audio SDK | `com.meta.xr.sdk.audio` | Spatial audio, HRTF |

## Step 3: Configure XR Plugin Management

1. Open **Edit > Project Settings > XR Plug-in Management**.
2. Select the **Android** tab.
3. Enable **Oculus** (or **OpenXR** if you prefer the open standard).
4. If using OpenXR, add the **Meta Quest Feature Group** under OpenXR settings.

### Oculus Loader vs OpenXR

- **Oculus loader**: Best compatibility with Meta XR SDK features, recommended for most projects.
- **OpenXR loader**: Standards-based, useful for cross-platform targeting. Requires Meta Quest OpenXR feature group.

## Step 4: Configure Project Settings

Open **Edit > Project Settings** and apply these recommended settings:

### Player Settings (Android Tab)

```
Company Name:         Your company name
Product Name:         Your app name
Package Name:         com.yourcompany.yourapp

Minimum API Level:    Android 10.0 (API level 29)
Target API Level:     Automatic (highest installed)
Scripting Backend:    IL2CPP
Target Architectures: ARM64 (uncheck ARMv7)
```

### Color Space

```
Edit > Project Settings > Player > Other Settings
Color Space: Linear
```

Linear color space is required for correct lighting and post-processing on Quest.

### Graphics API

```
Edit > Project Settings > Player > Other Settings
Auto Graphics API: OFF
Graphics APIs:
  1. Vulkan
  2. OpenGLES3
```

Vulkan is the primary rendering API for Quest and delivers better performance. Keep OpenGLES3 as a fallback.

### Quality Settings

```
Edit > Project Settings > Quality
Anti Aliasing:    4x Multi Sampling (MSAA)
VSync Count:      Don't Sync (VR runtime manages vsync)
Texture Quality:  Full Res
```

### URP Asset Settings

Select your URP Renderer Asset and configure:

```
HDR:                    OFF (saves GPU bandwidth)
MSAA:                   4x
Render Scale:            1.0
Depth Texture:          ON (if needed for effects)
Opaque Texture:         OFF (unless needed)
SH Mode:               Per Vertex
Additional Lights:      Per Vertex (or disabled for performance)
```

## Step 5: Configure Meta XR Settings

### OVR Manager

1. In your scene, find or create the **OVRCameraRig** prefab (from Meta XR Core SDK).
2. Select the OVRCameraRig and configure the **OVR Manager** component:

```
Target Devices:         Quest 2, Quest 3, Quest Pro (select as needed)
Tracking Origin Type:   Floor Level
Hand Tracking Support:  Controllers and Hands (if using hand tracking)
Passthrough Support:    Supported or Required (if using MR)
```

### Enable Single-Pass Rendering

```
OVR Manager > Quest Features
Multiview:              Multi View (single-pass multiview)
```

Single-pass multiview renders both eyes in a single draw call, roughly doubling rendering performance.

### Fixed Foveated Rendering (FFR)

```
OVR Manager > Quest Features
Fixed Foveated Rendering:  ON
FFR Level:                 High (or Dynamic for automatic adjustment)
```

FFR reduces rendering resolution at the edges of the view where the user is unlikely to look, saving significant GPU time.

## After manual setup, see [unity-project.md](unity-project.md) for build, deploy, and testing steps (Step 6 onward).
