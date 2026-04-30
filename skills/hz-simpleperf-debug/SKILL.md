---
name: hz-simpleperf-debug
description: Profiles Meta Quest and Horizon OS application CPU performance using simpleperf — workload classification, CPU hotspot recording, kernel overhead measurement. Use when diagnosing whether an app is CPU-bound, memory-bound, or I/O-bound on Quest devices.
allowed-tools:
  - Bash(hzdb:*)
---

# Simpleperf Debug Skill

## When to Use

Use this skill when you need hardware-level CPU performance insights on Meta Quest devices:

- Classifying whether an app is CPU-bound, memory-bound, or I/O-bound
- Finding CPU hotspot functions consuming the most cycles
- Measuring kernel vs userspace CPU overhead per thread
- Identifying cache-thrashing or branch-prediction issues
- Supplementing Perfetto trace analysis with hardware PMU counter data

This skill complements `hz-perfetto-debug`. Perfetto shows *what* your app is doing over time. Simpleperf shows *where* the CPU is spending hardware cycles — cache misses, branch mispredictions, and instruction throughput that Perfetto can't see.

## VR Performance Context

Quest devices run on mobile ARM SoCs with strict thermal and power budgets. CPU-bound apps hit frame drops when:

| Refresh Rate | CPU Frame Budget | Notes |
|-------------|-----------------|-------|
| 120 Hz | 8.3 ms | Tight — simpleperf critical for finding hotspots |
| 90 Hz | 11.1 ms | Default target for most apps |
| 72 Hz | 13.9 ms | Fallback for heavier apps |

Simpleperf's hardware counters reveal bottlenecks invisible to software tracing.

## hzdb Setup

Simpleperf profiling is powered by the hzdb CLI. Invoke via `npx` — no install required:

```bash
npx -y @meta-quest/hzdb --version
```

Examples below use the bare `hzdb` command for brevity — substitute `npx -y @meta-quest/hzdb`. Connect your Quest via USB with developer mode enabled.

## Quick Start Workflow

### 1. Classify the Workload

Before optimizing, determine the bottleneck type:

```bash
# Classify the foreground app's workload (10-second sample)
hzdb perf simpleperf classify

# Target a specific app
hzdb perf simpleperf classify --app com.example.myapp

# Custom duration
hzdb perf simpleperf classify --duration 15
```

Returns a classification with evidence:

| Classification | Indicator | Optimization Strategy |
|---------------|-----------|----------------------|
| **CPU-bound** | High IPC, low stall ratio | Optimize algorithms, reduce draw calls, batch work |
| **Memory-bound** | High stall ratio (stalled-cycles-backend / cpu-cycles) | Reduce cache misses, improve data locality, shrink working set |
| **I/O-bound** | High context switches per second | Reduce blocking I/O, use async, minimize thread contention |

### 2. Record CPU Hotspots

Capture a CPU cycle profile to find the most expensive functions:

```bash
# Record CPU hotspots for the foreground app
hzdb perf simpleperf record

# Custom frequency and duration
hzdb perf simpleperf record --frequency 4000 --duration 10

# Target a specific app
hzdb perf simpleperf record --app com.example.myapp
```

The recording samples CPU cycles at the specified frequency (default 4000 Hz) and generates a profile showing which functions consume the most CPU time.

### 3. Measure Kernel Overhead

Determine how much CPU time is spent in kernel vs userspace per thread:

```bash
# Measure kernel overhead for the foreground app
hzdb perf simpleperf kernel-overhead

# Custom duration
hzdb perf simpleperf kernel-overhead --app com.example.myapp --duration 10
```

Returns per-thread breakdown of user-mode vs kernel-mode CPU cycles. High kernel overhead (>20%) in a thread suggests:

- Excessive syscalls (file I/O, memory allocation)
- Driver overhead (GPU command submission, sensor access)
- Lock contention in kernel synchronization primitives

## Analysis Workflow

### Step 1: Classify First

Always start with classification. This prevents wasting time optimizing the wrong thing.

```bash
hzdb perf simpleperf classify --app com.example.myapp --duration 10
```

**Decision tree based on results:**

- **CPU-bound** → Record hotspots (Step 2a), look at top functions
- **Memory-bound** → Record with cache-miss events, check data access patterns
- **I/O-bound** → Check kernel overhead, look at thread contention in Perfetto

### Step 2a: CPU-Bound Apps — Find Hotspots

```bash
hzdb perf simpleperf record --app com.example.myapp --duration 10
```

Review the top functions by CPU cycle consumption. Common VR hotspots:

| Function Pattern | Likely Cause | Fix |
|-----------------|-------------|-----|
| `Physics.*` / `PhysX` | Complex physics simulation | Reduce collider count, simplify meshes, increase fixed timestep |
| `Render*` / `Draw*` | Too many draw calls | Batch materials, use GPU instancing, reduce unique materials |
| `GC_*` / `gc_alloc` | Garbage collection pressure | Pool allocations, avoid per-frame allocations |
| `memcpy` / `memmove` | Large data copies | Use references, reduce buffer sizes, avoid unnecessary copies |
| `LZ4_*` / `compress` | Asset decompression | Pre-decompress, use lighter compression, cache results |

### Step 2b: Memory-Bound Apps — Check Cache Behavior

If classification shows memory-bound, the issue is likely cache misses or memory bandwidth:

- Large working sets thrashing L1/L2 cache
- Random access patterns defeating prefetcher
- False sharing between threads on adjacent cache lines

Use Perfetto `hz-perfetto-debug` to correlate memory-bound regions with specific code paths.

### Step 3: Measure Kernel Overhead

```bash
hzdb perf simpleperf kernel-overhead --app com.example.myapp
```

**Interpreting results by thread:**

| Thread | Expected Kernel % | High Kernel % Indicates |
|--------|------------------|------------------------|
| Main/Game thread | < 5% | Excessive file I/O, logging, or allocations |
| Render thread | 5-15% | Normal (GPU driver overhead). >20% = driver issue |
| Worker threads | < 5% | Thread synchronization overhead |
| Audio thread | < 10% | Normal for audio HAL calls |

### Step 4: Combine with Perfetto

Simpleperf tells you *where* cycles go. Perfetto tells you *when* and in what context. Use together:

1. Simpleperf classification reveals the bottleneck type
2. Simpleperf hotspot recording identifies the expensive functions
3. Perfetto trace (`hzdb perf capture`) shows when those functions run relative to frame boundaries
4. Use `hzdb perf query` to correlate function timing with frame drops

## Common Pitfalls

- **Don't profile in thermal throttling.** Let the device cool before recording — throttled clocks distort cycle counts. Check thermal state first with `hzdb device info`.
- **Sample duration matters.** Short recordings (<5s) may not capture representative behavior. Use at least 10 seconds for classification.
- **simpleperf requires shell access.** If `adb shell simpleperf` fails, ensure developer mode is enabled and USB debugging is authorized.
- **Frequency vs accuracy tradeoff.** Higher sampling frequency (>8000 Hz) can perturb the workload on mobile SoCs. Default 4000 Hz is a good balance.
- **Classification is a snapshot.** An app can be CPU-bound during gameplay and I/O-bound during scene loads. Profile the specific scenario you're optimizing.

## References

For detailed guides on specific topics, see:

- [Workload Classification](references/workload-classification.md) — PMU counter interpretation and bottleneck identification
- [CPU Hotspot Analysis](references/cpu-hotspot-analysis.md) — Recording and analyzing CPU cycle profiles
- [Kernel Overhead](references/kernel-overhead.md) — Measuring and reducing kernel-mode CPU usage
