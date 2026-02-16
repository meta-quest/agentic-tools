# Common Issues and Diagnostics

This reference catalogs common issues encountered during Meta Quest development and provides diagnostic steps for each.

## Black Screen on Launch

The application launches but the user sees only a black screen.

### Diagnostic Steps

1. **Check logs for exceptions during startup:**
   ```bash
   hzdb adb logcat --tag AndroidRuntime --level E
   ```

2. **Verify entitlement:**
   Look for `Entitlement check failed` in the logs. During development, ensure the device is registered as a test device in the Meta Quest Developer Hub.

3. **Check VR focus:**
   Look for `vrapi_EnterVrMode` in the logs. If the app never enters VR mode, there may be an initialization error.
   ```bash
   hzdb log | grep -i "VrMode\|focus"
   ```

4. **Verify rendering pipeline:**
   For Unity apps, check that the XR plugin is loaded:
   ```bash
   hzdb log | grep -i "XR\|OpenXR\|Oculus"
   ```

5. **Check permissions:**
   Some features require permissions that, if missing, cause a silent black screen. Verify the AndroidManifest.xml includes required VR permissions.

## Tracking Loss

The headset loses positional tracking, causing the view to freeze or show a warning.

### Diagnostic Steps

1. **Check tracking state in logs:**
   ```bash
   hzdb adb logcat --tag XrRuntime --level W
   hzdb adb logcat --tag Guardian --level W
   ```

2. **Common causes:**
   - Poor lighting -- the inside-out cameras need visible features to track
   - Reflective surfaces nearby (mirrors, glass, TV screens)
   - Rapid head movement exceeding tracking speed
   - Cameras physically covered or obstructed
   - Blank walls with no visual features

3. **Resolution:**
   Improve room lighting, avoid reflective surfaces, and ensure the headset cameras are clean and unobstructed.

## Controller Disconnect

One or both controllers lose connection to the headset.

### Diagnostic Steps

1. **Check controller status in logs:**
   ```bash
   hzdb adb logcat --tag InputDispatcher --level W
   ```

2. **Common causes:**
   - Low battery in the controller
   - Controller firmware needs updating
   - Bluetooth interference from nearby devices
   - Controller physically too far from headset

3. **Resolution:**
   - Replace or recharge controller batteries
   - Update controller firmware via Settings in the headset
   - Re-pair the controller: hold the Menu + Y (left) or Oculus + B (right) buttons
   - Move closer to the headset and away from interfering devices

## App Crashes

The application terminates unexpectedly.

### Diagnostic Steps

1. **Check for Java/Kotlin exceptions:**
   ```bash
   hzdb log | grep "FATAL EXCEPTION"
   ```
   The stack trace following this line identifies the crash location.

2. **Check for native crashes:**
   ```bash
   hzdb adb logcat --tag DEBUG --level E
   ```
   Look for signal information (`SIGSEGV`, `SIGABRT`, `SIGBUS`) and the backtrace.

3. **Check for out-of-memory:**
   ```bash
   hzdb log | grep -i "OutOfMemoryError\|OOM\|lowmemory"
   ```
   Quest devices have limited memory. Large textures, uncompressed audio, and memory leaks are common culprits.

4. **Check for ANR (Application Not Responding):**
   ```bash
   hzdb log | grep "ANR in"
   ```
   ANRs occur when the main thread is blocked for more than 5 seconds. Common causes include synchronous file I/O, network calls on the main thread, and deadlocks.

5. **Review the full crash context:**
   ```bash
   hzdb log --level W
   ```
   Look at warnings leading up to the crash for clues about the root cause.

## Performance Issues

The application stutters, judders, or has visible frame drops.

### Diagnostic Steps

1. **Check frame rate in logs:**
   ```bash
   hzdb adb logcat --tag VrApi | grep "FPS"
   ```
   Quest targets 72Hz, 90Hz, or 120Hz depending on the app configuration. Sustained FPS below the target indicates a problem.

2. **Check for thermal throttling:**
   ```bash
   hzdb adb logcat --tag ThermalService --level W
   ```
   When the device overheats, the system reduces CPU and GPU clock speeds, causing performance drops.

3. **Check CPU/GPU timing:**
   ```bash
   hzdb adb logcat --tag VrApi | grep -i "cpu\|gpu\|frame"
   ```
   If CPU frame time exceeds the frame budget, the app is CPU-bound. If GPU frame time exceeds it, the app is GPU-bound.

4. **Common causes and solutions:**
   - **Too many draw calls** -- Batch geometry, use instancing, reduce material count
   - **Overdraw** -- Reduce transparent layers, simplify shaders
   - **Complex shaders** -- Optimize fragment shaders, reduce texture sampling
   - **Physics or scripts** -- Profile C#/Blueprint logic, reduce per-frame allocations
   - **Memory pressure** -- Compress textures (use ASTC), reduce mesh complexity

## Permission Errors

A feature silently fails or throws an error due to missing permissions.

### Required Permissions

| Feature                | Permission                                     |
| ---------------------- | ---------------------------------------------- |
| Hand tracking          | `com.oculus.permission.HAND_TRACKING`           |
| Passthrough            | `com.oculus.permission.USE_SCENE`               |
| Eye tracking           | `com.oculus.permission.EYE_TRACKING`            |
| Face tracking          | `com.oculus.permission.FACE_TRACKING`           |
| Microphone             | `android.permission.RECORD_AUDIO`              |
| Internet access        | `android.permission.INTERNET`                  |
| File read/write        | `android.permission.READ_EXTERNAL_STORAGE`     |
| Spatial anchors        | `com.oculus.permission.USE_ANCHOR_API`          |
| Scene understanding    | `com.oculus.permission.USE_SCENE`               |
| Body tracking          | `com.oculus.permission.BODY_TRACKING`           |

### Diagnostic Steps

1. **Check the app manifest for the required permission.**
2. **Check logs for permission denial:**
   ```bash
   hzdb log | grep -i "permission\|denied\|security"
   ```
3. **Verify runtime permission grants:**
   Some permissions require runtime user approval in addition to the manifest declaration.

## Audio Issues

No audio, distorted audio, or audio not spatialized correctly.

### Diagnostic Steps

1. **Check audio focus:**
   ```bash
   hzdb adb logcat --tag AudioFlinger --level W
   hzdb adb logcat --tag AudioManager --level W
   ```
   If another app or the system holds audio focus, your app may be ducked or muted.

2. **Verify spatial audio setup:**
   - Ensure the spatial audio SDK is initialized in your app
   - Check that audio sources have 3D spatial properties configured
   - Verify the audio listener is attached to the head/camera object

3. **Check device volume:**
   The user may have the volume set to zero or the headset may be muted via the physical buttons.

## Hand Tracking Not Working

Hand tracking does not activate or hands are not detected.

### Diagnostic Steps

1. **Check manifest entries:**
   The application manifest must include:
   ```xml
   <uses-permission android:name="com.oculus.permission.HAND_TRACKING" />
   <uses-feature android:name="oculus.software.handtracking" android:required="false" />
   ```

2. **Verify hand tracking is enabled on the device:**
   Hand tracking must be enabled in the headset settings under Movement Tracking.

3. **Check logs for hand tracking initialization:**
   ```bash
   hzdb log | grep -i "hand"
   ```

4. **Common causes:**
   - Hands are outside the camera field of view
   - Poor lighting prevents hand detection
   - Gloves or accessories interfere with detection
   - Controllers are still active (some apps switch modes)

## Passthrough Not Working

The passthrough camera feed does not appear or shows a black/gray background.

### Diagnostic Steps

1. **Check device compatibility:**
   Full-color passthrough is available on Quest 3, Quest 3S, and Quest Pro. Quest 2 supports grayscale passthrough only.

2. **Check manifest permissions:**
   ```xml
   <uses-permission android:name="com.oculus.permission.USE_SCENE" />
   ```

3. **Verify passthrough initialization in logs:**
   ```bash
   hzdb log | grep -i "passthrough\|scene\|insight"
   ```

4. **Check that the environment blend mode is set correctly:**
   For OpenXR apps, the session must request `XR_ENVIRONMENT_BLEND_MODE_ALPHA_BLEND` for passthrough.

5. **Common causes:**
   - Missing permission in manifest
   - Passthrough layer not added to the compositor
   - Camera access blocked by another process
   - Device does not support the requested passthrough mode
