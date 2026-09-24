# Verifying an ISDK setup

## Contents
- [Edit-time checks are not enough](#edit-time-checks-are-not-enough)
- [The loop](#the-loop)
- [Runtime assertions to expect and what they mean](#runtime-assertions-to-expect-and-what-they-mean)
- [Benign noise](#benign-noise)
- [Confirm the wiring, not just the absence of errors](#confirm-the-wiring-not-just-the-absence-of-errors)
- [Silent failures Play mode will not flag](#silent-failures-play-mode-will-not-flag)
- [What Play mode does not prove](#what-play-mode-does-not-prove)

## Edit-time checks are not enough

ISDK validates its wiring in `Start()` through `AssertUtils.AssertField`. A scene whose
Inspector looks correct - every component present, every visible field filled - can still throw
on entering Play mode, because the fields ISDK asserts are often private serialized references
that no edit-time component listing surfaces.

**Never report an interaction setup as working on the basis of component presence alone.**
Enter Play mode and read the console.

## The loop

With a live Editor (see the `unity-cli` skill):

```bash
unity command clear_console   --project-path <project>
unity command editor_play     --project-path <project>
unity command get_console_logs --project-path <project> --format json
unity command editor_stop     --project-path <project>
```

Entering Play mode triggers a domain reload; a command issued immediately after `editor_play`
can fail with a transport error. Re-issue it - check `unity command editor_status` and look for
`playMode: playing` before reading logs.

**The bar is zero errors and zero exceptions.** Warnings are usually fine (see below).

## Runtime assertions to expect and what they mean

| Message | Cause |
|---|---|
| `At GameObject X, component Y. Required Z reference is missing.` | A serialized reference was never wired. The message names the component and field - fix that exact field. |
| `AssertionException ... OVRCameraRigRef.GetHandCached` | No `OVRHand` under the rig's hand anchor while `_requireOvrHands` is true. Add `OVRHandPrefab` under `Left/RightHandAnchor` with matching handedness, or use `AddOVRInteractionRig()` which handles it. |
| `LocomotionTurnerInteractor ... Transformer reference is missing` | Teleport interactors installed without locomotion wiring. Either wire `_transformer` or omit `QuickActionInteractorTypes.Teleport`. |
| `Could not execute <Name>Wizard` (edit time) | The wizard could not run - usually the wrong target, or every interactor type it would add is already present. |
| `Hand Skeleton Version in OVRManager must be set to ...` | `OVRRuntimeSettings.HandSkeletonVersion` disagrees with the SDK's `ISDK_OPENXR_HAND` compile flag. |

## Benign noise

- `[MetaXRFeature]: ErrorFormFactorUnavailable xrGetSystem` - no headset or XRSIM attached.
- `[OVRPlugin] [xrDestroySession] ...` / `has undestroyed spaces` - session teardown.
- `[OVRPlugin] [xrSuggestInteractionProfileBindings] Unsupported path: .../proximity_fb` -
  a binding this runtime does not expose.
- `This project uses Input Manager, which is marked for deprecation.`

## Confirm the wiring, not just the absence of errors

A silent console only proves nothing threw. Assert the pairing explicitly:

```csharp
// Interactables resolve to the object's Grabbable.
// PointableElement is assigned in Awake(), so this check is only valid on a LIVE scene
// object - on a prefab asset it is always null. For assets, read the serialized field:
//   new SerializedObject(i).FindProperty("_pointableElement").objectReferenceValue
foreach (var i in Object.FindObjectsByType<HandGrabInteractable>(
             FindObjectsInactive.Include, FindObjectsSortMode.None))
    Debug.Log($"{i.gameObject.name} -> {i.PointableElement}");   // must not be null

// The rig actually carries interactors to reach them with
Debug.Log(Object.FindObjectsByType<HandGrabInteractor>(
    FindObjectsInactive.Include, FindObjectsSortMode.None).Length);         // expect 4
Debug.Log(Object.FindObjectsByType<DistanceHandGrabInteractor>(
    FindObjectsInactive.Include, FindObjectsSortMode.None).Length);         // expect 4
Debug.Log(Object.FindObjectsByType<GrabInteractor>(
    FindObjectsInactive.Include, FindObjectsSortMode.None).Length);         // expect 2
```

Run this **in Play mode** to also confirm tracking is live:

```csharp
foreach (var h in Object.FindObjectsByType<Oculus.Interaction.Input.Hand>(
             FindObjectsInactive.Include, FindObjectsSortMode.None)
         .Where(h => h.GetType() == typeof(Oculus.Interaction.Input.Hand)))
    Debug.Log($"{h.Handedness} connected={h.IsConnected} valid={h.IsTrackedDataValid}");
```

With XRSIM running you should see `connected=True valid=True` for a tracked hand. Without a
headset or simulator, `False` everywhere is expected and is not a scene defect.

## Silent failures Play mode will not flag

Some defects produce no log line at all. Watch the Game view, not just the console:

- **The view drifts downward.** The rig's locomotor has falling enabled and the scene has no
  ground collider. See the Smooth Locomotion note in
  [ovr-quick-actions-api.md](ovr-quick-actions-api.md).
- **An object sits where you dropped it when you expected it to return** (or vice versa) - check
  for an `ISDK_SnapZone` and its `SnapInteractor._timeOut`.
- **A grab registers on the wrong volume** - a child collider joined the target's Rigidbody as a
  compound collider.

For grabs that hover but never select, Meta has a dedicated page:
[Grab Interaction Troubleshooting](https://developers.meta.com/horizon/documentation/unity/unity-isdk-hand-grab-troubleshooting).

## What Play mode does not prove

Play mode verifies wiring and startup. It does not verify that an interaction *feels* right, is
reachable, or is comfortable. For that, run in the Meta XR Simulator (`hz-xr-simulator-install-and-configure`)
or deploy to a device (`hz-vr-debug`) - and say plainly which of the three you actually did.
