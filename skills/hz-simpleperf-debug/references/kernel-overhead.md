# Kernel Overhead Analysis

## Overview

The kernel overhead measurement separates CPU cycles into user-mode (your app code) and kernel-mode (OS/driver code) on a per-thread basis. This reveals hidden costs from syscalls, driver interactions, and kernel synchronization that are invisible to application-level profiling.

## Recording Kernel Overhead

```bash
# Measure kernel overhead for the foreground app
hzdb perf simpleperf kernel-overhead

# Target a specific app with custom duration
hzdb perf simpleperf kernel-overhead --app com.example.myapp --duration 15
```

## Interpreting Results

### Per-Thread Breakdown

```
Kernel Overhead Analysis (10.0s)
Process: com.example.myapp (PID 12345)

Thread                   User Cycles    Kernel Cycles   Kernel %
UnityMain                1,247,000,000  62,350,000      4.8%
UnityGfx                 891,000,000    178,200,000     16.7%
Job.Worker 0             423,000,000    8,460,000       2.0%
Job.Worker 1             418,000,000    7,524,000       1.8%
FMOD mixer               156,000,000    31,200,000      16.7%
AudioTrack               89,000,000     44,500,000      33.3%
```

### What Kernel Overhead Means

When a thread is in kernel mode, it's executing OS code on behalf of your app. Common causes:

| Kernel Activity | Typical Source | VR Impact |
|----------------|---------------|-----------|
| GPU driver calls | `vkQueueSubmit`, `vkWaitForFences` | Render thread blocked on GPU |
| File I/O | `read()`, `write()`, `open()` | Stalls if on main thread |
| Memory mapping | `mmap()`, `munmap()` | Large asset loads |
| Thread sync | `futex()` | Lock contention, condition variables |
| Network | `send()`, `recv()`, `poll()` | Multiplayer, analytics |
| Audio HAL | ALSA/AudioFlinger calls | Expected for audio threads |
| Sensor access | IMU/tracking driver calls | Expected for tracking threads |

### Healthy vs Unhealthy Kernel Overhead

| Thread Type | Healthy | Investigate | Problem |
|------------|---------|-------------|---------|
| Main/Game thread | < 5% | 5-10% | > 10% |
| Render thread | < 15% | 15-25% | > 25% |
| Worker/Job threads | < 3% | 3-8% | > 8% |
| Audio thread | < 20% | 20-40% | > 40% |
| I/O thread | Any % | N/A | N/A (expected) |

## Common Issues and Fixes

### High Kernel % on Main Thread (>10%)

**Likely causes:**
- Synchronous file reads during gameplay (asset streaming)
- Logging to disk (`__android_log_print`)
- `PlayerPrefs` / persistent storage writes
- Analytics/telemetry flushes

**Fixes:**
- Move file I/O to background threads
- Buffer log writes, flush asynchronously
- Batch analytics events
- Use memory-mapped files for frequent reads

### High Kernel % on Render Thread (>25%)

**Likely causes:**
- Excessive Vulkan API calls (too many draw calls → too many `vkCmd*` calls)
- GPU sync stalls (`vkWaitForFences` blocking)
- Shader compilation at runtime (driver compiling shaders on submit)
- Large uniform buffer updates

**Fixes:**
- Batch draw calls (reduce to <100 per frame)
- Use async compute to overlap CPU/GPU work
- Pre-warm all shader variants during loading
- Use push constants instead of large UBOs for small data

### High Kernel % on Worker Threads (>8%)

**Likely causes:**
- Mutex contention (`futex()` calls)
- False sharing on adjacent cache lines
- Thread pool starvation causing excessive wake/sleep cycles

**Fixes:**
- Use lock-free data structures where possible
- Align shared data to cache line boundaries (64 bytes on ARM)
- Size thread pool to core count minus 2 (leave room for main + render)

## Combining with Other Analysis

Kernel overhead analysis works best as part of a three-step workflow:

1. **Classify** (`hzdb perf simpleperf classify`) — What type of bottleneck?
2. **Kernel overhead** (`hzdb perf simpleperf kernel-overhead`) — How much is OS/driver vs app?
3. **Hotspots** (`hzdb perf simpleperf record`) — Which specific functions?

If kernel overhead is high, Perfetto tracing (`hzdb perf capture`) can show exactly *when* the kernel calls happen relative to frame boundaries, revealing whether they're causing frame drops or happening during slack time.
