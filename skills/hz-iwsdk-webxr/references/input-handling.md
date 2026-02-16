# Input Handling

This reference covers XR input handling in IWSDK, including controllers, hand tracking, pointer interaction, grabbing, and locomotion systems.

## XR Input Stack

IWSDK organizes input in a layered architecture:

```
XROrigin
  └── XRInputManager
        ├── Head (gaze)
        ├── Left Controller / Left Hand
        │     ├── RayPointer
        │     ├── GrabPointer
        │     └── StatefulGamepad
        └── Right Controller / Right Hand
              ├── RayPointer
              ├── GrabPointer
              └── StatefulGamepad
```

The `XROrigin` represents the user's physical space. The `XRInputManager` manages all input sources and their associated pointers.

## Input Sources

### Accessing Input Sources

```typescript
import { XRInputManager, InputSource } from '@meta-quest/iwsdk/input';

const inputManager = world.get(XRInputManager);

// Access specific input sources
const leftController = inputManager.getInputSource(InputSource.LeftController);
const rightController = inputManager.getInputSource(InputSource.RightController);
const leftHand = inputManager.getInputSource(InputSource.LeftHand);
const rightHand = inputManager.getInputSource(InputSource.RightHand);
const head = inputManager.getInputSource(InputSource.Head);
```

### Checking Input Source Availability

Controllers and hands are mutually exclusive -- when the user puts down controllers and uses hands, the controller input sources become inactive:

```typescript
if (leftHand.isActive) {
  // Hand tracking is active for the left hand
}

if (leftController.isActive) {
  // Controller is active for the left hand
}
```

## Pointer Types

### RayPointer

The `RayPointer` casts a ray from the controller or hand into the scene. It is used for pointing at UI panels, selecting objects, and interacting from a distance.

```typescript
import { RayPointer } from '@meta-quest/iwsdk/input';

const ray = rightController.getPointer(RayPointer);

if (ray.isHitting) {
  const hitPoint = ray.hitPoint;       // THREE.Vector3
  const hitNormal = ray.hitNormal;     // THREE.Vector3
  const hitEntity = ray.hitEntity;     // Entity or null
  const hitDistance = ray.hitDistance;  // number (meters)
}
```

### GrabPointer

The `GrabPointer` detects when the user grabs an object using a grip button (controller) or pinch/grab gesture (hand).

```typescript
import { GrabPointer } from '@meta-quest/iwsdk/input';

const grab = rightController.getPointer(GrabPointer);

if (grab.isGrabbing) {
  const grabbedEntity = grab.grabbedEntity;
  const grabOffset = grab.grabOffset;   // Offset from grab point to entity origin
}
```

### MultiPointer

The `MultiPointer` combines multiple pointer types and resolves which one is active based on context and priority:

```typescript
import { MultiPointer } from '@meta-quest/iwsdk/input';

const multiPointer = rightController.getPointer(MultiPointer);
const activePointer = multiPointer.activePointer; // The currently dominant pointer
```

## Grabbing System

IWSDK provides a built-in grabbing system for picking up and manipulating objects.

### Making an Entity Grabbable

```typescript
import { Grabbable } from '@meta-quest/iwsdk/interaction';

const cube = world.createEntity();
cube.addComponent(Grabbable, {
  grabType: 'one-hand',   // 'one-hand', 'two-hand', or 'both'
  throwable: true,         // Allow throwing when released
  snapToHand: false,       // If true, object snaps to hand position
});
cube.addObject3D(mesh);
```

### One-Hand Grab

The user grabs an object with one hand and can move and rotate it freely:

```typescript
cube.addComponent(Grabbable, {
  grabType: 'one-hand',
  throwable: true,
});
```

### Two-Hand Grab

The user grabs with both hands simultaneously to scale and rotate the object:

```typescript
cube.addComponent(Grabbable, {
  grabType: 'two-hand',
  scalable: true,          // Allow scaling with two-hand distance
  minScale: 0.1,
  maxScale: 5.0,
});
```

### Distance Grab

The user can grab objects from afar using the ray pointer. The object flies to the user's hand:

```typescript
cube.addComponent(Grabbable, {
  grabType: 'one-hand',
  distanceGrab: true,
  distanceGrabRange: 10.0,   // Maximum grab distance in meters
  distanceGrabSpeed: 8.0,    // Speed the object flies to hand
});
```

### Grab Events

Listen for grab lifecycle events in a system:

```typescript
import { GrabEvent } from '@meta-quest/iwsdk/interaction';

const grabEventQuery = createQuery({ all: [Grabbable, GrabEvent] });

export const GrabHandlerSystem = createSystem({
  name: 'grabHandler',
  queries: [grabEventQuery],
  execute: (world, queries) => {
    for (const entity of queries.get(grabEventQuery)) {
      const event = entity.get(GrabEvent);
      switch (event.type) {
        case 'grab-start':
          console.log('Object grabbed by', event.inputSource);
          break;
        case 'grab-end':
          console.log('Object released');
          break;
      }
    }
  },
});
```

## Locomotion

IWSDK includes built-in locomotion systems that can be enabled and configured.

### Teleport

Parabolic arc teleportation with configurable range and visual indicator:

```typescript
import { TeleportSystem } from '@meta-quest/iwsdk/locomotion';

world.registerSystem(TeleportSystem, {
  inputSource: InputSource.LeftController,
  activateButton: 'thumbstick-forward',
  maxDistance: 10.0,          // Maximum teleport range in meters
  arcColor: '#00aaff',        // Color of the parabolic arc
  validColor: '#00ff00',      // Indicator color on valid landing
  invalidColor: '#ff0000',    // Indicator color on invalid surface
  navMeshQuery: navMeshQuery,  // Optional: restrict to nav mesh
});
```

### Slide (Smooth Locomotion)

Thumbstick-based smooth movement:

```typescript
import { SlideSystem } from '@meta-quest/iwsdk/locomotion';

world.registerSystem(SlideSystem, {
  inputSource: InputSource.LeftController,
  speed: 2.0,                  // Meters per second
  sprintMultiplier: 1.8,       // Speed multiplier when thumbstick fully pressed
  directionReference: 'head',  // 'head' or 'controller'
});
```

### Snap Turn

Discrete rotation for comfort:

```typescript
import { SnapTurnSystem } from '@meta-quest/iwsdk/locomotion';

world.registerSystem(SnapTurnSystem, {
  inputSource: InputSource.RightController,
  angle: 45,                   // Degrees per snap
  deadzone: 0.5,               // Thumbstick deadzone threshold
});
```

### Smooth Turn

Continuous rotation:

```typescript
import { SmoothTurnSystem } from '@meta-quest/iwsdk/locomotion';

world.registerSystem(SmoothTurnSystem, {
  inputSource: InputSource.RightController,
  speed: 90,                   // Degrees per second
  deadzone: 0.2,
});
```

## Gamepad API

The `StatefulGamepad` wraps the WebXR Gamepad API and provides edge-triggered button events (pressed, released) in addition to continuous state.

### Reading Button State

```typescript
import { StatefulGamepad } from '@meta-quest/iwsdk/input';

const gamepad = rightController.get(StatefulGamepad);

// Continuous state
const triggerValue = gamepad.getButtonValue('trigger');      // 0.0 to 1.0
const gripValue = gamepad.getButtonValue('grip');            // 0.0 to 1.0
const thumbstickX = gamepad.getAxis('thumbstick-x');         // -1.0 to 1.0
const thumbstickY = gamepad.getAxis('thumbstick-y');         // -1.0 to 1.0

// Edge-triggered (true only on the frame the button state changes)
if (gamepad.wasPressed('trigger')) {
  console.log('Trigger pressed this frame');
}

if (gamepad.wasReleased('trigger')) {
  console.log('Trigger released this frame');
}

if (gamepad.wasPressed('a-button')) {
  console.log('A button pressed');
}
```

### Button Names

| Button Name         | Controller Location                      |
| ------------------- | ---------------------------------------- |
| `'trigger'`         | Index finger trigger                     |
| `'grip'`            | Side grip button                         |
| `'a-button'`        | A button (right controller)              |
| `'b-button'`        | B button (right controller)              |
| `'x-button'`        | X button (left controller)               |
| `'y-button'`        | Y button (left controller)               |
| `'thumbstick'`      | Thumbstick press                         |
| `'menu'`            | Menu button (left controller)            |

### Axis Names

| Axis Name           | Description                              |
| ------------------- | ---------------------------------------- |
| `'thumbstick-x'`    | Thumbstick horizontal (-1 left, +1 right) |
| `'thumbstick-y'`    | Thumbstick vertical (-1 down, +1 up)     |

## Best Practices

- **Support both controllers and hands.** Not all users have controllers available. Design interactions that work with both input methods.
- **Provide visual feedback for pointers.** Show a visible ray and cursor dot so the user knows where they are pointing.
- **Use appropriate grab types.** One-hand grab for small objects, two-hand grab for large objects that need precise positioning.
- **Add deadzones to thumbstick input.** Raw thumbstick values drift near zero. Use a deadzone of 0.15-0.25 to prevent unintended movement.
- **Offer locomotion options.** Some users prefer teleport (less motion sickness), others prefer smooth locomotion. Provide a settings menu to choose.
- **Test hand tracking precision.** Hand tracking is less precise than controllers. Make interactive targets (buttons, grab zones) larger when hand tracking is active.
- **Use edge-triggered events for actions.** Use `wasPressed()` for discrete actions (fire, jump) and continuous values for analog input (throttle, steering).
