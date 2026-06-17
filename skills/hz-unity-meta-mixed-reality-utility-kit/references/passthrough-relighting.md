---
name: passthrough-relighting
description: Configure Meta MRUK Passthrough Relighting (PTRL) so virtual lights actually cast shadows and highlights onto the real-world surfaces visible through passthrough. Use whenever setting up, fixing, or reviewing PTRL in a Meta XR + MRUK Unity project. Covers the silent-failure modes — most importantly that the URP **Renderer asset's `Transparent Receive Shadows` flag must be ON**, because the PTRL shader renders in the Transparent queue and the EffectMesh surfaces are transparent; without it, lights look right but the floor/walls stay un-shadowed.
---

# Passthrough Relighting (PTRL)

PTRL casts Unity virtual lights' **shadows and highlights** onto the real world by rendering invisible scene-anchor geometry (`EffectMesh`) with the `Meta/MRUK/Scene/HighlightsAndShadows` shader. That shader writes a shadow/highlight contribution into the framebuffer alpha, which the OpenXR compositor blends with the passthrough layer.

It is **silent-failure heavy**: every piece can look correctly configured in the Inspector while nothing actually appears on the real floor. This skill documents the minimum correct setup.

## The five things that all have to be true

### 1. URP Renderer asset → `Transparent Receive Shadows` = ON  (silent #1)

This is the most overlooked one and the reason "the lights are there, the shaders are there, the directional light has shadows, but the floor stays clean."

The PTRL material renders in the **Transparent queue**. URP only samples the main-light shadowmap inside transparent passes when `m_ShadowTransparentReceive = true` on the **Renderer asset** (NOT the URP Asset). With it off, the entire transparent pass — including the EffectMesh — skips shadow sampling, so the alpha never gets darkened where a shadow should be.

- File: `Assets/Settings/<Mobile|PC>_Renderer.asset` (the `UniversalRendererData` referenced by the URP Asset)
- Inspector: **Renderer asset → Shadows → Transparent Receive Shadows ✓**
- YAML: `m_ShadowTransparentReceive: 1`
- Default is `true`, but custom pipeline configs frequently turn it off for perf — always re-verify on a clean project.

**Set this on every renderer asset that ships**, especially the **Mobile** renderer (that is what runs on Quest). Setting it on PC only fixes the in-editor preview.

### 2. URP Asset → Additional Lights = Per Pixel + per-object limit ≥ N

Point and spot light **highlights** come from the PTRL shader's per-pixel additive pass. If `Additional Lights` is `Per Vertex` or `Disabled`, point lights still illuminate opaque geometry but produce **no highlight on the EffectMesh** — same silent-failure shape as #1.

- `m_AdditionalLightsRenderingMode = 1` (Per Pixel)
- `m_AdditionalLightsPerObjectLimit ≥ 4` (raise to 8 if you have multiple point lights per region)
- `m_AdditionalLightShadowsSupported = true`
- `m_MainLightShadowsSupported = true`
- `m_ShadowType = 2` (Soft Shadows)

Apply to **every quality-level URP Asset** that ships (Mobile + PC).

### 3. `EffectMesh` component configured for PTRL

Create a GameObject named `EffectMesh` with only the `Meta.XR.MRUtilityKit.EffectMesh` component. Required settings:

```
MeshMaterial:    Packages/com.meta.xr.mrutilitykit/Core/Materials/TransparentSceneAnchor.mat
                 (shader: Meta/MRUK/Scene/HighlightsAndShadows)
Labels:          all SceneLabels EXCEPT GLOBAL_MESH      (int = 442367)
SpawnOnStart:    CurrentRoomOnly                          (enumValueIndex = 1)
castShadows:     false                                    (it only RECEIVES shadows)
Colliders:       false                                    (PTRL is render-only; obstacles belong to SceneNavigation)
hideMesh:        false                                    (the mesh IS visible — it draws the alpha)
```

`EffectMesh` auto-subscribes to `MRUK.SceneLoadedEvent` in its `Start()`, so you **don't** need to wire SceneLoadedEvent manually (see `Library/PackageCache/com.meta.xr.mrutilitykit@*/Core/Scripts/EffectMesh.cs` `Start()` → `RegisterSceneLoadedCallback`).

If you want shadows on the full scanned mesh (handles non-planar clutter), make a **second** EffectMesh with `Labels = GLOBAL_MESH only` and switch between them at runtime via `CreateMesh()` / `DestroyMesh()` — same pattern as the Meta PTRL sample.

### 4. `OVRPassthroughLayer` placement = `Underlay` + camera alpha = 0

PTRL relies on alpha compositing through OpenXR. If passthrough is rendered as an `Overlay`, it draws **on top** of virtual content and hides everything. If the camera background isn't transparent, there's no alpha for the compositor to read.

- `OVRPassthroughLayer.overlayType = Underlay` (default, but verify)
- `OVRManager.isInsightPassthroughEnabled = true`
- CenterEye `Camera.clearFlags = SolidColor`, `backgroundColor = (0,0,0,0)`

### 5. Lights actually cast shadows

- Directional Light: `shadows = LightShadows.Soft`, reasonable `shadowStrength` (~0.85), tilted (not pointing straight down — you want shadows visible at an angle on the floor).
- The objects you want to cast shadows must have `Renderer.shadowCastingMode = On`.
- Point/spot lights: leave `LightShadows.None` for performance — they create highlights through the shader's additive pass, not real shadowmaps.

## Reference values for `EffectMesh.Labels` (MRUKAnchor.SceneLabels bitmask)

Same flags enum used by `SceneNavigation`. From `Library/PackageCache/com.meta.xr.mrutilitykit@*/Core/Scripts/MRUKAnchor.cs:57`:

| Label | Int value |
|---|---|
| `FLOOR` | 1 |
| `CEILING` | 2 |
| `WALL_FACE` | 4 |
| `TABLE` | 8 |
| `COUCH` | 16 |
| `DOOR_FRAME` | 32 |
| `WINDOW_FRAME` | 64 |
| `OTHER` | 128 |
| `STORAGE` | 256 |
| `BED` | 512 |
| `SCREEN` | 1024 |
| `LAMP` | 2048 |
| `PLANT` | 4096 |
| `WALL_ART` | 8192 |
| `GLOBAL_MESH` | 16384 |
| `INVISIBLE_WALL_FACE` | 32768 |
| `UNKNOWN` | 131072 |
| `INNER_WALL_FACE` | 262144 |

Sum of all bits = `458751`. PTRL "scene-model" preset = `458751 & ~16384 = 442367`. Global-mesh preset = `16384`.

## PTRL shader knobs (`TransparentSceneAnchor.mat`)

Useful at runtime via `material.SetFloat(...)`:

| Property | Effect |
|---|---|
| `_HighLightAttenuation` | Falloff curve of point-light highlights — higher = tighter |
| `_HighlightOpacity` | How much the highlights brighten the passthrough |
| Shadow intensity is driven by the URP main light's `shadowStrength` |

The Meta PTRL sample's `DebugPanel.cs` shows how to wire these to wrist-UI sliders.

## Setup recipe via Unity MCP (Unity_RunCommand)

When automating PTRL setup, do it in this order:

1. Locate every `UniversalRendererData` (`*_Renderer.asset`) and set `m_ShadowTransparentReceive = true` via `SerializedObject`. `SaveAssetIfDirty` each one.
2. Locate every `UniversalRenderPipelineAsset` (`*_RPAsset.asset`) and set `m_AdditionalLightsRenderingMode = 1`, `m_AdditionalLightsPerObjectLimit ≥ 4`, `m_ShadowType = 2`, `m_MainLightShadowsSupported = true`, `m_AdditionalLightShadowsSupported = true`. `SaveAssetIfDirty`.
3. Create `EffectMesh` GameObject. Resolve `Meta.XR.MRUtilityKit.EffectMesh` via assembly reflection (`AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(...))`) since `CommandScript` doesn't reference the MRUK assembly directly.
4. Set its serialized properties via `SerializedObject.FindProperty(...).intValue / objectReferenceValue / boolValue` then `ApplyModifiedPropertiesWithoutUndo`.
5. Load `TransparentSceneAnchor.mat` from `Packages/com.meta.xr.mrutilitykit/Core/Materials/`. Assign to `MeshMaterial`.
6. Configure the Directional Light (`shadows = Soft`, tilt). Add child Point Light on the moving NPC for highlights.
7. `MarkSceneDirty` + `SaveScene` in a separate `RunCommand` call.

## Verification

PTRL can be verified **in the editor** by switching MRUK to its prefab-room fallback — you do not need to push to device just to confirm setup. The prefab room generates the same EffectMesh anchors the PTRL shader needs, so shadows and highlights render in the Game view exactly as they will on Quest.

Editor verification (prefab room fallback):

1. Select the `MRUK` GameObject and set `SceneSettings.DataSource = Prefab` (or `DeviceWithPrefabFallback`) and assign one of the bundled rooms from `Packages/com.meta.xr.mrutilitykit/Core/SceneData/Prefabs/` to `RoomPrefabs` (e.g. `RoomPrefab_BedRoom`). Keep `LoadSceneOnStartup = true`.
2. Enter Play mode. MRUK fires `SceneLoadedEvent`, `EffectMesh` spawns, and `SceneNavigation` bakes the navmesh — same code path as on device.
3. Position a virtual object (e.g. the NPC) above the prefab floor. In Game view you should see a soft shadow under it and a highlight halo around any nearby point light. Use `Unity_Camera_Capture` or `Unity_SceneView_Capture2DScene` to grab proof.
4. `Unity_GetConsoleLogs` `logTypes: "Error"` → only the harmless `MRUKEditor.OnInspectorGUI` NRE (it tries to call `GetCurrentRoom()` while no room loaded) should appear. Anything else means something is broken.

Pre-flight checks (asset state, before entering Play mode):

1. Read back `m_ShadowTransparentReceive` from the active build target's Renderer asset — confirm `True`.
2. Confirm the URP Asset linked to the active Quality Level (typically the Mobile one for Android) has `m_AdditionalLightsRenderingMode = 1`.
3. Confirm `EffectMesh.Labels` is nonzero (otherwise no geometry will be generated).

On-device check (final): stand in a scanned room, look at the floor — a virtual object should produce both a soft shadow beneath it AND a highlight halo around any nearby point light. Remember to switch `DataSource` back to `Device` (or `DeviceWithPrefabFallback`) before building.

## Common failure modes — quick lookup

| Symptom | Likely cause |
|---|---|
| Highlights visible, shadows missing on real floor | **Renderer asset `m_ShadowTransparentReceive = false`** — the #1 silent failure |
| Shadows visible, no highlights | URP Asset `m_AdditionalLightsRenderingMode != PerPixel`, or per-object limit too low |
| Neither shadows nor highlights, only opaque virtual content | EffectMesh missing, or `Labels = 0`, or material not set, or `MeshMaterial` is on a different shader |
| Passthrough hidden by black | `OVRPassthroughLayer.overlayType = Overlay` (should be Underlay), or camera background alpha ≠ 0 |
| Everything works in editor but not on Quest | Configured the **PC** renderer/URP asset but not the **Mobile** one |
| Shadow flickers / acne | Directional light `shadowBias`/`shadowNormalBias` too low for the scene scale |
| `MRUKEditor.OnInspectorGUI` NRE in console | Harmless — fires only when MRUK is selected in Inspector with no current room. Ignore. |

## Minimum working scene (verified)

```
Directional Light          shadows = Soft, strength 0.85, tilted ~55°
OVRCameraRig               OVRManager.isInsightPassthroughEnabled = true
  ├ OVRPassthroughLayer    overlayType = Underlay
  └ TrackingSpace/CenterEyeAnchor  Camera clearFlags = SolidColor, bg alpha = 0
MRUK                       SceneSettings.LoadSceneOnStartup = true, DataSource = Device
EffectMesh                 MeshMaterial = TransparentSceneAnchor.mat
                           Labels = 442367 (all except GLOBAL_MESH)
                           SpawnOnStart = CurrentRoomOnly, castShadows = off
NPC                        Renderer.shadowCastingMode = On
  └ HighlightLight         Point, no shadows, warm color, range ~2.5m
```

Plus, for every URP Asset + Renderer asset in `Assets/Settings/`:
- Renderer asset: `m_ShadowTransparentReceive = true`
- URP Asset: `m_AdditionalLightsRenderingMode = 1`, `m_ShadowType = 2`, main + additional shadows supported

## Reference

- SDK material: `Packages/com.meta.xr.mrutilitykit/Core/Materials/TransparentSceneAnchor.mat`
- SDK shader: `Packages/com.meta.xr.mrutilitykit/Core/Shaders/HighlightsAndShadows.shader`
- SDK source: `Library/PackageCache/com.meta.xr.mrutilitykit@*/Core/Scripts/EffectMesh.cs`
- Meta sample: `Assets/MRUKSamples/PassthroughRelighting/PassthroughRelighting.unity` (Unity-MRUtilityKitSample repo on GitHub)
- Public docs (via `metavr docs fetch "documentation/unity/unity-passthrough-relighting.md"`): `https://developers.meta.com/horizon/llmstxt/documentation/unity/unity-passthrough-relighting.md`
- Sample overview: `https://developers.meta.com/horizon/llmstxt/documentation/unity/unity-sample-mruk-passthrough-relighting.md`
- Related skill: [[mruk-scene-navigation]] (uses the same SceneLabels bitmask)
