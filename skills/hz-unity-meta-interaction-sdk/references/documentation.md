# Interaction SDK documentation map

All links are public `developers.meta.com` pages, verified via the `metavr` doc tools.

## Contents

- [Look it up live](#look-it-up-live---do-not-answer-isdk-questions-from-memory)
- [Start here](#start-here)
- [Quick Actions - the primary reference](#quick-actions---the-primary-reference)
- [The rig](#the-rig)
- [Grabbing](#grabbing)
- [Poke, ray, gaze, UI](#poke-ray-gaze-ui)
- [Locomotion](#locomotion)
- [Hands, poses, visuals](#hands-poses-visuals)
- [Sample scenes](#sample-scenes)
- [Design guidelines](#design-guidelines)
- [Docs vs. the installed package](#docs-vs-the-installed-package)

## Look it up live - do not answer ISDK questions from memory

Meta's docs change faster than any snapshot. Prefer a live lookup:

```
vr_docs_search(query="...", scope="unity")        # find the page (returns doc_path)
vr_docs_get_page(url="documentation/unity/...")   # fetch full text
vr_api_search(query="HandGrabInteractable", engine="unity")   # exact signatures
```

Or from a terminal: `npx metavr --help`.

**Do not hand-construct doc URLs.** The namespace is non-obvious
(`/documentation/...` vs `/horizon/documentation/...` vs `/horizon/llmstxt/...`). Always take
`doc_path` from a search result.

Two URL spaces are in play:

| Space | Base | Use |
|---|---|---|
| Guides | `https://developers.meta.com/horizon/documentation/unity/<slug>` | Concepts, tutorials, Quick Actions |
| API reference | `https://developers.meta.com/horizon/reference/interaction/latest/<symbol>` | Class/interface signatures, e.g. `class_oculus_interaction_hand_grab_distance_hand_grab_interactor` |

## Start here

| Page | Link |
|---|---|
| Interaction SDK (overview) | [unity-isdk-interaction-sdk-overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interaction-sdk-overview) |
| Features Overview | [unity-isdk-features-overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-features-overview) |
| Getting Started with Interaction SDK | [unity-isdk-getting-started](https://developers.meta.com/horizon/documentation/unity/unity-isdk-getting-started) |
| **Packages and Requirements** (install, deps, supported devices) | [unity-isdk-packages-and-requirements](https://developers.meta.com/horizon/documentation/unity/unity-isdk-packages-and-requirements) |
| Interaction Model Overview | [unity-isdk-interaction-model-overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interaction-model-overview) |
| Interactors | [unity-isdk-interactor](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interactor) |
| Input Data Overview | [unity-isdk-input-processing](https://developers.meta.com/horizon/documentation/unity/unity-isdk-input-processing) |

## Quick Actions - the primary reference

| Page | Link |
|---|---|
| **Add Interactions with Quick Actions** (landing) | [unity-isdk-quick-actions](https://developers.meta.com/horizon/documentation/unity/unity-isdk-quick-actions) |
| OVR Interaction Rig Quick Action | [unity-isdk-ovr-interaction-rig-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-ovr-interaction-rig-quick-action) |
| UnityXR Interaction Rig Quick Action | [unity-isdk-unityxr-interaction-rig-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-unityxr-interaction-rig-quick-action) |
| Grab Quick Action | [unity-isdk-grab-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-grab-quick-action) |
| Distance Grab Quick Action | [unity-isdk-distance-grab-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-distance-grab-quick-action) |
| Ray Grab Quick Action | [unity-isdk-ray-grab-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-ray-grab-quick-action) |
| Teleport Quick Action | [unity-isdk-teleport-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-teleport-quick-action) |
| Poke Canvas Quick Action | [unity-isdk-poke-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-poke-quick-action) |
| Ray Canvas Quick Action | [unity-isdk-ray-quick-action](https://developers.meta.com/horizon/documentation/unity/unity-isdk-ray-quick-action) |

The Quick Actions landing page documents the wizard anatomy that the APIs drive headlessly:
**Settings**, **Required Components** (Fix / Fix All), **Optional Components** (Add / Add All).
Calling the API is equivalent to clicking **Fix All** *and* **Add All** - read that page to know
what a given action will add.

## The rig

| Page | Link |
|---|---|
| Create the Comprehensive Interaction Rig | [unity-isdk-add-comprehensive-interaction-rig](https://developers.meta.com/horizon/documentation/unity/unity-isdk-add-comprehensive-interaction-rig) |
| The Interaction Rig (concept) | [unity-isdk-cameraless-rig](https://developers.meta.com/horizon/documentation/unity/unity-isdk-cameraless-rig) |
| Create the UnityXR Interaction Rig | [unity-isdk-add-comprehensive-interaction-rig-unityxr](https://developers.meta.com/horizon/documentation/unity/unity-isdk-add-comprehensive-interaction-rig-unityxr) |
| Configure Meta XR camera settings (OVRCameraRig) | [unity-ovrcamerarig](https://developers.meta.com/horizon/documentation/unity/unity-ovrcamerarig) |

## Grabbing

| Page | Link |
|---|---|
| Grab Interaction Overview | [unity-isdk-grab-interaction-overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-grab-interaction-overview) |
| Hand Grab Interactions | [unity-isdk-hand-grab-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-hand-grab-interaction) |
| Distance Hand Grab Interactions | [unity-isdk-distance-hand-grab-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-distance-hand-grab-interaction) |
| Touch Hand Grab Interactions | [unity-isdk-touch-hand-grab-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-touch-hand-grab-interaction) |
| Movement Providers | [unity-isdk-movement-providers](https://developers.meta.com/horizon/documentation/unity/unity-isdk-movement-providers) |
| Creating Grabbable Objects (tutorial) | [unity-isdk-create-grabbable-object](https://developers.meta.com/horizon/documentation/unity/unity-isdk-create-grabbable-object) |
| **Grab Interaction Troubleshooting** | [unity-isdk-hand-grab-troubleshooting](https://developers.meta.com/horizon/documentation/unity/unity-isdk-hand-grab-troubleshooting) |
| Create Ghost Reticles | [unity-isdk-create-ghost-reticles](https://developers.meta.com/horizon/documentation/unity/unity-isdk-create-ghost-reticles) |

## Poke, ray, gaze, UI

| Page | Link |
|---|---|
| Poke Interaction | [unity-isdk-poke-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-poke-interaction) |
| Ray Interaction | [unity-isdk-ray-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-ray-interaction) |
| Gaze Interaction | [unity-isdk-gaze-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-gaze-interaction) |
| Creating UIs (overview) | [unity-isdk-create-ui-overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-create-ui-overview) |
| Unity Canvas Integration | [unity-isdk-canvas-integration](https://developers.meta.com/horizon/documentation/unity/unity-isdk-canvas-integration) |
| Create a Curved or Flat UI | [unity-isdk-create-ui](https://developers.meta.com/horizon/documentation/unity/unity-isdk-create-ui) |
| Creating Pokeable UIs | [unity-isdk-create-pokeable-ui](https://developers.meta.com/horizon/documentation/unity/unity-isdk-create-pokeable-ui) |
| Use a Ray Interaction with a UI | [unity-isdk-use-ray-with-ui](https://developers.meta.com/horizon/documentation/unity/unity-isdk-use-ray-with-ui) |

## Locomotion

| Page | Link |
|---|---|
| Locomotion Interactions | [unity-isdk-locomotion-interactions](https://developers.meta.com/horizon/documentation/unity/unity-isdk-locomotion-interactions) |
| Teleport Interaction | [unity-isdk-teleport-interaction](https://developers.meta.com/horizon/documentation/unity/unity-isdk-teleport-interaction) |
| Locomotion sample | [unity-sample-isdk-locomotion](https://developers.meta.com/horizon/documentation/unity/unity-sample-isdk-locomotion) |

## Hands, poses, visuals

| Page | Link |
|---|---|
| Detecting Poses | [unity-isdk-detecting-poses](https://developers.meta.com/horizon/documentation/unity/unity-isdk-detecting-poses) |
| Custom Hand Models | [unity-isdk-customize-hand-model](https://developers.meta.com/horizon/documentation/unity/unity-isdk-customize-hand-model) |
| Hand Visual | [unity-isdk-hand-visual](https://developers.meta.com/horizon/documentation/unity/unity-isdk-hand-visual) |
| OpenXR Hand | [unity-isdk-openxr-hand](https://developers.meta.com/horizon/documentation/unity/unity-isdk-openxr-hand) |
| Hand Tracking Overview (Core SDK) | [unity-handtracking-overview](https://developers.meta.com/horizon/documentation/unity/unity-handtracking-overview) |

## Sample scenes

| Page | Link |
|---|---|
| Example Scenes index | [unity-isdk-example-scenes](https://developers.meta.com/horizon/documentation/unity/unity-isdk-example-scenes) |
| DistanceGrabExamples | [unity-isdk-distance-grab-examples-scene](https://developers.meta.com/horizon/documentation/unity/unity-isdk-distance-grab-examples-scene) |
| TouchGrabExamples | [unity-isdk-touch-grab-examples-scene](https://developers.meta.com/horizon/documentation/unity/unity-isdk-touch-grab-examples-scene) |
| Interaction SDK in DroneRage | [unity-dronerage-interaction-sdk](https://developers.meta.com/horizon/documentation/unity/unity-dronerage-interaction-sdk) |

Samples install from **Package Manager > Meta XR Interaction SDK > Samples** and land under
`Assets/Samples/`.

## Design guidelines

Meta's human-interface standards. Consult these when choosing an interaction, not just when
implementing one.

- [Grab](https://developers.meta.com/horizon/design/grab_usage) · [Touch](https://developers.meta.com/horizon/design/touch_usage) · [Ray casting](https://developers.meta.com/horizon/design/raycasting_usage) · [Microgestures](https://developers.meta.com/horizon/design/design-microgestures)
- [Hands design](https://developers.meta.com/horizon/design/hands) · [Best Practices](https://developers.meta.com/horizon/design/hands-best-practices) · [Limitations & Mitigations](https://developers.meta.com/horizon/design/hands-limitations-mitigations)
- [Input mappings](https://developers.meta.com/horizon/design/interactions-input-mappings) · [Multimodality](https://developers.meta.com/horizon/design/interactions-multimodality) · [Comfort](https://developers.meta.com/horizon/design/comfort) · [Accessibility](https://developers.meta.com/horizon/design/accessibility)

## Docs vs. the installed package

Docs and package can drift - a page may describe a menu label, prefab name, wizard setting, or
dependency that differs from the version you have. When they disagree, **the installed package
wins for behavior** and the docs win for intent and design guidance.

Check the installed package before relying on a specific doc detail, and say which source you
used.
