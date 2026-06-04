# Smart Camera SDK

**Status:** Binary pending. Gradle coordinate planned but not yet published: `com.facebook.portal:smartcamera:1.1.+`. Until it ships, this file describes the API surface so you can sketch integrations now and wire up the dependency later.

## What it is

Portal's camera has a system service that owns face/body detection, subject tracking, and framing. By default, the camera "auto-frames" — it pans, zooms, and crops to keep people centered.

Apps that just want a video feed use the standard `Camera2` API and get whatever framing the system has chosen.

Apps that want **control over framing** use the Smart Camera SDK: request an exclusive session, pick a `ModeSpec`, and the service handles the actual camera. The app doesn't move pixels — it tells the service what kind of behavior it wants.

This is the only thing on Portal that isn't standard Android.

## Modes (`ModeSpec`)

- **`DefaultAuto`** — what the system does normally: pan and zoom to keep visible people in frame, transition smoothly between subjects.
- **`BasicSpotlight`** — single-subject framing. Picks one person and tracks them.
- **`Desk`** — sit-at-a-desk framing. Tunable sensitivity, transition speed, framing tightness. Optimized for one person at a fixed distance.
- **`Meeting`** — wide group framing. Pulls back to include everyone.
- **`Fixed`** — manual crop. You pass center (x, y) and scale; no auto-tracking.

## API shape (sketch)

```kotlin
// 1. Build a factory
val factory = SmartCameraControlConnectionFactory(context)

// 2. Connect (async)
val connection: ControlConnection = factory.connect().await()

// 3. Request exclusive control
val session: ControlSession = connection.requestControls()

// 4. Pick a mode
session.setMode(ModeSpec.DefaultAuto.create())
// or
session.setMode(
    ModeSpec.Desk.newBuilder()
        .setAdditionalStableFramingTightness(0.3f)
        .setTrackingSensitivityPercentage(0.5f)
        .setTransitionSpeedPercentage(0.7f)
        .build()
)
// or
session.setMode(
    ModeSpec.Fixed.newBuilder()
        .setCameraFrameCrop(0.5f, 0.5f, 0.8f)  // center, 80% scale
        .build()
)

// 5. Tear down
session.close()
connection.close()
```

## Exceptions to handle

- `ServiceOutOfDateException` — the Smart Camera service on the device is older than the SDK expects
- `OwnershipRevokedException` — another app took the session (only one app can hold control at a time)
- `ConnectionClosedException` — service died or was killed
- `SmartCameraAccessException` — generic access failure (permission, binding, etc.)

## Constants used internally (subject to change)

- Bind package: `com.facebook.portal.aiservice`
- Bind action: `com.facebook.portal.SMART_CAMERA_EXTERNAL_CONTROL_SERVICE`
- Required permission: `com.facebook.portal.permission.SMART_CAMERA_CONTROL`

## Streaming the actual video frames

Use the standard `Camera2` API. The Smart Camera SDK controls *framing* — what region of the sensor gets surfaced. The pixels come out of `Camera2` as normal.

## What this lets you do

- A photobooth that locks to one face (`BasicSpotlight`)
- A workout app that keeps the whole user in frame (`Meeting`, or `Fixed` with a wide scale)
- A storytime app that follows the reader (`Desk`)
- A monitoring app with a fixed crop on a doorway (`Fixed`)
- A presentation app that frames the speaker but lets you toggle to whiteboard via `Fixed`
