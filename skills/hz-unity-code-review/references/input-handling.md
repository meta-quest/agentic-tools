# Input Handling for Unity on Meta Quest

This document covers controller input, hand tracking, and eye tracking implementation for Quest applications in Unity.

## Controller Input

Quest controllers are the primary input method for most VR applications. Use the `OVRInput` API instead of Unity's legacy `Input` system for reliable Quest-specific input handling.

### Basic Controller Input

```csharp
using UnityEngine;

public class ControllerInput : MonoBehaviour
{
    void Update()
    {
        // Button presses
        if (OVRInput.GetDown(OVRInput.Button.One))  // A button (right) or X button (left)
            OnPrimaryButtonPressed();

        if (OVRInput.GetDown(OVRInput.Button.Two))  // B button (right) or Y button (left)
            OnSecondaryButtonPressed();

        // Triggers (analog)
        float indexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        float handTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);

        // Thumbstick
        Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Specific hand
        float rightTrigger = OVRInput.Get(
            OVRInput.Axis1D.SecondaryIndexTrigger,
            OVRInput.Controller.RTouch
        );

        // Thumbstick click
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
            OnThumbstickClicked();
    }
}
```

### Controller Button Mapping

| OVRInput Button | Right Controller | Left Controller |
|----------------|-----------------|-----------------|
| Button.One | A | X |
| Button.Two | B | Y |
| Button.Start | Menu (right) | Menu (left) |
| PrimaryIndexTrigger | Index trigger | Index trigger |
| PrimaryHandTrigger | Grip | Grip |
| PrimaryThumbstick | Thumbstick axis | Thumbstick axis |

### Haptic Feedback

```csharp
// Trigger haptic feedback
// Parameters: frequency (0-1), amplitude (0-1), duration in seconds
OVRInput.SetControllerVibration(0.5f, 0.7f, OVRInput.Controller.RTouch);

// Short pulse for button feedback
public void HapticPulse(OVRInput.Controller controller, float amplitude = 0.5f)
{
    OVRInput.SetControllerVibration(1.0f, amplitude, controller);
    StartCoroutine(StopHaptics(controller, 0.1f));
}

private System.Collections.IEnumerator StopHaptics(OVRInput.Controller controller, float delay)
{
    yield return new WaitForSeconds(delay);
    OVRInput.SetControllerVibration(0, 0, controller);
}
```

## Hand Tracking

Quest supports optical hand tracking as an alternative to controllers. Use `OVRHand` and `OVRSkeleton` for hand pose data.

### Setup

1. Enable hand tracking in **Player Settings > XR Plug-in Management > Oculus > Hand Tracking Support**
2. Set to "Controllers and Hands" to support both input methods
3. Add `OVRHand` and `OVRSkeleton` components to your hand anchors

### Reading Hand Data

```csharp
using UnityEngine;

public class HandTrackingInput : MonoBehaviour
{
    [SerializeField] private OVRHand _rightHand;
    [SerializeField] private OVRHand _leftHand;
    [SerializeField] private OVRSkeleton _rightSkeleton;

    void Update()
    {
        if (_rightHand == null) return;

        // Check if hand tracking is active
        bool isTracked = _rightHand.IsTracked;
        float confidence = _rightHand.HandConfidence == OVRHand.TrackingConfidence.High ? 1f : 0.5f;

        // Pinch detection (thumb + index finger)
        bool isPinching = _rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        float pinchStrength = _rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        // Check other finger pinches
        bool isMiddlePinching = _rightHand.GetFingerIsPinching(OVRHand.HandFinger.Middle);

        // Get specific bone positions from skeleton
        if (_rightSkeleton != null && _rightSkeleton.IsDataValid)
        {
            var indexTip = _rightSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip];
            Vector3 indexTipPosition = indexTip.Transform.position;
        }
    }
}
```

### Gesture Recognition

```csharp
public class GestureDetector : MonoBehaviour
{
    [SerializeField] private OVRHand _hand;
    [SerializeField] private OVRSkeleton _skeleton;

    // Detect a pointing gesture (index extended, other fingers curled)
    public bool IsPointing()
    {
        if (!_hand.IsTracked) return false;

        bool indexExtended = !_hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        bool middleCurled = _hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) > 0.8f;
        bool ringCurled = _hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) > 0.8f;

        return indexExtended && middleCurled && ringCurled;
    }

    // Get the pointing direction from the index finger
    public Ray GetPointingRay()
    {
        var indexTip = _skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip];
        var indexMiddle = _skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index3];

        Vector3 origin = indexTip.Transform.position;
        Vector3 direction = (indexTip.Transform.position - indexMiddle.Transform.position).normalized;

        return new Ray(origin, direction);
    }
}
```

## Eye Tracking

Eye tracking is available on Quest Pro and Quest 3. It requires explicit user permission and must be declared in the app manifest.

### Setup

1. Enable eye tracking in **Player Settings > XR Plug-in Management > Oculus > Eye Tracking Support**
2. Add the `com.oculus.permission.EYE_TRACKING` permission to the Android manifest
3. Request permission at runtime before accessing eye tracking data

### Reading Eye Gaze

```csharp
using UnityEngine;

public class EyeTrackingInput : MonoBehaviour
{
    [SerializeField] private OVREyeGaze _leftEyeGaze;
    [SerializeField] private OVREyeGaze _rightEyeGaze;

    private bool _hasPermission;

    async void Start()
    {
        // Request eye tracking permission
        _hasPermission = await OVRPermissionsRequester.Request(
            OVRPermissionsRequester.Permission.EyeTracking
        );
    }

    void Update()
    {
        if (!_hasPermission) return;

        // Check if eye tracking is valid
        if (_leftEyeGaze != null && _leftEyeGaze.EyeTrackingEnabled)
        {
            // Get gaze direction (in world space)
            Vector3 gazeDirection = _leftEyeGaze.transform.forward;
            Vector3 gazeOrigin = _leftEyeGaze.transform.position;

            // Confidence
            float confidence = _leftEyeGaze.Confidence;

            // Raycast from gaze
            if (Physics.Raycast(gazeOrigin, gazeDirection, out RaycastHit hit, 100f))
            {
                // User is looking at hit.collider.gameObject
                OnGazeHit(hit);
            }
        }
    }

    private void OnGazeHit(RaycastHit hit)
    {
        // Handle gaze-based interaction
    }
}
```

### Eye Tracking Best Practices

- Always check permission before accessing eye tracking data
- Provide a fallback interaction method for users who deny the permission
- Use eye tracking as a complement to other input, not the sole input method
- Smooth gaze data to reduce jitter (apply a low-pass filter)
- Respect user privacy — do not store raw eye tracking data

## Best Practices

### Use OVRInput, Not Unity Input

```csharp
// BAD: Unity's legacy Input system does not reliably map Quest buttons
if (Input.GetButtonDown("Fire1")) { }

// GOOD: OVRInput is purpose-built for Quest
if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) { }
```

### Cache Input References

```csharp
// BAD: Searching for components every frame
void Update()
{
    var hand = FindObjectOfType<OVRHand>();
    if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index)) { }
}

// GOOD: Cache at initialization
private OVRHand _hand;

void Start()
{
    _hand = GetComponentInChildren<OVRHand>();
}

void Update()
{
    if (_hand != null && _hand.GetFingerIsPinching(OVRHand.HandFinger.Index)) { }
}
```

### Handle Controller and Hand Switching

```csharp
public class InputManager : MonoBehaviour
{
    public bool IsUsingHands { get; private set; }

    void Update()
    {
        // Detect which input method is active
        var activeController = OVRInput.GetActiveController();
        IsUsingHands = (activeController == OVRInput.Controller.Hands);

        if (IsUsingHands)
            HandleHandInput();
        else
            HandleControllerInput();
    }

    private void HandleHandInput() { /* hand tracking logic */ }
    private void HandleControllerInput() { /* controller logic */ }
}
```

### Support Both Hand Dominance

Design interactions that work for both left-handed and right-handed users. Do not hard-code interactions to a specific hand.

### Implement Haptic Feedback

Haptic feedback is critical for VR immersion. Provide haptic cues for:
- Button presses and UI interactions
- Object grab and release
- Collision events
- Confirmation of actions

## Meta XR Interaction SDK

For new projects, consider using the **Meta XR Interaction SDK** instead of raw `OVRInput`. It provides higher-level abstractions:

- **Interactable/Interactor pattern**: Objects declare what interactions they support
- **Grab interactions**: Built-in grab with physics-based hand movement
- **Poke interactions**: Direct touch for UI panels
- **Ray interactions**: Pointer-based interaction for distant objects
- **Locomotion**: Built-in teleportation and smooth locomotion
- **Body tracking**: Full body pose estimation

The Interaction SDK handles controller/hand switching, input abstraction, and common interaction patterns automatically, reducing boilerplate code.
