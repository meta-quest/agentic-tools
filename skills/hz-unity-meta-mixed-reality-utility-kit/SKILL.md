---
name: hz-unity-meta-mixed-reality-utility-kit
license: Apache-2.0
description: Meta XR Mixed Reality Utility Kit (MRUK) (com.meta.xr.mrutilitykit) for Unity XR development. Use when working with Scene API data (rooms, walls, floors, furniture), spawning prefabs on scene anchors, placing virtual objects in the real world, world locking to prevent anchor drift, raycasting against room geometry, generating NavMesh from scene data, environment depth raycasting for instant placement without scanning, Passthrough Camera Access (PCA), trackable detection (keyboards, QR codes), destructible scene meshes, space maps, room sharing for multiplayer, or any MR scene-aware feature.
---

# Meta XR Mixed Reality Utility Kit (MR Utility Kit, MRUK, com.meta.xr.mrutilitykit)

The Meta XR Mixed Reality Utility Kit (MR Utility Kit, MRUK) is a Unity package that provides high-level tools on top of the Scene API for building scene-aware Mixed Reality (MR) experiences. It is designed to be used in conjunction with the Meta XR Core SDK.
It handles scene data loading and querying, spawning prefabs on real-world surfaces, world locking to prevent anchor drift, environment raycasting for instant placement without scanning, passthrough camera access, NavMesh generation from room geometry, and trackable detection (keyboards, QR codes).

Package: com.meta.xr.mrutilitykit

## Finding the SDK Source

The package `com.meta.xr.mrutilitykit` may be located in different places depending on the project setup:
- `Library/PackageCache/com.meta.xr.mrutilitykit@<hash>/` (cached from registry)
- `Packages/com.meta.xr.mrutilitykit/` (local package reference)
- A custom on-disk path (embedded or local folder)

**Before searching for SDK source, first locate the package root** by searching for a known file by filename pattern:
```
**/com.meta.xr.mrutilitykit*/Core/Scripts/MRUK.cs
```
Then use the resolved parent path for all subsequent search operations.

## Quick Reference

| Task | Class / Component | Source File | Key Entry Point |
|---|---|---|---|
| Load scene from device | `MRUK` | `Scripts/MRUK.cs` | `LoadSceneFromDevice()` |
| Load scene from prefab/JSON | `MRUK` | `Scripts/MRUK.cs` | `LoadSceneFromPrefab()`, `LoadSceneFromJsonString()` |
| Get current room | `MRUK` | `Scripts/MRUK.cs` | `GetCurrentRoom()` |
| Prevent anchor drift | `MRUK` | `Scripts/MRUK.cs` | `EnableWorldLock = true` |
| Query room geometry | `MRUKRoom` | `Scripts/MRUKRoom.cs` | `Raycast()`, `IsPositionInRoom()` |
| Find surfaces / closest point | `MRUKRoom` | `Scripts/MRUKRoom.cs` | `TryGetClosestSurfacePosition()`, `GenerateRandomPositionOnSurface()` |
| Get anchor properties | `MRUKAnchor` | `Scripts/MRUKAnchor.cs` | `Label`, `PlaneRect`, `VolumeBounds` |
| Spawn prefabs on anchors | `AnchorPrefabSpawner` | `Scripts/AnchorPrefabSpawner.cs` | `PrefabsToSpawn`, `SpawnOnStart` |
| Find random spawn positions | `FindSpawnPositions` | `Scripts/FindSpawnPositions.cs` | `StartSpawn()`, `SpawnLocations` |
| 27-slice mesh resizing | `GridSliceResizer` | `Scripts/GridSliceResizer.cs` | `ResizeTo(Vector3)` |
| Generate visual meshes | `EffectMesh` | `Scripts/EffectMesh.cs` | `CreateMesh()`, `CutHoles` |
| Breakable scene geometry | `DestructibleGlobalMeshSpawner` | `Scripts/DestructibleGlobalMeshSpawner.cs` | `CreateOnRoomLoaded` |
| GPU distance field texture | `SpaceMapGPU` | `Scripts/SpaceMapGPU.cs` | `GetSpaceMap()`, `StartSpaceMap()` |
| Boundary proximity fade | `RoomGuardian` | `Scripts/RoomGuardian.cs` | `GuardianDistance`, `GuardianMaterial` |
| NavMesh from scene | `SceneNavigation` | `Scripts/SceneNavigation.cs` | `BuildSceneNavMesh()` |
| Depth API raycasting | `EnvironmentRaycastManager` | `Scripts/EnvironmentRaycastManager.cs` | `Raycast()`, `PlaceBox()` |
| Passthrough camera images | `PassthroughCameraAccess` | `Scripts/PassthroughCameraAccess.cs` | `GetTexture()`, `GetCameraPose()` |
| Track keyboards / QR codes | `MRUKTrackable` | `Scripts/MRUKTrackable.cs` | `TrackableType`, `MarkerPayloadString` |
| Instant placement (no scan) | `PointAndLocate` | `Scripts/BuildingBlocks/InstantContentPlacement/` | `TryLocateSpace()` |
| Share rooms (multiplayer) | `MRUK` | `Scripts/MRUK.cs` | `ShareRoomsAsync()`, `LoadSceneFromSharedRooms()` |

All source file paths are relative to the package root (see "Finding the SDK Source" above).

## Architecture

### Object Hierarchy

```
MRUK (singleton MonoBehaviour)
 └── MRUKRoom (child GameObjects, one per room)
      └── MRUKAnchor (children of rooms: walls, floors, furniture, etc.)
```

- **`MRUK.Instance`** is the central hub. It loads scene data, manages rooms, handles world locking, and provides the `SceneSettings` configuration.
- **`MRUKRoom`** represents a physical room. Provides spatial queries (raycasting, position checks, surface queries) and holds `Anchors`, `WallAnchors`, `FloorAnchors`, `CeilingAnchors`, `GlobalMeshAnchor`.
- **`MRUKAnchor`** represents a single scene element (wall, table, couch, etc.). Has a `Label` (SceneLabels flags), optional `PlaneRect` (2D bounds), `VolumeBounds` (3D bounds), and `PlaneBoundary2D` (polygon points).
- **`MRUKTrackable`** extends `MRUKAnchor` for runtime-detected objects (keyboards, QR codes).

### Native Library

A C/C++ shared library handles world locking, anchor discovery, and coordinate conversion (OpenXR right-handed ↔ Unity left-handed). A hidden `MRUKGlobalContext` MonoBehaviour (DontDestroyOnLoad) ticks the native context every frame. You never call these directly.

### Component Pattern

All feature components (AnchorPrefabSpawner, EffectMesh, FindSpawnPositions, SceneNavigation, SpaceMapGPU, RoomGuardian, DestructibleGlobalMeshSpawner) follow the same pattern:
1. A `SpawnOnStart` / `CreateOnStart` / `BuildOnSceneLoaded` property of type `MRUK.RoomFilter` controls automatic initialization
2. In `Start()`, they call `MRUK.Instance.RegisterSceneLoadedCallback(...)` which fires immediately if the scene is already loaded
3. They also expose manual methods (`CreateMesh()`, `StartSpawn()`, `BuildSceneNavMesh()`, etc.) for programmatic control

### Dependencies

- **Required**: `com.meta.xr.sdk.core` (OVRCameraRig, OVRPlugin, OVRSpatialAnchor, OVRSemanticLabels)
- `com.unity.ai.navigation` (for SceneNavigation)
- `com.unity.nuget.newtonsoft-json` (for JSON scene serialization)

## Key Enums

### MRUKAnchor.SceneLabels (Flags)

```csharp
FLOOR, CEILING, WALL_FACE, TABLE, COUCH, DOOR_FRAME, WINDOW_FRAME,
OTHER, STORAGE, BED, SCREEN, LAMP, PLANT, WALL_ART, GLOBAL_MESH,
INVISIBLE_WALL_FACE, UNKNOWN, INNER_WALL_FACE
```

Use as flags: `SceneLabels.TABLE | SceneLabels.COUCH` matches tables OR couches.

### MRUKAnchor.ComponentType (Flags)

```csharp
None = 0, Plane = 1, Volume = 2, All = Plane | Volume
```

Anchors can have plane geometry (2D rect), volume geometry (3D bounds), or both.

### MRUK.SceneDataSource

```csharp
Device                    // Load from device Scene API
Prefab                    // Load from room prefab GameObjects
DeviceWithPrefabFallback  // Device first, prefab if no scene data (recommended for dev)
Json                      // Load from JSON string
DeviceWithJsonFallback    // Device first, JSON if no scene data
```

### MRUK.SurfaceType (Flags)

```csharp
FACING_UP = 1    // Floors, table tops
FACING_DOWN = 2  // Ceilings, undersides
VERTICAL = 4     // Walls
```

### MRUK.RoomFilter

```csharp
None             // Don't auto-initialize
CurrentRoomOnly  // Only the room the user is in
AllRooms         // All loaded rooms
```

### MRUK.SceneModel

```csharp
V1               // Standard scene model
V2               // High Fidelity: multiple floors/ceilings, slanted ceilings, inner walls
V2FallbackV1     // Try V2, fall back to V1 if unsupported
```

### LabelFilter

Struct for filtering anchors by label and component type. Used in raycast, surface query, and spawn methods.

```csharp
var filter = new LabelFilter(SceneLabels.TABLE | SceneLabels.COUCH, ComponentType.Volume);
bool passes = filter.PassesFilter(anchor.Label);
```

### MRUK.PositioningMethod

```csharp
DEFAULT  // Standard positioning
CENTER   // Center of surface (e.g., center of table for chess board)
EDGE     // Edge of surface (e.g., edge of table for piano)
```

## Core Classes

### MRUK (Singleton)

**Key Properties**: `Instance` (static), `IsInitialized`, `Rooms` (List\<MRUKRoom\>), `EnableWorldLock`, `IsWorldLockActive`, `TrackingSpaceOffset`, `SceneSettings`

**Key Methods**:
- `GetCurrentRoom()` → MRUKRoom
- `RegisterSceneLoadedCallback(UnityAction)` — fires immediately if already loaded
- `LoadSceneFromDevice(SceneModel)` → Task\<LoadDeviceResult\>
- `LoadSceneFromPrefab(GameObject)` → Task\<LoadDeviceResult\>
- `LoadSceneFromJsonString(string)` → Task\<LoadDeviceResult\>
- `SaveSceneToJsonString()` → string
- `ClearScene()`
- `HasSceneModel()` → Task\<bool\> (static, checks if device has scene data)
- `ShareRoomsAsync(...)`, `LoadSceneFromSharedRooms(...)` — co-located multiplayer

**Events**: `SceneLoadedEvent`, `RoomCreatedEvent`, `RoomUpdatedEvent`, `RoomRemovedEvent`

**MRUKSettings** (serialized config): `DataSource`, `RoomPrefabs[]`, `SceneJsons[]`, `LoadSceneOnStartup`, `EnableHighFidelityScene`, `SeatWidth`, `TrackerConfiguration`

### MRUKRoom

**Key Properties**: `Anchors`, `WallAnchors`, `FloorAnchors`, `CeilingAnchors`, `GlobalMeshAnchor`, `IsLocal`, `RoomMeshData`

**Key Methods**:
- `Raycast(Ray, float, out RaycastHit, out MRUKAnchor)` — collisionless raycast against scene geometry
- `RaycastAll(...)` — all intersections
- `IsPositionInRoom(Vector3)`, `IsPositionInSceneVolume(Vector3)`
- `TryGetClosestSurfacePosition(Vector3, out Vector3, out MRUKAnchor, LabelFilter)` → float (distance)
- `GenerateRandomPositionOnSurface(SurfaceType, float, LabelFilter, out Vector3, out Vector3)` → bool
- `GenerateRandomPositionInRoom(float, bool)` → Vector3?
- `FindLargestSurface(SceneLabels)` → MRUKAnchor
- `GetRoomBounds()` → Bounds
- `GetKeyWall(out Vector2, float)` → MRUKAnchor (widest wall facing room center)

**Events**: `AnchorCreatedEvent`, `AnchorUpdatedEvent`, `AnchorRemovedEvent`

### MRUKAnchor

**Key Properties**: `Label` (SceneLabels), `PlaneRect` (Rect?), `VolumeBounds` (Bounds?), `PlaneBoundary2D` (List\<Vector2\>), `Room` (MRUKRoom), `ParentAnchor`, `ChildAnchors`, `GlobalMesh` (Mesh)

**Key Methods**:
- `Raycast(Ray, float, out RaycastHit, ComponentType)` — collisionless raycast
- `HasAnyLabel(SceneLabels)` → bool
- `GetAnchorCenter()` → Vector3
- `GetClosestSurfacePosition(Vector3, out Vector3, out Vector3, ComponentType)` → float
- `IsPositionInVolume(Vector3)`, `IsPositionInBoundary(Vector2)`

### MRUKTrackable (extends MRUKAnchor)

Runtime-tracked objects (keyboards, QR codes). Managed by MRUK via `SceneSettings.TrackerConfiguration`.

**Key Properties**: `TrackableType` (OVRAnchor.TrackableType), `IsTracked`, `MarkerPayloadString`, `MarkerPayloadBytes`

**Events on SceneSettings**: `TrackableAdded`, `TrackableRemoved`

## Feature Components

### AnchorPrefabSpawner (`Scripts/AnchorPrefabSpawner.cs`)

Spawns prefabs on scene anchors based on labels. Configure `PrefabsToSpawn` (List\<AnchorPrefabGroup\>) mapping labels to prefab lists with scaling/alignment modes.

**AnchorPrefabGroup**: `Labels`, `Prefabs`, `PrefabSelection` (Random/ClosestSize/Custom), `Scaling` (Stretch/UniformScaling/UniformXZScale/NoScaling/Custom), `Alignment` (Automatic/Bottom/Center/NoAlignment/Custom), `MatchAspectRatio`, `CalculateFacingDirection`

Methods: `SpawnPrefabs()`, `SpawnPrefabs(MRUKRoom)`, `ClearPrefabs()`

### FindSpawnPositions (`Scripts/FindSpawnPositions.cs`)

Generates random valid positions for spawning objects. Supports floating, surface-mounted, and hanging positions with physics overlap checking.

**SpawnLocation** enum: `Floating`, `AnySurface`, `VerticalSurfaces`, `OnTopOfSurfaces`, `HangingDown`

Key props: `SpawnObject`, `SpawnAmount`, `MaxIterations`, `Labels`, `CheckOverlaps`, `SurfaceClearanceDistance`

### GridSliceResizer (`Scripts/GridSliceResizer.cs`)

27-slice 3D mesh resizer (3D analogue of 9-slice). Preserves mesh borders/corners while stretching the interior to match anchor dimensions. Attach to prefabs used with AnchorPrefabSpawner.

### EffectMesh (`Scripts/EffectMesh.cs`)

Generates visual meshes from scene anchors. Supports materials, colliders, hole cutting (for doors/windows), shadow casting, and visibility toggling.

Key props: `MeshMaterial`, `Colliders`, `CutHoles` (SceneLabels to cut holes for), `CastShadow`, `HideMesh`

Methods: `CreateMesh()`, `DestroyMesh()`, `ToggleEffectMeshVisibility(bool)`, `ToggleEffectMeshColliders(bool)`

### Passthrough Relighting (PTRL)

Casts virtual lights' shadows and highlights onto real-world surfaces by rendering an `EffectMesh` with the `Meta/MRUK/Scene/HighlightsAndShadows` shader (`TransparentSceneAnchor.mat`). Silent-failure heavy — the most common gotcha is the URP **Renderer asset's `Transparent Receive Shadows` flag must be ON**, otherwise shadows never appear on the floor.

For setup, URP/EffectMesh/passthrough requirements, common failure modes, and editor verification, see [references/passthrough-relighting.md](references/passthrough-relighting.md).

### DestructibleGlobalMeshSpawner (`Scripts/DestructibleGlobalMeshSpawner.cs`)

Segments the global mesh into breakable fragments using Voronoi-based segmentation.

Key props: `GlobalMeshMaterial`, `PointsPerUnitX/Y/Z` (density), `ReservedTop/Bottom` (unbreakable zones)

### SpaceMapGPU (`Scripts/SpaceMapGPU.cs`)

GPU-computed 2D top-down distance field of the room layout. Outputs to `RenderTexture`.

Key props: `TextureDimension`, `MapGradient`; Methods: `GetSpaceMap()` → RenderTexture, `GetColorAtPosition(Vector3)` → Color

Sets shader global `_SpaceMapProjectionViewMatrix` for UV calculation.

### RoomGuardian (`Scripts/RoomGuardian.cs`)

Fades a boundary material as the user approaches room walls. Sets `_GuardianFade` float on the material.

Key props: `GuardianMaterial`, `GuardianDistance` (meters, default 1.0)

### SceneNavigation (`Scripts/SceneNavigation.cs`)

Generates Unity NavMesh from scene geometry. Requires `com.unity.ai.navigation`.

Key props: `NavigableSurfaces` (SceneLabels), `Agents` (List\<NavMeshAgent\>), `AgentRadius`

Events: `OnNavMeshInitialized`

For more info see [references/scene-navigation.md](references/scene-navigation.md).

### EnvironmentRaycastManager (`Scripts/EnvironmentRaycastManager.cs`)

Depth API raycasting against the physical environment **without requiring pre-scanned scene data**. Requires Quest 3+. Namespace: `Meta.XR`.

Check `EnvironmentRaycastManager.IsSupported` before use.

Methods: `Raycast(Ray, out EnvironmentRaycastHit, float)`, `PlaceBox(Ray, Vector3, Vector3, out EnvironmentRaycastHit)`, `CheckBox(Vector3, Vector3, Quaternion)`

### PassthroughCameraAccess (`Scripts/PassthroughCameraAccess.cs`)

Raw passthrough camera image access. Requires `horizonos.permission.HEADSET_CAMERA` in AndroidManifest. Quest 3+ only. Namespace: `Meta.XR`.

Key props: `CameraPosition` (Left/Right), `RequestedResolution`, `MaxFramerate`, `TargetMaterial`

Methods: `GetTexture()` → Texture, `GetColors()` → NativeArray\<Color32\> (expensive), `GetCameraPose()` → Pose, `ViewportPointToRay()`, `WorldToViewportPoint()`

Struct `CameraIntrinsics`: `FocalLength`, `PrincipalPoint`, `SensorResolution`, `LensOffset`

### Instant Content Placement (`Scripts/BuildingBlocks/InstantContentPlacement/`)

Building block scripts using EnvironmentRaycastManager for device-scan-free placement:
- **`SpaceLocator`** (abstract): `TryLocateSpace(out Pose)`, `OnSpaceLocateCompleted` event, `PreferredSurfaceOrientation`
- **`PointAndLocate`**: raycast from a Transform, `Locate()` method
- **`GrabAndLocate`**: grab-and-release placement via Interaction SDK
- **`PlaceWithAnchor`**: creates `OVRSpatialAnchor` at placed position to prevent drift

## Common Patterns

### Scene Loaded Callback

```csharp
void Start()
{
    MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
}

void OnSceneLoaded()
{
    var room = MRUK.Instance.GetCurrentRoom();
    // Scene is ready — query anchors, spawn objects, etc.
}
```

`RegisterSceneLoadedCallback` fires immediately if the scene is already loaded — safe to call at any time.

### Iterating Anchors with LabelFilter

```csharp
var room = MRUK.Instance.GetCurrentRoom();
foreach (var anchor in room.Anchors)
{
    if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE | MRUKAnchor.SceneLabels.STORAGE))
    {
        Debug.Log($"{anchor.Label} at {anchor.GetAnchorCenter()}");
    }
}
```

### Raycasting Against Room Geometry

```csharp
var room = MRUK.Instance.GetCurrentRoom();
var ray = new Ray(origin, direction);
if (room.Raycast(ray, Mathf.Infinity, out var hit, out var anchor))
{
    Debug.Log($"Hit {anchor.Label} at {hit.point}, normal {hit.normal}");
}
```

### Editor Testing with Prefab Fallback

Set `MRUK.SceneSettings.DataSource` to `DeviceWithPrefabFallback` and assign room prefabs in `SceneSettings.RoomPrefabs`. On device, real scene data is used. In editor (no device), prefab rooms load automatically. The package includes ~50 sample room prefabs under `Core/Rooms/Prefabs/` (Bedroom, LivingRoom, Office, LargeScale).

## Project Setup

1. Install `com.meta.xr.mrutilitykit` package
2. In OVRManager > Quest Features, set **Scene Support** to "Required"
3. In OVRManager > Quest Features, enable **Anchor Support**
4. Ensure `OVRCameraRig` is in the scene
5. Add an `MRUK` GameObject with the `MRUK` component to the scene
6. Configure `SceneSettings.DataSource` (use `DeviceWithPrefabFallback` for development)
7. For PassthroughCameraAccess: add `horizonos.permission.HEADSET_CAMERA` to AndroidManifest and regenerate via `OVRManifestPreprocessor.GenerateOrUpdateAndroidManifest()`

## Calling SDK Methods via Unity MCP

MRUK classes live in the `Meta.XR.MRUtilityKit` namespace in the `meta.xr.mrutilitykit` assembly, which is **not directly referenceable** from Unity MCP compiled scripts. Use runtime reflection:

```csharp
// 1. Find the type
System.Type t = null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try { t = asm.GetType("Meta.XR.MRUtilityKit.MRUK"); } catch { }
    if (t != null) break;
}

// 2. Get method (no BindingFlags — parameterless overload only)
var m = t.GetMethod("GetCurrentRoom");

// 3. Get singleton instance
var instanceProp = t.GetProperty("Instance");
var instance = instanceProp.GetValue(null);

// 4. Invoke
var room = m.Invoke(instance, null);
```

**Rules**: Never add `using System.Reflection;` (causes MCP crashes). Never use `BindingFlags` overloads. Always catch `System.Reflection.TargetInvocationException` and log `InnerException`.

## Using metavr Tools for Latest Docs

If the `metavr` MCP server is available, use the `mcp__metavr__meta_docs_search` and `mcp__metavr__meta_docs_get_page` tools to verify current API details, as Meta SDK documentation updates frequently. Use `mcp__metavr__search_api_reference` with `engine='unity'` to look up exact method signatures for MRUK, MRUKRoom, MRUKAnchor, and other classes.

## Documentation Links

- [MR Utility Kit Overview](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-overview)
- [MRUK API Reference](https://developers.meta.com/horizon/reference/mruk/latest/)
- [GitHub Samples](https://github.com/oculus-samples/Unity-MRUtilityKitSample) (12 sample scenes, moved from package since v76)
- [Scene API Overview](https://developers.meta.com/horizon/documentation/unity/unity-scene-overview)
