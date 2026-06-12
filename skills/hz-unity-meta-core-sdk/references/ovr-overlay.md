# OVROverlay (Compositor Layers) Reference

OVROverlay enables compositor layers — textures rendered directly by the VR compositor instead of the eye buffer. This produces sharper text, UI, and video because the texture is sampled only once (source to screen) instead of twice (source to eye buffer, then eye buffer to screen).

## When to Use

- **Text and UI** — sharper rendering allows smaller fonts while remaining readable
- **Video playback** — critical for quality; always use compositor layers for video
- **Gaze cursors / crosshairs** — focal elements benefit from maximum clarity
- **Loading screens** — compositor layers render at compositor framerate even when the app drops frames

## Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **OVROverlay**: `Scripts/OVROverlay.cs` — grep for `public` members, overlay types, shapes
- **Underlay shaders**: `Resources/` directory — look for `Underlay Transparent Occluder.shader` and `Underlay Impostor.shader`

## Setup

1. Create an empty GameObject in the scene
2. Add the `OVROverlay` component to it
3. Configure the overlay type, shape, and assign a texture

OVROverlay requires OVRManager to be present in the scene.

## Overlay Types

| Type | Description |
|---|---|
| **Overlay** | Rendered in front of the eye buffer (default) |
| **Underlay** | Rendered behind the eye buffer; requires punching a hole in the eye buffer with an alpha mask. More bandwidth-intensive than overlays |
| **None** | Hides the layer |

## Overlay Shapes

| Shape | Use Case |
|---|---|
| **Quad** | Flat panel — text, UI, information displays |
| **Cylinder** | Curved UI — wraps around the camera |
| **Cubemap** | Skyboxes, reflections, surroundings |
| **Equirect** | 360/180 video playback |
| **Offcenter Cubemap** | Higher resolution in front of user (30-degree z offset) |
| **Fisheye** | Fisheye image display with configurable FOV |

## Best Practices

### World-Locked vs Head-Locked

- **World-locked (default)**: Maintains position relative to the world. Uses TimeWarp, much less prone to judder. Use for almost all overlays.
- **Head-locked**: Follows head motion exactly, bypasses TimeWarp. Only use for small UI elements like gaze cursors. To make head-locked: parent the overlay to `OVRCameraRig > CenterEyeAnchor`.

### Performance

- Maximum **15 OVROverlay layers** per scene (the compositor supports 16 total, but the eye buffer uses one). Layers beyond this limit are not rendered.
- Only **one cylinder** and **one cubemap** layer per scene.
- Each additional layer costs ~0.1ms on Quest 2 (CPU/GPU level 4).
- A fullscreen layer costs ~0.6ms on Quest 2.
- Underlays are more expensive than overlays due to alpha mask hole-punching.
- Setting a layer texture to 0-alpha still incurs the full rendering cost — destroy layers you don't need instead of hiding them.
- If you need more than 16 layers, combine planar elements into a single RenderTexture and use one OVROverlay.

### Layer Filtering

- **Supersampling** — reduces flicker for high-contrast edges on undersampled layers
- **Sharpening** — improves clarity when upsampling to display resolution
- **Auto filtering** — runtime applies supersampling/sharpening only when beneficial, with no overhead when not needed. Accounts for layer resizing, player movement, PPD, and GPU utilization.
- Expensive filter variants consume more GPU. Weigh fidelity against performance.

### Underlays

When using underlays:
1. Set alpha to 1 on opaque scene objects that should occlude the underlay
2. For transparent occluders (alpha < 1), use `Underlay Transparent Occluder.shader`
3. Use `Underlay Impostor.shader` to punch holes in the eye buffer — draw after opaque geometry, before alpha

## Common Patterns

### Video Playback
- Use **Is External Surface** to feed external Android video directly to the compositor
- Or use **Override Default Rects** to render a single stereo texture at higher resolution

### Loading Screen
- One cubemap layer for the background (or leave blank for black void)
- One quad overlay for loading text/indicator

### Gaze Cursor
- Add a quad OVROverlay as a child of `OVRCameraRig > CenterEyeAnchor` (head-locked)

## Key Properties

| Property | Description |
|---|---|
| **Composition Depth** | Controls ordering between overlays/underlays. Smaller values render in front. |
| **Dynamic Texture** | Check if texture updates each frame. Auto-checked for RenderTextures. |
| **Is Protected Content** | Enables HDCP (Rift) / L1 Widevine DRM (Quest) |
| **Is External Surface** | Layer receives textures from an external Android Surface |
| **Bicubic Filtering** | GPU hardware bicubic filtering tuned for Quest display. Falls back to bilinear on older devices. |
| **Override Color Scale** | Per-layer color scale and offset, overriding global settings |

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-ovroverlay
- https://developers.meta.com/horizon/documentation/unity/os-compositor-layers
