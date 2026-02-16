# Interaction SDK

This reference covers the Interaction SDK (ISDK) for handling spatial input in Meta Spatial SDK applications, including grabbables, pointer events, controller input, hand tracking, and haptics.

## Overview

The Interaction SDK (ISDK) is a device-agnostic input handling layer that provides a unified API for interacting with spatial content. It abstracts over controllers, hand tracking, and other input modalities so that applications can support multiple input methods without writing separate logic for each.

To enable ISDK, add the interaction feature to your activity:

```kotlin
override fun getSpatialFeatures(): List<SpatialFeature> {
  return listOf(
    SpatialFeature.INTERACTION
  )
}
```

And add the ISDK dependency to your Gradle build:

```kotlin
dependencies {
  implementation("com.meta.spatial:meta-spatial-sdk-isdk:latest")
}
```

## IsdkSupportingSystems

The easiest way to set up input handling is to register `IsdkSupportingSystems`, which automatically configures hand and controller interaction:

```kotlin
override fun registerSystems(): List<SystemBase> {
  return listOf(
    IsdkSupportingSystems(),
    // Your other systems...
  )
}
```

`IsdkSupportingSystems` creates the necessary entities and components for ray pointers, near-field interaction zones, and pointer visualizations for both hands and controllers.

## Grabbables

Make any entity grabbable by adding the `Grabbable` component:

```kotlin
val model = Entity.create(
  Mesh(Uri.parse("apk:///models/cube.glb")),
  Transform(Pose(Vector3(0f, 1f, -1.5f))),
  Grabbable()
)
```

### Grab Constraints

Use `IsdkGrabConstraints` to control how an entity behaves when grabbed:

```kotlin
val model = Entity.create(
  Mesh(Uri.parse("apk:///models/slider_handle.glb")),
  Transform(Pose(Vector3(0f, 1f, -1.5f))),
  Grabbable(),
  IsdkGrabConstraints(
    constrainPosition = true,   // Lock position axes
    constrainRotation = true,   // Lock rotation axes
    positionAxisLock = Vector3(1f, 0f, 0f),  // Only allow movement on X axis
    rotationAxisLock = Vector3.ZERO           // Lock all rotation
  )
)
```

### Two-Hand Grab

Entities can be grabbed with both hands simultaneously for scaling and rotation:

```kotlin
val resizable = Entity.create(
  Mesh(Uri.parse("apk:///models/photo_frame.glb")),
  Transform(Pose(Vector3(0f, 1.2f, -2f))),
  Grabbable(
    twoHandGrab = true,
    allowScaling = true,
    minScale = 0.5f,
    maxScale = 3.0f
  )
)
```

## Input Events

Use `InputListener` to respond to pointer events on entities:

```kotlin
class ButtonInteractionSystem : SystemBase() {
  override fun execute() {
    val query = Query.where { has(InteractiveButton.id, InputListener.id) }
    for (entity in query.eval()) {
      val listener = entity.getComponent<InputListener>()
      for (event in listener.getPointerEvents()) {
        when (event.type) {
          PointerEventType.HOVER_ENTER -> highlightButton(entity)
          PointerEventType.HOVER_EXIT -> unhighlightButton(entity)
          PointerEventType.SELECT -> activateButton(entity)
          PointerEventType.RELEASE -> deactivateButton(entity)
        }
      }
    }
  }
}
```

### Pointer Event Types

| Event Type      | Description                                           |
| --------------- | ----------------------------------------------------- |
| `HOVER_ENTER`   | Pointer ray enters the entity's collision volume       |
| `HOVER_EXIT`    | Pointer ray leaves the entity's collision volume       |
| `SELECT`        | User performs a select action (trigger press, pinch)   |
| `RELEASE`       | User releases the select action                       |
| `MOVE`          | Pointer moves while hovering over the entity           |

### Adding InputListener to Entities

```kotlin
val button = Entity.create(
  Mesh(Uri.parse("apk:///models/button.glb")),
  Transform(Pose(Vector3(0f, 1f, -1.5f))),
  Collider(ColliderShape.BOX),  // Required for raycasting
  InputListener()
)
```

A `Collider` component is required for an entity to receive pointer events, as the ray must intersect a collision shape.

## Pointer Types

ISDK supports multiple pointer modalities:

### Ray Pointer

The default interaction mode. A ray extends from the controller or hand and intersects with colliders in the scene. Best for interacting with distant objects and panels.

### Direct Touch

Near-field interaction where the user's finger or controller tip directly contacts an entity. Useful for buttons, sliders, and close-range interactions.

### Near-Field Interaction

A proximity-based interaction zone around the hand or controller. Entities within the zone can be grabbed or manipulated without precise aiming.

## Controller Input

Access raw controller state for custom input handling:

```kotlin
class ControllerInputSystem : SystemBase() {
  override fun execute() {
    val controller = getController(Handedness.RIGHT)

    // Buttons
    if (controller.isButtonPressed(ControllerButton.A)) {
      // A button is currently held down
    }
    if (controller.isButtonJustPressed(ControllerButton.B)) {
      // B button was pressed this frame
    }

    // Trigger (analog 0.0 to 1.0)
    val triggerValue = controller.getTriggerValue()
    if (triggerValue > 0.8f) {
      // Trigger is mostly pressed
    }

    // Grip (analog 0.0 to 1.0)
    val gripValue = controller.getGripValue()

    // Thumbstick (Vector2, range -1 to 1 on each axis)
    val thumbstick = controller.getThumbstickValue()
    movePlayer(thumbstick.x, thumbstick.y)
  }
}
```

### Controller Button Reference

| Button             | Description                          |
| ------------------ | ------------------------------------ |
| `A` / `X`          | Primary face buttons (right / left)  |
| `B` / `Y`          | Secondary face buttons               |
| `TRIGGER`          | Index finger trigger (analog)        |
| `GRIP`             | Side grip button (analog)            |
| `THUMBSTICK`       | Thumbstick press (click)             |
| `MENU`             | Menu button (left controller only)   |

## Hand Tracking

Access hand tracking data for gesture-based interactions:

```kotlin
class HandGestureSystem : SystemBase() {
  override fun execute() {
    val hand = getHand(Handedness.RIGHT)

    if (hand.isTracked) {
      // Get individual joint positions
      val indexTip = hand.getJointPose(HandJoint.INDEX_TIP)
      val thumbTip = hand.getJointPose(HandJoint.THUMB_TIP)

      // Detect pinch gesture
      val pinchDistance = Vector3.distance(indexTip.position, thumbTip.position)
      if (pinchDistance < 0.02f) {
        onPinchDetected(hand)
      }

      // Get pinch strength (0.0 to 1.0)
      val pinchStrength = hand.getPinchStrength()
    }
  }
}
```

### Hand Joints

The hand skeleton provides access to 26 joints per hand, including fingertips, knuckles, palm, and wrist. Key joints:

| Joint               | Description                    |
| -------------------- | ------------------------------ |
| `WRIST`             | Base of the hand               |
| `PALM`              | Center of the palm             |
| `THUMB_TIP`         | Tip of the thumb               |
| `INDEX_TIP`         | Tip of the index finger        |
| `MIDDLE_TIP`        | Tip of the middle finger       |
| `RING_TIP`          | Tip of the ring finger         |
| `LITTLE_TIP`        | Tip of the pinky finger        |

### Microgestures

ISDK can detect high-level gestures beyond simple pinch:

- **Pinch**: thumb and index finger touch
- **Point**: index finger extended, others curled
- **Open hand**: all fingers extended
- **Fist**: all fingers curled

## Haptics

Trigger haptic feedback on controllers to provide tactile responses:

```kotlin
class HapticFeedbackSystem : SystemBase() {

  fun playHapticClick(handedness: Handedness) {
    val controller = getController(handedness)
    controller.playHaptic(
      amplitude = 0.5f,    // Vibration strength (0.0 to 1.0)
      duration = 0.05f,    // Duration in seconds
      frequency = 160f     // Vibration frequency in Hz
    )
  }

  fun playHapticBuzz(handedness: Handedness) {
    controller.playHaptic(
      amplitude = 0.8f,
      duration = 0.2f,
      frequency = 320f
    )
  }
}
```

### Haptic Guidelines

- Use short, subtle haptics for confirmations (button press, hover enter)
- Use stronger haptics for important actions (grab, collision, error)
- Avoid continuous haptics for extended periods
- Different frequencies convey different sensations (low = rumble, high = buzz)

## Panels with ISDK

Panels automatically receive touch and pointer input from ISDK. Standard Android touch events are translated from spatial pointer interactions.

### Grabbable Panels

Make panels grabbable so users can reposition them:

```kotlin
val panelEntity = Entity.createPanelEntity(
  "movable_panel",
  Transform(Pose(Vector3(0f, 1.2f, -2f)))
)
panelEntity.setComponent(Grabbable())
```

### Touch Limiting

Control which interactions a panel responds to:

```kotlin
PanelRegistration("display_panel") {
  layoutParams = LayoutParams(400f, 300f, SpatialPanelLayoutParams.HORIZONTAL)
  interactionMode = PanelInteractionMode.POINTER_ONLY  // No direct touch
  panel {
    ReadOnlyDisplay()
  }
}
```

## Best Practices

- **Support both controllers and hands**: test your interactions with both input modalities. Use ISDK abstractions rather than checking specific device types.
- **Provide visual feedback**: highlight objects on hover, animate on select, show grab affordances. Users need spatial feedback to understand what is interactive.
- **Use large interaction targets**: minimum 4 cm for touch targets, minimum 2 cm for ray-based targets. Small targets are difficult to interact with in spatial environments.
- **Add colliders**: every entity that should receive input events needs a `Collider` component. Without it, rays and touches will pass through.
- **Handle input gracefully**: always check for null or missing input data. Hand tracking may lose visibility, controllers may disconnect.
- **Prefer ray interaction for distant objects**: direct touch requires reaching, which causes fatigue over time. Use ray pointers for objects beyond arm's reach.
