# OVRManager Reference

`OVRManager.cs` is the main interface to VR hardware. It is a **singleton** component attached to the OVRCameraRig prefab that exposes the Meta XR SDK to Unity.

## Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **Source file**: `Scripts/OVRManager.cs`
- **Singleton**: grep for `public static OVRManager instance`
- **Events**: grep for `public static event` in OVRManager.cs
- **Properties**: grep for `public static` properties in OVRManager.cs
- **Enums**: grep for `enum` definitions (e.g., `TrackingOrigin`)
- **Hand tracking config**: stored in `OVRProjectConfig` (Editor/OVRProjectConfig.cs), not OVRManager — grep for `enum HandTrackingSupport`, `enum HandTrackingFrequency`

## Settings Sections

### Target Devices

Controls which Meta Quest headsets the app targets. Target device selection automatically adds appropriate `<meta-data/>` elements to AndroidManifest.

### Performance and Quality

| Setting | Description | Default |
|---|---|---|
| **Use Recommended MSAA Level** | Auto-select MSAA based on device (4x for Quest). Built-in pipeline only. For URP, manually set MSAA to 4x. | true |
| **Monoscopic** | Both eyes render same image from center pose. Not recommended. | false |
| **Enable Adaptive Resolution** | Scale resolution based on GPU utilization (85% target). Link PC-VR only. | false |
| **Min/Max Dynamic Resolution Scale** | Bounds for adaptive resolution (default 1.0) | 1.0 |
| **Head Pose Relative Offset Rotation** | Offset rotation of head poses | (0,0,0) |
| **Head Pose Relative Offset Translation** | Offset translation of head poses | (0,0,0) |

### Tracking

| Setting | Description |
|---|---|
| **Tracking Origin Type** | Eye Level, Floor Level, Stage, or Stationary (experimental) |
| **Use Position Tracking** | Head tracking affects camera position |
| **Use IPD In Position Tracking** | Eye distance affects camera positions |
| **Reset Tracker On Load** | Reset head pose on scene load (Link PC-VR only) |
| **Allow Recenter** | Allow Reset View from universal menu |
| **Late Controller Update** | Update controller pose immediately before render for lower latency |

**Tracking Origin Type recommendations:**
- **VR apps**: Use Eye Level or Floor Level (repositioned with user recentering)
- **MR apps**: Use Floor Level + Spatial Anchors for tracking space sync, or Stationary (experimental)
- **Stage**: Not recommended (does not respond to user recentering on Quest)

### Display

- **Color Gamut**: Set specific color space at runtime to overcome color variation

### Quest Features > General

| Setting | Description |
|---|---|
| **Focus Aware** | Allow system UI without context switching away from app |
| **Hand Tracking Support** | None, ControllersOnly, HandsOnly, ControllersAndHands |
| **Hand Tracking Frequency** | LOW, HIGH (better gesture detection, more GPU cost) |
| **Requires System Keyboard** | Enable system keyboard interaction |
| **System Splash Screen** | 2D texture for app splash screen |
| **Allow Optional 3DoF Head Tracking** | Support 3DoF + 6DoF, run without head tracking |
| **Passthrough Support** | None, Supported, Required |

### Quest Features > Build Settings

- **Skip Unneeded Shaders**: Strip unused shaders from compilation to reduce build time

### Quest Features > Security

| Setting | Description |
|---|---|
| **Custom Security XML Path** | Path to custom security XML file |
| **Disable Backups** | Adds `allowBackup="false"` to AndroidManifest |
| **Enable NSC Configuration** | Force HTTPS, prevent cleartext HTTP |

### Quest Features > Experimental

Enable experimental features (e.g., Stationary tracking origin):
1. Open scene with OVRCameraRig
2. Select OVRCameraRig in Hierarchy
3. In Inspector > OVRManager > Quest Features > Experimental
4. Check **Experimental Features Enabled**

### Insight Passthrough & Guardian Boundary

| Property | Type | Description |
|---|---|---|
| `isInsightPassthroughEnabled` | bool | Enable Insight Passthrough |
| `shouldBoundaryVisibilityBeSuppressed` | bool | Request boundary suppression (contextual boundaryless) |
| `isBoundaryVisibilitySuppressed` | bool (read-only) | Current system boundary state |

### Mixed Reality Capture

Enable `enableMixedReality` under Show Properties to combine real-world footage with VR.

## Key API Categories

To find current signatures for any of these, grep `OVRManager.cs` in the SDK package:

- **Events**: `public static event` — tracking acquired/lost, boundary visibility changed, spatial anchor callbacks
- **Static methods**: passthrough init checks, foveated rendering queries, camera lookup
- **Properties**: singleton instance, boundary/display/tracker refs, tracking origin, headset type, rendering settings, multimodal support check

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-ovrcamerarig
- API: https://developers.meta.com/horizon/reference/unity/latest/class_o_v_r_manager
