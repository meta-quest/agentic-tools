# hzdb Performance Tools

Performance commands let you capture, load, query, and analyze Perfetto traces from
Meta Quest devices. These tools help identify frame drops, CPU/GPU bottlenecks, and
threading issues.

## Commands Overview

| Command | Description |
|---|---|
| `hzdb perf capture` | Capture a Perfetto trace from a connected device |
| `hzdb perf load <session_id>` | Load a Perfetto trace for analysis |
| `hzdb perf context [session_id]` | Get CPU+GPU performance overview |
| `hzdb perf analyze-trace [session_id]` | Run a complete trace analysis |
| `hzdb perf open <session_id>` | Open a trace in ui.perfetto.dev |
| `hzdb perf query <session_id> <sql>` | Run SQL queries against a loaded trace |
| `hzdb perf thread-state` | Get thread state summary for a time range |
| `hzdb perf gpu-counters` | Get GPU metric counters for frame ranges |
| `hzdb perf start` | Start a background Perfetto capture |
| `hzdb perf stop <pid> <output_name>` | Stop a background capture and pull the trace |
| `hzdb perf compare <baseline_id> <comparison_id>` | Compare two traces and produce a delta report |
| `hzdb perf traces` | List locally available trace sessions |
| `hzdb perf hex-to-datetime <hex>` | Convert hex timestamp to datetime |

## Performance Targets

Meta Quest headsets run at different refresh rates. Your app must hit the frame
budget for the configured refresh rate to avoid dropped frames and user discomfort.

| Refresh Rate | Frame Budget |
|---|---|
| 120 Hz | 8.3 ms per frame |
| 90 Hz | 11.1 ms per frame |
| 72 Hz | 13.9 ms per frame |
| 60 Hz | 16.7 ms per frame |

Both the CPU and GPU must finish their work within the budget. If either exceeds
the budget, the frame is "stale" and the runtime will reproject the previous frame.

## Key Metrics

When analyzing Quest performance, focus on these metrics:

- **Frame time** — total time to produce each frame (should stay under budget)
- **Stale frames** — frames that missed the deadline and were reprojected
- **Main thread utilization** — time the main thread spends doing work vs. idle
- **Render thread utilization** — time the render thread spends submitting draw calls
- **GPU utilization** — how busy the GPU is each frame
- **GPU frequency** — current GPU clock speed (throttling indicates thermal issues)

## hzdb perf capture

Capture a Perfetto trace directly from a connected device.

```bash
# Capture for 5 seconds (default)
hzdb perf capture

# Use a mode preset
hzdb perf capture --mode gpu --duration 10000

# Capture for a specific duration
hzdb perf capture --duration 10000

# Specify target app (auto-detects if not specified)
hzdb perf capture --app com.mycompany.myapp

# Specify output filename
hzdb perf capture --output my_trace

# Enable GPU render stage tracing
hzdb perf capture --mode custom --gpu-render-stage

# Enable XR runtime metrics
hzdb perf capture --mode custom --xr-runtime
```

### Capture Options

| Option | Description | Default |
|---|---|---|
| `--mode <mode>` | Preset: `standard`, `gpu`, `cpu`, `lightweight`, `full`, or `custom` | `standard` |
| `--duration <ms>` | Capture duration in milliseconds | 5000 |
| `--app <package>` | App package name to trace | Auto-detected |
| `--output <name>` | Output filename (without extension) | Auto-generated |
| `--gpu-render-stage` | Enable GPU render stage tracing | `custom` mode only |
| `--gpu-metrics` | Enable GPU metrics tracing | `custom` mode only |
| `--cpu-scheduling` | Enable CPU scheduling tracing | `custom` mode only |
| `--xr-runtime` | Enable XR runtime metrics | `custom` mode only |
| `--vulkan-layer` | Enable Vulkan OS layer tracing | `custom` mode only |
| `--extended-scheduling` | Enable extended scheduling events | `custom` mode only |

After capture completes, the tool prints the session ID for use with other perf commands.

## hzdb perf load

Load a Perfetto trace file for analysis. This parses the trace and prepares it
for querying.

```bash
# Load by session ID
hzdb perf load a1b2c3d4

# Load by filename
hzdb perf load mytrace.pftrace

# Load by full path
hzdb perf load /path/to/mytrace.pftrace
```

Once loaded, the trace can be queried with `perf query`, `perf thread-state`, and
`perf gpu-counters`.

## hzdb perf context

Get a high-level performance analysis overview for a loaded trace, covering both
CPU and GPU metrics.

```bash
# Get context for a specific trace
hzdb perf context my_session_id

# Get general performance analysis context
hzdb perf context
```

The context includes:

- Frame timing statistics (average, p50, p90, p99)
- Stale frame count and percentage
- CPU thread utilization breakdown
- GPU utilization and frequency data
- Identified bottlenecks and recommendations

This is the best starting point for any performance investigation.

## hzdb perf analyze-trace

Run a complete analysis for a trace. If no session ID is provided, hzdb uses the
most recently captured trace.

```bash
# Analyze the latest trace
hzdb perf analyze-trace

# Focus on GPU, CPU, frames, or threads
hzdb perf analyze-trace my_session_id --focus gpu
hzdb perf analyze-trace my_session_id --focus frames
```

Use this before custom SQL when you want a guided summary and recommended next
steps.

## hzdb perf open

Open a captured trace in the Perfetto web UI.

```bash
hzdb perf open my_session_id
```

Use this when visual inspection is faster than command-line summaries.

## hzdb perf query

Run SQL queries directly against a loaded Perfetto trace using the Perfetto SQL
engine.

```bash
hzdb perf query my_session_id "SELECT * FROM slice LIMIT 10"
```

### Useful Queries

List all tracks (threads, processes, counters):

```sql
SELECT * FROM thread ORDER BY name
```

Find the longest slices (potential performance bottlenecks):

```sql
SELECT name, dur/1e6 AS dur_ms, ts
FROM slice
ORDER BY dur DESC
LIMIT 20
```

Get frame timing from the compositor:

```sql
SELECT ts, dur, dur/1e6 AS dur_ms
FROM slice
WHERE name LIKE '%Frame%'
ORDER BY ts
```

Find all slices for a specific thread:

```sql
SELECT s.name, s.dur/1e6 AS dur_ms, s.ts
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
JOIN thread t ON tt.utid = t.utid
WHERE t.name = 'MainThread'
ORDER BY s.dur DESC
LIMIT 20
```

Check GPU frequency over time:

```sql
SELECT ts, value AS freq_mhz
FROM counter c
JOIN counter_track ct ON c.track_id = ct.id
WHERE ct.name LIKE '%GPU%freq%'
ORDER BY ts
```

## hzdb perf thread-state

Get a summary of thread states (Running, Sleeping, Blocked, etc.) for a specific
thread within a time range. This helps identify if a thread is CPU-bound, blocked
on I/O, or waiting on synchronization.

```bash
hzdb perf thread-state my_session_id 42 \
  --start-ts 1000000000 \
  --end-ts 2000000000
```

Arguments:

- `session_id` — the trace session identifier
- `utid` — the unique thread ID from the Perfetto trace
- `--start-ts` — start time in nanoseconds (default: 0)
- `--end-ts` — end time in nanoseconds (default: max)

The output includes:

- Duration spent in each thread state
- Percentage breakdown (Running, Sleeping, Runnable, Blocked, etc.)
- State transition details

### Finding a Thread's utid

```sql
SELECT utid, tid, name FROM thread WHERE name LIKE '%Main%'
```

## hzdb perf gpu-counters

Get GPU hardware counter statistics across a set of frame time ranges. Provide
start and end timestamps for at least 20 frames for statistically meaningful results.

```bash
hzdb perf gpu-counters my_session_id \
  --start-ts 1000000,2000000,3000000 \
  --end-ts 1500000,2500000,3500000
```

Arguments:

- `session_id` — the trace session identifier
- `--start-ts` — comma-separated start timestamps in nanoseconds
- `--end-ts` — comma-separated end timestamps in nanoseconds

Returns per-counter statistics including:

- Mean value across frames
- Standard deviation
- Quantile breakdown (p50, p90, p99)

### Key GPU Counters

| Counter | What It Measures |
|---|---|
| GPU % Utilization | Overall GPU busyness |
| Fragment % Utilization | Pixel/fragment shader load |
| Vertex % Utilization | Vertex shader load |
| GPU Frequency | Current clock speed in MHz |
| Stalled on System Memory | Cycles stalled waiting on memory |

High GPU utilization with low CPU utilization indicates a GPU-bound workload.
Consider reducing draw calls, shader complexity, resolution, or overdraw.

## hzdb perf start / stop

Use manual start/stop capture when you need to bracket a scenario yourself
instead of using a fixed duration.

```bash
# Start a background capture
hzdb perf start --mode full --app com.mycompany.myapp --output before_fix

# Reproduce the issue, then stop using the PID and output name printed by start
hzdb perf stop <pid> before_fix
```

This is useful for scenarios with variable setup time, such as entering a level,
joining a multiplayer session, or waiting for a thermal/performance state.

## hzdb perf compare

Compare two traces and produce a delta report.

```bash
hzdb perf compare before_fix after_fix
```

Use this after an optimization to check whether frame timing, CPU behavior, or
GPU behavior changed in the expected direction.

## hzdb perf traces

List available local trace sessions.

```bash
hzdb perf traces
hzdb perf traces --limit 20
```

## hzdb perf hex-to-datetime

Convert a hexadecimal timestamp to a human-readable datetime.

```bash
hzdb perf hex-to-datetime 67c8a2b6
```

Returns the timestamp in RFC 3339 format. Useful when working with trace filenames
or session IDs that encode timestamps.

## Performance Investigation Workflow

A typical performance debugging session:

```bash
# 1. Capture a trace from the device
hzdb perf capture --duration 10000 --app com.mycompany.myapp

# 2. Run the guided analyzer first
hzdb perf analyze-trace a1b2c3d4

# 3. Load the trace and get context when you need to drill in
hzdb perf load a1b2c3d4
hzdb perf context a1b2c3d4

# 4. Drill into specific threads or GPU counters
hzdb perf thread-state a1b2c3d4 42 \
  --start-ts 1000000000 --end-ts 2000000000

# 5. Run custom SQL queries for deeper analysis
hzdb perf query a1b2c3d4 \
  "SELECT name, dur/1e6 AS ms FROM slice WHERE dur > 11100000 ORDER BY dur DESC"

# 6. Compare before/after traces after an optimization
hzdb perf compare before_fix after_fix
```

## Tips

- Always start with `hzdb perf context` to get the big picture before diving into
  specific queries.
- Frame budgets are hard limits. Even occasional misses cause visible judder in VR.
- Thermal throttling reduces GPU/CPU frequency over time. Check frequency counters
  if performance degrades during longer sessions.
- Provide at least 20 frames of data to `gpu-counters` for reliable statistics.
- Nanosecond timestamps are used throughout. Divide by 1e6 for milliseconds or
  1e9 for seconds in query output.
