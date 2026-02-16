# Debugging

This reference covers debugging tools and techniques for Meta Spatial SDK applications, including the Data Model Inspector, OVR Metrics Tool, logcat filtering, hzdb integration, and common issue diagnosis.

## Data Model Inspector (DMI)

The Data Model Inspector is a real-time ECS debugging tool integrated into Android Studio via the Meta Horizon plugin. It provides a live view of all entities and their components in the running application.

### Accessing DMI

1. Connect your Quest device via USB
2. Launch your Spatial SDK app on the device
3. In Android Studio, open the Data Model Inspector panel (View > Tool Windows > Data Model Inspector)
4. Select your running application process

### Capabilities

- **Entity browser**: view all active entities in the DataModel, listed by ID and name
- **Component inspector**: select an entity to see all attached components and their current attribute values
- **Live updates**: attribute values update in real time as the application runs
- **Entity search**: filter entities by name, component type, or ID
- **Entity creation/destruction monitoring**: see when entities are created or destroyed

### Using DMI Effectively

```
Example DMI output:

Entity #42 "robot_arm"
  ├── Transform
  │     position: (1.2, 0.8, -2.1)
  │     rotation: (0, 0.707, 0, 0.707)
  │     scale: (1.0, 1.0, 1.0)
  ├── Mesh
  │     uri: apk:///models/robot_arm.glb
  ├── Grabbable
  │     enabled: true
  └── Health
        current: 85
        max: 100
```

DMI is invaluable for verifying that:

- Entities have the expected components attached
- Component attribute values match expectations
- Entities are being created and destroyed at the right times
- Transform positions match what you see in the headset

## OVR Metrics Tool

The OVR Metrics Tool provides a real-time performance overlay displayed inside the headset, showing key metrics like frame rate, CPU/GPU utilization, and memory usage.

### Enabling OVR Metrics Tool

The performance overlay can be enabled through Meta Quest Developer Hub (MQDH) or via ADB shell commands:

```bash
# Enable the performance overlay via ADB
hzdb adb shell setprop debug.oculus.perf.level 1
```

### Key Metrics to Monitor

| Metric            | Target                   | Warning Threshold           |
| ----------------- | ------------------------ | --------------------------- |
| App FPS           | 72/90/120 Hz (matches refresh) | Below target consistently  |
| CPU frame time    | Under frame budget       | Above 11ms (90 Hz)          |
| GPU frame time    | Under frame budget       | Above 11ms (90 Hz)          |
| Draw calls        | Varies by workload (80-600 Quest 2, 200-1000 Quest 3) | Above range for workload type |
| Triangles         | 750K-1M (Quest 2), 1M-2M (Quest 3)  | Above range                    |
| Memory usage      | Under 2 GB               | Above 3 GB                  |

### Frame Budget Reference

| Refresh Rate | Frame Budget |
| ------------ | ------------ |
| 72 Hz        | 13.9 ms      |
| 90 Hz        | 11.1 ms      |
| 120 Hz       | 8.3 ms       |

If either CPU or GPU frame time exceeds the budget, the application will drop frames.

## Logcat Debugging

Use logcat to monitor Spatial SDK runtime messages, warnings, and errors.

### Spatial SDK Log Tags

| Tag                | Description                                     |
| ------------------ | ----------------------------------------------- |
| `SpatialSDK`       | General Spatial SDK runtime messages             |
| `Compositor`       | VR compositor frame submission and layer info    |
| `Panel`            | Panel rendering, creation, and lifecycle events  |
| `Scene`            | Scene loading, environment configuration         |
| `DataModel`        | Entity and component operations                  |
| `ECS`              | System execution, queries, lifecycle             |
| `ISDK`             | Interaction SDK input events and state           |
| `Physics`          | Physics simulation messages                      |
| `MRUK`             | Mixed Reality Utility Kit scene understanding    |
| `GlXF`             | Scene file loading and parsing                   |

### Filtering for Spatial SDK Messages

```bash
# View all Spatial SDK messages
hzdb log | grep "SpatialSDK\|Panel\|Scene\|DataModel"

# View only errors
hzdb log --level E

# View panel-specific messages
hzdb adb logcat --tag Panel

# View scene loading messages
hzdb adb logcat --tag Scene --level I

# View ECS system execution issues
hzdb adb logcat --tag ECS --level W
```

## Screenshot and Video Capture

### Screenshot Capture

```bash
# Capture a screenshot of the current VR view
hzdb capture screenshot

# Capture and save to a specific location
hzdb capture screenshot --output ./debug_screenshot.png
```

### Performance Tracing

```bash
# Capture a Perfetto performance trace (default 5 seconds)
hzdb perf capture --duration 10000

# The trace is captured and saved with a session ID
# Load and analyze the trace using the session ID
hzdb perf load <session_id>
```

## hzdb Usage for Debugging

The `hzdb` CLI provides essential debugging commands for Spatial SDK development.

### Application Logs

```bash
# View recent logs
hzdb log

# Show only errors
hzdb log --level E

# Show warnings and above
hzdb log --level W

# Stream logs continuously
hzdb adb logcat --follow
```

### Device State

```bash
# Check device connection
hzdb device list

# Show device details (requires device_id argument)
hzdb device info <device_id>

# Check battery status
hzdb device battery

# Capture visual state for debugging
hzdb capture screenshot
```

### Build and Deploy Cycle

```bash
# Install a new build
hzdb app install app/build/outputs/apk/debug/app-debug.apk

# Force-stop and relaunch
hzdb app stop com.example.myspatialapp
hzdb app launch com.example.myspatialapp

# Clear app data (reset to fresh state)
hzdb app clear com.example.myspatialapp
```

### Performance Analysis

```bash
# Check current performance state
hzdb perf capture

# Monitor thermal state
hzdb adb logcat --tag ThermalService --level W
```

## Common Debugging Scenarios

### Panel Not Rendering

**Symptoms**: the panel entity exists but nothing is visible in the headset.

**Diagnostic steps**:

1. Verify the panel is registered with the correct name:
   ```bash
   hzdb log | grep -i "panel\|registration"
   ```
2. Check that `Entity.createPanelEntity("panel_name")` uses the exact name from `PanelRegistration`.
3. Verify the panel's `Transform` places it within the user's field of view (not behind or too far away).
4. Check for Compose or View exceptions in the logs:
   ```bash
   hzdb log --level E | grep -i "compose\|view\|layout\|render"
   ```
5. Ensure the panel dimensions are reasonable (not zero or negative).

### 3D Object Not Visible

**Symptoms**: an entity with a `Mesh` component exists but the model does not appear.

**Diagnostic steps**:

1. Verify the glTF file path is correct and the file is included in the APK assets:
   ```bash
   hzdb log | grep -i "mesh\|glb\|gltf\|asset\|load"
   ```
2. Check the `Transform` position -- the object might be too far away, behind the user, or inside the floor.
3. Verify the model scale is not zero or extremely small.
4. Check for mesh loading errors in the logs:
   ```bash
   hzdb log --level E | grep -i "mesh\|load\|file\|uri"
   ```
5. Ensure the glTF file is valid (test it in a glTF viewer like glTF Sample Viewer or Blender).

### Input Not Responding

**Symptoms**: entities do not respond to pointer or grab interactions.

**Diagnostic steps**:

1. Verify `SpatialFeature.INTERACTION` is enabled in `getSpatialFeatures()`.
2. Check that `IsdkSupportingSystems` is registered in `registerSystems()`.
3. Ensure interactive entities have a `Collider` component (required for raycasting).
4. Verify `InputListener` is attached to entities that need pointer events.
5. Check ISDK logs for errors:
   ```bash
   hzdb log | grep -i "isdk\|input\|pointer\|collider"
   ```
6. Ensure collider shapes match the visible geometry (a mismatched collider can cause misses).

### Physics Issues

**Symptoms**: physics simulation produces unexpected behavior (objects falling through floors, incorrect collisions, instability).

**Diagnostic steps**:

1. Verify `SpatialFeature.PHYSICS` is enabled.
2. Check that physics entities have both `Collider` and `RigidBody` components.
3. Ensure static objects (floors, walls) use `RigidBody(type = RigidBodyType.STATIC)`.
4. Check for physics errors in the logs:
   ```bash
   hzdb log | grep -i "physics\|collision\|rigidbody"
   ```
5. Verify scale is realistic -- physics simulation works best with objects between 0.1 and 10 meters.
6. Check that collider shapes are appropriate for the geometry.

### Performance Degradation

**Symptoms**: frame drops, judder, or low FPS reported by OVR Metrics Tool.

**Diagnostic steps**:

1. Check the current performance metrics:
   ```bash
   hzdb perf capture
   ```
2. Identify whether the bottleneck is CPU or GPU (check which frame time exceeds the budget).
3. For CPU bottlenecks:
   - Check the number of active systems and their execution time
   - Look for expensive queries or large entity counts
   - Profile with Perfetto for detailed CPU flame graphs
4. For GPU bottlenecks:
   - Reduce polygon count, draw calls, and texture resolution
   - Simplify shaders and reduce overdraw
   - Check for unnecessarily high panel DPI settings
5. Check for thermal throttling:
   ```bash
   hzdb adb logcat --tag ThermalService --level W
   ```
6. Capture a Perfetto trace for detailed analysis:
   ```bash
   hzdb perf capture --duration 10000
   # Then load and analyze the trace
   hzdb perf load <session_id>
   ```
