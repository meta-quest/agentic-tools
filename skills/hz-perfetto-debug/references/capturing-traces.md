# Capturing Perfetto Traces on Meta Quest

## Overview

Perfetto traces record detailed timing information about CPU scheduling, GPU rendering, and application behavior. A good trace is the foundation of any performance investigation.

## Capture with hzdb

### Basic Capture

Capture a trace from the currently running VR app on a connected Quest device:

```bash
hzdb perf capture
```

This auto-detects the foreground VR app, captures a 5-second trace with CPU scheduling and GPU metrics enabled, then pulls the `.pftrace` file to your local machine.

### Specifying Duration and App

```bash
# 10-second capture targeting a specific app
hzdb perf capture --duration 10000 --app com.example.myapp

# Short 2-second capture for frame timing
hzdb perf capture --duration 2000
```

### Enabling Additional Trace Categories

By default, `hzdb perf capture` enables CPU scheduling (`--cpu-scheduling`) and GPU metrics (`--gpu-metrics`). You can enable additional categories:

```bash
# Enable GPU render stage tracing (detailed per-pass timing)
hzdb perf capture --gpu-render-stage

# Enable XR runtime metrics (OpenXR frame timing)
hzdb perf capture --xr-runtime

# Enable everything for a comprehensive trace
hzdb perf capture --gpu-render-stage --xr-runtime
```

### Custom Output Name

```bash
hzdb perf capture -o my-session-name
```

The trace is saved locally and appears in `hzdb perf traces` output.

### Capture Options Reference

| Option | Default | Description |
|--------|---------|-------------|
| `--duration <ms>` | 5000 | Capture duration in milliseconds |
| `--app <package>` | Auto-detect | Target app package name |
| `-o, --output <name>` | Auto-generated | Output filename (without extension) |
| `--gpu-render-stage` | Off | Enable GPU render stage tracing |
| `--gpu-metrics` | On | Enable GPU metrics tracing |
| `--cpu-scheduling` | On | Enable CPU scheduling tracing |
| `--xr-runtime` | Off | Enable XR runtime metrics |

## Programmatic Capture

For automated testing pipelines and CI environments, use the Perfetto SDK to trigger traces from within the application or test harness. This does not require hzdb or a USB connection.

## Trace Duration Guidelines

| Use Case | Recommended Duration | Notes |
|----------|---------------------|-------|
| Frame timing analysis | 2-5 seconds | Captures 140-450 frames at 90 Hz |
| Stutter investigation | 5-10 seconds | Longer window to catch intermittent hitches |
| Thermal throttling | 30-60 seconds | Thermal events happen over longer periods |
| Loading screen analysis | Cover full load | Start before load, stop after scene is active |
| Startup profiling | 10-20 seconds | From app launch to first rendered frame |

**Important**: Traces over 10 seconds generate large files (100+ MB) and take longer to process. Use the shortest duration that captures the issue.

## Trace Categories

These are the data sources captured by hzdb, controllable via capture flags:

| Category | Captured By | What It Records |
|----------|------------|-----------------|
| CPU scheduling | `--cpu-scheduling` (default on) | Thread states, context switches, sched events |
| GPU metrics | `--gpu-metrics` (default on) | GPU hardware counters (ALU, texture fetch, vertex) |
| GPU render stages | `--gpu-render-stage` | Per-pass GPU timing, surface workloads |
| XR runtime | `--xr-runtime` | OpenXR frame timing, compositor metrics |
| App trace events | Always on | App-instrumented trace markers (atrace) |
| Process stats | Always on | Process-level CPU/memory stats |

## Finding Traces

After capture, traces are stored locally and discoverable via:

```bash
hzdb perf traces
```

This lists `.pftrace` files sorted newest first, searching standard directories including `~/Documents`, `~/Downloads`, and the current working directory.

## Verifying a Captured Trace

After capture, verify the trace is usable before starting analysis:

```bash
# List available traces (most recent first)
hzdb perf traces

# Load the trace
hzdb perf load <session-id>

# Quick validation query
hzdb perf query <session-id> "SELECT (MAX(ts) - MIN(ts)) / 1e9 AS duration_sec, COUNT(*) AS slices FROM slice"
```

A good trace should have:
- Duration of at least 2 seconds
- Several thousand slices (typically 5,000-50,000 for a 5-second trace)
- The target application process present in the process list

## Troubleshooting Capture Issues

| Problem | Cause | Solution |
|---------|-------|----------|
| No device found | Device not connected or not authorized | Run `hzdb device list` to check connection. Ensure USB debugging is enabled and authorized. |
| Empty or very small trace | Perfetto daemon not running on device | Check device logs. May need to restart the device. |
| Missing GPU data | GPU trace events not enabled | Use `--gpu-render-stage` and `--gpu-metrics` flags |
| Missing app slices | App not using atrace/Trace API | Ensure app has tracing instrumentation enabled |
| App not auto-detected | No VR app in foreground | Specify the app explicitly with `--app <package>` |
| Very large trace (>500 MB) | Too many categories or too long | Reduce `--duration` or disable categories you don't need |
