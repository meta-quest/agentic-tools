# Frame Timing

## Overview

VR frame timing is fundamentally different from desktop rendering. Every frame must meet a hard deadline tied to the display refresh rate. Missing a deadline causes the compositor to show a stale frame (reprojection), which degrades the user experience.

## VR Frame Markers by Engine

### OpenXR (Standard API)

All Quest VR applications ultimately go through OpenXR (or the legacy Oculus Mobile SDK). These markers appear in traces regardless of the game engine:

| Marker | Purpose | Thread |
|--------|---------|--------|
| `xrWaitFrame` | Blocks until the compositor signals it is time to start a new frame | Main or dedicated thread |
| `xrBeginFrame` | Marks the start of application rendering for this frame | Main or render thread |
| `xrEndFrame` | Submits the rendered frame to the compositor | Render thread |

The time from `xrBeginFrame` to `xrEndFrame` is the application's render time for that frame.

### Oculus Mobile SDK (Legacy)

Older applications or those using the legacy SDK directly show these markers:

| Marker | Purpose |
|--------|---------|
| `vrapi_WaitFrame` | Equivalent to `xrWaitFrame` |
| `vrapi_BeginFrame` | Equivalent to `xrBeginFrame` |
| `vrapi_SubmitFrame` | Equivalent to `xrEndFrame` |

### Unity Engine

Unity adds its own instrumentation on top of OpenXR:

| Marker | Purpose | Thread |
|--------|---------|--------|
| `PlayerLoop` | One complete Unity frame (all systems) | UnityMain |
| `FixedUpdate` | Physics and fixed-timestep logic | UnityMain |
| `Update` | Per-frame game logic | UnityMain |
| `LateUpdate` | Post-update logic (camera follow, etc.) | UnityMain |
| `PostLateUpdate.FinishRendering` | Final rendering submission | UnityMain |
| `PhaseSync` | VR vsync synchronization wait | UnityMain |
| `Gfx.WaitForPresent` | Waiting for GPU to finish previous frame | UnityMain |
| `Camera.Render` | Camera rendering (one per camera) | UnityMain |
| `RenderPipelineManager.DoRenderLoop` | SRP/URP render loop | UnityMain |
| `BatchRenderer.Flush` | Batched draw call submission | UnityGfx |

### Unreal Engine

| Marker | Purpose | Thread |
|--------|---------|--------|
| `FEngineLoop::Tick` | One complete Unreal frame | GameThread |
| `UGameEngine::Tick` | Game engine tick (highlighted in Meta Perfetto docs) | GameThread |
| `FSceneRenderer::Render` | Scene rendering | RenderThread |
| `FRHICommandList::Execute` | GPU command submission | RHI Thread |
| `TickWidgets` | UI widget updates | GameThread |
| `WorldTick` | World actor ticking | GameThread |
| `BlueprintTime` | Blueprint execution | GameThread |
| `SkeletalMeshComponent::TickPose` | Animation evaluation | GameThread |

## Frame Pacing and Compositor Timing

### How VR Frame Pacing Works

```
Timeline (90 Hz, 11.1 ms per frame):

|--- Frame N ---|--- Frame N+1 ---|--- Frame N+2 ---|
|  App Render   |  App Render     |  App Render     |
                |  Compositor     |  Compositor     |
                |  Display Scan   |  Display Scan   |

Vsync  --------+--------+--------+--------+--------
```

1. The application renders a frame (CPU + GPU work)
2. The compositor picks up the completed frame
3. The compositor applies TimeWarp correction and submits to display
4. The display scans out at the next vsync

### PhaseSync

**PhaseSync is a VR-specific vsync alignment mechanism.** It appears as idle time at the beginning of `PlayerLoop` in Unity traces.

**Important**: PhaseSync is NOT wasted time and should NOT be flagged as a performance issue. It serves a critical purpose:

- Aligns the application's frame start with the compositor's schedule
- Minimizes motion-to-photon latency by starting the frame as late as possible
- Dynamically adjusts based on recent frame times

PhaseSync typically shows 1-4 ms of wait time. This is normal and expected. If PhaseSync shrinks to near zero, it means the application is using almost its entire frame budget.

The developer has no direct control over PhaseSync timing — it is managed by the runtime.

### TimeWarp / Asynchronous SpaceWarp

TimeWarp (ATW - Asynchronous TimeWarp) is the compositor's reprojection mechanism:

- Runs 2-3 ms before each vsync
- Applies head rotation correction to the most recently completed frame
- Compensates for rendering latency
- If the app misses a frame, TimeWarp reprojects the previous frame (stale frame)

**Asynchronous SpaceWarp (ASW)** is an enhanced version that also corrects for translational movement, but at higher computational cost.

TimeWarp/ASW appear on the compositor's threads, not the application's threads. You generally do not need to analyze their timing unless investigating compositor-level issues.

## Detecting Frame Drops

### Method 1: Frame Duration Analysis

```sql
-- Find frames exceeding 90 Hz target (11.1 ms)
SELECT
  s.ts,
  CAST(s.dur / 1e6 AS REAL) AS frame_ms,
  CASE
    WHEN s.dur > 16700000 THEN 'CRITICAL (>16.7ms)'
    WHEN s.dur > 13900000 THEN 'SEVERE (>13.9ms)'
    WHEN s.dur > 11100000 THEN 'DROPPED (>11.1ms)'
    ELSE 'OK'
  END AS status
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
  AND s.dur > 11100000
ORDER BY s.dur DESC
```

### Method 2: Stale Frame Rate

```sql
-- Calculate stale frame percentage
SELECT
  COUNT(*) AS total_frames,
  SUM(CASE WHEN dur > 11100000 THEN 1 ELSE 0 END) AS stale_frames,
  CAST(
    100.0 * SUM(CASE WHEN dur > 11100000 THEN 1 ELSE 0 END) / COUNT(*)
    AS REAL
  ) AS stale_pct
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
```

### Method 3: Frame-to-Frame Variance

```sql
-- Frame time variance (jitter analysis)
WITH frames AS (
  SELECT
    ROW_NUMBER() OVER (ORDER BY s.ts) AS frame_num,
    s.dur
  FROM slice s
  JOIN thread_track tt ON s.track_id = tt.id
  WHERE tt.utid = <MAIN_THREAD_UTID>
    AND s.name = 'PlayerLoop'
)
SELECT
  CAST(AVG(dur) / 1e6 AS REAL) AS avg_ms,
  CAST(MIN(dur) / 1e6 AS REAL) AS min_ms,
  CAST(MAX(dur) / 1e6 AS REAL) AS max_ms,
  CAST(
    SQRT(AVG(dur * dur) - AVG(dur) * AVG(dur)) / 1e6
    AS REAL
  ) AS stddev_ms
FROM frames
```

**Variance targets:**
- Standard deviation < 1 ms: Excellent consistency
- Standard deviation 1-2 ms: Acceptable
- Standard deviation 2-4 ms: Noticeable jitter, investigate
- Standard deviation > 4 ms: Severe jitter, likely hitches or GC pauses

## Frame Time Distribution

Understanding the distribution of frame times reveals whether issues are systemic or intermittent:

```sql
-- Frame time histogram (1 ms buckets)
SELECT
  CAST(dur / 1000000 AS INT) AS frame_ms_bucket,
  COUNT(*) AS frame_count,
  CAST(100.0 * COUNT(*) / SUM(COUNT(*)) OVER () AS REAL) AS percentage
FROM slice s
JOIN thread_track tt ON s.track_id = tt.id
WHERE tt.utid = <MAIN_THREAD_UTID>
  AND s.name = 'PlayerLoop'
GROUP BY frame_ms_bucket
ORDER BY frame_ms_bucket
```

A healthy distribution shows a tight cluster below the frame budget with rare outliers. A bimodal distribution (two peaks) suggests the app alternates between hitting and missing vsync.
