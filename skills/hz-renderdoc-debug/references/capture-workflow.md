# RenderDoc Capture Workflow for Quest

## Device Preparation

### ADB Connection

```bash
# Verify device is connected
adb devices

# If no device found, check USB connection and Developer Mode
# Developer Mode: Settings > System > Developer > USB debugging ON
```

### Clean Up Old RenderDoc Layers

Old RenderDoc server APKs can conflict with new captures. Remove them:

```bash
adb shell pm uninstall com.renderdoc.renderdoccmd.arm32
adb shell pm uninstall com.renderdoc.renderdoccmd.arm64
adb shell pm uninstall org.renderdoc.renderdoccmd.arm32
adb shell pm uninstall org.renderdoc.renderdoccmd.arm64
```

## Capture Steps

1. Open the RenderDoc Meta Fork application
2. Set **Replay Context** in the bottom-left dropdown to "Oculus Quest 2/3 Profiling Mode"
3. Set **Executable Path** to the app package/activity (e.g., `com.example.myapp/.MainActivity`)
4. Click **Launch**
5. Navigate in-headset to the desired scene
6. Click **Capture Frame(s) Immediately**
7. Right-click the capture thumbnail and select **Save** (reduces disconnect errors)
8. Re-select Replay Context (this exits the app on Quest)
9. **File > Open Capture** to begin analysis

## Replay Context Modes

The Replay Context dropdown in the bottom-left of the RenderDoc window controls how captures are replayed:

- **Normal mode**: Enables render target export and screenshots but no GPU metrics
- **Profiling mode**: Enables PIL GPU counters but disables screenshots and render target export

Select the appropriate mode before opening a capture file.

## Capture File Management

- Capture files use `.rdc` extension
- Files can be large (100MB to 1GB+) depending on scene complexity
- Store captures locally; they contain all GPU state needed for offline replay
- Captures are device-specific — a Quest 3 capture should be replayed on Quest 3 (or compatible) hardware context
