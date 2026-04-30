# Workload Classification

## Overview

Workload classification uses hardware Performance Monitoring Unit (PMU) counters to determine whether your app's CPU bottleneck is compute-bound, memory-bound, or I/O-bound. This guides your optimization strategy before you spend time profiling.

## How It Works

The `hzdb perf simpleperf classify` command runs `simpleperf stat` on the target process, collecting five hardware counters simultaneously:

| Counter | What It Measures |
|---------|-----------------|
| `cpu-cycles` | Total CPU clock cycles consumed |
| `instructions` | Total instructions retired |
| `cache-misses` | Last-level cache misses (main memory accesses) |
| `context-switches` | Voluntary + involuntary context switches |
| `stalled-cycles-backend` | Cycles stalled waiting for memory/cache |

## Interpreting Results

### Instructions Per Cycle (IPC)

IPC = `instructions / cpu-cycles`. Measures how efficiently the CPU executes instructions.

| IPC | Meaning | Typical Cause |
|-----|---------|---------------|
| > 2.0 | Excellent efficiency | Well-optimized compute code |
| 1.0 - 2.0 | Good | Normal application code |
| 0.5 - 1.0 | Moderate stalls | Some cache pressure |
| < 0.5 | Severe stalls | Memory-bound, cache thrashing |

ARM Cortex-A cores (Quest SoC) can theoretically retire 4-8 instructions per cycle. Real-world VR apps typically achieve 1.0-2.5 IPC.

### Stall Ratio

Stall ratio = `stalled-cycles-backend / cpu-cycles`. Measures what fraction of cycles the CPU was waiting for data from memory.

| Stall Ratio | Classification |
|-------------|---------------|
| < 20% | Not memory-bound |
| 20-40% | Moderate memory pressure |
| > 40% | Severely memory-bound |

### Context Switch Rate

Context switches per second = `context-switches / duration`.

| Rate | Classification |
|------|---------------|
| < 1000/s | Normal |
| 1000-5000/s | Moderate contention |
| > 5000/s | I/O-bound or lock contention |

High context-switch rates in VR apps often indicate:
- Blocking file I/O on the main thread
- Mutex contention between game logic and render threads
- Excessive thread synchronization in job systems

## Classification Algorithm

The classification uses these decision rules:

1. **I/O-bound**: Context switch rate > 5000/s AND stall ratio < 30%
2. **Memory-bound**: Stall ratio > 30% AND IPC < 1.0
3. **CPU-bound**: IPC > 1.5 AND stall ratio < 20%
4. **Mixed**: Doesn't clearly fit one category — investigate further

## Example Output

```
Workload Classification: CPU-BOUND

Duration: 10.0s
Process: com.example.myapp (PID 12345)

Counters:
  cpu-cycles:              2,847,391,000
  instructions:            4,128,556,000
  cache-misses:               12,847,000
  context-switches:                1,247
  stalled-cycles-backend:    341,687,000

Derived Metrics:
  IPC:                    1.45
  Stall ratio:           12.0%
  Context switches/sec:  124.7
  Cache miss rate:        0.31%

Diagnosis: App is compute-bound. High IPC with low memory stalls
indicates the CPU is executing efficiently but has too much work
to complete within the frame budget. Focus on algorithmic
optimization, reducing per-frame workload, or offloading to GPU.
```

## VR-Specific Guidance

### Per-Thread Classification

VR apps have multiple critical threads with different characteristics:

| Thread | Expected Profile | Action if Unexpected |
|--------|-----------------|---------------------|
| Main thread | CPU-bound (game logic) | If memory-bound: check asset loading on main thread |
| Render thread | Mixed (CPU for draw calls, some I/O for driver) | If I/O-bound: check GPU driver stalls |
| Physics thread | CPU-bound (simulation) | If memory-bound: reduce collision mesh complexity |
| Audio thread | I/O-bound (HAL calls) | Normal — audio uses blocking ring buffers |

### Thermal Considerations

Quest devices throttle CPU clocks under sustained load. Classification during throttling shows artificially low IPC because the CPU is running at reduced frequency. Always:

1. Let the device cool before profiling (5 minutes idle)
2. Check clock frequency: `hzdb shell cat /sys/devices/system/cpu/cpu*/cpufreq/scaling_cur_freq`
3. Profile during the first 30 seconds of a cold session for unthrottled behavior
