# Controller-to-Hand Tracking Migration Guide

Step-by-step checklist for converting a controller-based Unity VR project to hand tracking.

## Phase 1: Assessment

- [ ] Audit all `OVRInput.Get()`, `OVRInput.GetDown()`, `OVRInput.GetUp()` calls
- [ ] List all controller button references (`OVRInput.Button.One`, `Axis2D.PrimaryThumbstick`, etc.)
- [ ] Identify Unity legacy Input system usage (`Input.GetButton`, `Input.GetAxis`)
- [ ] Map XR Interaction Toolkit components (`XRGrabInteractable`, `XRRayInteractor`)
- [ ] Catalog all grab/release mechanics and how they're triggered
- [ ] Document UI interaction patterns (raycasts, pointer clicks)
- [ ] Note any movement systems (thumbstick locomotion, snap turning)

## Phase 2: ISDK Setup

- [ ] Add Meta Interaction SDK (ISDK) package to the project
- [ ] Add `OVRHand` and `OVRHandPrefab` to the camera rig
- [ ] Configure hand tracking in `OVRManager` (set Hand Tracking Support to "Controllers and Hands" or "Hands Only")
- [ ] Add `HandRef` components for left and right hands
- [ ] Verify hand tracking works in the XR Simulator or on-device

## Phase 3: Core Interactions

### Grab Interactions
- [ ] Replace trigger-based grabs with `HandGrabInteractable` + `Grabbable`
- [ ] Add `HandGrabPose` components to define natural grab poses
- [ ] Configure `GrabInteractable` for objects that need distance grabbing
- [ ] Test pinch grab with both hands

### Poke / UI Interactions
- [ ] Replace ray-based UI with `PokeInteractable` on buttons
- [ ] Add `PokeInteractor` to index fingertip
- [ ] Implement hover states (scale up) and press states (scale down)
- [ ] Add audio feedback on hover and click
- [ ] Test menu navigation with both hands

### Object Manipulation
- [ ] Replace thumbstick rotation with direct hand rotation
- [ ] Add two-handed scaling support via `TwoHandGrabTransformer`
- [ ] Implement constrained rotation for dials/knobs using `OneGrabRotateTransformer`
- [ ] Add visual feedback (highlights, outlines) on hover

## Phase 4: Gesture-Based Features

- [ ] Identify gameplay actions that were on buttons (weapon switch, ability use, etc.)
- [ ] Design hand poses or gestures for each action
- [ ] Implement `HandPose` detection for custom gestures
- [ ] Add visual guides to teach users the gestures
- [ ] Implement palm-up menu summoning if applicable

## Phase 5: Movement

- [ ] Evaluate if thumbstick locomotion is needed or if room-scale/teleport suffices
- [ ] If locomotion needed: implement hand-gesture-based movement (e.g., arm swing, point-to-teleport with pinch)
- [ ] Replace snap turn with physical turning (encourage room-scale)
- [ ] If seated experience: implement gaze-based or hand-ray teleportation

## Phase 6: Polish

- [ ] Add hand presence visuals (hand mesh or ghost hands)
- [ ] Implement haptic-like feedback (visual pulses, audio cues) since hands have no haptics
- [ ] Add proximity highlights when hands approach interactable objects
- [ ] Test with various hand sizes and skin tones
- [ ] Verify no interactions require simultaneous hand actions that are physically uncomfortable
- [ ] Performance profile: ensure hand tracking + gameplay stays within frame budget

## Phase 7: Fallback & Accessibility

- [ ] Keep controller support as fallback (use `OVRInput.IsControllerConnected()` to detect)
- [ ] Add input method toggle in settings
- [ ] Test full gameplay flow with hands only
- [ ] Test full gameplay flow with controllers only
- [ ] Document any interactions that work differently between input methods

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| Assuming hand tracking is always available | Check `OVRHand.IsTracked` before processing |
| Tiny interaction targets | Make grab volumes generous (at least 5cm) |
| Requiring precise finger poses | Use relaxed pose matching with high thresholds |
| Ignoring hand occlusion | Hands can be occluded by objects — add re-acquisition grace periods |
| No visual feedback | Hands lack haptics — always provide visual and audio feedback |
| Two-handed interactions blocking single-hand use | Design interactions to work one-handed first, enhance with two hands |
