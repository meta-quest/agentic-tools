# Boundaryless Mode Reference

Boundaryless and Boundary API allow disabling the Guardian boundary for mixed reality experiences where the physical world is visible.

## Overview

The Guardian boundary is a safety system for fully immersive VR apps. For MR experiences where the user can see their physical surroundings via passthrough, the boundary can be unnecessary and disruptive.

Two approaches:
1. **Boundaryless**: Disable boundary for the entire app
2. **Contextual Boundaryless (Boundary API)**: Disable/enable boundary at runtime

## Boundaryless Apps

Disable the boundary for the entire experience. The app must NOT have any fully immersive (non-passthrough) moments.

### Setup

Enable boundaryless via OVRManager/OVRProjectConfig settings. **After changing these settings, update the AndroidManifest** — see [android-manifest.md](android-manifest.md).

### Limitations
- Only works when running standalone APK on headset
- Does NOT work over PC/Link (AndroidManifest has no effect over Link)
- Only supports 6DoF apps (3DoF apps will not be boundaryless)

## Contextual Boundaryless (Boundary API)

Disable/enable boundary at runtime via `OVRManager`.

### Prerequisites
- **Passthrough Support** must be "Supported" or "Required" in OVRManager Quest Features
- An `OVRPassthroughLayer` component must be active when suppressing boundary

### Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **OVRManager boundary properties**: grep `Scripts/OVRManager.cs` for `shouldBoundaryVisibilityBeSuppressed`, `isBoundaryVisibilitySuppressed`
- **Boundary events**: grep `Scripts/OVRManager.cs` for `BoundaryVisibilityChanged`
- **OVRBoundary class**: grep `Scripts/OVRBoundary.cs` for `public` members (`GetConfigured`, `GetDimensions`)

### Important Behavior
- If Passthrough is not enabled (OVRManager property disabled or no active OVRPassthroughLayer), OVRManager will still try to suppress boundary every frame
- App should update `shouldBoundaryVisibilityBeSuppressed` based on passthrough state
- Subscribe to `BoundaryVisibilityChanged` to know when system changes visibility

## World-Locked Content

**Do NOT use Stage tracking space for boundaryless apps.** Stage tracking is ill-defined when no boundary exists or when user crosses multiple boundaries.

### Solutions:
1. **MRUK World Locking** (preferred): If `com.meta.xr.mrutilitykit` is installed, use its world-locking feature — it handles anchor management internally.
2. **Spatial Anchor Alignment** (fallback): If MRUK is not available, create a spatial anchor and align OVRCameraRig to its inverse transform every frame. See [spatial-anchors.md](spatial-anchors.md) for the code example.

## Safety Considerations

Developers are responsible for:
- Following Boundaryless and Contextual-boundaryless Safety Best Practices
- Following Mixed Reality Health and Safety Guidelines
- Ensuring the app does not create safety risks
- Using Scene and Depth APIs to improve safety in boundaryless MR experiences

## Mutual Exclusivity

Boundaryless (manifest-based) and Contextual Boundaryless (Boundary API) are mutually exclusive and OS version gated.

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-boundaryless
- https://developers.meta.com/horizon/documentation/unity/unity-ovrboundary
