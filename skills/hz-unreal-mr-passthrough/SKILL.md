---
name: hz-unreal-mr-passthrough
license: Apache-2.0
description: Guides building mixed reality experiences in Unreal Engine 5 for Meta Quest and Horizon OS, covering Persistent Passthrough via the OculusXR Passthrough Subsystem, passthrough styling, room tracking and object placement via MR Utility Kit (MRUK), Spatial Anchors persistence, and MR Occlusions/Depth API. Use when adding passthrough, room/scene tracking, object placement, spatial anchors, or occlusion to a UE5 Meta Quest project.
---

# UE5 Mixed Reality / Passthrough for Meta Quest / Horizon OS

Guide for building MR features in UE5 targeting Meta Quest.

## 1. Persistent passthrough

- Use **Persistent Passthrough**, accessed through the **OculusXR Passthrough Subsystem** (a Game Instance Subsystem) — this is Meta's recommended approach, remaining active across level loads.
- **The Passthrough Layer Component is deprecated** — do not use it in new work; migrate existing usages to Persistent Passthrough. Note: Meta's Unreal docs currently give two conflicting anchors for exactly when — the Persistent Passthrough page says "v74"; the deprecated-node Blueprint reference page says "Unreal Engine 5.6." Regardless of which is correct, new work should use Persistent Passthrough via the OculusXR Passthrough Subsystem.
- Prerequisite: `Edit > Project Settings > Plugins > Meta XR > Mobile` → enable **Passthrough Enabled**. Under **General**, set **XR API** to **Epic Native OpenXR (Recommended)**.
- Blueprint setup: right-click in the Event Graph → **Get Oculus XR Passthrough Subsystem** → call **Initialize Persistent Passthrough** to create the layer. These nodes work in any Event Graph (Level Blueprints or Actor Blueprints).
- Configure the layer in the **Details** tab of the **Initialize Persistent Passthrough** node, or enable **Show Input Pin** to expose a **Parameters** pin you can wire up dynamically (e.g. per-Actor instance config).
- Fetch the active layer later with **Get Persistent Passthrough**; destroy it anytime with **Destroy Persistent Passthrough**. Calling **Initialize Persistent Passthrough** again overrides the current layer's parameters.
- **Layer Placement: set to Underlay.** Overlay is deprecated and does not work over Meta Quest Link.
- Passthrough is asynchronous — content may not be ready for several frames after layer creation, causing a brief black-frame gap. Wait for the **LayerResumed** event before assuming passthrough content is visible.
- Respect the user's system-level MR/VR preference: query `UOculusXRFunctionLibrary::IsPassthroughRecommended()` (C++ or Blueprint) and default your app's mode to it rather than hardcoding VR or MR.

## 2. MR / transparent-background rendering

- For MR apps, the camera background must render **transparent** so the composited passthrough video shows through instead of a skybox/solid clear color.
- Confirm the project's rendering pipeline is configured for transparent background compositing (not just "passthrough enabled" — a non-transparent clear will occlude it).
- Verify this visually on-device; a black or opaque background where passthrough should show is the most common integration bug.

## 3. Passthrough styling

- Passthrough supports styling (color mapping / brightness-contrast-saturation adjustments, edge rendering) via the subsystem's styling API.
- Use styling deliberately for UX purposes (e.g. dimming passthrough behind virtual UI panels) rather than leaving defaults if the app's visual design calls for it.

## 4. Room tracking and object placement — MR Utility Kit (MRUK)

**MRUK is the current API for room/scene understanding and content placement.** The older `OculusXRSceneActor`-based scene-mesh workflow is **deprecated**; new features ship through MRUK only. Use MRUK for all new projects — see [Scene-to-MRUK migration guide](https://developers.meta.com/horizon/documentation/unreal/unreal-scene-migrate-mruk) if migrating an existing project.

### Loading the room

- `LoadSceneFromDeviceAsync` — async Blueprint node, loads live Scene data captured on-device via Space Setup.
- `LoadSceneFromJsonAsync` — loads previously-saved Scene data from a JSON string (via `SaveSceneToJsonString`). Useful for iterating in-editor without a headset.
- On success, an `AMRUKRoom` actor is spawned per room (usually one), containing an `AMRUKAnchor` actor per wall/floor/ceiling/object. Access via `GetCurrentRoom`/`Rooms` on `MRUKSubsystem`, and per-anchor lists `AllAnchors`, `WallAnchors`, `FloorAnchors`, `CeilingAnchors`, or `GetAnchorsByLabel()` (labels like `WALL`, `COUCH`, `BED`).
- Handle the case where device Scene data isn't available (user hasn't run Space Setup) gracefully — e.g. fall back to `LoadSceneFromJsonAsync` with a bundled sample room, or prompt the user.

### World Locking

- **Enabled by default.** Keeps virtual content in sync with the real room by making small, imperceptible adjustments to the camera rig's tracking space — content near the player stays accurate; content far away may drift slightly.
- This replaces the old requirement (from the `OculusXRSceneActor` workflow) of attaching every piece of content to an anchor just to stay in sync — with World Locking, static content can simply stay static, avoiding the physics/networking/rendering complications of per-frame anchor-attached movement.
- Disable via `Project Settings > Plugins > Meta XR > MR Utility Kit > Enable World Lock`, only if you have a specific reason not to use it.

### Placing objects in the room

- **`AMRUKAnchorActorSpawner`** — drop-in actor that spawns a Blueprint class at each anchor's location, scaled to fit. Configure per semantic-label `Spawn Groups` (e.g. spawn a virtual couch mesh at every `COUCH` anchor); pick `Random` (optionally seeded for deterministic/multiplayer-consistent results) or `Closest Size` selection; `Match Aspect Ratio` and `Calculate Facing Direction` for volumes; `Scaling Mode` (`Stretch`, `UniformScaling`, `UniformXYScale`, `NoScaling`). Falls back to a procedural mesh (with assignable material) if no actor is configured for a label.
- **Raycast-based placement:** `Raycast()`/`RaycastAll()` against scene anchors (independent of Unreal physics), then `GetBestPoseFromRaycast()` for a suggested placement pose — the standard "point controller at a surface, place object here" pattern. Requires the user to have completed Space Setup.
- **Environment Raycasting** — dynamic alternative using live Depth data instead of captured Scene data, so no Space Setup required. Requires `com.oculus.permission.USE_SCENE`. Start with `UMRUKSubsystem::CreateEnvironmentRaycaster()`, cast with `RaycastEnvironment()` (may return `Failure` for the first few frames while it initializes — check `EnvironmentRaycasterStatus()`), stop with `DestroyEnvironmentRaycaster()`.
- Other placement helpers on `AMRUKRoom`/`MRUKSubsystem`: `IsPositionInRoom()`, `IsPositionInSceneVolume()`, `TryGetClosestSurfacePosition()`, `TryGetClosestSeatPose()`, `GetLargestSurface()`, `GetKeyWall()` (longest wall with nothing behind it), `RoomBounds`, `ParentAnchor`/`ChildAnchors`, `GenerateRandomPositionInRoom()` (optionally avoiding other anchor volumes and specifying minimum surface distance).
- **`UMRUKDebugComponent`** — add to your Pawn to visualize anchors at runtime (label, scale, collision point, suggested placement pose) via `ShowAnchorAtRayHit`/`HideAnchor`. Useful during development.
- Other MRUK tools worth knowing about: `AMRUKGuardianSpawner` (Guardian-like protective mesh for fully-virtual scenes), `UMRUKBlobShadowComponent` (cheap blob shadows for spawned content), `AMRUKDistanceMapGenerator` (distance-to-scene-geometry texture for VFX/materials), `AMRUKDestructibleGlobalMeshSpawner` (breakable global mesh).
- Reference sample: [Unreal-MRUtilityKitSample on GitHub](https://github.com/oculus-samples/Unreal-MRUtilityKitSample) — ~30 pre-captured rooms included for in-editor iteration.

## 5. Spatial Anchors — persisting placed content across sessions

Use Spatial Anchors when placed content must reappear in the exact same real-world spot the next time the app runs.

- Project setup: `Edit > Project Settings > Plugins > Meta XR > Mobile` → enable **Passthrough Enabled** and **Anchor Support**.
- All operations are **asynchronous** — Blueprint latent nodes with Success/Failure execution pins, or C++ delegates. No mandatory manager/component needs to be added to the scene to use them.

| Blueprint latent action | Purpose |
|---|---|
| `OculusXR Async Create Spatial Anchor` | Create an anchor at a transform, attached to an actor (auto-creates the anchor component if missing) |
| `OculusXR Async Save Anchor` / `Async Save Anchors` | Persist one or many anchors to `Local` or `Cloud` storage |
| `OculusXR Async Query Anchors` | Look up anchors by a list of UUIDs from `Local` or `Cloud`; use `Spawn Oculus Anchor Actor From Query` to recreate actors from results |
| `OculusXR Async Erase Anchor` | Delete a **locally-saved** anchor (cloud anchors are rejected); also usable as an "unsave" |

- **Locally-saved anchors persist indefinitely; cloud-saved anchors are not persistent indefinitely.**
- Save anchor UUIDs to an app-specific persistent save game to reload the same placement next session — don't assume anchors persist across different physical spaces or different user accounts.
- `DiscoverAnchors` is a newer streaming-results API for finding anchors; `QueryAnchors` remains available for UUID-based lookups.
- If migrating a legacy (pre-Oculus-SDK-v43) project: old event-listener-bound calls map directly to the latent actions above (e.g. legacy "Create Spatial Anchor" → `OculusXR Async Create Spatial Anchor`).
- Common failure codes to handle: `Failure_SpaceMappingInsufficient` (user didn't move around enough — prompt them to walk/look around the space before retrying save), `Failure_SpaceLocalizationFailed`, `Failure_SpaceTooDark`/`Failure_SpaceTooBright`, `Failure_SpaceCloudStorageDisabled` (user hasn't permitted cloud storage).

## 6. MR Occlusions / Depth API

- **Soft occlusions** (accurate, depth-based occlusion of virtual content behind real-world geometry with proper edges) require the **Oculus-VR fork** — it includes shader changes not present in Epic's standard engine.
- **Epic UE5 + Meta XR plugin (Option B)** only supports **hard occlusion**, and **hard occlusions are deprecated as of Meta XR Plugin for Unreal Engine 5.5** — its `EOculusXROcclusionsMode` enum value was renamed to `HardOcclusions_Deprecated`. Use soft occlusions for new projects.
- If soft occlusion is a hard requirement for the app's visual quality bar, this constrains your engine choice back at project-setup time — see `hz-unreal-project-setup`.
- Depth API requires **Meta Quest 3 / 3S** hardware and **Passthrough** enabled first; minimum effective sensing range is ~0.2m (closer objects get unreliable depth).

### Project settings

- `Edit > Project Settings > Meta XR > Mobile` → check **Scene Support**.
- Soft occlusions only: `Edit > Project Settings > Engine > Rendering > VR` → check **Support XR Soft Occlusions**. This significantly increases shader permutation compile time — only enable it if soft occlusions are actually used at runtime.
- Scene permission (`com.oculus.permission.USE_SCENE`) is requested automatically on first `StartEnvironmentDepth` call; use the `Request Android Permissions` Blueprint node for explicit timing control instead.

### Blueprint implementation (`UOculusXRFunctionLibrary`)

| Node | Type | Purpose |
|---|---|---|
| `StartEnvironmentDepth` | Callable | Begin capturing depth into a swap chain (e.g. on BeginPlay) |
| `StopEnvironmentDepth` | Callable | Stop capture, release resources when no longer needed |
| `IsEnvironmentDepthStarted` | Pure | Query current capture state |
| `SetXROcclusionsMode` | Callable | Set `EOculusXROcclusionsMode`: `Disabled` (0), `HardOcclusions_Deprecated` (1), `SoftOcclusions` (2) |
| `SetEnvironmentDepthHandRemoval` | Callable | Toggle hand removal from depth textures |

- The Passthrough layer must be configured as **Underlay** and **Reconstructed** for occlusion to work correctly.
- Soft occlusions inject pixel-shader instructions into every material by default (comparing environment depth to the rendered pixel's depth). Exempt materials that should never be occluded (e.g. UI) by unchecking **XR Soft Occlusions** in that material's Details panel.
- **Depth bias:** thin virtual objects placed against a real wall/floor can z-fight/flicker. Fix via the **XR Soft Occlusions Depth Bias** field on the material (Material Editor → Details → Material section). Small positive values (e.g. `0.06`) work well: `biased linear depth = linear depth − linear depth × bias`.
- Reference sample (Blueprint-only, demonstrates disabled/hard/soft modes + hand removal): [Unreal-OcclusionSample on GitHub](https://github.com/oculus-samples/Unreal-OcclusionSample).

## 7. MR health & safety

- MR apps that mix real and virtual content have specific **health & safety** obligations: never obscure real-world hazards behind virtual content in a way that could cause a user to trip, collide with furniture/walls, or lose track of their real surroundings.
- Passthrough should remain legible (not overly stylized to the point of obscuring real obstacles) in any state where the user is expected to move physically.
- Test with actual room boundaries and real furniture, not just an empty test space.

## 8. Verify on device

MR/passthrough behavior cannot be meaningfully evaluated in desktop PIE — verify on hardware:

```bash
metavr device list          # confirm headset connected
metavr app install <path-to-apk>
metavr capture perfetto     # or the project's equivalent capture command, to confirm frame cost of passthrough compositing/occlusion
```

Visually confirm: passthrough shows correctly (no black background), styling applied as intended, room/anchors load correctly against the real room, placed objects persist across an app restart, and occlusion behaves as expected against real furniture.

## Checklist

- [ ] Using Persistent Passthrough (OculusXR Passthrough Subsystem), not the deprecated Passthrough Layer Component (exact deprecation version disputed between sources — see Last verified note; use Persistent Passthrough regardless)
- [ ] Passthrough Layer Placement set to Underlay (not the deprecated Overlay)
- [ ] Background rendering confirmed transparent on-device (not black/opaque)
- [ ] Passthrough styling applied deliberately per UX design, if used
- [ ] App defaults to MR/VR based on `IsPassthroughRecommended()` rather than a hardcoded mode
- [ ] Using MRUK for room tracking/placement, not the deprecated `OculusXRSceneActor` scene-mesh workflow (new projects)
- [ ] Scene-load failure (no Space Setup data) handled gracefully (e.g. JSON fallback room or prompt)
- [ ] World Locking left enabled unless there's a specific reason to disable it
- [ ] Object placement uses raycast + `GetBestPoseFromRaycast()` or `AMRUKAnchorActorSpawner`, not hardcoded/guessed positions
- [ ] Spatial Anchors used where content must persist in the same real-world spot across sessions; UUIDs saved to a persistent save game
- [ ] Spatial Anchor failure codes handled (e.g. prompt user to walk around on `Failure_SpaceMappingInsufficient`)
- [ ] Occlusion approach (soft vs hard) matches engine choice (fork vs Epic+plugin); hard occlusions avoided for new work (deprecated as of Meta XR Plugin for Unreal Engine 5.5)
- [ ] Scene Support enabled (Meta XR Mobile settings); Support XR Soft Occlusions enabled only if soft occlusions are actually used
- [ ] Passthrough layer configured as Underlay + Reconstructed before enabling occlusion
- [ ] Materials that shouldn't be occluded (e.g. UI) have XR Soft Occlusions disabled; depth bias applied where thin objects z-fight against real surfaces
- [ ] MR health & safety reviewed: no virtual content obscures real hazards during expected movement
- [ ] Verified on real device with real room geometry, not just desktop PIE / empty test space

## Last verified

Checked directly against live developers.meta.com pages (via metavr docs tools), not from model training data.

| Claim | Source | Updated |
|---|---|---|
| Passthrough Layer Component deprecated ("v74") | [Persistent Passthrough](https://developers.meta.com/horizon/documentation/unreal/unreal-persistent-passthrough/) | 2026-04-15 |
| Passthrough Layer Component deprecated ("Unreal Engine 5.6") — **conflicts with the above; unresolved in Meta's own docs** | [Add Passthrough Layer Component (Deprecated)](https://developers.meta.com/horizon/documentation/unreal/unreal-blueprints-add-passthrough-layer-component/) | 2026-04-15 |
| Overlay deprecated / doesn't work over Meta Quest Link; use Underlay | [Get Started with Passthrough](https://developers.meta.com/horizon/documentation/unreal/unreal-passthrough-overview-gs/) | 2026-04-15 |
| MRUK is current path; `OculusXRSceneActor` deprecated | [Scene Mesh (deprecated)](https://developers.meta.com/horizon/documentation/unreal/unreal-scene-mesh/) (same notice repeated on 4 other Scene pages) | 2026-04-14 |
| MRUK room/anchor/placement API detail | [Mixed Reality Utility Kit Features](https://developers.meta.com/horizon/documentation/unreal/unreal-mr-utility-kit-features/) | 2026-07-24 |
| Spatial Anchors Blueprint/C++ API, Local vs Cloud persistence | [Use Spatial Anchors](https://developers.meta.com/horizon/documentation/unreal/unreal-local-spatial-anchors/) | 2026-04-14 |
| Hard occlusion deprecated as of Meta XR Plugin for Unreal Engine 5.5; `HardOcclusions_Deprecated` | [Get started with Occlusions](https://developers.meta.com/horizon/documentation/unreal/unreal-depthapi-occlusions-get-started/), [Occlusions Overview](https://developers.meta.com/horizon/documentation/unreal/unreal-depthapi-occlusions/) | 2026-04-14 (both) |

**Open item for PR reviewers:** the Passthrough Layer Component deprecation-version conflict (v74 vs UE 5.6) is unresolved in Meta's live documentation as of this check. Consider filing it as a doc bug with Meta rather than treating either number as settled.
