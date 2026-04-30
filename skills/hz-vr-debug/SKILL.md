---
name: hz-vr-debug
description: Debugs Meta Quest and Horizon OS VR/MR applications using the hzdb CLI — view logs, capture screenshots, diagnose common issues. Use when troubleshooting crashes, errors, or unexpected behavior on Quest devices.
allowed-tools:
  - Bash(hzdb:*)
---

# VR Debug Skill

Debug Meta Quest VR and MR applications using the `hzdb` command-line interface. This skill covers viewing application logs, capturing device state, diagnosing crashes, and resolving common issues encountered during Quest development.

## When to Use This Skill

Use this skill when you need to:

- Debug an application running on a connected Meta Quest device
- View real-time or historical application logs (logcat)
- Capture screenshots of the VR/MR view
- Diagnose application crashes, rendering glitches, or performance problems
- Investigate tracking, controller, audio, or permission issues
- Pull diagnostic files from the device for offline analysis

This skill is relevant for any Meta Quest headset (Quest 2, Quest 3, Quest 3S, Quest Pro) running Horizon OS.

## Prerequisites

Before using this skill, ensure the following are in place:

1. **hzdb CLI ready via `npx`** -- The `hzdb` CLI is invoked on demand; no global install required:
   ```bash
   npx -y @meta-quest/hzdb --version
   ```
   hzdb wraps ADB and adds Quest-specific device management, log viewing, screenshot capture, and file management. Examples below use the bare `hzdb` command for brevity — substitute `npx -y @meta-quest/hzdb` in front.
2. **Meta Quest device connected via USB** -- Use a USB-C cable that supports data transfer (not charge-only).
3. **Developer mode enabled** -- Developer mode must be turned on in the Meta Horizon app on your phone, under your headset's settings.
4. **ADB authorization accepted** -- The first time you connect, you must put on the headset and accept the "Allow USB debugging" prompt.

## Quick Start Workflow

The fastest way to begin debugging a Quest application:

```bash
# 1. Verify the device is connected and recognized
hzdb device list

# 2. List applications currently installed
hzdb app list

# 3. View live logs (most recent 100 lines)
hzdb log

# 4. Capture a screenshot of the current VR view
hzdb capture screenshot
```

If `hzdb device list` returns no devices, check the USB cable, developer mode, and ADB authorization.

## Key Debugging Commands

### Device Commands

| Command                    | Description                                      |
| -------------------------- | ------------------------------------------------ |
| `hzdb device list`         | List all connected Quest devices                 |
| `hzdb device info <id>`    | Show device model, OS version, and more          |
| `hzdb device battery`      | Show battery level and charging status           |
| `hzdb device wake`         | Wake the device from sleep                       |
| `hzdb device reboot`       | Reboot the device                                |
| `hzdb device connect <ip>` | Connect to a device over WiFi                    |

### Log Commands

| Command                              | Description                                    |
| ------------------------------------ | ---------------------------------------------- |
| `hzdb log`                           | View the last 100 log lines                    |
| `hzdb log -n 500`                    | View the last 500 log lines                    |
| `hzdb log --tag Unity`               | Filter logs by tag                             |
| `hzdb log --level E`                 | Filter by severity (V, D, I, W, E, F)          |
| `hzdb adb logcat`                    | Full logcat with advanced filtering options    |
| `hzdb adb logcat --follow`           | Stream logs continuously                       |

### Application Commands

| Command                          | Description                                    |
| -------------------------------- | ---------------------------------------------- |
| `hzdb app list`                  | List installed applications                    |
| `hzdb app info <package>`        | Show detailed info about an app                |
| `hzdb app launch <package>`      | Launch an application by package name          |
| `hzdb app stop <package>`        | Force-stop a running application               |
| `hzdb app clear <package>`       | Clear application data and cache               |
| `hzdb app install <apk>`         | Install an APK to the device                   |
| `hzdb app uninstall <package>`   | Uninstall an application                       |

### Capture Commands

| Command                              | Description                                        |
| ------------------------------------ | -------------------------------------------------- |
| `hzdb capture screenshot`            | Capture a screenshot of the current VR/MR view     |
| `hzdb capture screenshot -o file.png`| Save screenshot to a specific file                 |

### File Commands

| Command                                          | Description                              |
| ------------------------------------------------ | ---------------------------------------- |
| `hzdb files ls /sdcard/`                         | List files on the device                 |
| `hzdb files pull /sdcard/path/file ./local/`     | Pull a file from the device              |
| `hzdb files push ./local/file /sdcard/path/`     | Push a file to the device                |
| `hzdb files rm /sdcard/path/file`                | Delete a file on the device              |
| `hzdb files mkdir /sdcard/path/dir`              | Create a directory on the device         |

## Common Debugging Workflow

A typical debugging session follows this pattern:

### 1. Connect and Verify

```bash
hzdb device list
hzdb device info <device_id>
```

Confirm the device is recognized, check the OS version, and note the battery level. A low battery can cause thermal throttling that affects performance tests.

### 2. Identify the Application

```bash
hzdb app list
```

Find the package name for the application you want to debug. Package names typically follow the pattern `com.company.appname`.

### 3. Reproduce and Capture Logs

```bash
# Start logging before reproducing the issue
hzdb adb logcat --follow
```

Put on the headset and reproduce the issue. The logs stream in real time to your terminal. Press Ctrl+C to stop.

### 4. Capture Visual State

```bash
# Take a screenshot at the moment of the issue
hzdb capture screenshot
```

### 5. Analyze and Diagnose

Review the captured logs for errors, warnings, and crash signatures. Look for:

- `FATAL EXCEPTION` -- Unhandled Java/Kotlin exceptions
- `native crash` or `SIGABRT` / `SIGSEGV` -- Native code crashes
- `ANR` -- Application Not Responding (frozen UI thread)
- `OOM` or `OutOfMemoryError` -- Memory exhaustion

### 6. Iterate

Make code changes, rebuild, deploy, and test again:

```bash
hzdb app stop com.example.myapp
hzdb app launch com.example.myapp
hzdb log --tag Unity --level W
```

## Symptom-to-Diagnosis Decision Trees

When a developer reports a problem, use these decision trees to systematically diagnose the root cause. Start with the reported symptom and follow the branches.

### App Crashes on Launch

```
App crashes on launch
├── Does `hzdb app launch <pkg>` show "Error: Activity not found"?
│   └── YES → Package name is wrong or app is not installed.
│       Run `hzdb app list` to verify the correct package name.
├── Does logcat show `FATAL EXCEPTION` in the first 5 seconds?
│   ├── YES, with `ClassNotFoundException` or `NoClassDefFoundError`
│   │   └── Missing native library or wrong ABI. Check the APK is built for ARM64.
│   │       Run: `hzdb adb shell getprop ro.product.cpu.abi` → must show "arm64-v8a"
│   ├── YES, with `SecurityException` or `Permission denied`
│   │   └── Missing manifest permission. Check the logcat message for which permission.
│   │       Common: hand tracking, scene, camera permissions not declared.
│   └── YES, with `NullPointerException` or other Java exception
│       └── Application code bug. Read the stack trace for the failing class and method.
├── Does logcat show `native crash` / `SIGSEGV` / `SIGABRT`?
│   ├── Check if the crash is in a Unity/Unreal library (libunity.so, libUE4.so)
│   │   └── Engine bug or incompatible SDK version. Check Meta XR SDK release notes
│   │       for known issues with your engine version.
│   └── Check if the crash is in your own native code
│       └── Debug with `hzdb adb logcat --buffer crash` for the tombstone, then use
│           `addr2line` or `ndk-stack` on the crash address.
└── No crash visible in logs?
    └── Check if the app is being killed by the system.
        Run: `hzdb adb logcat --tag ActivityManager --level W`
        Look for "Force stopping" or "Process died" messages. Common cause: OOM killer
        triggered by excessive memory usage on launch.
```

### App Freezes / ANR (Application Not Responding)

```
App freezes or ANR dialog appears
├── Does logcat show "ANR in <package>"?
│   ├── YES, with "Reason: Input dispatching timed out"
│   │   └── The main/UI thread is blocked. Check for:
│   │       - Synchronous network calls on the main thread
│   │       - Large file I/O on the main thread
│   │       - Deadlocks between threads
│   │       Run: `hzdb adb shell kill -3 <pid>` to dump thread stacks, then
│   │       `hzdb files pull /data/anr/traces.txt ./` to retrieve the ANR trace.
│   └── YES, with "Reason: executing service"
│       └── A background service is taking too long. Check the service implementation.
└── No ANR, but app appears frozen?
    ├── Is the render loop still running? (Check VrApi logs for frame submission)
    │   ├── YES → The app is rendering but not processing input. Check input system.
    │   └── NO → The render thread is blocked or crashed silently.
    │       Check: `hzdb adb logcat --tag VrApi` for "FPS" lines stopping.
    └── Is the device overheating?
        Run: `hzdb device battery` — if battery temperature > 40°C, thermal
        throttling may have halted the app. Let device cool down and retry.
```

### Black Screen in Headset

```
Black screen after app launch
├── Is the app actually running?
│   Run: `hzdb adb shell pidof <package>` — if empty, app crashed silently.
│   └── Check crash logs: `hzdb adb logcat --buffer crash`
├── Is VrApi initialized?
│   Check: `hzdb adb logcat --tag VrApi | grep "VrApi" | head -20`
│   ├── No VrApi output → XR session never started. Check OpenXR/OVR initialization code.
│   └── VrApi output exists → Frames are being submitted but may be empty.
│       └── Check: rendering pipeline, camera setup, shader compilation errors.
├── Unity-specific: "Shader compiler" or "Compiling shaders" in logs?
│   └── Shader warmup can cause a black screen for several seconds on first launch.
│       Use shader prewarming/variant preloading to avoid this.
└── Is the correct rendering API being used?
    Check: `hzdb adb logcat --tag Unity --level E` for Vulkan/GLES errors.
    Quest requires OpenGL ES 3.0 minimum. Vulkan is preferred on Quest 3.
```

### Frame Drops / Stuttering

```
App stutters or drops frames
├── Check current FPS:
│   `hzdb adb logcat --tag VrApi | grep FPS`
│   ├── FPS consistently below 72 → GPU or CPU bottleneck.
│   │   ├── Check GPU: use Perfetto or OVR Metrics Tool. Look for GPU completion
│   │   │   time > 13.8ms (72Hz) or > 11.1ms (90Hz).
│   │   └── Check CPU: look for game thread or render thread exceeding frame budget.
│   └── FPS mostly stable but periodic drops
│       ├── Check for GC pauses: `hzdb adb logcat --tag dalvikvm --level D`
│       │   or `hzdb adb logcat --regex "GC_|clamp"` → reduce allocations per frame.
│       ├── Check for thermal throttling: `hzdb adb logcat --tag ThermalService --level W`
│       │   → sustained heavy load causes CPU/GPU frequency reduction.
│       └── Check for asset loading on main thread: large textures or models loaded
│           synchronously will cause frame spikes. Use async loading.
└── Only stutters in specific scenes?
    └── Profile that scene. Common causes: too many draw calls (>100), unculled
        off-screen geometry, expensive shaders, excessive overdraw, uncompressed textures.
```

### Tracking Issues

```
Controllers or hands not tracking correctly
├── Are controllers paired and connected?
│   `hzdb device info <id>` — check controller connection status.
├── Is hand tracking enabled in device settings?
│   Check: Settings > Movement Tracking > Hand and Body Tracking.
├── Does the app request the correct tracking mode?
│   ├── For hand tracking: manifest must include
│   │   `com.oculus.permission.HAND_TRACKING` and
│   │   `com.oculus.handtracking.frequency` set to "HIGH" if needed.
│   └── For controller tracking: ensure the app is not forcing hand-tracking-only mode.
└── Tracking works but is jittery or delayed?
    ├── Check lighting: tracking cameras need adequate, even lighting. Very bright
    │   or very dim environments degrade tracking quality.
    └── Check for occlusion: hands or controllers held outside the tracking camera
        FOV will lose tracking. The camera FOV is approximately 110 degrees.
```

### Audio Issues

```
No audio or wrong audio output
├── Is audio playing through the headset speakers?
│   └── Check: Settings > Sound — ensure headset speakers are selected, not Bluetooth.
├── Does the app use spatial audio?
│   ├── Check for FMOD/Wwise initialization errors in logcat.
│   └── Check that audio sources have correct 3D settings and are not muted.
├── Audio is distorted or crackling?
│   └── Audio buffer underruns. Check for CPU overload causing audio thread starvation.
│       Reduce audio complexity or increase buffer size.
└── Audio plays from wrong position?
    └── Check spatial audio source positions match visual object positions.
        Common issue: audio listener not attached to the camera/head transform.
```

## Gotchas

These are common debugging pitfalls specific to Quest development.

- **Logcat buffer overflow** -- On Quest, the logcat ring buffer fills quickly because the OS and other apps generate constant output. If you do not start logging before reproducing the issue, the crash logs may already be evicted. Start `hzdb adb logcat --follow` before reproducing.
- **USB cable quality matters** -- Many USB-C cables are charge-only and do not carry data. If `hzdb device list` shows nothing, try a different cable before troubleshooting software. The cable that came with the Quest works for data.
- **WiFi debugging disconnects** -- WiFi ADB connections (`hzdb device connect <ip>`) drop after the device sleeps. You must reconnect after waking the device. USB is more reliable for sustained debugging sessions.
- **Release builds strip logs** -- If your app uses `android:debuggable="false"` (release builds), some log output is suppressed. Debug with a debug build when investigating issues. Do not ship debuggable builds to the store.
- **Multiple logcat tags for the same component** -- Unity uses tags `Unity`, `il2cpp`, and `mono` depending on the scripting backend. Unreal uses `UE`, `LogVR`, and `LogOnline`. Filter broadly at first, then narrow down.
- **OVR Metrics Tool overlay conflicts** -- The OVR Metrics Tool overlay can interfere with your app's rendering or input. If your app behaves oddly, disable the metrics overlay and retest before filing a bug.

## Tips and Best Practices

### Filtering Logs Effectively

Use severity filters to cut through noise:

```bash
# Show only errors
hzdb log --level E

# Show warnings and above
hzdb log --level W
```

Filter by tag to focus on specific subsystems:

```bash
hzdb adb logcat --tag VrApi
hzdb adb logcat --tag Unity
```

Use advanced filters with `hzdb adb logcat`:

```bash
# Complex filter expressions
hzdb adb logcat --filter "Unity:W ActivityManager:I"

# Regex pattern matching
hzdb adb logcat --regex "error|exception"

# Specific log buffer
hzdb adb logcat --buffer crash
```

See [logcat-filtering.md](references/logcat-filtering.md) for a full guide on log filtering techniques.

### Searching for Crash Signatures

When investigating crashes, search the log output for known patterns:

```bash
hzdb log | grep -i "fatal\|crash\|exception\|anr"
```

Common crash-related tags include `AndroidRuntime`, `DEBUG`, and `libc`.

### Checking Permissions

Many Horizon OS features require specific manifest permissions. If a feature silently fails, check that the application manifest includes the required permissions. Common ones:

- `com.oculus.permission.HAND_TRACKING` -- Hand tracking access
- `com.oculus.permission.USE_SCENE` -- Scene API spatial data access
- `android.permission.RECORD_AUDIO` -- Microphone access
- `android.permission.CAMERA` -- Camera access (for mixed reality)

### Performance Debugging

If the application stutters or drops frames:

```bash
# Check device battery and thermal state
hzdb device battery

# Watch for thermal throttling messages in logs
hzdb adb logcat --tag ThermalService --level W

# Check VrApi frame timing
hzdb adb logcat --tag VrApi | grep FPS
```

See [common-issues.md](references/common-issues.md) for a catalog of known issues and their solutions.

## References

### Skill References

- [Logcat Filtering Guide](references/logcat-filtering.md) -- Detailed guide to filtering and interpreting device logs
- [Screenshots and Video Capture](references/screenshots-video.md) -- Capturing visual state from the device
- [Common Issues and Diagnostics](references/common-issues.md) -- Catalog of common Quest development issues and solutions
