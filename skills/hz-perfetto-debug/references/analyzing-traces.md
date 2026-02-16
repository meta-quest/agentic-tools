# Analyzing Perfetto Traces

## Overview

This reference walks through a systematic approach to analyzing a Perfetto trace from a Meta Quest device. Follow these steps in order for a thorough investigation.

## Loading a Trace

```bash
# Find available traces
hzdb perf traces

# Load by filename, path, or hex session ID
hzdb perf load my-trace.pftrace
hzdb perf load /path/to/trace.pftrace
hzdb perf load a1b2c3d4
```

Once loaded, the trace remains available for queries until a new trace is loaded.

## Step 0: Validate Trace Quality

Always verify the trace before investing time in analysis.

```sql
-- Check trace duration and slice count
SELECT
  CAST((MAX(ts) - MIN(ts)) / 1e9 AS REAL) AS duration_seconds,
  COUNT(*) AS total_slices
FROM slice
```

**Minimum requirements:**
- Duration: >= 2.0 seconds
- Slices: >= 1,000

```sql
-- Check for process data
SELECT COUNT(DISTINCT upid) AS process_count,
       COUNT(DISTINCT utid) AS thread_count
FROM thread
```

If the trace fails validation, request a new capture with proper configuration.

## Step 1: Find the Target Process

Filter out system processes to find the application under test:

```sql
SELECT upid, pid, name
FROM process
WHERE name IS NOT NULL
  AND name != ''
  AND name NOT LIKE '/system/%'
  AND name NOT LIKE 'com.oculus.%'
  AND name NOT LIKE 'com.android.%'
  AND name NOT LIKE 'com.qualcomm.%'
  AND name NOT LIKE 'android.%'
  AND name NOT LIKE 'surfaceflinger'
  AND name NOT LIKE 'system_server'
ORDER BY pid
```

If you know the package name, query directly:

```sql
SELECT upid, pid, name FROM process WHERE name LIKE '%com.example.myapp%'
```

Note the `upid` value for subsequent queries.

## Step 2: Identify Key Threads

```sql
SELECT
  t.utid,
  t.tid,
  t.name AS thread_name,
  COUNT(s.id) AS slice_count,
  CAST(SUM(s.dur) / 1e6 AS REAL) AS total_dur_ms
FROM thread t
LEFT JOIN thread_track tt ON tt.utid = t.utid
LEFT JOIN slice s ON s.track_id = tt.id
WHERE t.upid = <TARGET_UPID>
GROUP BY t.utid
ORDER BY total_dur_ms DESC
```

### Identifying Thread Roles

| Thread Name Pattern | Role | Engine |
|--------------------|------|--------|
| `UnityMain` | Main/game thread | Unity |
| `UnityGfx` | Render thread | Unity |
| `UnityChoreWorker` | Job worker | Unity |
| `Job.Worker *` | Job system workers | Unity |
| `GameThread` | Main/game thread | Unreal |
| `RenderThread` | Render command generation | Unreal |
| `RHI Thread` | GPU command submission | Unreal |
| `TaskGraph *` | Task system workers | Unreal |
| `GPU completion` | GPU fence monitoring | Both |
| `OVRPollEvent` | Oculus event polling | Both |

Record the `utid` values for the main thread, render thread, and GPU completion thread.

## Step 3: Detect Game Engine

```sql
-- Look for Unity markers
SELECT COUNT(*) AS unity_markers
FROM slice
WHERE name IN ('PlayerLoop', 'PhaseSync', 'UnityMain')

-- Look for Unreal markers
SELECT COUNT(*) AS unreal_markers
FROM slice
WHERE name LIKE 'FEngineLoop%' OR name LIKE 'FDeferredShading%'

-- Look for raw OpenXR markers
SELECT COUNT(*) AS openxr_markers
FROM slice
WHERE name LIKE 'xr%Frame'
```

## Step 4: Frame Boundary Analysis

### Unity Frame Boundaries

```sql
SELECT
  s.ts,
  CAST(s.dur / 1e6 AS REAL) AS frame_ms,
  s.name
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
ORDER BY s.ts
```

### Unreal Frame Boundaries

```sql
SELECT
  s.ts,
  CAST(s.dur / 1e6 AS REAL) AS frame_ms,
  s.name
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name LIKE 'FEngineLoop::Tick%'
ORDER BY s.ts
```

### Frame Time Statistics

```sql
SELECT
  COUNT(*) AS frame_count,
  CAST(AVG(dur) / 1e6 AS REAL) AS avg_ms,
  CAST(MIN(dur) / 1e6 AS REAL) AS min_ms,
  CAST(MAX(dur) / 1e6 AS REAL) AS max_ms,
  CAST(
    SQRT(AVG(dur * dur) - AVG(dur) * AVG(dur)) / 1e6
    AS REAL
  ) AS stddev_ms
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
```

## Step 5: CPU Breakdown — Expensive Functions

### Top Functions by Total Time

```sql
SELECT
  name,
  COUNT(*) AS calls,
  CAST(SUM(dur) / 1e6 AS REAL) AS total_ms,
  CAST(AVG(dur) / 1e6 AS REAL) AS avg_ms,
  CAST(MAX(dur) / 1e6 AS REAL) AS max_ms
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
GROUP BY name
ORDER BY total_ms DESC
LIMIT 20
```

### Hierarchical Call Tree

To understand parent-child relationships between slices:

```sql
SELECT
  parent.name AS parent_name,
  child.name AS child_name,
  COUNT(*) AS occurrences,
  CAST(SUM(child.dur) / 1e6 AS REAL) AS child_total_ms
FROM slice child
JOIN slice parent ON child.parent_id = parent.id
JOIN thread_track tt ON child.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
GROUP BY parent.name, child.name
ORDER BY child_total_ms DESC
LIMIT 20
```

### Per-Frame Function Costs

To see what functions consume time within individual frames:

```sql
SELECT
  frame.ts AS frame_start,
  CAST(frame.dur / 1e6 AS REAL) AS frame_ms,
  child.name AS function_name,
  CAST(child.dur / 1e6 AS REAL) AS function_ms,
  CAST(100.0 * child.dur / frame.dur AS REAL) AS pct_of_frame
FROM slice frame
JOIN slice child ON child.parent_id = frame.id
JOIN thread_track tt ON frame.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND frame.name = 'PlayerLoop'
ORDER BY frame.ts, function_ms DESC
```

## Reading the Output

### Time Units

All Perfetto timestamps and durations are in **nanoseconds**:

| Raw Value | Conversion | Result |
|-----------|-----------|--------|
| 1,000 ns | / 1e3 | 1 us |
| 1,000,000 ns | / 1e6 | 1 ms |
| 1,000,000,000 ns | / 1e9 | 1 s |

### Interpreting Frame Times

| Frame Time | At 90 Hz | Assessment |
|-----------|----------|------------|
| < 8 ms | Well under budget | Excellent |
| 8 - 11 ms | Within budget | Good |
| 11.1 - 13 ms | Over budget | Dropping frames |
| 13 - 16.7 ms | Significantly over | Severe frame drops |
| > 16.7 ms | Missing multiple frames | Critical |

### What to Look For

1. **Spikes**: Single frames significantly above average indicate hitches (GC, asset loading, shader compilation)
2. **Consistent over-budget**: Average frame time above target indicates systemic performance issues
3. **High variance**: Standard deviation > 2 ms indicates inconsistent performance even if average is acceptable
4. **Thread imbalance**: If one thread dominates frame time while others are idle, work distribution needs improvement
