# Hand Tracking Reference

Hand tracking enables natural hand interaction on Meta Quest headsets without controllers.

## Setup

### 1. Enable in OVRManager

On OVRCameraRig > OVRManager > Quest Features > General:
- Set **Hand Tracking Support** to:
  - `ControllersAndHands` - Both input methods (recommended)
  - `HandsOnly` - Hands only, no controller support
- Set **Hand Tracking Frequency**:
  - `LOW` - Standard tracking
  - `HIGH` - 60Hz, better gesture detection, higher GPU cost (for fitness/rhythm apps)

**After changing these settings, update the AndroidManifest** — see [android-manifest.md](android-manifest.md).

### 2. Use Interaction SDK (Recommended)

The recommended way to integrate hand tracking is via the **Meta XR Interaction SDK**, which provides standardized interactions and gestures.

## Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **OVRHand**: `Scripts/Util/OVRHand.cs` — grep for `public` members, `enum HandType`, `enum ShowState`
- **OVRSkeleton**: `Scripts/Util/OVRSkeleton.cs` — bone hierarchy and skeleton data
- **OVRMesh**: `Scripts/Util/OVRMesh.cs` — hand mesh visualization
- **Hand tracking enums**: grep for `enum HandTrackingSupport` and `enum HandTrackingFrequency` in `Editor/OVRProjectConfig.cs`
- **Multimodal properties**: grep `Scripts/OVRManager.cs` for `SimultaneousHandsAndControllers`, `controllerDrivenHandPosesType`, `wideMotionModeHandPosesEnabled`

## Core SDK Hand Components

| Component | Description |
|---|---|
| `OVRHand` | Hand tracking state and data |
| `OVRSkeleton` | Hand skeleton with bone hierarchy |
| `OVRMesh` | Hand mesh visualization |
| `OVRHandPrefab` | Pre-configured hand visualization prefab |

## Features in Core SDK

| Feature | Description |
|---|---|
| **Hand Tracking** | Basic hand pose detection |
| **Fast Motion Mode (FMM)** | 60Hz tracking for fast movements (fitness/rhythm) |
| **Wide Motion Mode (WMM)** | Track hands outside headset FOV with plausible poses |
| **Multimodal** | Simultaneous hands + controllers |
| **Capsense** | Logical hand poses when using controllers |
| **OpenXR Hand Skeleton** | OpenXR standard hand skeleton support |

## Interaction SDK Features (requires com.meta.xr.sdk.interaction)

| Feature | Description |
|---|---|
| **Pose Detection** | Detect when hand matches specific pose shapes |
| **Pose Recording** | Capture poses for detection |
| **Gesture Detection** | Sequence recognition for complex gestures |
| **Microgestures** | Thumb tap/swipe on index finger |
| **Poke** | Direct touch interaction with surfaces |
| **Hand Grab** | Physics-less grabbing designed for hands |
| **Distance Grab** | Grab objects out of arm's reach |
| **Ray Grab** | Grab via ray casting |
| **Custom Grab Poses** | Control hand conforming to grabbed objects |
| **Throw** | Throw objects using hands |
| **Raycast** | Interact from a distance via ray |

## Permissions

Hand tracking permissions are automatically managed. See [android-manifest.md](android-manifest.md) for the manifest update workflow.

**Data Usage**: Hand tracking data (hand size, pose) is only permitted for enabling hand tracking within the app.

## Sample Scenes (Core SDK)

| Scene | Description |
|---|---|
| HandTest | Basic hand tracking setup |
| HandTest_Custom | Custom hand models |
| HandTest_Custom_OpenXR | Custom models + OpenXR |
| HandTrackingWideMotionMode | Wide Motion Mode |
| SimultaneousHandsAndControllers | Multimodal setup |
| ControllerDrivenHandPoses | Capsense hand poses |

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-handtracking-overview
- https://developers.meta.com/horizon/documentation/unity/fast-motion-mode
- https://developers.meta.com/horizon/documentation/unity/unity-wide-motion-mode
- https://developers.meta.com/horizon/documentation/unity/unity-multimodal
