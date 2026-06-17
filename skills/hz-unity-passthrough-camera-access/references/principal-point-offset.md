# Passthrough camera principal-point offset

Correctly project the Meta Quest Passthrough Camera image onto a flat
world-space surface (frustum-slice quad, FOV visualizer, "what the camera sees
at distance d" overlay) when the camera's principal point is offset from the
center of the cropped sensor region. Use this reference when corner rays from
`PassthroughCameraAccess.ViewportPointToRay` produce a non-rectangular
quadrilateral on the plane you build, when a camera-aligned quad drifts
off-center, when `2 * d * tan(fov / 2)` gives the wrong width/height on
Phoenix/Stanley but worked on Quest 3, or when reasoning about the difference
between `centerRay = ViewportPointToRay(0.5, 0.5)` and the true optical axis.
For object-facing billboards anchored on depth raycasts, see
[detection-bounding-boxes.md](detection-bounding-boxes.md) instead.

## The trap

`Meta.XR.PassthroughCameraAccess.ViewportPointToRay(viewportPoint, cameraPose)`
implements a standard pinhole model. In camera-local space the ray direction
through viewport `(u, v)` is:

```
direction.x = (cropX + cropW * u - principalPoint.x) / focalLength.x
direction.y = (cropY + cropH * v - principalPoint.y) / focalLength.y
direction.z = 1
```

The ray's direction depends on the pixel position **relative to the principal
point**, not relative to the center of the cropped image.

On Quest 3 / Quest 3S the principal point happens to sit near the center of the
cropped sensor region, so `ViewportPointToRay(0.5, 0.5)` is approximately the
optical axis and the symmetry-based math below works. On **Phoenix (Stanley)
and any future device where the principal point is offset**, that assumption
silently breaks.

## Two distinct concepts that look identical on Quest 3

1. **`centerRay = ViewportPointToRay(0.5, 0.5)`** — the ray through the center
   of the viewport. With a principal-point offset, `centerRay.direction` is
   tilted relative to the optical axis by exactly the offset.
2. **Optical axis** — the camera's local +Z in world space. Get it from the
   pose: `cameraPose.rotation * Vector3.forward` (or equivalently
   `cameraPose.forward`, since Unity's `Pose` exposes a `forward` getter).

These two **are not the same** when the principal point is offset.

## Consequences for plane projection

For a plane perpendicular to direction `n` at distance `d` from the camera, each
edge/corner ray hits the plane at `t_i = (planePoint · n) / (rayDir_i · n)`.

- If `n == optical axis`: `rayDir_i · n` is the same constant for all rays
  (= the camera-local `z` component of the direction, which is 1 by
  construction). All `t_i` are equal, so the four corner rays land on a
  **rectangle**.
- If `n == centerRay.direction` (tilted by the principal-point offset):
  `rayDir_i · n` varies per corner. Each corner scales differently and the
  four intersections form a general quadrilateral, **not a rectangle**.

So for any visualization that wants the actual "image plane at distance d":

> Build the plane with the **optical axis** as the normal, not `centerRay`.

## Consequences for `2 * d * tan(fov / 2)`

The formula

```csharp
float width = 2f * distance * Mathf.Tan(horFovAngle * 0.5f * Mathf.Deg2Rad);
```

is the chord between two rays that are **symmetric** around a common axis at
half-angle `fov/2`. With a principal-point offset the left/right rays are *not*
symmetric around the optical axis — left makes angle `θ_L` from axis, right
makes `θ_R`, and `θ_L ≠ θ_R`. The true width on the optical-axis-perpendicular
plane is `d * (tan θ_L + tan θ_R)`, which is **not** equal to
`2 * d * tan((θ_L + θ_R) / 2)`.

Always derive width and height from the actual ray-plane intersection points:

```csharp
float width  = (rightPoint - leftPoint).magnitude;
float height = (topPoint   - bottomPoint).magnitude;
```

## Canonical pattern: a quad that exactly covers the camera image at distance `d`

```csharp
const float distance = 1f;

// 1. Cache the pose once so all rays share the same origin.
Pose cameraPose = passthroughCameraAccess.GetCameraPose();

// 2. Build the plane perpendicular to the OPTICAL AXIS at `distance` from the camera.
//    `cameraPose.forward` is the true optical axis (camera-local +Z in world space).
//    `centerRay.direction` is NOT the optical axis when the principal point is offset.
var opticalAxis = cameraPose.forward;
var planeCenter = cameraPose.position + opticalAxis * distance;
var plane       = new Plane(-opticalAxis, planeCenter);

// 3. Cast the four edge-midpoint rays and intersect them with the plane.
//    Pass `cameraPose` explicitly so all rays use the same pose snapshot.
var leftRay   = passthroughCameraAccess.ViewportPointToRay(new Vector2(0f,   0.5f), cameraPose);
var rightRay  = passthroughCameraAccess.ViewportPointToRay(new Vector2(1f,   0.5f), cameraPose);
var bottomRay = passthroughCameraAccess.ViewportPointToRay(new Vector2(0.5f, 0f),   cameraPose);
var topRay    = passthroughCameraAccess.ViewportPointToRay(new Vector2(0.5f, 1f),   cameraPose);

plane.Raycast(leftRay,   out float lDist);
plane.Raycast(rightRay,  out float rDist);
plane.Raycast(bottomRay, out float bDist);
plane.Raycast(topRay,    out float tDist);

var leftPoint   = leftRay.GetPoint(lDist);
var rightPoint  = rightRay.GetPoint(rDist);
var bottomPoint = bottomRay.GetPoint(bDist);
var topPoint    = topRay.GetPoint(tDist);

// 4. Derive size and center from the actual intersection points — not from
//    fov-based tan() formulas, which assume symmetric rays.
float width  = (rightPoint - leftPoint).magnitude;
float height = (topPoint   - bottomPoint).magnitude;
var   center = (leftPoint + rightPoint + bottomPoint + topPoint) * 0.25f;

// 5. Align the quad with the sensor (use the camera rotation directly).
quadTransform.SetPositionAndRotation(center, cameraPose.rotation);
quadTransform.localScale = new Vector3(width, height, 1f);
```

The quad's center will be offset from `cameraPose.position + cameraPose.forward * distance`
by exactly the principal-point shift. That's correct — the camera image isn't
centered on the optical axis.

## Verifying that the four corner intersections form a rectangle

When you suspect principal-point drift, drop four cubes at the corner
intersections (TL, TR, BR, BL) and assert:

```csharp
const float tolerance = 0.001f;
Assert.IsTrue(Mathf.Abs((tr - bl).magnitude - (tl - br).magnitude) < tolerance,
    "diagonals differ in length");
Assert.IsTrue(((tr + bl) - (tl + br)).magnitude < tolerance,
    "diagonals don't share a midpoint");
```

A quadrilateral whose diagonals are equal in length and bisect each other is a
rectangle. If either assertion fails, the plane normal is wrong (almost
always: `centerRay.direction` instead of the optical axis).

## When NOT to use `cameraPose.forward` as the plane normal

If you want a **billboard perpendicular to the line from camera to object
center** (e.g. a 2D detection's depth-anchored bounding box), use
`(worldCenter - cameraPose.position).normalized` as the normal — that's the
right choice for a face-the-object billboard and is what
[detection-bounding-boxes.md](detection-bounding-boxes.md) documents. The
principle from this reference applies only when you specifically want the image
plane (perpendicular
to the optical axis), e.g. a frustum slice or image-plane overlay.

## Pitfalls

1. **Using `centerRay.direction` as the plane normal.** Works on Quest 3 by
   coincidence (principal point happens to sit near the crop center), silently
   skews on Phoenix.
2. **Computing width/height from FOV angles.** `2*d*tan(angle/2)` only matches
   the chord on the perpendicular plane for symmetric rays. Always compute
   from ray-plane intersections.
3. **Calling `GetCameraPose()` per ray.** Pose can change between calls (it's
   resolved against the latest frame timestamp). Cache once and pass it to
   every `ViewportPointToRay(viewportPoint, cameraPose)` call so all rays
   share the same origin.
4. **Placing the quad at `cameraPose.position + cameraPose.forward * distance`.**
   That's where the optical axis hits the plane, not where the image-rectangle
   center hits it. Use the centroid of the four edge intersections (or the
   centerRay-plane intersection — they're the same point).
5. **Asymmetric focal length confusion.** `fx ≠ fy` on its own does NOT cause
   non-rectangular corner intersections — it only stretches the rectangle.
   The culprit for non-rectangles is always the principal-point offset.

## Quick mental model

- The camera's pinhole / optical center is `cameraPose.position`.
- The optical axis is `cameraPose.forward` (camera-local +Z in world).
- On the sensor plane, the pixel grid is a rectangle but the principal point
  can be anywhere inside (or even outside) it.
- A plane perpendicular to the optical axis at distance `d` slices the frustum
  into a rectangle. The image rectangle's center on that plane is offset from
  the optical-axis intersection by `(principalPointOffset / focalLength) * d`.
- A plane perpendicular to *any other ray* (such as `centerRay`) slices the
  frustum into a non-rectangle.
