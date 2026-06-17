# SceneNavigation Reference

`Meta.XR.MRUtilityKit.SceneNavigation` bakes a Unity NavMesh at runtime from scanned room geometry. It looks simple in the Inspector but has **four silent-failure modes** that produce zero compile errors and zero console errors — the navmesh just doesn't work. Use this reference whenever creating, fixing, or reviewing SceneNavigation setup.

Requires: `com.unity.ai.navigation` (NavMeshSurface) in addition to `com.meta.xr.mrutilitykit`.

## Source Lookup

First locate the MRUK package root (see "Finding the SDK Source" in SKILL.md), then grep:
- **SceneNavigation component**: `Core/Scripts/SceneNavigation.cs` — grep for `public` fields, `OnNavMeshInitialized`, `CreateNavMeshSurface`, `BuildSceneNavMesh`
- **SceneLabels enum** (values used for `NavigableSurfaces` / `SceneObstacles`): grep `Core/Scripts/MRUKAnchor.cs` for `public enum SceneLabels`
- **CustomAgent / Agents reassignment logic**: grep `SceneNavigation.cs` for `CustomAgent`, `agentTypeID`
- **MRUK.RoomFilter** (the type of `BuildOnSceneLoaded`): grep `Core/Scripts/MRUK.cs` for `public enum RoomFilter`

## The Four Required Setup Rules

If any of these is wrong, the navmesh bakes silently and the agent does nothing. There is no error in the console.

### 1. Attach only `SceneNavigation` — never pre-attach `NavMeshSurface`

`SceneNavigation.CreateNavMeshSurface()` does `GetComponent<NavMeshSurface>()` and adds one itself if absent. In scene-data mode it also sets the surface's `hideFlags = NotEditable` because its properties get overwritten on every bake.

- **Don't**: manually add `NavMeshSurface` next to it and configure it. Anything you set will be wiped.
- **Do**: create a fresh GameObject (e.g. `SceneNavigation`) with just `Transform` + `SceneNavigation`. Let the component own its surface.

### 2. Set `NavigableSurfaces` — defaults to `0` (nothing)

`NavigableSurfaces` is an `MRUKAnchor.SceneLabels` flags field. **Its default value is 0**, meaning no anchors qualify as walkable and the bake produces an empty mesh. Forgetting this is the #1 reason "the navmesh isn't working."

Minimum useful value: `FLOOR` (int = 1).

### 3. Set `SceneObstacles` — also defaults to `0`

Same enum, same default. Without obstacles, the agent paths through walls and furniture.

- Minimum: `WALL_FACE` (int = 4)
- Typical: `WALL_FACE | INNER_WALL_FACE`
- Full: `WALL_FACE | INNER_WALL_FACE | TABLE | COUCH | BED | STORAGE | SCREEN | LAMP | PLANT | OTHER`

### 4. Add every NavMeshAgent to the `Agents` list

`CustomAgent` defaults to `true`, which means SceneNavigation bakes the navmesh with a **freshly-created agent type** derived from its own `AgentRadius` / `AgentHeight` / `AgentClimb` / `AgentMaxSlope` fields. After the bake, only the `NavMeshAgent`s listed in the `Agents` field get their `agentTypeID` reassigned to that new agent type.

**If an agent isn't in the list, its `agentTypeID` doesn't match the baked navmesh and `SetDestination` silently fails.** No error — paths just never resolve.

Wire every wandering/chasing NavMeshAgent into the `Agents` list.

## Key Fields

| Field | Type | Default | Purpose |
|---|---|---|---|
| `BuildOnSceneLoaded` | `MRUK.RoomFilter` | `CurrentRoomOnly` | Auto-bake trigger on `MRUK.SceneLoadedEvent`. `Disabled` skips auto-bake — call `BuildSceneNavMesh()` manually. |
| `UseSceneData` | bool | `true` | Use MRUK room anchors as bake input. Set `false` to fall back to collected Unity meshes via `CollectObjects`. |
| `NavigableSurfaces` | `MRUKAnchor.SceneLabels` (flags) | `0` (nothing) | Which anchor labels become walkable surfaces. **MUST be set.** |
| `SceneObstacles` | `MRUKAnchor.SceneLabels` (flags) | `0` (nothing) | Which anchor labels become navmesh obstacles. **MUST be set.** |
| `Agents` | `List<NavMeshAgent>` | empty | Every agent that should walk on this navmesh. **MUST contain each one.** |
| `CustomAgent` | bool | `true` | Bake a fresh agent type from `AgentRadius`/`Height`/`Climb`/`MaxSlope` and reassign listed `Agents` to it. |
| `AgentRadius` / `AgentHeight` / `AgentClimb` / `AgentMaxSlope` | float | `0.2` / `0.5` / `0.04` / `5.5°` | Bake parameters used when `CustomAgent = true`. |
| `OnNavMeshInitialized` | `UnityEvent` | — | Fires after the bake. Agent logic must wait for this before calling `SetDestination` or `Warp`. |

## MRUKAnchor.SceneLabels Bitmask

For setting `NavigableSurfaces` / `SceneObstacles` via int (Unity MCP, YAML inspection):

| Label | Bit | Int |
|---|---|---|
| `FLOOR` | 0 | 1 |
| `CEILING` | 1 | 2 |
| `WALL_FACE` | 2 | 4 |
| `TABLE` | 3 | 8 |
| `COUCH` | 4 | 16 |
| `DOOR_FRAME` | 5 | 32 |
| `WINDOW_FRAME` | 6 | 64 |
| `OTHER` | 7 | 128 |
| `STORAGE` | 8 | 256 |
| `BED` | 9 | 512 |
| `SCREEN` | 10 | 1024 |
| `LAMP` | 11 | 2048 |
| `PLANT` | 12 | 4096 |
| `GLOBAL_MESH` | 14 | 16384 |
| `INVISIBLE_WALL_FACE` | 15 | 32768 |
| `INNER_WALL_FACE` | 18 | 262144 |

Source of truth: grep `Core/Scripts/MRUKAnchor.cs` for `public enum SceneLabels` (values are bit shifts of `OVRPlugin.Classification`). Verify against the source if the SDK version differs.

## Minimum Working Configuration

Barebones single-room demo:

```
BuildOnSceneLoaded:  CurrentRoomOnly  (int = 1, default)
UseSceneData:        true             (default)
CustomAgent:         true             (default)
NavigableSurfaces:   FLOOR            (int = 1)        — REQUIRED
SceneObstacles:      WALL_FACE        (int = 4)        — REQUIRED, expand as needed
AgentRadius:         0.2              (default)
AgentHeight:         0.5              (default)
Agents:              [<your NavMeshAgent>]             — REQUIRED, one entry per agent
```

GameObject hosting `SceneNavigation`:
- No `NavMeshSurface` — let SceneNavigation add and own it.
- No `BoxCollider` or rendered mesh — it's a logical controller, not a visual.

## NavMeshAgent Setup on the Agent GameObject

- Match the agent's `radius` / `height` to SceneNavigation's `AgentRadius` / `AgentHeight` to avoid corner-clipping warnings.
- `baseOffset` should equal half the agent's visual height (e.g. a 0.3-scale cube wants `baseOffset = 0.15` so its bottom sits on the floor).
- **Disable the `NavMeshAgent` in `Awake`** and re-enable it after `SceneNavigation.OnNavMeshInitialized` fires. Otherwise it errors trying to snap to a non-existent navmesh on the first frame.
- Use `agent.Warp(point)` to place the agent on the freshly-baked mesh — plain `transform.position = ...` leaves the agent's internal nav state out of sync.

## Sample Agent Driver

Minimal `WanderAgent` that waits for the navmesh, warps onto it, then walks to random points forever. Useful as a smoke test before building real agent behaviour.

```csharp
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderAgent : MonoBehaviour
{
    [SerializeField] private SceneNavigation sceneNavigation;
    [SerializeField] private float arrivalThreshold = 0.2f;

    private NavMeshAgent _agent;
    private bool _navReady;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = false;
        if (sceneNavigation == null) sceneNavigation = FindAnyObjectByType<SceneNavigation>();
        if (sceneNavigation != null) sceneNavigation.OnNavMeshInitialized.AddListener(OnNavReady);
    }

    private void OnDestroy()
    {
        if (sceneNavigation != null) sceneNavigation.OnNavMeshInitialized.RemoveListener(OnNavReady);
    }

    private void OnNavReady()
    {
        if (!TryPickPoint(out var p)) return;
        _agent.enabled = true;
        _agent.Warp(p);
        _navReady = true;
        _agent.SetDestination(p);
    }

    private void Update()
    {
        if (!_navReady || _agent.pathPending) return;
        if (_agent.remainingDistance <= arrivalThreshold && TryPickPoint(out var p))
            _agent.SetDestination(p);
    }

    private bool TryPickPoint(out Vector3 point)
    {
        point = Vector3.zero;
        var tri = NavMesh.CalculateTriangulation();
        if (tri.indices.Length < 3) return false;
        int t = Random.Range(0, tri.indices.Length / 3);
        var a = tri.vertices[tri.indices[t * 3]];
        var b = tri.vertices[tri.indices[t * 3 + 1]];
        var c = tri.vertices[tri.indices[t * 3 + 2]];
        float u = Random.value, v = Random.value;
        if (u + v > 1f) { u = 1f - u; v = 1f - v; }
        var cand = a + u * (b - a) + v * (c - a);
        if (!NavMesh.SamplePosition(cand, out var hit, 0.5f, NavMesh.AllAreas)) return false;
        point = hit.position;
        return true;
    }
}
```

## Building SceneNavigation via Unity MCP

Order matters — an asset delete that triggers a confirmation dialog rolls back the entire `RunCommand`.

1. Verify `MRUK` is in the scene with `SceneSettings.LoadSceneOnStartup = true` and a room prefab/json wired.
2. Create the `SceneNavigation` GameObject. Add only the `SceneNavigation` component. Fetch the type via reflection if the MRUK assembly isn't directly referenced by `CommandScript`:
   ```csharp
   var t = System.Type.GetType("Meta.XR.MRUtilityKit.SceneNavigation, Meta.XR.MRUtilityKit");
   go.AddComponent(t);
   ```
3. Set `NavigableSurfaces` and `SceneObstacles` via `SerializedObject`:
   ```csharp
   var so = new UnityEditor.SerializedObject(sn);
   so.FindProperty("NavigableSurfaces").intValue = 1;        // FLOOR
   so.FindProperty("SceneObstacles").intValue   = 262148;    // WALL_FACE | INNER_WALL_FACE
   so.ApplyModifiedPropertiesWithoutUndo();
   ```
4. Create the agent GameObject (primitive + `NavMeshAgent` + custom driver script).
5. Append the agent's `NavMeshAgent` to the SceneNavigation `Agents` array:
   ```csharp
   var agents = so.FindProperty("Agents");
   agents.arraySize++;
   agents.GetArrayElementAtIndex(agents.arraySize - 1).objectReferenceValue = navMeshAgent;
   so.ApplyModifiedPropertiesWithoutUndo();
   ```
6. `EditorSceneManager.MarkSceneDirty(scene)` + `EditorSceneManager.SaveScene(scene)`. **Run scene saves in a separate `RunCommand` call from any `AssetDatabase.DeleteAsset`** — the delete triggers a confirmation dialog that the MCP harness can't dismiss and rolls back the whole transaction. Use PowerShell `Remove-Item` for asset deletes, then call `AssetDatabase.Refresh()`.

## Verification

1. `Unity_GetConsoleLogs` with `logTypes: "Error"` → zero errors.
2. Enter Play mode. After `MRUK.SceneLoadedEvent` fires, `SceneNavigation.OnNavMeshInitialized` fires. The agent should immediately Warp and start moving.
3. If the agent freezes or never moves, check in order:
   - `NavigableSurfaces != 0` — an empty navmesh has no triangles.
   - The agent is in the `Agents` list — `agentTypeID` mismatch silently breaks `SetDestination`.
   - `NavMesh.CalculateTriangulation().indices.Length > 0`.
4. Optional visual check: `Unity_SceneView_CaptureMultiAngleSceneView` after a few seconds shows the agent moved from its spawn point.

## Common Failure Modes

| Symptom | Likely Cause |
|---|---|
| Agent stays at origin, no errors | `Agents` list empty → agent's `agentTypeID` doesn't match baked mesh |
| Agent walks through walls | `SceneObstacles` is 0 or missing wall labels |
| `NavMesh.CalculateTriangulation()` returns empty | `NavigableSurfaces` is 0 (default) |
| First-frame `NavMeshAgent` errors | Agent wasn't disabled in `Awake` while waiting for `OnNavMeshInitialized` |
| `AssetDatabase.DeleteAsset` rolls back the whole `RunCommand` | Confirmation dialog blocks MCP. Use PowerShell `Remove-Item` + `AssetDatabase.Refresh()` instead |
| Saved scene has different values than in-memory editor | User edited Inspector without saving. Call `EditorSceneManager.SaveScene` before re-reading the `.unity` YAML |

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-sample-mruk-navmesh
- https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-overview
- API: search via `metavr docs search "MRUK NavMesh"` for the latest version
