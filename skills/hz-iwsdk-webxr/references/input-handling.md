# Input Handling

This reference covers the current IWSDK input model for controllers, hand
tracking, pointers, grabbing, and locomotion.

The important high-level split is:

- `world.player` is the XR rig (`XROrigin`)
- `world.input` is the live `XRInputManager`
- `world.input.gamepads.left/right` expose controller state
- `world.input.multiPointers.left/right` manage ray, touch, and grab pointers
- entities become interactable through ECS tags such as `RayInteractable`,
  `PokeInteractable`, `OneHandGrabbable`, and `DistanceGrabbable`

## Enabling Input-Heavy Features

The most reliable way to enable higher-level input behavior is through
`World.create(...)` feature flags:

```typescript
import {
  ReferenceSpaceType,
  SessionMode,
  TurningMethod,
  World,
} from '@iwsdk/core';

const world = await World.create(container, {
  xr: {
    sessionMode: SessionMode.ImmersiveVR,
    referenceSpace: ReferenceSpaceType.LocalFloor,
    features: {
      handTracking: true,
      layers: true,
    },
  },
  features: {
    grabbing: { useHandPinchForGrab: true },
    locomotion: {
      comfortAssistLevel: 0.5,
      turningMethod: TurningMethod.SnapTurn,
    },
  },
});
```

This matches the way the current starter templates wire grabbing and locomotion.

## Input Sources And Primary Devices

### Accessing The Manager

```typescript
const input = world.input;
const leftGamepad = input.gamepads.left;
const rightGamepad = input.gamepads.right;
const leftPointer = input.multiPointers.left;
const rightPointer = input.multiPointers.right;
```

### Checking Which Device Is Primary

Controllers and hands can both be tracked, but each hand has a current primary
source:

```typescript
if (world.input.isPrimary('hand', 'left')) {
  // left hand is the active primary source
}

if (world.input.isPrimary('controller', 'right')) {
  // right controller is the active primary source
}
```

## Reading Controller State

`StatefulGamepad` adds edge-triggered helpers on top of the raw WebXR gamepad.

```typescript
import { AxesState, InputComponent } from '@iwsdk/core';

const gamepad = world.input.gamepads.right;

if (gamepad?.getButtonDown(InputComponent.Trigger)) {
  console.log('trigger pressed this frame');
}

if (gamepad?.getButtonPressed(InputComponent.Squeeze)) {
  console.log('grip is currently held');
}

const stick = gamepad?.getAxesValues(InputComponent.Thumbstick);
if (stick) {
  console.log(stick.x, stick.y);
}

if (gamepad?.getAxesState(InputComponent.Thumbstick) === AxesState.Left) {
  console.log('thumbstick is pushed left');
}
```

Useful current helpers:

- `getButtonDown(...)` / `getButtonUp(...)` for edge-triggered actions
- `getButtonPressed(...)` and `getButtonValue(...)` for continuous state
- `getAxesValues(...)` for analog stick vectors
- `getAxesState(...)` and `getAxesEnteringLeft/Right/Up/Down(...)` for
  locomotion and turn gestures

## Pointer Interaction

To make an entity respond to UI-style pointer events, add the relevant
interactable tags:

```typescript
import {
  Hovered,
  PokeInteractable,
  Pressed,
  RayInteractable,
  createSystem,
} from '@iwsdk/core';

const button = world.createTransformEntity(mesh);
button.addComponent(RayInteractable).addComponent(PokeInteractable);

export class ButtonFeedbackSystem extends createSystem({
  buttons: { required: [RayInteractable] },
}) {
  update() {
    this.queries.buttons.entities.forEach((entity) => {
      if (!entity.object3D) {
        return;
      }
      entity.object3D.scale.setScalar(entity.hasComponent(Pressed) ? 0.95 : 1);
      if (entity.hasComponent(Hovered)) {
        entity.object3D.position.z = -1.45;
      }
    });
  }
}
```

`Hovered` and `Pressed` are transient tags managed by `InputSystem`. They are
the easiest way to build declarative interaction feedback.

## Grabbing

For most projects, enable grabbing through `features.grabbing` on
`World.create(...)`. Then use the appropriate grabbable components on the
entities themselves.

### One-Hand Grab

```typescript
import { OneHandGrabbable } from '@iwsdk/core';

cube.addComponent(OneHandGrabbable, {
  rotate: true,
  translate: true,
});
```

### Two-Hand Grab

```typescript
import { TwoHandsGrabbable } from '@iwsdk/core';

cube.addComponent(TwoHandsGrabbable, {
  rotate: true,
  scale: true,
});
```

### Distance Grab

```typescript
import {
  DistanceGrabbable,
  Interactable,
  MovementMode,
} from '@iwsdk/core';

cube
  .addComponent(Interactable)
  .addComponent(DistanceGrabbable, {
    movementMode: MovementMode.MoveTowardsTarget,
    returnToOrigin: false,
  });
```

Use `MovementMode.MoveTowardsTarget` as the default starting point for
telekinetic grabs unless your app needs a different remote-manipulation feel.

## Locomotion

Current IWSDK locomotion is usually enabled through `features.locomotion`:

```typescript
import { TurningMethod, World } from '@iwsdk/core';

const world = await World.create(container, {
  features: {
    locomotion: {
      comfortAssistLevel: 0.5,
      turningMethod: TurningMethod.SnapTurn,
      enableJumping: true,
    },
  },
});
```

Under the hood, IWSDK exposes `TeleportSystem`, `SlideSystem`, and
`TurnSystem`, but most app code should start from the higher-level feature
configuration unless you are building a custom locomotion stack.

## Best Practices

- **Support both controllers and hands.** Use `world.input.isPrimary(...)` to
  adapt affordances when hand tracking becomes the main path.
- **Prefer ECS interactable tags over ad hoc raycasting.** `RayInteractable`,
  `PokeInteractable`, `Hovered`, and `Pressed` are the stable surface other
  systems build around.
- **Use built-in feature flags first.** `features.grabbing` and
  `features.locomotion` are safer starting points than bespoke low-level input
  wiring.
- **Offer comfort options.** Teleport vs slide, snap vs smooth turning, and
  comfort assist settings materially affect usability on Quest.
- **Test hand tracking precision on-device.** Desktop emulation is useful, but
  Quest hardware still reveals the real pinch, reach, and pointer feel.
