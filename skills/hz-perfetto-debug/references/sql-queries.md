# SQL Queries for VR Performance Analysis

## Overview

This reference contains ready-to-use SQL queries for common Perfetto trace analysis tasks on Meta Quest. All queries use the Perfetto SQL schema and can be executed with `hzdb perf query <session-id> "..."`.

All timestamps are in nanoseconds. Divide by `1e6` for milliseconds, `1e9` for seconds.

## Process and Thread Identification

### List All Processes

```sql
-- Find all processes with their thread counts
SELECT
  p.upid,
  p.pid,
  p.name,
  COUNT(t.utid) AS thread_count
FROM process p
LEFT JOIN thread t ON t.upid = p.upid
WHERE p.name IS NOT NULL AND p.name != ''
GROUP BY p.upid
ORDER BY thread_count DESC
```

### Find Application Process (Filter System Processes)

```sql
-- Identify the target app by excluding known system processes
SELECT upid, pid, name
FROM process
WHERE name IS NOT NULL
  AND name != ''
  AND name NOT LIKE '/system/%'
  AND name NOT LIKE 'com.oculus.%'
  AND name NOT LIKE 'com.android.%'
  AND name NOT LIKE 'com.qualcomm.%'
  AND name NOT LIKE 'android.%'
  AND name NOT IN ('surfaceflinger', 'system_server', 'zygote', 'zygote64')
ORDER BY pid
```

### List Threads for a Process

```sql
-- Get all threads for a specific process with activity metrics
SELECT
  t.utid,
  t.tid,
  t.name,
  COUNT(s.id) AS slice_count,
  CAST(SUM(s.dur) / 1e6 AS REAL) AS total_dur_ms
FROM thread t
LEFT JOIN thread_track tt ON tt.utid = t.utid
LEFT JOIN slice s ON s.track_id = tt.id
WHERE t.upid = <TARGET_UPID>
GROUP BY t.utid
ORDER BY total_dur_ms DESC
```

## Frame Timing Analysis

### Frame Timing Distribution

```sql
-- Get frame time statistics for Unity (change 'PlayerLoop' to
-- 'FEngineLoop::Tick' for Unreal)
SELECT
  COUNT(*) AS frame_count,
  CAST(AVG(dur) / 1e6 AS REAL) AS avg_ms,
  CAST(MIN(dur) / 1e6 AS REAL) AS min_ms,
  CAST(MAX(dur) / 1e6 AS REAL) AS max_ms,
  CAST(
    SQRT(AVG(dur * dur) - AVG(dur) * AVG(dur)) / 1e6
    AS REAL
  ) AS stddev_ms,
  -- Percentiles approximated via sorting
  CAST(
    (SELECT dur FROM slice s2
     JOIN thread_track tt2 ON s2.track_id = tt2.id
     WHERE tt2.utid = <MAIN_THREAD_UTID> AND s2.name = 'PlayerLoop'
     ORDER BY dur
     LIMIT 1 OFFSET (SELECT COUNT(*) * 95 / 100 FROM slice s3
       JOIN thread_track tt3 ON s3.track_id = tt3.id
       WHERE tt3.utid = <MAIN_THREAD_UTID> AND s3.name = 'PlayerLoop')
    ) / 1e6 AS REAL
  ) AS p95_ms
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
```

### Find Stutters (Frames Over 20 ms)

```sql
-- Identify individual frames that took too long, with their timestamps
-- so you can correlate with other events
SELECT
  s.ts,
  CAST(s.dur / 1e6 AS REAL) AS frame_ms,
  CAST((s.ts - (SELECT MIN(ts) FROM slice)) / 1e9 AS REAL) AS time_in_trace_sec
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
  AND s.dur > 20000000  -- 20 ms in nanoseconds
ORDER BY s.dur DESC
```

### Frame Time Histogram

```sql
-- Bucket frame times into 1 ms bins to see distribution shape
SELECT
  CAST(dur / 1000000 AS INT) AS ms_bucket,
  COUNT(*) AS frames,
  CAST(100.0 * COUNT(*) / SUM(COUNT(*)) OVER () AS REAL) AS pct
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
GROUP BY ms_bucket
ORDER BY ms_bucket
```

## Thread CPU Usage

### Thread CPU Time Breakdown

```sql
-- How much CPU time each thread consumed across the trace
SELECT
  t.name AS thread_name,
  t.utid,
  CAST(SUM(s.dur) / 1e6 AS REAL) AS cpu_time_ms,
  COUNT(s.id) AS slice_count
FROM thread t
JOIN thread_track tt ON tt.utid = t.utid
JOIN slice s ON s.track_id = tt.id
WHERE t.upid = <TARGET_UPID>
GROUP BY t.utid
ORDER BY cpu_time_ms DESC
```

### Thread State Analysis

```sql
-- How much time a thread spent in each state (running, sleeping, blocked)
-- Uses the sched_slice table which records OS-level scheduling
SELECT
  CASE state
    WHEN 'Running' THEN 'Running (on CPU)'
    WHEN 'S' THEN 'Sleeping (interruptible)'
    WHEN 'D' THEN 'Blocked (uninterruptible, I/O)'
    WHEN 'R' THEN 'Runnable (waiting for CPU)'
    WHEN 'R+' THEN 'Runnable (preempted)'
    ELSE state
  END AS thread_state,
  COUNT(*) AS occurrences,
  CAST(SUM(dur) / 1e6 AS REAL) AS total_ms,
  CAST(100.0 * SUM(dur) / (SELECT SUM(dur) FROM sched_slice WHERE utid = <UTID>)
    AS REAL) AS pct
FROM sched_slice
WHERE utid = <UTID>
GROUP BY state
ORDER BY total_ms DESC
```

## Function-Level Analysis

### High-Frequency Function Detection

```sql
-- Functions called an unusually high number of times (possible batching issues)
-- Short-duration, high-count calls waste overhead on call setup
SELECT
  name,
  COUNT(*) AS call_count,
  CAST(SUM(dur) / 1e6 AS REAL) AS total_ms,
  CAST(AVG(dur) / 1e6 AS REAL) AS avg_ms,
  CAST(AVG(dur) / 1e3 AS REAL) AS avg_us
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
GROUP BY name
HAVING call_count > 100
ORDER BY call_count DESC
LIMIT 20
```

### Top Functions by Total Duration

```sql
-- Most expensive functions by cumulative time on a given thread
SELECT
  name,
  COUNT(*) AS calls,
  CAST(SUM(dur) / 1e6 AS REAL) AS total_ms,
  CAST(AVG(dur) / 1e6 AS REAL) AS avg_ms,
  CAST(MAX(dur) / 1e6 AS REAL) AS max_ms
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <UTID>
GROUP BY name
ORDER BY total_ms DESC
LIMIT 25
```

### Functions Within a Specific Frame

```sql
-- Drill into a single frame to see its call tree
-- Replace <FRAME_TS> with the ts value of the target frame
SELECT
  s.depth,
  s.name,
  CAST(s.dur / 1e6 AS REAL) AS dur_ms,
  CAST(s.ts / 1e6 AS REAL) AS start_ms
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.ts >= <FRAME_TS>
  AND s.ts + s.dur <= <FRAME_TS> + <FRAME_DUR>
ORDER BY s.ts, s.depth
```

## GPU Analysis

### GPU Render Pass Timing

```sql
-- Find all GPU render passes and their durations
SELECT
  s.name AS render_pass,
  COUNT(*) AS occurrences,
  CAST(AVG(s.dur) / 1e6 AS REAL) AS avg_ms,
  CAST(MAX(s.dur) / 1e6 AS REAL) AS max_ms,
  CAST(SUM(s.dur) / 1e6 AS REAL) AS total_ms
FROM slice s
JOIN track t ON s.track_id = t.id
WHERE t.name LIKE '%GPU%' OR t.name LIKE '%gpu%'
GROUP BY s.name
ORDER BY total_ms DESC
```

### GPU Counter Values Over Time

```sql
-- Get GPU counter samples for a specific metric
-- First, list available counters:
SELECT DISTINCT ct.name
FROM counter_track ct
WHERE ct.name LIKE '%GPU%'
   OR ct.name LIKE '%Fragment%'
   OR ct.name LIKE '%Vertex%'
   OR ct.name LIKE '%Texture%'
   OR ct.name LIKE '%Shader%'
ORDER BY ct.name
```

```sql
-- Then query a specific counter's values
SELECT
  CAST(c.ts / 1e6 AS REAL) AS time_ms,
  c.value
FROM counter c
JOIN counter_track ct ON c.track_id = ct.id
WHERE ct.name = '<COUNTER_NAME>'
ORDER BY c.ts
```

### GPU Counter Statistics

```sql
-- Aggregate statistics for a GPU counter
SELECT
  ct.name,
  COUNT(*) AS samples,
  CAST(AVG(c.value) AS REAL) AS mean,
  CAST(MIN(c.value) AS REAL) AS min_val,
  CAST(MAX(c.value) AS REAL) AS max_val
FROM counter c
JOIN counter_track ct ON c.track_id = ct.id
WHERE ct.name LIKE '%Fragment%'
   OR ct.name LIKE '%Vertex%'
   OR ct.name LIKE '%Texture%'
   OR ct.name LIKE '%Shader%'
GROUP BY ct.name
ORDER BY ct.name
```

## Synchronization and Waiting

### FenceChecker::Wait Analysis

```sql
-- FenceChecker::Wait indicates the CPU waiting for GPU completion
-- High wait times mean the GPU is the bottleneck
SELECT
  s.name,
  COUNT(*) AS wait_count,
  CAST(SUM(s.dur) / 1e6 AS REAL) AS total_wait_ms,
  CAST(AVG(s.dur) / 1e6 AS REAL) AS avg_wait_ms,
  CAST(MAX(s.dur) / 1e6 AS REAL) AS max_wait_ms
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
JOIN thread t ON tt.utid = t.utid
WHERE s.name LIKE '%FenceChecker%'
   OR s.name LIKE '%WaitForGpu%'
   OR s.name LIKE '%Gfx.WaitForPresent%'
GROUP BY s.name
ORDER BY total_wait_ms DESC
```

### VR Compositor Latency

```sql
-- Measure time between xrBeginFrame and xrEndFrame to get
-- the application's rendering time as seen by the compositor
SELECT
  begin_frame.ts AS begin_ts,
  end_frame.ts AS end_ts,
  CAST((end_frame.ts - begin_frame.ts) / 1e6 AS REAL) AS render_time_ms
FROM slice begin_frame
JOIN slice end_frame ON end_frame.ts > begin_frame.ts
  AND end_frame.ts < begin_frame.ts + 50000000  -- within 50ms window
  AND end_frame.track_id = begin_frame.track_id
WHERE begin_frame.name = 'xrBeginFrame'
  AND end_frame.name = 'xrEndFrame'
ORDER BY begin_frame.ts
```

## Slice Metadata

### Extract Slice Arguments

```sql
-- Get key-value metadata attached to slices (draw calls, texture info, etc.)
SELECT
  s.name AS slice_name,
  a.key,
  a.string_value,
  a.int_value,
  a.real_value
FROM slice s
JOIN args a ON s.arg_set_id = a.arg_set_id
WHERE s.name LIKE '%Render%'
  AND a.key IS NOT NULL
ORDER BY s.ts
LIMIT 50
```

### Draw Call Information

```sql
-- Find slices that contain draw call count information
SELECT
  s.name,
  a.key,
  a.int_value AS draw_calls
FROM slice s
JOIN args a ON s.arg_set_id = a.arg_set_id
WHERE a.key LIKE '%draw%'
   OR a.key LIKE '%batch%'
   OR a.key LIKE '%call%'
ORDER BY a.int_value DESC
LIMIT 20
```

## Trace Validation

### Quick Trace Health Check

```sql
-- Single query to validate trace quality
SELECT
  CAST((MAX(s.ts) - MIN(s.ts)) / 1e9 AS REAL) AS duration_sec,
  COUNT(*) AS total_slices,
  (SELECT COUNT(DISTINCT upid) FROM thread) AS processes,
  (SELECT COUNT(DISTINCT utid) FROM thread) AS threads,
  (SELECT COUNT(*) FROM counter) AS counter_samples
FROM slice s
```

### Identify Trace Contents

```sql
-- What types of data does this trace contain?
SELECT 'slices' AS data_type, COUNT(*) AS count FROM slice
UNION ALL
SELECT 'counter_samples', COUNT(*) FROM counter
UNION ALL
SELECT 'sched_slices', COUNT(*) FROM sched_slice
UNION ALL
SELECT 'processes', COUNT(*) FROM process WHERE name IS NOT NULL
UNION ALL
SELECT 'threads', COUNT(*) FROM thread WHERE name IS NOT NULL
```
