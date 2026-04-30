---
name: hz-perfetto-debug
description: Analyzes Meta Quest and Horizon OS VR performance using Perfetto traces — frame timing, CPU/GPU bottlenecks, render pass analysis. Use when profiling frame drops, jank, or thermal issues on Quest devices.
allowed-tools:
  - Bash(hzdb:*)
---

# Perfetto Debug Skill

## When to Use

Use this skill when investigating VR performance issues on Meta Quest devices:

- Frame drops, jank, or stuttering
- CPU or GPU bottlenecks
- Render pass overhead and GPU utilization
- Thermal throttling and clock frequency changes
- Frame timing variance and missed vsync deadlines
- Thread contention and synchronization issues
- High draw call counts or overdraw

## VR Frame Time Targets

These are the hard deadlines for each refresh rate. If a frame exceeds its target, the compositor must reproject or the user sees a stale frame.

| Refresh Rate | Frame Time Budget | Notes |
|-------------|------------------|-------|
| 120 Hz | 8.3 ms | Supported on Quest 2, Quest 3, Quest 3S |
| 90 Hz | 11.1 ms | Supported on Quest 2, Quest Pro, Quest 3, Quest 3S |
| 72 Hz | 13.9 ms | Default on all Quest devices |
| 60 Hz | 16.7 ms | Media apps only (Quest 2); interactive apps must use 72 Hz+ |

Missing a frame deadline by even 1 ms causes a stale frame (reprojection). Stale frames above 10% of total frames indicate a serious performance problem.

## hzdb Setup

Perfetto tracing is powered by the hzdb CLI. Invoke via `npx` — no install required:

```bash
npx -y @meta-quest/hzdb --version
```

Examples below use the bare `hzdb` command for brevity — substitute `npx -y @meta-quest/hzdb`. Connect your Quest via USB with developer mode enabled before capturing traces.

## Quick Start Workflow

### 1. Capture a Trace

```bash
# Capture a 5-second trace from the currently running VR app
hzdb perf capture

# Specify duration and target app
hzdb perf capture --duration 10000 --app com.example.myapp

# Enable GPU render stage tracing for detailed pass analysis
hzdb perf capture --gpu-render-stage

# Enable XR runtime metrics
hzdb perf capture --xr-runtime

# Custom output name
hzdb perf capture -o my-session-name
```

The capture auto-detects the foreground VR app if `--app` is not specified. CPU scheduling and GPU metrics tracing are enabled by default. The trace is pulled to your local machine automatically.

### 2. List Available Traces

```bash
hzdb perf traces
```

Returns `.pftrace` files sorted by modification time (newest first). Searches standard directories including `~/Documents`, `~/Downloads`, and the current working directory.

### 3. Load a Trace

```bash
hzdb perf load <trace-file>
```

Loads and processes the trace for analysis. Accepts a hex session ID, filename (with or without `.pftrace` extension), or a full/relative path.

### 4. Get Performance Overview

```bash
hzdb perf context
```

Returns a structured performance analysis including:
- CPU and GPU frame timing statistics
- Thread breakdown with utilization percentages
- GPU counter summaries (if available)
- Detected bottlenecks and recommendations

### 5. Run SQL Queries

```bash
hzdb perf query <session-id> "SELECT ts, dur, name FROM slice WHERE name LIKE '%PlayerLoop%' LIMIT 20"
```

Executes arbitrary SQL against the loaded Perfetto trace database. All Perfetto tables are available: `slice`, `thread_track`, `thread`, `process`, `counter`, `counter_track`, `args`, `sched_slice`, and more.

### 6. Analyze Thread States

```bash
hzdb perf thread-state <session-id> <utid>

# With time range
hzdb perf thread-state <session-id> <utid> --start-ts 1000000 --end-ts 5000000000
```

Returns a thread state breakdown showing how much time the thread spent running, sleeping, blocked, or waiting for CPU. Useful for identifying whether a thread is CPU-bound, I/O-bound, or starved.

### 7. Get GPU Metrics

```bash
hzdb perf gpu-counters <session-id> --start-ts 100,200,300 --end-ts 150,250,350
```

Returns GPU metric counters (mean, standard deviation, quantiles) for GPU frame ranges. Requires at least 20 frames for statistical accuracy. Metrics include texture fetch rates, shader ALU capacity, vertex processing, and fragment shading statistics.

## Detailed Analysis Workflow

Follow these steps in order for a thorough performance investigation.

### Step 1: Validate Trace Quality

Before analyzing, confirm the trace is usable:

- **Duration**: At least 2 seconds of data (ideally 3-5 seconds)
- **Slice count**: Should have thousands of slices for a meaningful trace
- **Process presence**: The target app process must be present

```sql
SELECT
  (MAX(ts) - MIN(ts)) / 1e9 AS duration_seconds,
  COUNT(*) AS total_slices
FROM slice
```

If the trace has fewer than 1000 slices or is under 1 second, it may not contain enough data for meaningful analysis. Capture a new trace with `hzdb perf capture`.

### Step 2: Identify Target Process

Find the application process (not system services):

```sql
SELECT upid, pid, name
FROM process
WHERE name NOT LIKE 'com.oculus%'
  AND name NOT LIKE '/system%'
  AND name NOT LIKE 'com.android%'
  AND name IS NOT NULL
ORDER BY pid
```

For known apps, filter directly by package name.

### Step 3: Identify Game Engine

Look for engine-specific markers:

| Engine | Key Markers |
|--------|------------|
| Unity | `PlayerLoop`, `UnityMain`, `PhaseSync`, `PostLateUpdate.FinishRendering` |
| Unreal | `UGameEngine::Tick`, `FEngineLoop::Tick`, `RHI Thread` |
| Native OpenXR | `xrWaitFrame`, `xrBeginFrame`, `xrEndFrame` without engine markers |

### Step 4: Find Key Threads

Identify the threads that matter for VR rendering:

```sql
SELECT t.utid, t.tid, t.name, p.name AS process_name
FROM thread t
JOIN process p USING(upid)
WHERE p.name = '<target-process>'
ORDER BY t.name
```

Critical threads to locate:

| Thread | Purpose |
|--------|---------|
| Main thread (UnityMain / GameThread) | Game logic, physics, scripts |
| Render thread (UnityGfx / RenderThread) | Draw call submission |
| GPU completion (GPU completion / RHI Thread) | GPU fence waiting |
| Worker threads (Job.Worker / TaskGraph) | Parallel workloads |

Once you have a thread's `utid`, use `hzdb perf thread-state <session-id> <utid>` to get a quick breakdown of its running/sleeping/blocked time.

### Step 5: Detect Frame Boundaries

Find frame start/end markers to segment per-frame analysis:

- **Unity**: `PlayerLoop` slices on the main thread define frame boundaries
- **Unreal**: `FEngineLoop::Tick` slices on the game thread
- **OpenXR**: `xrWaitFrame` to `xrEndFrame` sequences

### Step 6: Analyze Expensive Functions

Find what consumes the most time per frame:

```sql
SELECT name, COUNT(*) AS call_count, SUM(dur)/1e6 AS total_ms, AVG(dur)/1e6 AS avg_ms
FROM slice
WHERE track_id IN (
  SELECT id FROM thread_track WHERE utid = <main_thread_utid>
)
GROUP BY name
ORDER BY total_ms DESC
LIMIT 20
```

### Step 7: Check High-Frequency Calls

Functions called excessively per frame can indicate batching issues:

```sql
SELECT name, COUNT(*) AS calls
FROM slice
WHERE track_id IN (
  SELECT id FROM thread_track WHERE utid = <utid>
)
  AND dur < 100000
GROUP BY name
HAVING calls > 1000
ORDER BY calls DESC
```

### Step 8: Analyze GPU Render Passes

See the GPU analysis reference for detailed render pass breakdown, surface analysis, and GPU counter interpretation.

## Key Perfetto Concepts

| Concept | Description |
|---------|------------|
| **Slice** | A timed span of execution (function call, frame, render pass). Has `ts` (start), `dur` (duration), `name`, and `track_id`. |
| **Track** | A timeline lane. Thread tracks hold slices for a specific thread. Counter tracks hold metric values over time. |
| **Thread (utid)** | Unique thread ID within the trace. Use `utid` (not `tid`) for joins — `tid` can be reused. |
| **Process (upid)** | Unique process ID within the trace. Use `upid` (not `pid`) for joins. |
| **Timestamps** | All timestamps are in **nanoseconds**. Divide by 1e6 for milliseconds, 1e9 for seconds. |
| **Counter** | A time-series metric (GPU utilization, clock frequency, temperature). Stored in the `counter` table. |
| **Args** | Key-value metadata attached to slices. Accessed via the `args` table joined on `arg_set_id`. |

## Performance Targets

| Metric | Target | Warning | Critical |
|--------|--------|---------|----------|
| Frame time (90 Hz) | < 11.1 ms | > 11.1 ms | > 16.7 ms |
| Stale frame rate | < 5% | > 10% | > 25% |
| Main thread utilization | < 80% of budget | > 80% | > 95% |
| GPU utilization | < 85% of budget | > 85% | > 95% |
| Frame variance (std dev) | < 1 ms | > 2 ms | > 4 ms |
| Draw calls per frame | < 100 | > 200 | > 500 |

## Engine-Specific Notes

### Unity

- **PhaseSync**: VR vsync alignment mechanism. Appears as idle time at the start of `PlayerLoop`. This is normal and intentional — do NOT flag as wasted time.
- **Single-pass multiview**: Both eyes rendered in one pass. If you see two render passes per frame, the app may be using multi-pass rendering (less efficient).
- **Dynamic batching**: Watch for high `SetPass` call counts, which indicate materials are not being batched.
- **IL2CPP vs Mono**: IL2CPP builds have different function naming in traces. Look for mangled C++ names instead of C# method names.

### Unreal Engine

- **RHI Thread**: Unreal uses a separate RHI (Render Hardware Interface) thread for GPU command submission. Check this thread for driver overhead.
- **Forward vs Deferred**: Forward rendering is preferred on Quest. Deferred rendering has significantly higher GPU cost.
- **Blueprint Tick**: Heavy Blueprint usage shows up as `UObject::ProcessEvent`. High counts indicate Blueprints should be converted to C++.
- **Nativized Blueprints**: Show up with `__StaticExec` suffix in trace names.

## Common Pitfalls

- Do NOT report PhaseSync or xrWaitFrame idle time as a performance problem — these are intentional frame pacing mechanisms.
- GPU render pass names like `surface#0` are not descriptive — correlate them with the resolution and MSAA level to identify what they render.
- Thread names can be truncated in traces. `UnityMain` may appear as `UnityMai` or similar.
- Always use `utid` (not `tid`) when joining thread-related tables in SQL queries.
- Timestamps are nanoseconds. A common mistake is treating them as microseconds.
- Counter values are instantaneous samples, not averages over a period.

## References

For detailed guides on specific topics, see:

- [Capturing Traces](references/capturing-traces.md) — How to capture Perfetto traces on Quest
- [Analyzing Traces](references/analyzing-traces.md) — Step-by-step trace analysis with SQL queries
- [GPU Analysis](references/gpu-analysis.md) — Render pass analysis, GPU counters, and metrics
- [Frame Timing](references/frame-timing.md) — VR frame markers, pacing, and compositor timing
- [SQL Queries](references/sql-queries.md) — Ready-to-use SQL queries for common analysis tasks
