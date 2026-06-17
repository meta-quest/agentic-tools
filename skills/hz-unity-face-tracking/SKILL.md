---
name: hz-unity-face-tracking
license: Apache-2.0
description: Drive ARKit-blendshape-rigged head/face models in Unity with the wearer's facial expressions on Meta Quest via Meta Movement SDK (face tracking + A2E). Use when a user has an FBX with the 52 ARKit blendshapes (any prefix, _L/_R suffixes) and wants it to animate from face tracking on Quest Pro / Quest 3 / Quest 3S.
---

# Unity Face Tracking for ARKit-Rigged Models (Meta Movement SDK)

End-to-end recipe to make a head/face model rigged with the standard 52 ARKit blendshapes animate from the wearer's face on Quest. Uses the **public** `OVRCustomFace` extension hook — no `OVR_INTERNAL_CODE`, ships to 3P.

The model's blendshape names must follow the ARKit naming convention (camelCase, `_L`/`_R` suffixes, e.g. `eyeBlink_L`, `jawOpen`, `mouthSmile_R`). An optional prefix like `blendShape2.eyeBlink_L` is automatically stripped.

## When to use

- User has an FBX/GLB/mesh with ARKit-named blendshapes and wants it driven by Quest face tracking.
- User asks: "animate this head with my face", "drive these blendshapes from face tracking", "use Movement SDK A2E with my model", "wire ARKit shapes to Quest".
- Target device: Quest Pro, Quest 3, Quest 3S (Quest 2 is no-op — no face cameras).

## Prerequisites checklist

1. **Quest face tracking-capable headset** (Pro / 3 / 3S).
2. **Packages** in `Packages/manifest.json`:
   - `com.meta.xr.sdk.core` (Meta XR Core — provides `OVRFaceExpressions`, `OVRCustomFace`)
   - `com.meta.xr.sdk.movement` (Meta Movement SDK — A2E + retargeting helpers)
3. **`Assets/Oculus/OculusProjectConfig.asset`** (verify via Project Settings → Meta XR):
   - `faceTrackingSupport: 1` (Supported) or `2` (Required)
   - `eyeTrackingSupport: 1` if the rig has gaze
4. **Android manifest** (`Assets/Plugins/Android/AndroidManifest.xml`) permissions:
   - `<uses-feature android:name="oculus.software.face_tracking" android:required="false" />`
   - `<uses-permission android:name="com.oculus.permission.FACE_TRACKING" />`
   - `<uses-permission android:name="android.permission.RECORD_AUDIO" />` (required for A2E)
   - For eye gaze: `oculus.software.eye_tracking` + `com.oculus.permission.EYE_TRACKING`
5. **OVRCameraRig** in the scene with `OVRManager.FaceTrackingDataSources` including `Audio` (A2E) — if you skip Audio, mouth motion is visual-only.

After any change to OculusProjectConfig, call `meta_update_android_manifest` to regenerate the manifest.

## Approach (high level)

1. Drop the **`ARKitOVRCustomFace`** script (in `references/ARKitOVRCustomFace.cs`) into the project.
2. Add an `OVRFaceExpressions` component to the OVRCameraRig (or anywhere in the scene).
3. On the GameObject that has the model's `SkinnedMeshRenderer`, add `ARKitOVRCustomFace`. Adding the component triggers `Reset()`, which auto-populates the blendshape→FaceExpression mapping by scanning the mesh's blendshape names.
4. Wire the component's `FaceExpressions` field to the `OVRFaceExpressions` instance.
5. For eye gaze: drop **`ARKitEyeGazeBlendshapeDriver`** in (see [Eye tracking](#eye-tracking-do-this--face-expressions-alone-dont-work) below).
6. Press Play with Meta XR Link, or build APK and side-load. On first launch, accept the Face Tracking / Eye Tracking / Microphone permission prompts.

That's the whole flow. Details below.

## Eye tracking (DO THIS — face expressions alone don't work)

**Don't rely on `OVRFaceExpressions.EyesLook*` for eye gaze.** Those fields are derived from face-camera visuals, not the dedicated eye tracker. On Quest Pro they're often zero or noisy even when face tracking is otherwise working. The right API is `OVREyeGaze`, which taps the eye tracker directly.

For ARKit rigs with `eyeLook*` blendshapes (no eye bones), this skill ships **`ARKitEyeGazeBlendshapeDriver`**:
- Reads gaze rotation from two `OVREyeGaze` components (one per eye, `TrackingMode = HeadSpace`).
- Computes rotation relative to a head reference (e.g. `CenterEyeAnchor`).
- Decomposes pitch → `eyeLookUp/Down_{L,R}`, yaw → `eyeLookIn/Out_{L,R}` (with the ARKit "_In = toward nose" convention).
- Writes weights in `LateUpdate`, so it overrides whatever `ARKitOVRCustomFace` wrote in `Update`.

For rigs with **eye bones** (no `eyeLook*` blendshapes), skip the blendshape decomposition and just parent `OVREyeGaze` to each eye bone with `ApplyRotation = true` — the component will rotate the bone directly.

### Eye gaze setup (blendshape rigs)

1. Create two empty GameObjects under `CenterEyeAnchor` (or your head transform): `LeftEyeGaze`, `RightEyeGaze`.
2. Add `OVREyeGaze` to each. Set `Eye = Left` / `Right`, `TrackingMode = HeadSpace`, `ApplyPosition = false`, `ApplyRotation = true`, `ConfidenceThreshold = 0.5`.
3. Add `ARKitEyeGazeBlendshapeDriver` to the head's SkinnedMeshRenderer GameObject (alongside `ARKitOVRCustomFace`). Wire `leftEye`, `rightEye`, and `referenceFrame` (= `CenterEyeAnchor`).
4. Tweak `maxAngleDeg` (default 30°) and `smoothing` (default 0.4) to taste.

## Step-by-step

### 1. Install the script

Copy `references/ARKitOVRCustomFace.cs` into `Assets/Scripts/ARKitOVRCustomFace.cs`. It defines the public, 3P-shippable ARKit ↔ OVR FaceExpression table and a `MapBlendshapes()` method that scans `SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i)`, strips any prefix before the last `.`, lowercases, and matches against the table. Unmatched mesh blendshapes are set to `OVRFaceExpressions.FaceExpression.Max` (sentinel — skipped at runtime).

### 2. Enable Movement SDK + permissions

Project settings:
```
Project Settings → Meta XR → Face Tracking Support = Supported
Project Settings → Meta XR → Eye Tracking Support = Supported (if needed)
```

Then regenerate manifest:
- Unity MCP: `meta_update_android_manifest`
- Or Editor menu: **Meta → Tools → Update AndroidManifest.xml**

### 3. Scene setup

```
Scene Hierarchy
├── OVRCameraRig                         (from meta_add_camerarig)
│   └── (add) OVRFaceExpressions         component
└── YourHeadModel
    └── ...SkinnedMeshRenderer GO...
        ├── SkinnedMeshRenderer          (existing)
        └── (add) ARKitOVRCustomFace     component
             └── FaceExpressions = the OVRFaceExpressions ref
             └── retargetingType = Custom (set automatically by base when overriding)
             └── Mappings[] = auto-filled on Reset()
             └── BlendShapeStrengthMultiplier = 100 (default; OVR weights are 0–1, mesh wants 0–100)
```

If the head has multiple `SkinnedMeshRenderer`s (e.g. separate teeth/tongue meshes), add `ARKitOVRCustomFace` to each one.

### 4. OVRManager — enable Audio as a data source (A2E)

On `OVRCameraRig`'s OVRManager component:
- **Face Tracking Data Sources** → check **Visual** AND **Audio** (Audio = A2E; produces mouth shapes from microphone when the visual face cameras can't see something — talking, occlusion, etc.).

### 5. Trigger the mapping if the component already existed

`MapBlendshapes()` runs automatically when the component is added (`Reset()`) and from `OnValidate()` when `Mappings` is empty. If you need to remap manually (e.g. after re-importing the FBX), use the component's inspector context menu → **Map Blendshapes**, or from a script:

```csharp
go.GetComponent<ARKitOVRCustomFace>().MapBlendshapes();
```

From Unity MCP `Unity_RunCommand`, the cleanest invocation (the OVR/MSDK types aren't referenced in the MCP dynamic assembly, so use SendMessage to avoid reflection):

```csharp
GameObject.Find("YourHeadModel")
    .GetComponentInChildren<SkinnedMeshRenderer>().gameObject
    .SendMessage("MapBlendshapes", SendMessageOptions.RequireReceiver);
```

### 6. Verify

- **In Editor**, with Meta XR Link / Quest Link, enter Play mode and make faces. The model should mirror them.
- **On device**, build APK, side-load, grant Face Tracking + Microphone permissions on first launch.
- **Check the mapping** at edit time: inspect the `ARKitOVRCustomFace` component. `Mappings.Length` should equal `SkinnedMeshRenderer.sharedMesh.blendShapeCount`. The Console log from `MapBlendshapes()` reports `mapped X/N blendshapes` — X should be 50 (or 52 if the FBX has all of them).

## Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `mapped 0/N` in Console | Mesh blendshape names don't follow ARKit convention. Verify with `mesh.GetBlendShapeName(i)`. Names must match e.g. `eyeBlink_L`, `jawOpen` — case-insensitive, prefix before last `.` is stripped. |
| Face is frozen | `OVRFaceExpressions` not assigned, or scene has no `OVRCameraRig`/`OVRManager` with face tracking enabled. Check `OVRFaceExpressions.FaceTrackingEnabled` and `ValidExpressions` at runtime. |
| Mouth doesn't move when speaking | A2E disabled. Enable **Audio** under `OVRManager → Face Tracking Data Sources`, and ensure `RECORD_AUDIO` permission is granted on device. |
| Eyes don't blink | Mesh's eyelid shapes aren't named `eyeBlink_L`/`_R`. Either rename, or add a custom row to `ARKitTable`. |
| Eyes don't move (look around) | You're relying on `OVRFaceExpressions.EyesLook*` instead of `OVREyeGaze` — switch to `ARKitEyeGazeBlendshapeDriver`. |
| Eye gaze in wrong direction | Note the deliberate ARKit ↔ OVR swap: ARKit's `eyeLookIn_L` (eye looking nose-ward, i.e. right) maps to OVR `EyesLookRightL`. The table already does this — don't "fix" it. |
| Weights look half-strength or clamped | `BlendShapeStrengthMultiplier` defaults to 100 because Unity blendshapes are 0–100 while OVR is 0–1. Don't lower this unless intentional. |
| `cannot change access modifiers when overriding` compile error | Base method is `protected internal` in another assembly. The override must use `protected` (not `protected internal`) — already correct in the supplied script. |
| `_mappings out of sync with shared mesh` assertion at Start | Mesh changed since mapping was generated. Re-run `MapBlendshapes()` via the component context menu. |

## ARKit ↔ OVR FaceExpression mapping (reference)

The 52 ARKit shapes don't 1:1 a FACS-based OVR enum. Notable choices baked into `ARKitTable`:

- **Centrally-named ARKit shapes that have L+R OVR pairs** (`browInnerUp`, `cheekPuff`, `mouthFunnel`, `mouthPucker`, `mouthRollLower`, `mouthRollUpper`) pick **only the L-side** OVR expression. If your model has obviously asymmetric mouth/brow when the user uses these expressions and you want true symmetric drive, subclass and additively sum L+R in a custom `MapBlendshapes` (use `SlothARKitFaceDriver`-style per-blendshape sum). For most heads the L-only choice is fine because the face is roughly symmetric and the asymmetry is below visual threshold.
- **`eyeLookIn/Out`** are deliberately swapped per side relative to OVR's left/right semantics (see Troubleshooting).
- **`mouthClose` → `LipsToward`** (closest FACS analogue).
- **`tongueOut` → `TongueOut`** (requires HorizonOS ≥ 65 + tongue tracking; otherwise stays at 0).

## Files in this skill

- `SKILL.md` — this file
- `references/ARKitOVRCustomFace.cs` — drop-in `OVRCustomFace` subclass
- `references/ARKitEyeGazeBlendshapeDriver.cs` — OVREyeGaze → `eyeLook*` blendshape driver (LateUpdate)

## Why this approach (vs. alternatives)

- **Custom `MonoBehaviour` driver** that reads `OVRFaceExpressions[expr]` and writes blendshape weights directly: works, but doesn't integrate with MSDK's correctives, eye constraints, or future retargeting upgrades. Use only if you can't extend `OVRCustomFace`.
- **`OVRCustomFace` + `RetargetingType.ARKitBlendshapes`**: clean, but guarded behind `#if OVR_INTERNAL_CODE` in the public Oculus Integration package — **not 3P-shippable**.
- **`OVRCustomFace` + `RetargetingType.Custom` + `GetCustomBlendShapeNameAndExpressionPairs` override** (this skill): fully public API, shippable, and gets all base-class behavior (data validity gating, weight scaling, mesh assertion).
