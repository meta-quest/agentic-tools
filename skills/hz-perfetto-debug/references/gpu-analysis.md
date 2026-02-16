# GPU Analysis

## Overview

GPU analysis in Perfetto traces involves two main areas: render pass timing (from GPU track slices) and GPU hardware counters (from counter tracks). Together, these reveal what the GPU is doing each frame and where bottlenecks lie.

## GPU Render Pass Analysis

### Finding GPU Render Passes

GPU work appears as slices on GPU-specific tracks. Render passes are named with a `surface#` prefix followed by a number.

```sql
-- Find GPU track slices (render passes)
SELECT
  s.name,
  COUNT(*) AS pass_count,
  CAST(AVG(s.dur) / 1e6 AS REAL) AS avg_ms,
  CAST(SUM(s.dur) / 1e6 AS REAL) AS total_ms
FROM slice s
JOIN track t ON s.track_id = t.id
WHERE t.name LIKE 'GPU%' OR t.name LIKE 'gpu%'
GROUP BY s.name
ORDER BY total_ms DESC
```

### Understanding Render Pass Surfaces

Each `surface#N` corresponds to a render target. The surface name encodes important information:

| Surface Pattern | Typical Use |
|----------------|------------|
| `surface#0` | Main eye buffer (primary render target) |
| `surface#1` | Secondary eye buffer or UI overlay |
| `surface#2+` | Shadow maps, reflection probes, post-processing |

To decode what each surface renders, check the associated metadata:

```sql
-- Get render pass details from args
SELECT
  s.name AS pass_name,
  CAST(s.dur / 1e6 AS REAL) AS dur_ms,
  a.key,
  a.string_value,
  a.int_value,
  a.real_value
FROM slice s
JOIN args a ON s.arg_set_id = a.arg_set_id
JOIN track t ON s.track_id = t.id
WHERE t.name LIKE 'GPU%'
  AND s.name LIKE 'surface%'
ORDER BY s.ts
LIMIT 50
```

### Surface Properties

Key properties to extract from render pass args:

| Property | What It Means | Optimal for Quest |
|----------|--------------|-------------------|
| Resolution | Render target dimensions | 1440x1584 per eye (Quest 3) |
| MSAA level | Multisample anti-aliasing samples | 2x or 4x |
| Color format | Bits per pixel for color | RGBA8 (32 bpp) |
| Depth format | Bits per pixel for depth | D24S8 or D32F |

Higher resolution, MSAA level, or color depth directly increases GPU fill rate requirements.

### GPU Frame Breakdown

A typical VR frame has multiple render passes. The breakdown looks like:

```
Frame N (total GPU time: 8.2 ms)
  Pass 0: surface#0 (eye buffer)     - 5.1 ms (62%)
  Pass 1: surface#1 (shadow map)     - 1.8 ms (22%)
  Pass 2: surface#2 (post-process)   - 0.9 ms (11%)
  Pass 3: surface#3 (UI overlay)     - 0.4 ms  (5%)
```

**What to look for:**
- The eye buffer pass should dominate (50-70% of GPU time)
- Shadow map passes over 25% suggest shadow resolution is too high
- Multiple post-process passes indicate a complex post-processing chain that may need simplification
- More than 4-5 passes per frame is unusual and worth investigating

### Render Pass Timing Query

```sql
-- Per-frame GPU render pass breakdown
SELECT
  s.ts,
  s.name AS render_pass,
  CAST(s.dur / 1e6 AS REAL) AS dur_ms
FROM slice s
JOIN track t ON s.track_id = t.id
WHERE (t.name LIKE 'GPU%' OR t.name LIKE 'gpu%')
  AND s.name LIKE 'surface%'
ORDER BY s.ts
```

## GPU Metric Counters

GPU counters are hardware performance counters sampled during rendering. Use `hzdb perf gpu-counters` to retrieve aggregated statistics, or query them directly.

### Texture Metrics

| Counter | Description | Warning Threshold |
|---------|------------|-------------------|
| `% Anisotropic Filtered` | Percentage of texture fetches using anisotropic filtering | > 50% |
| `% Texture Fetch Stall` | Time stalled waiting for texture data | > 20% |
| `Textures/Fragment` | Average texture lookups per fragment shader invocation | > 8 |
| `% Non-Base Level Textures` | Mipmapped texture fetches (not base level) | Informational |

**High texture fetch stall** indicates textures are too large for the cache, causing memory bandwidth bottlenecks. Solutions: reduce texture resolution, enable mipmaps, use compressed formats (ASTC).

**High anisotropic filtering** is expensive on mobile GPUs. Consider reducing anisotropic filtering quality or using trilinear filtering for distant objects.

### Fragment Metrics

| Counter | Description | Warning Threshold |
|---------|------------|-------------------|
| `Fragment Instructions/Fragment Shaded` | Average shader instructions per fragment | > 50 |
| `% Time Shading Fragments` | GPU time spent in fragment shaders | > 70% |
| `% Shader ALU Capacity` | ALU utilization in shaders | > 85% |
| `% Time Computing` | Time in compute shaders | Context-dependent |
| `Fragments Shaded / sec` | Fragment throughput | Informational |

**High fragment instructions** indicate complex shaders. Simplify materials, reduce shader variant count, or use simpler lighting models.

**High ALU capacity** means the GPU's compute units are saturated. This is a shader complexity bottleneck — reduce math operations per pixel.

### Vertex Metrics

| Counter | Description | Warning Threshold |
|---------|------------|-------------------|
| `Vertex Instructions/Vertex Shaded` | Average instructions per vertex shader | > 30 |
| `% Vertex Fetch Stall` | Time stalled fetching vertex data | > 15% |
| `Avg Bytes/Vertex` | Average vertex size in bytes | > 64 |
| `Vertices Shaded / sec` | Vertex throughput | Informational |

**High vertex fetch stall** suggests vertex buffers are too large or poorly laid out in memory. Solutions: reduce vertex attribute count, use compressed vertex formats, improve mesh LOD.

**High bytes per vertex** means vertex attributes are bloated. Strip unused attributes (second UV set, vertex colors) and use half-precision where possible.

### Binning Metrics

| Counter | Description | Warning Threshold |
|---------|------------|-------------------|
| `Binning time` | Time spent in tile-based binning pass | > 2 ms |
| `% Prims Trivially Rejected` | Primitives culled during binning | Informational |
| `% Prims Clipped` | Primitives requiring clipping | > 20% |

Qualcomm Adreno GPUs (used in Quest) are tile-based renderers. The binning pass sorts geometry into screen-space tiles. High binning time indicates excessive geometry (too many triangles or draw calls).

### MSAA Costs

| MSAA Level | Relative Cost | Recommended For |
|-----------|---------------|-----------------|
| No MSAA | 1.0x | Not recommended for VR (aliasing) |
| 2x MSAA | ~1.2x | Minimum for VR |
| 4x MSAA | ~1.5x | Best quality/performance balance |
| 8x MSAA | ~2.0x+ | Too expensive for most Quest apps |

MSAA cost scales with fragment shading. If `% Time Shading Fragments` is already high, reducing MSAA level can provide significant savings.

### Querying GPU Counters Directly

```sql
-- List available GPU counters
SELECT DISTINCT ct.name
FROM counter_track ct
WHERE ct.name LIKE '%GPU%'
   OR ct.name LIKE '%gpu%'
   OR ct.name LIKE '%Fragment%'
   OR ct.name LIKE '%Vertex%'
   OR ct.name LIKE '%Texture%'
ORDER BY ct.name

-- Get counter values over time
SELECT
  ct.name AS counter_name,
  c.ts,
  c.value
FROM counter c
JOIN counter_track ct ON c.track_id = ct.id
WHERE ct.name = '<counter_name>'
ORDER BY c.ts
```

## Interpreting GPU Results

### GPU-Bound Indicators

The application is GPU-bound when:
- GPU frame time exceeds the frame budget (11.1 ms at 90 Hz)
- CPU threads show significant idle or wait time
- `FenceChecker::Wait` or `GPU completion` thread shows the CPU waiting for GPU

### Common GPU Bottlenecks

| Bottleneck | Indicators | Solutions |
|-----------|-----------|-----------|
| Fill rate | High fragment %, high ALU capacity | Reduce resolution, simplify shaders, lower MSAA |
| Bandwidth | High texture fetch stall, large vertex size | Compress textures (ASTC), reduce render target count |
| Geometry | High binning time, many draw calls | Reduce polycount, use LODs, enable GPU instancing |
| Overdraw | Fragment count >> pixel count | Reduce transparent objects, use depth prepass |
| Shader complexity | High instructions/fragment | Simplify materials, use mobile-optimized shaders |
