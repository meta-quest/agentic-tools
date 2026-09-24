# The ISDK interaction model

Official background:
[Interaction Model Overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interaction-model-overview)
· [Interactors](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interactor)
· [Input Data Overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-input-processing)
· [The Interaction Rig](https://developers.meta.com/horizon/documentation/unity/unity-isdk-cameraless-rig)

Understanding these four roles explains most "why isn't my interaction firing" questions.

## Contents

- [Hand vs controller](#hand-vs-controller)
- [Rig anatomy](#rig-anatomy)
- [The rig is modular](#the-rig-is-modular-data-sources--interactions--visuals)
- [Tracking origin and placement](#tracking-origin-and-placement)
- [Parenting an interactable under another interactable](#parenting-an-interactable-under-another-interactable)
- [Physics](#physics)

| Role | Lives on | Examples |
|---|---|---|
| **Interactor** | the rig (hands, controllers) | `HandGrabInteractor`, `DistanceHandGrabInteractor`, `GrabInteractor`, `PokeInteractor`, `RayInteractor` |
| **Interactable** | the object being acted on | `HandGrabInteractable`, `DistanceHandGrabInteractable`, `GrabInteractable`, `PokeInteractable`, `RayInteractable` |
| **PointableElement** | the object being moved | `Grabbable` — receives interactable events and actually moves the transform |
| **Data source** | the rig | `FromOVRHandDataSource`, `FromOVRControllerDataSource`, `FromOVRHmdDataSource` — feed tracking data in |

An interaction needs a matching **pair**: a `HandGrabInteractor` on the rig and a
`HandGrabInteractable` on the object. One without the other does nothing and logs nothing —
which is why "nothing happens" is usually a missing interactor, not a broken interactable.

Interactables are normally placed on a **child** of the target object (`ISDK_HandGrabInteraction`,
`ISDK_DistanceHandGrabInteraction`), while the `Rigidbody` and `Grabbable` go on the target
itself. The interactable's `PointableElement` points back at that `Grabbable`. Verify with:

```csharp
var i = Object.FindAnyObjectByType<HandGrabInteractable>();
if (i == null)
    Debug.LogError("No HandGrabInteractable in the loaded scene");
else
    Debug.Log(i.PointableElement);   // on a live scene object, resolves to the target's Grabbable
```

`PointableElement` is assigned in `Awake()` (in the `PointerInteractable` base class), so it is
**always null on a prefab asset or any object whose `Awake()` has not run** - a null there does
not mean the wiring is broken. To check a prefab asset, read the serialized field instead:
`new SerializedObject(i).FindProperty("_pointableElement").objectReferenceValue`.

## Hand vs controller

Most grab Quick Actions install **both** an ISDK hand interactable and a controller
interactable, so a single call supports hand tracking and controllers:

- `HandGrabInteractable` / `DistanceHandGrabInteractable` — hands
- `GrabInteractable` / `DistanceGrabInteractable` — controllers

`GrabTypeFlags` (default `All`) controls which hand grab types count — pinch and palm.

## Rig anatomy

Produced by `OVRQuickActionsAPI.AddOVRInteractionRig()`:

```
OVRCameraRig                               Core SDK: OVRCameraRig + OVRManager
  ├ TrackingSpace                          CenterEyeAnchor, Left/RightHandAnchor, ...
  └ OVRComprehensiveInteractionRig
      ├ OVRHmd/OVRHands/OVRControllers     data sources -> tracking data
      ├ ComprehensiveInteractorsLeft       Interactors, Features, SyntheticHandData
      ├ ComprehensiveInteractorsRight      Interactors, Features, SyntheticHandData
      ├ OVRHandVisual* / OVRControllerVisual*
      └ Locomotor                          PlayerController, teleport, tunneling
```

`OVRCameraRigRef` on the interaction rig is the bridge: it holds the `OVRCameraRig` reference
and exposes `LeftHand` / `RightHand` / `LeftController` / `RightController` to the data sources.

### The rig is modular: Data Sources → Interactions → Visuals

Per [Comprehensive Interaction Rig](https://developers.meta.com/horizon/documentation/unity/unity-isdk-cameraless-rig),
each stage can be swapped without touching the others — swapping OVR data sources for UnityXR
ones leaves the Interactions intact. Practical consequences:

- **Add interactors** under `Interactions / Interactors`, and register them in the
  **Interactor Group** if they need priority ordering.
- **Input-conditional subdirectories.** Interactors sit under child objects toggled by what is
  available — hand-only, controller-only, hand-and-no-controller, controller-and-no-hand,
  controller-and-hand. Poke prefers a real index finger; ray and locomotion prefer a controller.
  Place a new interactor in the matching subdirectory to inherit that toggling.
- **Synthetic hands are a chain.** Each Interactions module exposes three chained synthetic
  hands so interactors can write at different stages — hand grab wraps fingers around an object,
  poke then clamps the fingertip at a surface. **Order matters.** Visuals reference only the
  final link, which is why one visual per input device suffices.
- **Mirroring.** Left and right Interactions share a handedness-agnostic prefab. With
  **Generate as Editable Copy**, edit one hand and *Apply as Override* to propagate to the other,
  use **Overrides** to diff against the stock rig, and revert to restore it.
- **Delete what you don't need.** The rig ships in full form; unused interactors can be removed
  and recovered later through prefab overrides.
- **Locomotion is its own module**, a sibling of Data Sources / Interactions / Visuals. It
  consumes interactors and broadcasters from the Interactions layer via
  `LocomotionEventsConnection` and moves the character and camera rig.

## Tracking origin and placement

`OVRManager.trackingOriginType` decides what `y = 0` means, and therefore where to put
interactable objects:

- **`FloorLevel`** — origin at the floor. A comfortable standing reach is roughly
  `y = 1.0`, `z = 0.3–0.5`.
- **`EyeLevel`** — origin at the headset. Objects near `y = 0` sit at eye height.

Place objects from **real bounds**, not assumed numbers, when they relate to each other:

```csharp
var cubeBounds = cube.GetComponent<Renderer>().bounds;
var radius     = sphere.GetComponent<Renderer>().bounds.extents.y;
sphere.transform.position = new Vector3(
    cubeBounds.center.x, cubeBounds.max.y + radius, cubeBounds.center.z);
```

## Parenting an interactable under another interactable

Making object B a child of grabbable object A puts B's collider into A's `Rigidbody` as part of
a **compound collider**, changing A's grab volume. If you want B to travel with A, do it through
the interaction model rather than plain parenting — otherwise keep them siblings.

## Physics

Every grab wizard adds a Rigidbody with `useGravity = false; isKinematic = true`. Objects hold
their position rather than falling, which matters because a fresh ISDK scene has no floor
collider. If you want thrown objects to obey physics you must change this deliberately and
provide something for them to land on.
