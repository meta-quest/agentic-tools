# XR Simulator Testing Workflows

## Basic Testing Workflow

Follow these steps each time you test your application in the XR Simulator:

1. **Configure the simulator environment**. Open your engine's XR Simulator settings and select the appropriate room layout, input mode (controllers or hands), and feature flags for the scenario you want to test.

2. **Set up input mapping**. Review the keyboard/mouse mapping. Adjust if your application requires specific controller buttons or hand poses that differ from defaults.

3. **Launch the app in simulator mode**. In Unity, enter Play Mode. In Unreal, use VR Preview. The simulator intercepts OpenXR calls and renders the scene.

4. **Navigate the scene with mouse and keyboard**. Use right-click + mouse to look around, WASD to move, and mapped keys to interact with objects.

5. **Test interactions**. Trigger controller buttons or hand poses to interact with UI elements, grab objects, or activate locomotion. Observe that your application responds correctly.

6. **Review logs and output**. Check the engine console or log files for warnings, errors, or unexpected behavior during the session.

## Common Testing Scenarios

### UI Interaction Testing

Test that menus, panels, buttons, and sliders respond to simulated input:

- Point the simulated controller ray at UI elements and press the trigger key.
- With hand tracking enabled, simulate a pinch gesture while the index finger ray intersects a button.
- Verify hover states, click feedback, and navigation between UI screens.
- Test UI at varying distances and angles from the user.

### Locomotion Testing

Validate movement systems in your application:

- **Teleport**: Aim with the thumbstick (mapped keys), press to show the arc, release to teleport. Confirm landing position and orientation are correct.
- **Smooth locomotion**: Use thumbstick-mapped keys for continuous movement. Check speed, direction relative to head or controller, and collision with obstacles.
- **Snap turn**: Press thumbstick left/right mappings. Verify the turn angle and smoothness.
- **Climbing or flying**: Test vertical movement mechanics if your app supports them.

### Object Interaction Testing

Test grab, release, throw, and manipulation:

- Move the simulated hand or controller near a grabbable object. Press the grip key to grab.
- Move the object around. Verify it follows the controller/hand position and rotation.
- Release the grip key. Confirm the object drops or remains in place as expected.
- Test two-handed manipulation by switching between left and right controller input groups.
- Verify distance grab mechanics if implemented.

### Mixed Reality Testing

For apps using passthrough and scene understanding:

- Enable passthrough in the simulator feature flags.
- Verify that your app renders correctly over the synthetic passthrough feed.
- Test plane detection by placing virtual content on detected surfaces (tables, walls, floors).
- Validate spatial anchors by creating, saving, and reloading them across sessions.
- Test occlusion behavior where virtual objects should appear behind real-world surfaces.

### Guardian Boundary Testing

Validate boundary-aware behavior:

- Move the simulated headset toward the edge of the configured room.
- Confirm your application responds to guardian proximity events (e.g., dimming the scene, showing warnings).
- Test what happens when the user "steps outside" the boundary.
- Verify stationary boundary mode vs. room-scale boundary mode behavior.

## Scriptable Testing

### Automating Input Sequences

Create repeatable test sequences by defining input scripts:

```json
{
  "name": "grab_and_place_test",
  "steps": [
    { "time": 0.0, "action": "move", "position": [0.0, 1.5, 1.0] },
    { "time": 1.0, "action": "look_at", "target": [0.0, 1.0, 2.0] },
    { "time": 2.0, "action": "controller_button", "hand": "right", "button": "grip", "state": "press" },
    { "time": 3.0, "action": "move", "position": [1.0, 1.5, 1.0] },
    { "time": 4.0, "action": "controller_button", "hand": "right", "button": "grip", "state": "release" },
    { "time": 5.0, "action": "screenshot", "filename": "result.png" }
  ]
}
```

Load scripts via the simulator CLI or configuration to automate regression testing.

### Record and Replay

1. Enable recording in the simulator settings or JSON config (`"autoRecord": true`).
2. Perform a manual test session. The simulator captures all head and controller movement, button presses, and hand poses.
3. Save the recording to a file.
4. Replay the recording later to reproduce the exact same session. This is useful for verifying bug fixes.

### CI/CD Integration

Integrate the XR Simulator into automated build pipelines:

1. Build your application as a standalone desktop executable with the XR Simulator runtime enabled.
2. Launch the simulator in headless mode (no visible window) using CLI flags.
3. Run a scripted test sequence against the application.
4. Capture screenshots, logs, and test results as artifacts.
5. Fail the pipeline if errors are detected in logs or if expected visual output does not match.

Example CI step (conceptual):

```bash
# Build the application
unity-editor -batchmode -buildTarget StandaloneWindows64 -executeMethod BuildScript.Build

# Run simulator test
xr-simulator --headless --config test_config.json --script grab_test.json --timeout 30

# Check results
test -f results/result.png && echo "Test passed" || exit 1
```

## Debugging with XR Simulator

### Engine Debugger

The simulator runs your application on the desktop, so standard debugging tools work:

- Set breakpoints in your IDE (Visual Studio, Rider, VS Code) and step through XR interaction code.
- Watch variables for controller state, hand pose data, and head tracking values.
- Inspect the OpenXR call stack to understand how your application communicates with the runtime.

### Visual Debugging Overlays

Enable debug overlays in the simulator to visualize:

- Controller ray directions and hit points
- Hand skeleton joint positions
- Guardian boundary outlines
- Detected plane surfaces and their normals
- Spatial anchor positions

### Log Output Analysis

The XR Simulator writes detailed logs:

- **Windows**: `%LOCALAPPDATA%\Meta\XRSimulator\logs\`
- **macOS**: `~/Library/Logs/Meta/XRSimulator/`

Review logs for:

- Runtime initialization and feature activation messages
- Input event traces showing button presses and pose updates
- Warnings about unsupported API calls
- Error messages from failed operations

### Comparing Simulator vs. Device Behavior

When behavior differs between simulator and device:

1. Reproduce the issue in both environments.
2. Capture logs from both the simulator and the device (via `adb logcat` for on-device).
3. Compare the OpenXR call sequences and return values.
4. Check whether the issue is related to timing (simulator runs at desktop frame rate, device at 72/90/120 Hz).
5. Verify that mock data returned by the simulator matches the format your code expects.

## Limitations to Be Aware Of

- **Performance characteristics differ from Quest hardware**. The simulator runs on a desktop CPU and GPU. Frame times, draw call budgets, and memory limits do not match Quest. Never use simulator results for performance optimization decisions.
- **Some platform APIs return mock data**. System-level APIs such as the system keyboard, social/friends list, in-app purchases, and notifications return stub responses or may not be available.
- **Rendering quality and resolution differ**. The simulator does not replicate the Quest display's resolution, refresh rate, or lens distortion. Fixed foveated rendering is not applied.
- **Thermal throttling does not apply**. The Quest dynamically adjusts performance based on thermal state. The simulator does not simulate this behavior.
- **Audio spatialization may differ**. The simulator outputs audio through desktop speakers or headphones. HRTF and spatial audio behavior may not match the Quest's built-in audio pipeline.
- **Haptics are not available**. Controller vibration and haptic feedback cannot be experienced in the simulator.

## When to Test on a Physical Device

The simulator is valuable for rapid iteration but cannot replace on-device testing for:

- **Final performance validation**: Confirm frame rate, GPU utilization, and memory usage on Quest hardware.
- **Haptic feedback tuning**: Adjust vibration patterns and intensities with physical controllers in hand.
- **Comfort testing**: Evaluate motion sickness, field of view, and ergonomics in the actual headset.
- **Store submission**: Meta requires on-device testing and validation before accepting store submissions.
- **Sensor-dependent features**: Test actual camera-based hand tracking, passthrough quality, and environment mesh accuracy with real sensors.
- **Multi-user co-location**: Validate shared physical spaces with multiple real headsets.

Use the simulator for development and functional testing, then validate on device before shipping.
