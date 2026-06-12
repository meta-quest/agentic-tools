# Scene API Reference

Scene provides access to the user's physical environment model for building scene-aware mixed reality experiences.

## What is Scene?

Scene is a comprehensive representation of the physical world that can be indexed and queried. It provides geometric and semantic information about the user's space:

- Walls, floor, ceiling positions and dimensions
- Furniture locations and sizes
- Scene mesh (3D mesh of the environment)

Combined with Passthrough and Spatial Anchors, Scene enables mixed reality experiences.

## How It Works

### Scene Model

A scene graph for the physical world, managed and persisted by Meta Quest OS. Main use cases:
- **Physics**: Collide virtual objects with real walls/furniture
- **Static occlusion**: Virtual objects hidden behind real furniture
- **Navigation**: Virtual characters navigate real floors

### Space Setup

Users capture their environment via Settings > Environment Setup > Space Setup. The system assists in capturing walls, floor, ceiling, and furniture. Cannot be done over Link - must be on-device.

### Scene Anchors

The fundamental element of a scene model. Each anchor has:
- **Semantic labels**: WALL, FLOOR, CEILING, TABLE, COUCH, etc.
- **Geometric components**: 2D plane (surface), 3D bounding box, or scene mesh
- **Pose**: Position and orientation in the real world

## Accessing Scene Data

### Preferred: Mixed Reality Utility Kit (MRUK)

If `com.meta.xr.mrutilitykit` is installed in the project, use MRUK for all Scene interactions. It provides high-level tools (world locking, scene queries, room management) on top of the raw Scene APIs. Check for the package before falling back to direct OVRAnchor access.

### Fallback: Direct OVRAnchor API

If MRUK is not available, use the asynchronous C# OVRAnchor API directly. First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **OVRAnchor**: `Scripts/OVRAnchor/OVRAnchor.cs` — grep for scene-related methods
- **Scene components**: `Scripts/OVRAnchor/OVRAnchorComponents/` — grep for `OVRSemanticLabels`, `OVRBounded2D`, `OVRBounded3D`, `OVRTriangleMesh`

## Requesting Space Setup

Apps can check if a scene model exists and invoke Space Setup if needed. See the Scene documentation for implementation details.

## Multiple Rooms

- Users can scan and maintain up to 15 rooms
- New rooms don't erase previous scans
- System locates rooms based on user's current position
- All anchor operations work as users move between rooms

## Spatial vs Scene Anchors

| Aspect | Spatial Anchors | Scene Anchors |
|---|---|---|
| Created by | Application | Meta Quest OS (Space Setup) |
| Owned by | Application (private) | System (shared across apps) |
| Contains | Position/orientation | Position, geometry, semantics |
| Modifiable | Yes (create, save, erase) | Read-only (query only) |

## Permissions

Accessing scene data requires the **spatial data runtime permission** since it contains information about the user's physical space.

## Sample Scenes

| Scene | Description |
|---|---|
| MixedReality | Scene + Passthrough + Boundary integration |
| CustomSceneManager | Direct OVRAnchor API for custom scene management |

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-scene-overview
- https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-overview
- https://developers.meta.com/horizon/documentation/unity/unity-scene-build-mixed-reality
