# Scene API to MRUK migration best practices

Search for and fetch the current official guide with `metavr`. If the search
does not return it, record the gap and use the canonical page:

- [Migrating from OVRSceneManager to MRUK](https://developers.meta.com/horizon/documentation/unity/unity-scene-migrate-mruk)

Then verify the target package API and apply the migration checks below.

The legacy Scene API may still compile with deprecation warnings in a target release. Treat Scene API-to-MRUK as a semantic and runtime migration, not evidence that a removed type merely needs renaming. Verify the target version before deciding whether retaining a deprecated path is within scope.

## Migrate the complete data path

Replacing `OVRSceneManager` is not just a type rename. Follow the scene data from acquisition through every consumer:

- load device scene data through `MRUK.Instance.LoadSceneFromDevice(...)`
- obtain the active room from `MRUK.Instance.GetCurrentRoom()`
- enumerate `MRUKRoom.Anchors`
- read `MRUKAnchor.AnchorLabels`
- distinguish planar and volumetric anchors through `PlaneRect` and `VolumeBounds`
- read the global mesh from `MRUKAnchor.GlobalMesh`
- update custom adapters, mocks, exporters, tests, and serialized prefab references

Inspect the installed MRUK version for exact overloads and result enums. Do not copy signatures from this reference without verifying them.

Migrate the producer and consumers through one coherent contract. A loader that emits MRUK-backed objects while a debug view, exporter, or gameplay consumer still expects legacy components can compile and silently produce no scene data.

## Serialized prefab mapping

The old scene manager can own a prefab-per-classification mapping. Moving that mapping into fields on a new loader does not preserve the serialized assignments automatically. Compare the pre-migration scene/prefab, reassign every classification the app uses, and separately assign the plane, volume, and global-mesh representations the application uses. A valid but empty field produces an empty room without a compiler error.

## Global mesh is a separate shape path

If a custom loader maps semantic classifications to prefabs, treat
`GLOBAL_MESH` as a separate path rather than as a plane or volume. A common
migration failure is:

1. classify every anchor as plane-or-volume;
2. select the generic plane/volume prefab;
3. reject a null prefab;
4. handle `GLOBAL_MESH` later, after it has already been skipped.

Select the custom loader's global-mesh representation before any shared
null-prefab guard, then pass `anchor.GlobalMesh` to the rendering or collision
components used by the application. Validate that a missing global mesh
produces a bounded failure or fallback; an endless rescan is not successful
recovery.

## MRUK lifetime and settings

Prefer a serialized, configured MRUK component or package-provided rig. If legacy architecture requires runtime creation, verify that:

- `MRUK.Instance` is established before loading;
- `SceneSettings` is non-null;
- startup loading is configured deliberately so the custom loader does not race MRUK auto-load;
- callbacks and object lifetime survive scene transitions as intended.

A bare `AddComponent<MRUK>()` establishes the singleton but does not reproduce the configured MRUK prefab. Assign a verified `MRUKSettings`/`SceneSettings` value and deliberately disable or enable startup loading so a custom loader does not race automatic loading.

## Permissions and device behavior

Preserve the scene-use declaration and runtime permission flow. A successful Unity import or Horizon Link session can mask missing Android permission or scene-capture behavior. Test the no-scene-data path, permission denial/grant, capture prompt, room load, global-mesh creation, and fallback on a headset.

Declared and granted are separate states. When the project loads manually rather than using MRUK startup loading, ensure permission is granted before `LoadSceneFromDevice`. Confirm the built Android manifest and the runtime grant independently.

## Horizon Link and device constraints

An editor or Horizon Link path may deliberately select static scene data and never exercise MRUK. Log which source actually ran. Room capture and global-mesh behavior must be validated on a suitable standalone headset; Horizon Link success is not proof of either. Distinguish no captured rooms from a successful room load that contains no global mesh, since they require different remediation.

## Serialized rewiring

Removing legacy components can leave `Missing Script` entries or null prefab fields even when C# compiles. Inspect affected scenes and prefabs for the MRUK component, scene settings, plane/volume/global-mesh prefabs, event listeners, and downstream scene-anchor consumers.

## Critical completion checklist

Before the final compile and report, verify all of these directly in the migrated project:

- the loader reads `MRUKAnchor.AnchorLabels`
- planar and volumetric anchors use `PlaneRect` and `VolumeBounds`
- if a custom loader maps classifications to prefabs, it selects its global-mesh representation before any shared null-prefab guard
- if the application consumes global mesh, it routes `MRUKAnchor.GlobalMesh` to its rendering or collision components
- runtime-created MRUK has non-null `SceneSettings`, with startup loading chosen deliberately
- every loader consumer, including debug, parser, exporter, mock, and test code, uses the migrated contract
- scenes and prefabs contain the MRUK component/settings and all required serialized mappings
- permission denial, no-room, room-without-global-mesh, and successful-room paths terminate without an endless rescan loop
