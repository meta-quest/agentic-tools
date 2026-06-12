# Passthrough (Mixed Reality) Reference

Passthrough provides real-time 3D visualization of the physical world inside Meta Quest headsets, enabling mixed reality experiences.

## How It Works

Passthrough is rendered by a dedicated system service into a separate layer. Apps create an `OVRPassthroughLayer` which the XR Compositor replaces with the actual passthrough rendition. Apps cannot access raw camera images directly.

## Setup Steps

### 1. Enable Passthrough in OVRManager

On the OVRCameraRig Inspector:
1. Under **OVRManager > Quest Features > General**, set **Passthrough Support** to "Supported" or "Required"
2. Under **Insight Passthrough & Guardian Boundary**, check **Enable Passthrough** (`isInsightPassthroughEnabled = true`)

**After changing these settings, update the AndroidManifest** — see [android-manifest.md](android-manifest.md).

### 2. Add OVRPassthroughLayer

1. Create a new empty GameObject in the scene
2. Add Component > Scripts > **OVR Passthrough Layer**

### 3. Configure Camera Background

1. Expand OVRCameraRig > TrackingSpace > CenterEyeAnchor
2. Set **Background Type** to "Solid Color"
3. Set **Background** color to black with alpha 0 (RGBA: 0, 0, 0, 0)

### 4. Remove Skybox

1. Window > Rendering > Lighting > Environment tab
2. Set **Skybox Material** to None

### 5. Set Tracking Origin (for MR)

Set **Tracking Origin Type** to "Floor Level" (recommended for MR). Do **not** use Stage — it does not respond to user recentering on Quest and is ill-defined for boundaryless apps.

## OVRPassthroughLayer Properties

| Property | Type | Description |
|---|---|---|
| `edgeColor` | Color | Color for edge rendering |
| `edgeRenderingEnabled` | bool | Enable/disable edge rendering |
| `hidden` | bool | Hide passthrough layer and pause system |
| `overlayType` | OverlayType | Layer overlay type |
| `overlayShape` | OverlayShape | Layer shape |
| `overridePerLayerColorScaleAndOffset` | bool | Apply colorScale/colorOffset to layer |

### Styling

OVRPassthroughLayer supports visual styling:
- Colorize the passthrough feed
- Highlight edges
- Contrast adjustment
- Posterization effects
- Color LUTs (via `OVRPassthroughColorLut`)

### Color LUT

To find current Color LUT API, locate the SDK root (see "Finding the SDK Source" in SKILL.md), then:
- grep `Scripts/Util/Passthrough/OVRPassthroughColorLut.cs` for the API
- Check `PassthroughCapabilities.MaxColorLutResolution` for max resolution

## Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **OVRPassthroughLayer**: `Scripts/OVRPassthroughLayer.cs` — grep for `public` members
- **Init state methods**: grep `Scripts/OVRManager.cs` for `InsightPassthrough` (init checks, pending state, failure)
- **Events**: grep `Scripts/OVRPassthroughLayer.cs` for `event` and `UnityEvent` (layer resumed callbacks)

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-passthrough
- https://developers.meta.com/horizon/documentation/unity/unity-passthrough-tutorial
