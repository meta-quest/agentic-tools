# OVRQuickActionsAPI

```
namespace Oculus.Interaction.OVR.Editor.QuickActions
assembly  Oculus.Interaction.OVR.Editor        (Editor-only)
source    com.meta.xr.sdk.interaction.ovr/Editor/QuickActions/Scripts/OVRQuickActionsAPI.cs
```

The OVR-side counterpart to `QuickActionsAPI`. `QuickActionsAPI` makes *objects* interactive;
`OVRQuickActionsAPI` builds the *rig* that interacts with them.

> The file named above is authoritative for the version you have - read it rather than trusting
> the signature below. At the time of writing it exposes a single method.

## `AddOVRInteractionRig`

```csharp
/// Creates the OVR Comprehensive Interaction Rig under the OVRCameraRig.
/// If no OVRCameraRig exists, one is created automatically.
/// Performs all wizard setup including disabling duplicate hand visuals
/// and creating editable interactor variants.
public static List<GameObject> AddOVRInteractionRig(bool generateAsEditableCopy = true)
```

- **`generateAsEditableCopy`** (default `true`) - create an editable copy of the interactor
  prefab rather than a prefab instance. Keep the default when you intend to tune interactors;
  pass `false` to stay linked to the package prefab and inherit upstream changes.
- **Returns** the roots it created, e.g. `[OVRCameraRig, OVRComprehensiveInteractionRig]`.

```csharp
using Oculus.Interaction.OVR.Editor.QuickActions;

OVRQuickActionsAPI.AddOVRInteractionRig();
```

Official page: [OVR Interaction Rig Quick Action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-ovr-interaction-rig-quick-action)
· walkthrough: [Create the Comprehensive Interaction Rig](https://developers.meta.com/horizon/documentation/unity/unity-isdk-add-comprehensive-interaction-rig)

## The wizard has settings the API doesn't expose

`AddOVRInteractionRig` takes one parameter. The interactive wizard offers more - Prefab Path and
Smooth Locomotion among them (see the official page for the current set). If you need a setting
the API doesn't take, use the menu item instead, or adjust the rig after creating it, and say
which you did.

**Smooth Locomotion is on by default and the API gives you no way to opt out at creation time.**
Per [Comprehensive Interaction Rig](https://developers.meta.com/horizon/documentation/unity/unity-isdk-cameraless-rig):

> "Smooth Locomotion (sliding movement via thumbstick) is enabled by default in the
> Comprehensive Interaction Rig. If your app does not require smooth locomotion, you can disable
> it by removing or deactivating the relevant slide locomotion interactors under the
> **Locomotion** module in the rig hierarchy."

And from the walkthrough: *"your camera might fall infinitely when starting the scene if there
is no ground collider present."* A fresh ISDK scene has no ground collider, so if the view
drifts downward on entering Play mode, that is the cause. Disable the slide locomotion
interactors under the rig's **Locomotion** module, or add a floor collider. **This produces no
console error** - a clean Play-mode log will not catch it. Watch the Game view.

## It creates the camera rig for you

The camera rig is a **required** dependency of this action, and required dependencies are always
satisfied - so:

- **`OVRCameraRig` already in the scene** → the rig is parented under it, existing rig untouched.
- **No `OVRCameraRig`** → one is created from the package prefab, then the rig goes under it.

You do **not** need to pre-create a camera rig, and you should not call this expecting a second
one. If you need specific `OVRManager` settings (tracking origin, hand tracking support,
passthrough), set them on the rig *after* this call - see the `hz-unity-meta-core-sdk` skill.

## What you get

```
OVRCameraRig                              OVRCameraRig, OVRManager
  ├ TrackingSpace                         eye + hand + controller anchors
  └ OVRComprehensiveInteractionRig
      ├ OVRHmd / OVRHands / OVRControllers          data sources
      ├ ComprehensiveInteractorsLeft / ...Right     the interactors + features
      ├ OVRHandVisualLeft / ...Right                hand visuals
      ├ OVRControllerVisualLeft / ...Right          controller visuals
      └ Locomotor                                   teleport / movement / tunneling
```

Interactor counts in a fresh rig: **4** `HandGrabInteractor`, **4**
`DistanceHandGrabInteractor`, **2** `GrabInteractor`, plus poke, ray, and teleport interactors -
both hands and both controllers, near and at distance.

## Why not assemble the rig by hand

The `OVRInteraction`, `OVRHands`, and `OVRControllers` prefabs cannot serialize references to
each other, so by hand you must wire the camera rig, tracking-to-world transformer, and
hand-skeleton provider into every OVR data source yourself, and add an `OVRHandPrefab` under each
hand anchor with matching handedness on `OVRHand`, `OVRSkeleton`, and `OVRMesh`. Miss one and it
looks correct in the Inspector, then throws `AssertionException` on entering Play mode. You also
have to suppress duplicate hand visuals, which this call handles.
