# QuickActionsAPI

```
namespace Oculus.Interaction.Editor.QuickActions
assembly  Oculus.Interaction.Editor            (Editor-only)
source    com.meta.xr.sdk.interaction/Editor/QuickActions/Scripts/QuickActionsAPI.cs
```

> "Public API for programmatically invoking Interaction SDK Quick Actions. Each method runs the
> corresponding wizard headlessly with default settings and returns all GameObjects created
> during the operation."

Every method returns `List<GameObject>` - the root objects the wizard created. Every method
takes the object you want to make interactive (or the hand/controller to equip) as `target`.

> **The signatures below are a quick reference, not the contract.** The file named above is
> authoritative for the version you have - methods, parameters, and defaults all change. Read it
> when a call does not behave as described here, and before reporting that something is missing.

## Contents
- [Interactables - make an object interactive](#interactables---make-an-object-interactive)
- [Interactable UI - make a Canvas interactive](#interactable-ui---make-a-canvas-interactive)
- [Interactors - equip the rig](#interactors---equip-the-rig)
- [Enums - read them, don't recall them](#enums---read-them-dont-recall-them)
- [What the grab calls actually build](#what-the-grab-calls-actually-build)
- [The snap zone you didn't ask for](#the-snap-zone-you-didnt-ask-for)

## Interactables - make an object interactive

| Method | Signature |
|---|---|
| Hand grab | `AddGrabInteraction(GameObject target)` |
| Distance grab | `AddDistanceGrabInteraction(GameObject target, DistanceGrabMode mode = DistanceGrabMode.PullToHand)` |
| Ray grab | `AddRayGrabInteraction(GameObject target)` |
| Gaze grab | `AddGazeInteraction(GameObject target, bool enableRayFallback = true)` |
| Teleport surface | `AddTeleportInteraction(GameObject target, TeleportSurfaceType surfaceType = TeleportSurfaceType.Hotspot, TeleportHotspotSnap hotspotSnap = TeleportHotspotSnap.SnapPosition, string walkableArea = "Walkable", int layerMask = -1)` |

## Interactable UI - make a Canvas interactive

| Method | Signature |
|---|---|
| Ray-point a Canvas | `AddRayCanvasInteraction(GameObject target, bool fixPointableCanvasModule = true)` |
| Poke a Canvas | `AddPokeCanvasInteraction(GameObject target, bool fixPointableCanvasModule = true)` |
| Gaze a Canvas | `AddGazeCanvasInteraction(GameObject target, bool enableRayFallback = true, bool addToChildCanvases = true, bool fixPointableCanvasModule = true)` |

`fixPointableCanvasModule` ensures a scene-wide `PointableCanvasModule` exists. Leave it `true`
unless the module already comes from a prefab or an additive scene that is not currently loaded.

## Interactors - equip the rig

| Method | Signature |
|---|---|
| Hand | `AddHandInteractors(GameObject target, QuickActionInteractorTypes interactorTypes = QuickActionInteractorTypes.All)` |
| Controller | `AddControllerInteractors(GameObject target, QuickActionInteractorTypes interactorTypes = QuickActionInteractorTypes.All)` |
| Controller-driven hand | `AddControllerHandInteractors(GameObject target, QuickActionInteractorTypes interactorTypes = QuickActionInteractorTypes.All)` |

`target` must be the object carrying the `Hand` / `Controller` component itself. Passing a child
that holds a `Hand`-**derived** component (`LastKnownGoodHand`, `HandFilter`) resolves back to
the same, already-equipped hand and logs `Could not execute HandInteractorWizard`. Filter on
exact type:

```csharp
foreach (var hand in root.GetComponentsInChildren<Hand>(true)
                         .Where(h => h.GetType() == typeof(Hand)))
    QuickActionsAPI.AddHandInteractors(hand.gameObject, types);
```

Calling twice is otherwise harmless - interactor types already present are skipped - but it
does log that error.

## Enums - read them, don't recall them

All four enums are declared at the top of `QuickActionsAPI.cs`, above the class. **Read them
there** rather than trusting any list, including this one - members and flag values change
between versions:

Open `QuickActionsAPI.cs` (locate it via the filename pattern
`**/com.meta.xr.sdk.interaction*/Editor/QuickActions/Scripts/QuickActionsAPI.cs`)
and read the enum declarations at the top of the file, above the class.

| Enum | Passed to |
|---|---|
| `QuickActionInteractorTypes` *(flags)* | `AddHandInteractors`, `AddControllerInteractors`, `AddControllerHandInteractors` |
| `DistanceGrabMode` | `AddDistanceGrabInteraction` |
| `TeleportSurfaceType`, `TeleportHotspotSnap` | `AddTeleportInteraction` |

Two things to check in the source before you rely on a default, because both have bitten:

- **What `QuickActionInteractorTypes.All` actually includes.** It is not every member - at least
  one is opt-in and excluded. Never assume `All` means all; read the declaration.
- **Whether `Teleport` is in `All`.** If it is, `AddHandInteractors(hand)` with defaults installs
  a `LocomotionTurnerInteractor`, which needs locomotion wiring the rest of your scene may not
  have. Pass an explicit mask when you only want grabbing:

```csharp
QuickActionsAPI.AddHandInteractors(hand.gameObject,
    QuickActionInteractorTypes.Poke | QuickActionInteractorTypes.Grab |
    QuickActionInteractorTypes.Ray  | QuickActionInteractorTypes.DistanceGrab);
```

`DistanceGrabMode` selects the interactable's **Movement Provider**. The enum name, the component
you will find in the scene, and Meta's doc label all differ - this table reconciles them:

| `DistanceGrabMode` | Movement Provider component | Doc label |
|---|---|---|
| `PullToHand` *(default)* | `MoveTowardsTargetProvider` | Pull Interactable to Hand |
| `AnchorAtHand` | `MoveFromTargetProvider` | Grab Relative to Hand |
| `ManipulateInPlace` | `MoveAtSourceProvider` | Manipulate in Place |

Other providers exist (e.g. `AutoMoveTowardsTargetProvider`, which completes the pull even if
selection is interrupted); swap the component after creation, or implement `IMovementProvider`. See
[Movement Providers](https://developers.meta.com/horizon/documentation/unity/unity-isdk-movement-providers)
and [Distance Hand Grab Interactions](https://developers.meta.com/horizon/documentation/unity/unity-isdk-distance-hand-grab-interaction).

## What the grab calls actually build

`AddGrabInteraction(cube)` on a primitive cube:

```
Cube                                Rigidbody (isKinematic, no gravity), Grabbable, BoxCollider
  └ ISDK_HandGrabInteraction        HandGrabInteractable (hands), GrabInteractable (controllers)
```

`AddDistanceGrabInteraction(sphere)` on a primitive sphere:

```
Sphere                              Rigidbody (isKinematic, no gravity), Grabbable, SphereCollider
  └ ISDK_DistanceHandGrabInteraction
        SnapInteractor, DistanceHandGrabInteractable, DistanceGrabInteractable,
        MoveTowardsTargetProvider, ReticleDataMesh, ReticleDataIcon
ISDK_SnapZone (Sphere)              <- extra ROOT object, localScale (0,0,0)
        SnapInteractable, Rigidbody
```

Both grab calls set `useGravity = false; isKinematic = true` on the Rigidbody they add, and
auto-generate a collider if none exists under it.

## The snap zone you didn't ask for

Calls create **Optional Components** as well as Required ones (wizard terms - see
[Add Interactions with Quick Actions](https://developers.meta.com/horizon/documentation/unity/unity-isdk-quick-actions)).
`ISDK_SnapZone` is the case you will actually hit.

`AddDistanceGrabInteraction` treats a *time-out snap zone* as optional and creates one: a
zero-scaled `SnapInteractable` at the target's pose, parented alongside the target (so a root
target yields a root zone), wired to the `SnapInteractor` on the new interactable. At runtime it
pulls the object back to that pose once the object has gone unpointed-at for the interactor's
`_timeOut` seconds. Delete the zone to opt out - the object then stays where you drop it.

**Before calling an unfamiliar Quick Action, open its wizard once from the menu** to see the
Required and Optional components it will add.

Per-action Meta doc pages are indexed in
[documentation.md](documentation.md#quick-actions--the-primary-reference). Grab not behaving?
[Grab Interaction Troubleshooting](https://developers.meta.com/horizon/documentation/unity/unity-isdk-hand-grab-troubleshooting).
