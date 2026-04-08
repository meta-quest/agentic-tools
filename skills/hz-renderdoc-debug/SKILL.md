---
name: hz-renderdoc-debug
description: Guides users through RenderDoc frame capture and GPU debugging workflows for Meta Quest and Horizon OS applications — draw call inspection, shader optimization, render target export, and GPU performance metrics.
allowed-tools:
  - Bash(adb:*)
---

# RenderDoc Debug Skill

## When to Use

Use this skill when investigating GPU rendering issues on Meta Quest devices:

- Draw call inspection and render pass analysis
- Shader debugging and optimization
- Render target and texture export for visual debugging
- GPU performance metrics via Performance Instrumentation Layer (PIL)
- Pipeline state inspection (bound shaders, viewports, scissor rects)
- Overdraw and geometry complexity analysis
- Visual regression debugging (comparing before/after screenshots)

## Prerequisites

### RenderDoc Meta Fork

Install the RenderDoc Meta Fork (NOT the standard public RenderDoc):

- **Windows**: https://developers.meta.com/horizon/downloads/package/renderdoc-oculus/
- **macOS**: https://developers.meta.com/horizon/downloads/package/renderdoc-meta-fork-for-mac-installer/

### Device Setup

1. Connect your Quest device via USB
2. Enable Developer Mode on the device
3. Verify ADB connection:

```bash
adb devices
```

### Debuggable APK

The target application must be debuggable. One of:

- APK built with "Developer Build" flag (Unity) or debug configuration
- Userdebug OS firmware on the device
- `com.oculus.gpu_debuggable` manifest flag set

## Connection Modes

RenderDoc supports two replay modes with different capabilities:

| Mode | PIL Metrics | Screenshots/RT Export | Use Case |
|------|-------------|----------------------|----------|
| **Normal** | No | Yes | Visual debugging, draw call inspection |
| **Profiling** | Yes | No | GPU performance measurement |

Choose the mode based on your debugging goal. Set the mode via the **Replay Context** dropdown in the bottom-left of the RenderDoc window.

## Capture Workflow

1. Open the RenderDoc Meta Fork application
2. Set **Replay Context** in the bottom-left dropdown to "Oculus Quest 2/3 Profiling Mode"
3. Set **Executable Path** to the app package/activity (e.g., `com.example.myapp/.MainActivity`)
4. Click **Launch**
5. Navigate in-headset to the desired scene
6. Click **Capture Frame(s) Immediately**
7. Right-click the capture thumbnail and select **Save** (reduces disconnect errors)
8. Re-select Replay Context (this exits the app on Quest)
9. **File > Open Capture** to begin analysis

## Key Analysis Patterns

### Inspect Draw Calls

The Event Browser shows all draw calls organized by render pass. Click any draw call to inspect:

- **Pipeline state**: Bound shaders, render targets, viewports, blend state
- **Vertex/Index data**: Geometry being rendered
- **Textures**: All bound textures at that draw call
- **Shader source**: View and debug vertex/fragment shaders

### Identify Render Pass Structure

RenderDoc organizes draw calls by render pass:

| Pass Type | Typical Purpose |
|-----------|----------------|
| Depth-only | Z-prepass, shadow maps |
| Colour | Main scene rendering |
| Post-process | Bloom, tone mapping, distortion |
| Compositor | Final eye buffer composition |

### Check for Overdraw

Export render targets at different draw calls and compare. Multiple draws writing to the same pixels indicate overdraw.

### Shader Debugging

RenderDoc allows you to inspect and debug shaders:

- View vertex and fragment shader source code
- Step through shader execution for specific pixels
- Inspect input/output variables at each stage
- Identify expensive shader operations

### GPU Metrics (Profiling Mode Only)

When the Replay Context is set to Profiling mode, RenderDoc collects PIL (Performance Instrumentation Layer) GPU counters:

- GPU cycle counts per draw
- Texture fetch rates
- Shader ALU utilization
- Vertex/fragment processing stats

## Troubleshooting

| Problem | Solution |
|---------|----------|
| App fails to launch | Ensure APK is debuggable and device is in Developer Mode |
| Timeout on launch | Wake the headset (proximity sensor or `adb shell am broadcast -a com.oculus.vrpowermanager.prox_close`) |
| RenderDoc layer conflicts | Uninstall old server APKs (see references) |
| No PIL metrics | Ensure Replay Context is set to Profiling mode |
| Capture file too large | Reduce scene complexity or capture fewer frames |
| Mac disconnection after save | Re-select the Replay Context dropdown to reconnect |

## Common Pitfalls

- **Normal mode has no GPU metrics.** If you need PIL counters, you must set the Replay Context to Profiling mode.
- **Profiling mode has no screenshots.** If you need visual output, use Normal mode.
- **PIL metrics have overhead.** Frame times in profiling mode are slower than production. Use relative comparisons (before vs after), not absolute values.
- **Old RenderDoc APKs conflict.** If capture fails, uninstall legacy RenderDoc server packages from the device.

## References

For detailed guides on specific topics, see:

- [Shader Optimization](references/shader-optimization.md) — Shader optimization techniques for Adreno GPUs
- [Capture Workflow](references/capture-workflow.md) — Detailed capture and device setup
- [Troubleshooting](references/troubleshooting.md) — Common issues, layer conflicts, and device connection problems
