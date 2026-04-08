# Shader Optimization with RenderDoc on Quest

## Overview

RenderDoc's shader inspection capabilities combined with PIL (Performance Instrumentation Layer) metrics enable iterative shader optimization on Adreno GPUs found in Quest devices.

## Optimization Loop

```
Identify expensive draw → Inspect shader → Edit → Rebuild → Recapture → Compare
```

### Step 1: Identify Target Shaders

Open your capture in RenderDoc and sort draw calls by GPU cost. Focus on draws that:

- Consume more than 5 percent of total frame GPU time
- Have high fragment shader cost (full-screen effects, complex materials)
- Are called many times per frame (instanced draws, particles)

### Step 2: Inspect Shader

In the RenderDoc GUI, select a draw call and navigate to the Pipeline State tab. Click on the fragment or vertex shader to view its source code. RenderDoc can display shaders in multiple encodings:

- **SPIR-V**: Raw GPU bytecode
- **GLSL**: Cross-compiled, human-readable
- **HLSL**: Cross-compiled (if available)

### Step 3: Common Optimizations

#### Reduce Register Pressure (Adreno-Specific)

Adreno GPUs have a limited register file. High register usage reduces wave occupancy:

- Reduce simultaneous live variables
- Break complex expressions into sequential operations
- Replace `vec4` with `vec3` where the w component is unused
- Use `mediump` precision where full precision is unnecessary

#### Simplify ALU Operations

- Replace `pow(x, 2.0)` with `x * x`
- Replace `sqrt` with `inversesqrt` where applicable
- Combine multiple texture samples into fewer fetches with channel packing
- Remove dead code paths behind compile-time-constant branches

#### Reduce Texture Fetches

- Use texture atlases to reduce bind changes
- Implement bilinear filtering manually only if hardware filtering is insufficient
- Consider lower mip levels for distant objects

### Step 4: Rebuild and Recapture

After modifying shaders in your application source:

1. Rebuild the APK with the optimized shaders
2. Recapture a frame with RenderDoc
3. Compare PIL metrics between the original and optimized captures

### Step 5: Compare Results

Compare PIL metrics before and after:

| Metric | What to Watch |
|--------|---------------|
| GPU cycles per draw | Primary performance indicator |
| ALU utilization | Should decrease if ALU-bound |
| Texture fetch rate | Should decrease if texture-bound |
| Register count | Lower = better wave occupancy |

### Step 6: Visual Verification

Always compare screenshots before and after shader changes. Optimization that introduces visual artifacts is not acceptable.

## Adreno GPU Architecture Notes

Quest devices use Qualcomm Adreno GPUs with tile-based rendering:

- **Tile-based**: Framebuffer is divided into tiles processed independently. Minimize render target switches to avoid tile flushes.
- **GMEM (Graphics Memory)**: On-chip memory used for tile storage. Render targets that fit in GMEM avoid costly system memory round-trips.
- **Wave occupancy**: More waves = better latency hiding. Register pressure directly reduces occupancy.
- **Half-precision**: Adreno has dedicated `mediump` ALU paths that run at 2x throughput. Use `mediump` for color, UV, and normal calculations where precision permits.
