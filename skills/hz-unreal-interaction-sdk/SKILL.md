---
name: hz-unreal-interaction-sdk
license: Apache-2.0
description: Guides integrating the Meta XR Interaction SDK in Unreal Engine 5 projects targeting Meta Quest and Horizon OS, covering plugin installation, OpenXR vs Meta XR backend tradeoffs, prebuilt hand/controller interactor rigs, Enhanced Input mapping contexts, poke/ray/grab/distance-grab/transformer interactions, hand tracking feature support, and Microgestures (thumb tap/swipe input). Use when adding grab, poke, raycasting, hand/controller-driven UI interactions, or thumb tap/swipe microgesture input to a UE5 Meta Quest project.
---

# UE5 Interaction SDK for Meta Quest / Horizon OS

Guide for integrating the Meta XR Interaction SDK — out-of-the-box grab, poke, and ray interactions for controllers and hand tracking — into a UE5 Meta Quest project.

## 1. What it is and when to use it

- Interaction SDK gives VR users grab/scale, throw, button-press-via-ray, and poke/scroll interactions without building the interaction logic yourself.
- Consists of: a closed-source native DLL (ray/poke/grab/pose-detection), Unreal component wrappers around it, and **Prebuilts** — ready-to-use interactor rigs.
- Supported devices: Quest 2, Quest 3, Quest 3S, Quest Pro. Requires **UE 5.4+**.

## 2. Install

Interaction SDK is distributed separately from the Fab marketplace — via **Meta Quest Developer Hub**.

1. Download the **Meta XR Interaction SDK** package for Unreal Engine from Meta Quest Developer Hub.
2. In `[UnrealDir]\Engine\Plugins`, create a `Marketplace` directory if it doesn't exist.
3. Extract the downloaded zip's `MetaXRInteraction` folder into `[UnrealDir]\Engine\Plugins\Marketplace`.
4. In the editor: `Edit > Plugins` → search "Meta" (searching "Meta XR" may miss results) → enable both **Meta XR** and **Meta XR Interaction SDK**.
5. Restart the editor.

## 3. Choose your input backend: Meta XR vs OpenXR

Interaction SDK reads hand/controller data from whichever backend plugin is present:

- **Meta XR plugin** — recommended for Quest deployment; unlocks the full feature set.
- **Unreal's built-in OpenXR HandTracking plugin** — restricted-mode fallback, mainly for non-Meta devices.

**Gaps on the OpenXR path** (vs Meta XR):

| Feature | Meta XR | OpenXR |
|---|---|---|
| Procedural Controller Hand Animation | ✓ | – |
| Hand Tracking Confidence (granular) | ✓ | – (binary valid/invalid only) |
| Direct input binding for index pinch | ✓ | – (use `PinchGrabStarted`/`PinchGrabFinished` delegates on `IsdkHandFingerPinchGrabRecognizer` instead) |

**Rule:** for a Quest-only project, use the Meta XR plugin backend. Only fall back to OpenXR if you must also support non-Meta OpenXR headsets from the same build.

## 4. Build the interaction-ready Pawn

Prebuilt rig components attach to a Pawn's `MotionControllerComponent`s to give it a full interactor set per hand:

```
RootComponent (SceneComponent)
├─ VRCamera (CameraComponent)
├─ RightHand (MotionControllerComponent, Motion Source = Right)
│   ├─ RightHandInteractors (IsdkHandRigComponentRight)
│   └─ RightControllerInteractors (IsdkControllerRigComponentRight)
└─ LeftHand (MotionControllerComponent, Motion Source = Left)
    ├─ LeftHandInteractors (IsdkHandRigComponentLeft)
    └─ LeftControllerInteractors (IsdkControllerRigComponentLeft)
```

- Add **both** hand and controller rig components per side if the app must support switching between hands and controllers at runtime.
- Switch input mode live via `ISDK_SetControllerHandBehavior` — no level reload required, all active interactors update immediately.
- Set tracking origin to **Local Floor** on `BeginPlay` (`Is Head Mounted Display Enabled` → branch → `Set Tracking Origin`).

## 5. Wire up Enhanced Input mapping contexts

Interaction SDK uses Unreal's **Enhanced Input** system. Default mapping context assets ship in `Plugins > OculusInteraction > Inputs`:

| Context asset | Source | Interaction | Input |
|---|---|---|---|
| `IMC_IsdkHand` | Hand | Poke | Extend index finger |
| `IMC_IsdkHand` | Hand | Ray Select | Pinch index + thumb |
| `IMC_IsdkControllerAnimation` | Controller | Poke | Poke with controller tip |
| `IMC_IsdkControllerAnimation` | Controller | Ray Select | Press and hold Trigger |

Add both contexts to the `EnhancedInputLocalPlayerSubsystem` (via `Get Player Controller → Get EnhancedInputLocalPlayerSubsystem → Add Mapping Context`) with a priority high enough to override the VR Template's default contexts. Note: these mapping contexts target the Meta XR backend — the OpenXR path uses automatic recognizer-based fallback instead (see pinch note above).

## 6. Core interaction types

| Type | Behavior |
|---|---|
| **Poke** | Near-field UI press/scroll — via `UIsdkInteractableWidgetComponent` on any widget actor |
| **Ray** | Far-field selection with optional hover feedback (e.g. audio) |
| **Grab** | Physically grab/move/scale objects, free or axis-constrained; supports pre-authored grab-pose data assets so hands conform naturally |
| **Distance Grab** | Remote manipulation — `PullToHand` (object flies to hand), `RelativeToPointer` (maintains pointer offset), `ManipulateInPlace` (transforms without pulling) |
| **Transformer** | Two-handed manipulation (e.g. scale/rotate with both hands) |

### Making a UMG widget interactable

- Use the `IsdkInteractableWidget` actor to place a `Widget Blueprint` in 3D world space with poke/ray support built in — no custom hit-testing needed.
- Key properties: **Draw Size** (render resolution), **Widget Scale** (world size), **Blend Mode = Transparent** if the UI needs transparency, **Two Sided** if visible from both sides.
- **Create Poke Interactable** / **Create Ray Interactable** toggles let you disable a modality per-widget if not wanted.
- Assign a **Default Poke Interactable Config Asset** (e.g. `IsdkPokeInteractablePanelConfig`) for sane poke defaults.
- Make a widget or object grabbable by adding an `IsdkGrabbableComponent`.

## 7. Hand tracking feature support

Hand tracking itself is provided by **Meta Core SDK**; Interaction SDK is the recommended layer on top of it for building interactions — the docs explicitly warn that building custom pinch/poke/gesture logic without it "makes it difficult to get approved in the store."

| Feature | Supported (Unreal) | Notes |
|---|---|---|
| Fast Motion Mode (FMM) | ✓ | 60Hz tracking for fitness/rhythm apps with fast hand movement |
| Wide Motion Mode (WMM) | – | Tracks hands outside headset FOV — available in Unity, **not yet in Unreal** |
| Multimodal | ✓ | Simultaneous hand + controller tracking |
| Capsense | ✓ | Logical hand poses shown while holding controllers |
| Pose Detection | ✓ (v78+) | Detects when tracked hand matches a recorded pose's shape/transform. Meta's hand-tracking feature table lists this as "v78+" without naming the product for that row; "Meta XR plugin" is inferred from the same "Vnn" numbering convention used elsewhere on the same page family (e.g. Microgestures' "Meta XR plugin V77"), not a verbatim label on this row. |
| Gesture Detection (multi-step sequences) | – | Chaining multiple IActiveStates over time — **not yet supported in Unreal** |
| Microgestures | ✓ | Thumb tap/swipe input — see below |

## 8. Microgestures — thumb tap/swipe input

A separate input system from the grab/poke/ray interactors above: **Microgestures** recognize subtle thumb tap and swipe motions on the side of the index finger, producing discrete D-pad-like directional input. Useful for menu navigation without a pinch, ray, or controller.

**Gesture set** (thumb starts raised above the index finger, not touching it, other fingers slightly curled):
- **Tap**: touch the middle segment of the index finger with the thumb, then lift.
- **Left / Right swipe**: touch, slide toward/away from the fingertip (direction is mirrored between hands), then lift.
- **Forward / Backward swipe**: touch, slide forward or backward/downward, then lift.

Gestures are detected at the end of the motion; perform them in one smooth motion at moderate-to-quick speed.

**Requirements:** UE 5.5+, Meta XR plugin v77+, Quest 2 / Quest Pro / Quest 3 family.

**Setup — via Enhanced Input, not `IsdkHandRigComponent`:**

1. Create an **Input Action** per microgesture (e.g. `IA_Microgesture_L_Left_Swipe`), **Value Type = Digital (bool)**.
2. Create an **Input Mapping Context** (e.g. `IMC_Microgestures`), and map each Input Action to its microgesture input (e.g. *Oculus Hand (L) Microgesture - Swipe Left*) via the mapping dropdown.
3. Register the context: either add it to `Project Settings > Enhanced Input > Default Mapping Contexts` for a global mapping, or add it dynamically in Blueprint via **Add Mapping Context** on the `EnhancedInputLocalPlayerSubsystem` (e.g. on Pawn `BeginPlay`, or per-level for specialized inputs).
4. Bind each Input Action's event in Blueprint: **Started** for edge-triggered actions (e.g. tap opens a menu), **Triggered** for continuous detection while the gesture is active. (Click the down-arrow on the event node to expose the **Started** pin.)

Common pattern: bind Thumb Tap to open a menu, directional swipes to navigate it, Thumb Tap again to select.

## 9. Verify on device

VR Preview in the editor approximates behavior, but hand-tracking confidence, pinch recognition, and haptic/audio feedback timing should be confirmed on real hardware:

```bash
metavr device list                 # confirm target headset connected
metavr app install <path-to-apk>
metavr log                         # watch for Interaction SDK init errors on launch
```

Confirm on-device: hand tracking and controller rigs both register interactions correctly, runtime switching (`ISDK_SetControllerHandBehavior`) works without a level reload, and poke/ray hit targets feel accurate at the configured Widget Scale.

## Checklist

- [ ] Meta XR Interaction SDK plugin installed via Meta Quest Developer Hub (not Fab) into `Engine/Plugins/Marketplace`
- [ ] Both **Meta XR** and **Meta XR Interaction SDK** plugins enabled
- [ ] Input backend deliberately chosen: Meta XR (full features, Quest-only) vs OpenXR (fallback, feature gaps noted)
- [ ] Pawn has both hand and controller rig components per side, if runtime switching is needed
- [ ] Tracking origin set to Local Floor on BeginPlay
- [ ] `IMC_IsdkHand` / `IMC_IsdkControllerAnimation` mapping contexts added to `EnhancedInputLocalPlayerSubsystem` with correct priority
- [ ] Interactable UI built via `IsdkInteractableWidget`, not raw hit-testing
- [ ] Grab/Distance Grab/Transformer strategy chosen deliberately per interaction (not left at default if a specific feel is needed)
- [ ] If using Microgestures: UE 5.5+ and Meta XR plugin v77+ confirmed; Input Actions set to Digital (bool)
- [ ] Microgesture Input Mapping Context registered (Default Mapping Contexts or dynamic Add Mapping Context) with correct priority
- [ ] Verified on real device: hand tracking, controller input, runtime hand/controller switching, and any microgesture bindings all confirmed working

## Last verified

Checked directly against live developers.meta.com pages (via metavr docs tools), not from model training data.

| Claim | Source | Updated |
|---|---|---|
| Interaction SDK overview, install path, Meta XR vs OpenXR backend gaps | [Interaction SDK for Unreal Engine](https://developers.meta.com/horizon/documentation/unreal/unreal-isdk-overview/) | 2026-04-15 |
| Prebuilt rig components, Enhanced Input mapping, UI widget setup | [Getting Started with Interaction SDK](https://developers.meta.com/horizon/documentation/unreal/unreal-isdk-getting-started/) | 2026-04-15 |
| Hand tracking feature support table (FMM, WMM, Pose Detection v78+, Gesture Detection, Microgestures) | [Hand Tracking in Unreal Engine](https://developers.meta.com/horizon/documentation/unreal/unreal-hand-tracking-overview/) | 2026-03-02 |
| Microgestures gesture set, UE 5.5+ / Meta XR plugin V77+ requirement, Enhanced Input setup | [Hand tracking Microgestures in Unreal Engine](https://developers.meta.com/horizon/documentation/unreal/unreal-microgestures/) | 2026-04-14 |

**Note on Pose Detection "v78+":** confirmed verbatim as a version number, but the product-name qualifier ("Meta XR plugin") is an inference from a site-wide numbering convention, not stated on that specific table row — see the table entry above.
