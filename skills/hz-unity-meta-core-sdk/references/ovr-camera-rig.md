# OVRCameraRig Reference

The OVRCameraRig prefab is the core XR rig in the Meta XR Core SDK (`com.meta.xr.sdk.core`). It replaces Unity's Main Camera for VR/MR development.

## Prefab Location

The OVRCameraRig prefab is part of the `com.meta.xr.sdk.core` package. Search for "OVRCameraRig" in the Project tab with filter set to "All" or "In Packages".

## Hierarchy

The OVRCameraRig hierarchy includes eye anchors, hand/controller anchors, and multimodal anchors under a TrackingSpace parent. To see the current structure, inspect the OVRCameraRig prefab directly or instantiate it in a scene.

## Attached Scripts

### OVRCameraRig.cs

Controls stereo rendering and head tracking. Maintains anchor Transforms for eyes and hands.

**Key Settings:**
- **Use Per Eye Cameras**: Use separate cameras for left/right eyes
- **Use Fixed Update For Tracking**: Update anchors in FixedUpdate() for physics fidelity (may cause judder if fixed rate doesn't match render rate)
- **Disable Eye Anchor Cameras**: Disable eye anchor cameras, use main camera for VR rendering

**Key Properties:**
- `OVRCameraRig.Instance` - Static singleton reference
- `UpdatedAnchors` - Event fired when eye pose anchors are set (subscribe for post-tracking-update logic)

**Key Methods:**
- `EnsureGameObjectIntegrity()` - Validates hierarchy structure
- `ComputeTrackReferenceMatrix()` - Gets the tracking reference matrix

### OVRManager.cs

The main VR hardware interface singleton. See [ovr-manager.md](ovr-manager.md) for full details.

## Setting Up OVRCameraRig

1. Delete the existing **Main Camera** from the scene
2. Search for `OVRCameraRig` in the Project tab (filter: All or In Packages)
3. Drag the prefab into the scene Hierarchy
4. Configure OVRManager settings in the Inspector

## Extended Rig Variants

The Meta XR Interaction SDK includes extended rig prefabs:

- **OVRCameraRigInteraction**: Extends OVRCameraRig with controller and hand tracking for interactions
- **OVRCameraRigInteractionComprehensive**: Adds controller and hand tracking support to existing rigs

## Camera Configuration for MR/Passthrough

When enabling passthrough, the camera background and skybox must be configured. See [passthrough.md](passthrough.md) for the complete setup steps.

## Tracking Space

The TrackingSpace GameObject defines the reference frame for all tracking. Head-tracked pose values override the camera's Transform values, so the camera position always matches the user's real-world position and orientation.

For character-following setups, make the OVRCameraRig a child of the player object or create a tracking script that references the player.

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-ovrcamerarig
- API: https://developers.meta.com/horizon/reference/unity/latest/class_o_v_r_camera_rig
