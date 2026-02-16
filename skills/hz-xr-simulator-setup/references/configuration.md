# XR Simulator Configuration

## Environment Setup

### Room Configuration

The XR Simulator uses a synthetic environment that represents the physical space around the user. You can configure:

- **Room size**: Define width, depth, and height of the simulated play space. Default is 3m x 3m x 2.5m.
- **Room shape**: Rectangular (default), L-shaped, or custom polygon.
- **Furniture placement**: Add virtual furniture objects (tables, chairs, couches) to test scene understanding and spatial awareness features.
- **Wall and floor materials**: Configure surface textures that affect passthrough simulation appearance.

### Guardian Boundary Simulation

- The simulator generates a guardian boundary matching the configured room dimensions.
- You can adjust the boundary shape independently of the room if needed.
- Test guardian proximity warnings by moving the simulated headset toward the boundary edges.
- Configure stationary vs. room-scale boundary modes.

### Lighting Conditions

- Set ambient lighting level (bright, normal, dim, dark) to test how your app handles varying illumination.
- Useful for mixed reality apps that adapt to real-world lighting.

### Passthrough Simulation

- Enable synthetic passthrough to test mixed reality features.
- The simulator provides a rendered view of the synthetic room as the passthrough feed.
- Scene understanding APIs (plane detection, mesh generation, semantic labeling) return data based on the configured room geometry.
- Anchor placement and spatial anchor persistence can be tested in the simulated environment.

## Input Mapping

### Head Tracking (Mouse)

| Action | Input |
|---|---|
| Rotate head (yaw/pitch) | Hold right mouse button + move mouse |
| Reset head orientation | R key |
| Move forward | W |
| Move backward | S |
| Strafe left | A |
| Strafe right | D |
| Move up | E |
| Move down | Q |

Mouse sensitivity and movement speed can be adjusted in the simulator settings or JSON configuration file.

### Controller Simulation (Keyboard)

Left controller mapping:

| Button | Key |
|---|---|
| Thumbstick click | F |
| X button | X |
| Y button | C |
| Menu button | Tab |
| Trigger | Left mouse button |
| Grip | G |
| Thumbstick axes | Arrow keys or IJKL |

Right controller mapping:

| Button | Key |
|---|---|
| Thumbstick click | ; (semicolon) |
| A button | N |
| B button | M |
| Trigger | Right mouse button |
| Grip | H |
| Thumbstick axes | Numpad arrows or custom |

These default mappings can be remapped in the JSON configuration file.

### Hand Tracking Simulation

- **Predefined hand poses**: Trigger common poses with number keys:
  - `1` -- Open hand
  - `2` -- Pinch
  - `3` -- Grab / Fist
  - `4` -- Point
  - `5` -- Thumbs up
- **Gesture sequences**: Define sequences of poses with timing in the JSON configuration to simulate gestures like pinch-and-drag or swipe.
- **Hand position**: Move simulated hands using dedicated key groups or by switching active hand with a toggle key.

## Feature Flags

The following features can be toggled on or off in the simulator settings panel or JSON configuration:

| Feature | Default | Description |
|---|---|---|
| Hand tracking | Enabled | Simulate hand tracking input alongside or instead of controllers |
| Eye tracking | Disabled | Simulate eye gaze direction (follows head direction by default) |
| Passthrough | Disabled | Enable synthetic passthrough feed for MR testing |
| Controller type | Touch Plus | Select between Touch Pro and Touch Plus controller models |
| Body tracking | Disabled | Simulate upper body tracking |
| Face tracking | Disabled | Simulate facial expression tracking |
| Scene understanding | Disabled | Enable spatial data APIs (planes, meshes, anchors) |
| Depth API | Disabled | Enable simulated depth estimation data |

## JSON Configuration File

### File Location

The simulator reads its configuration from a JSON file:

- **Windows**: `%LOCALAPPDATA%\Meta\XRSimulator\config.json`
- **macOS**: `~/Library/Application Support/Meta/XRSimulator/config.json`

If the file does not exist, the simulator creates one with default values on first launch.

### Key Settings

```json
{
  "room": {
    "width": 3.0,
    "depth": 3.0,
    "height": 2.5,
    "shape": "rectangular",
    "furniture": []
  },
  "input": {
    "mouseSensitivity": 1.0,
    "movementSpeed": 1.5,
    "controllerType": "touch_plus"
  },
  "features": {
    "handTracking": true,
    "eyeTracking": false,
    "passthrough": false,
    "sceneUnderstanding": false
  },
  "recording": {
    "outputDirectory": "./recordings",
    "autoRecord": false
  }
}
```

### Custom Room Definitions

Add furniture and obstacles to the room configuration:

```json
{
  "room": {
    "width": 4.0,
    "depth": 5.0,
    "height": 2.8,
    "furniture": [
      { "type": "table", "position": [1.0, 0.0, 2.0], "rotation": 0, "scale": [1.0, 1.0, 1.0] },
      { "type": "chair", "position": [1.5, 0.0, 2.5], "rotation": 45, "scale": [1.0, 1.0, 1.0] },
      { "type": "couch", "position": [-1.0, 0.0, 3.0], "rotation": 90, "scale": [1.2, 1.0, 1.0] }
    ]
  }
}
```

## Multi-Instance Configuration

For multiplayer testing, you can run multiple simulator instances:

1. Create separate configuration files for each instance (e.g., `config_player1.json`, `config_player2.json`).
2. Launch each instance with the `--config` flag pointing to the appropriate file.
3. Each instance can have different room positions, input mappings, and feature flags.
4. Ensure your application's networking layer treats each instance as a separate user.
5. This approach is useful for testing shared spaces, co-location, and multiplayer interactions.
