# Controller Input (OVRInput) Reference

OVRInput is Meta's unified API for controller input and tracking in Unity, designed for Meta Quest Touch controllers.

## Important Note

For new projects, Meta recommends using Unity's Input System Package instead of OVRInput. OVRInput is maintained for legacy support.

## Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **Source file**: `Scripts/OVRInput.cs`
- **Query methods**: grep for `public static bool Get`, `public static bool GetDown`, `public static bool GetUp`
- **Controller enum**: grep for `enum Controller` in OVRInput.cs
- **Button/Touch/Axis enums**: grep for `enum Button`, `enum Touch`, `enum NearTouch`, `enum Axis1D`, `enum Axis2D`
- **Raw mappings**: grep for `enum RawButton`, `enum RawTouch`, `enum RawAxis1D`, `enum RawAxis2D`
- **Pose tracking**: grep for `GetLocalControllerPosition`, `GetLocalControllerRotation`
- **State queries**: grep for `GetActiveController`, `GetConnectedControllers`

## Setup

1. Install Meta XR Interaction SDK
2. Ensure OVRManager exists in the scene (on OVRCameraRig)
3. Call `OVRInput.Update()` at the start of `Update()`
4. Call `OVRInput.FixedUpdate()` at the start of `FixedUpdate()`

## Key Concepts

- **Virtual mappings** (Button, Touch, NearTouch, Axis1D, Axis2D) — platform-agnostic
- **Raw mappings** (RawButton, RawTouch, etc.) — direct hardware access
- **Controller designations** — Touch (combined pair), LTouch/RTouch (individual), Active (most recent)
- **Pose tracking** — positions/rotations relative to initial center eye pose, predicted in sync with headset

## Touch Sensor States

Controllers support multiple finger detection states:
1. **NearTouch = false**: Finger fully removed from surface
2. **NearTouch = true**: Finger approaching, in close proximity
3. **Touch = true**: Finger making physical contact
4. **Button = true**: Finger pressing the control

## Recentering

```csharp
OVRManager.display.RecenterPose();  // Reset head + controller poses
```

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-ovrinput
- API: https://developers.meta.com/horizon/reference/unity/latest/class_o_v_r_input
