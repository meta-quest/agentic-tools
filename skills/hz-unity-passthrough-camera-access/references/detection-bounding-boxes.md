# Drawing precise 3D bounding boxes from passthrough-camera detections

Draw precise 3D bounding boxes around real-world objects detected by an ML model
on Meta Quest 3/3S Passthrough Camera. Use this reference when placing 2D ML
detections (YOLO, SSD, DETR, custom CNN) into world space, building a
camera-facing billboard quad from a 2D detection, debugging misaligned detection
boxes, fixing Y-flip / coordinate-space issues, or wiring `PassthroughCameraAccess`
+ `EnvironmentRaycastManager` for object localization. It covers model output
coordinate conventions, the 2D→3D mapping (viewport ray + depth raycast +
camera-facing plane projection), and the World Space Canvas configuration that
makes `sizeDelta` equal meters.

This skill describes the pipeline for turning 2D ML detections into
camera-facing 3D bounding-box quads anchored on real-world geometry on
Meta Quest 3/3S, using `PassthroughCameraAccess` and
`EnvironmentRaycastManager`.

## Step 0: identify the model output coordinate space

Before mapping anything, pin down exactly what your model emits. The three
common variants and how to normalize each into the form this pipeline expects:

| Output convention | How to normalize |
| --- | --- |
| Corners `(x1, y1, x2, y2)` in **model-input pixel space**, Y-down (most YOLO/SSD post-processed graphs) | Divide by `(modelInputW, modelInputH)`. Then Y-flip. |
| Center+size `(cx, cy, w, h)` in model-input pixel space, Y-down | Convert to corners first (`x1=cx-w/2`, etc.), then as above. Many ONNX exporters emit this — you can either bake the conversion into the model graph or do it in C#. |
| Already normalized `[0,1]` in image space, Y-down | Skip the division. Still Y-flip. |

If the runtime expects normalized rects in viewport space (origin
bottom-left, Y up), you must:

1. Normalize to `[0,1]` in image space (Y-down).
2. Flip Y: `viewportY = 1 - imageY`.

Common mistakes here:

- Treating raw model outputs as if they were `[0,1]` when they're in
  pixel space (visible as boxes clustered near a corner of the image).
- Forgetting to Y-flip (boxes mirrored vertically).
- Reading model input dims as `(H, W)` then using them as `(W, H)`. For NCHW
  tensors `(1, 3, H, W)`, dim 2 is `H` and dim 3 is `W`. Square inputs hide
  this bug; non-square inputs make it visible.

## Step 1: capture frame + pose atomically

Inference is async and can take many frames. The head moves in the meantime.
The camera pose used for ray-casting must be the pose at the **moment the
camera image was captured**, not the current head pose.

```csharp
// Capture pose synchronously with the texture you feed to the model.
var cachedCameraPose = passthroughCameraAccess.GetCameraPose();
var inputTexture     = passthroughCameraAccess.GetTexture();

// Schedule inference, await results...
yield return RunInferenceAsync(inputTexture);

// Pass cachedCameraPose — NOT a fresh GetCameraPose() — to the placement code.
PlaceBoxes(detections, cachedCameraPose);
```

Additionally, gate inference on the head pose being reliable at capture
time. If `OVRPlugin`'s `ovrp_GetNodePoseStateAtTime` for `Node.Head` does
not return success at the camera timestamp, skip the frame —
`GetCameraPose()` is unreliable in that state.

## Step 2: the 2D → 3D mapping algorithm

Per detection (with `boundingBox = (x1, y1, x2, y2)` already normalized to
`[0,1]` in image space, Y-down, and `cameraPose` from Step 1):

```csharp
// 1. Build a Rect, then Y-flip into viewport space (origin bottom-left, Y up).
var imgRect = new Rect(x1, y1, x2 - x1, y2 - y1);
var viewportRect = new Rect(
    imgRect.x,
    1f - imgRect.yMax,    // Y-flip
    imgRect.width,
    imgRect.height);

// 2. Find the box's depth via an environment raycast through the center.
var centerRay = passthroughCameraAccess.ViewportPointToRay(viewportRect.center, cameraPose);
if (!environmentRaycastManager.Raycast(centerRay, out var hit))
    return; // no depth hit — see Pitfalls for fallback options
var distance = Vector3.Distance(cameraPose.position, hit.point);

// 3. Define a plane perpendicular to the camera-to-center axis at that depth.
//    Recompute the world center from the *same* viewportRect.center so the
//    corner rays land symmetrically on the plane.
var worldCenter = centerRay.GetPoint(distance);
var normal      = (worldCenter - cameraPose.position).normalized;
var plane       = new Plane(normal, worldCenter);

// 4. Project the 2D corners onto that plane to get world-space corners.
var minRay = passthroughCameraAccess.ViewportPointToRay(viewportRect.min, cameraPose);
var maxRay = passthroughCameraAccess.ViewportPointToRay(viewportRect.max, cameraPose);
plane.Raycast(minRay, out float dMin);
plane.Raycast(maxRay, out float dMax);
var worldMin = minRay.GetPoint(dMin);
var worldMax = maxRay.GetPoint(dMax);

// 5. Convert world corners into a 2D size in meters via camera-local space.
var invRot      = Quaternion.Inverse(cameraPose.rotation);
var minLocal    = invRot * (worldMin - cameraPose.position);
var maxLocal    = invRot * (worldMax - cameraPose.position);
var sizeMeters  = new Vector2(Mathf.Abs(maxLocal.x - minLocal.x),
                              Mathf.Abs(maxLocal.y - minLocal.y));

// 6. Place a World Space Canvas RectTransform at the world center, facing the camera.
boxRect.SetPositionAndRotation(worldCenter, Quaternion.LookRotation(normal));
boxRect.sizeDelta = sizeMeters;   // see Step 3 for why this equals meters
```

The result is a flat quad lying on the plane perpendicular to the line
from the camera to the object's center, sized so its edges line up with
the model's 2D detection rectangle when viewed from the original camera
pose. This is a *billboard*, not a true 3D OBB — it does not capture
extent along the view axis. For typical 2D detectors that's the right
trade-off.

## Step 3: World Space Canvas configuration that makes `sizeDelta` equal meters

For `boxRect.sizeDelta = sizeMeters` to actually produce a quad that size
in world units, the canvas must be configured so that 1 UI unit = 1 world
unit (1 meter):

- `Canvas.renderMode = RenderMode.WorldSpace`
- `Canvas.worldCamera = null` (no override needed)
- `CanvasScaler.referencePixelsPerUnit = 1`
- `CanvasScaler.dynamicPixelsPerUnit = 1`
- `CanvasScaler.uiScaleMode = ConstantPixelSize` (or any mode where the
  scaler does not introduce a multiplier)
- The Canvas's `RectTransform.localScale = (1, 1, 1)` and no ancestor
  introduces a non-unit scale.

If any of these change, `sizeDelta` no longer equals meters and quads will
be the wrong physical size — usually visibly tiny (default
`dynamicPixelsPerUnit = 100` makes them 100× too small).

The box prefab itself is typically an `Image` for the outline/fill plus a
child `Text`/`TMP_Text` for the label, parented under this canvas.

## Step 4: anchor the canvas in world space

Inference results lag the user's head, and OVR's tracking origin can
re-localize subtly during a session. Without anchoring, boxes will drift
relative to real-world objects.

Parent the canvas under an `OVRSpatialAnchor`:

```csharp
var anchor = canvas.gameObject.AddComponent<OVRSpatialAnchor>();
// ... wait for anchor.Localized, then optionally SaveAnchorAsync ...
```

Skip placement on frames where `anchor == null || !anchor.IsTracked` —
positions computed against an untracked anchor will be visibly wrong.

## Prerequisites checklist

All of these must hold each frame for placement to be valid:

- `PassthroughCameraAccess.IsPlaying == true`.
- Headset pose at the camera timestamp is reliable
  (`ovrp_GetNodePoseStateAtTime(captureTime, Node.Head)` succeeds).
- `EnvironmentRaycastManager.IsSupported`.
- The `OVRSpatialAnchor` parent of the canvas exists and is tracked.

Permissions:

- `horizonos.permission.HEADSET_CAMERA` — Passthrough Camera Access.
- `com.oculus.permission.USE_SCENE` — depth API used by
  `EnvironmentRaycastManager`.

Hardware:

- Meta Quest 3 or 3S only (passthrough camera + depth API). Quest Pro and
  earlier are not supported.

## Pitfalls (the mistakes that produce inaccurate boxes)

1. **Pose mismatch.** Use the pose cached at frame-capture time, not the
   current head pose. Async inference + head motion = drifted boxes.
2. **Y-axis convention.** Image space is top-left/Y-down; Unity viewport
   space is bottom-left/Y-up. Apply the flip at *every* place you convert
   model output to a viewport point — center *and* min/max corners.
   Forgetting it on one of them produces vertically-mirrored boxes.
3. **Coords aren't necessarily normalized at the model boundary.** Many
   models emit pixel coords in model-input space. Always divide by the
   correct `(modelInputW, modelInputH)` before passing to viewport APIs.
4. **NCHW dim ordering.** For `(1, 3, H, W)`, dim 2 is `H`, dim 3 is `W`.
   Mixing them up only manifests on non-square inputs.
5. **Plane center must come from the same viewport rect as the corner
   rays.** If you build the plane from the original (unflipped) center
   but the corner rays from the flipped rect (or vice versa), asymmetric
   boxes will be skewed. Use the same source for both.
6. **Canvas units.** If `RenderMode != WorldSpace`,
   `DynamicPixelsPerUnit != 1`, or any ancestor has non-unit `localScale`,
   `sizeDelta = sizeMeters` no longer equals meters.
7. **No depth hit.** When the raycast misses (pointing at empty space, or
   scene mesh missing in that direction), there is no defensible "true"
   depth. Either skip the box, or fall back to a fixed plane distance —
   but be explicit, because a fallback hides whether the depth API is
   actually working.
8. **No spatial anchor.** Without anchoring, world positions drift as the
   tracking origin re-localizes; boxes end up sliding off objects on long
   sessions.
9. **Reusing texture or pose across multiple frames.** If you batch
   inferences across N camera frames, every frame's detection set must
   remember which pose/texture it came from — don't share one cached pose
   across N async results.

## Optional: stable boxes across frames via IoU dedupe

Per-frame instantiation makes labels flicker. A simple stabilizer:

- Maintain a pool of active boxes.
- For each new detection, compute IoU against existing boxes in their
  local rect space.
- Same class + IoU > 0 → reuse the existing RectTransform (preserves
  label identity).
- Different class + IoU > some small threshold (e.g. 0.1) → evict the
  existing box (newer detection wins).
- Boxes not refreshed within `N` seconds (e.g. 3) → return to pool.

This keeps allocations bounded and visually steady even when raw
detections jitter.

## Adaptation checklist

When porting to a new project:

1. **Identify and normalize your model's output convention** (Step 0).
2. **Wire the runtime dependencies**: `PassthroughCameraAccess`,
   `EnvironmentRaycastManager`, an `OVRSpatialAnchor`-parented World
   Space Canvas with the canvas-scaler values from Step 3.
3. **Cache the camera pose at the moment of texture capture**; thread it
   into the placement function (Step 1).
4. **For each detection**: normalize → Y-flip → viewport ray →
   depth raycast → build plane → corner rays → plane raycasts →
   camera-local size → set RectTransform position/rotation/sizeDelta
   (Step 2).
5. **Gate placement** on `IsPlaying`, head-pose validity at capture
   time, anchor tracked, and a non-null depth hit.
