# Rendering Best Practices for Unity on Meta Quest

This document covers Quest-specific rendering configuration, pipeline settings, and shader guidelines for optimal VR performance.

## Single-Pass Multiview

**This MUST be enabled. It is the single most impactful rendering optimization for VR.**

Single-pass multiview (also called single-pass instanced) renders both eyes in a single render pass using GPU instancing. Without it, the entire scene is rendered twice — doubling CPU draw call overhead.

### How to Enable

1. Go to **Player Settings > XR Plug-in Management > Oculus**
2. Set **Stereo Rendering Mode** to **Multiview**

### Code Considerations

Custom shaders must support single-pass multiview:

```hlsl
// In custom shaders, include the multiview macros:
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// Use UNITY_VERTEX_INPUT_INSTANCE_ID and UNITY_VERTEX_OUTPUT_STEREO
struct Attributes
{
    float4 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}
```

Shaders that do not include these macros will fall back to multi-pass rendering, defeating the optimization.

## Fixed Foveated Rendering (FFR)

FFR reduces the fragment shading resolution at the edges of each eye's view, where the lens optics already reduce perceived detail. This provides significant GPU savings with minimal visual impact.

### FFR Levels

| Level | Center | Edge Reduction | Use Case |
|-------|--------|---------------|----------|
| Off | Full | None | Only for benchmarking |
| Low | Full | Slight | Minimal quality impact |
| Medium | Full | Moderate | Good default |
| High | Full | Aggressive | Recommended for Quest |
| HighTop | Full | Maximum | Maximum GPU savings |

### How to Enable

```csharp
using UnityEngine;
using UnityEngine.XR;

public class FFRController : MonoBehaviour
{
    void Start()
    {
        // Set FFR level via OVRManager
        OVRManager.foveatedRenderingLevel = OVRManager.FoveatedRenderingLevel.High;

        // Enable dynamic FFR to auto-adjust based on GPU load
        OVRManager.useDynamicFoveatedRendering = true;
    }
}
```

### Dynamic FFR

When `useDynamicFoveatedRendering` is enabled, the system automatically increases FFR level when GPU utilization is high and decreases it when there is headroom. This provides the best balance of quality and performance without manual tuning.

## Application SpaceWarp (AppSW)

Application SpaceWarp halves the frame rate requirement by generating intermediate frames through motion extrapolation. The application renders at half the display rate (e.g., 36 fps instead of 72 fps), and the runtime synthesizes the missing frames.

### When to Use

- Complex scenes that cannot maintain full frame rate
- Visually rich environments with limited fast motion
- Applications where slight extrapolation artifacts are acceptable

### When to Avoid

- Fast-moving content (sports, action games)
- Fine UI text that may exhibit artifacts
- Applications requiring precise per-frame input response

### How to Enable

```csharp
// Enable Application SpaceWarp
OVRManager.SetSpaceWarp(true);

// Your application must provide motion vectors for correct extrapolation.
// URP must be configured to output motion vectors.
// Set the target frame rate to half the display rate:
Application.targetFrameRate = 36; // For 72 Hz display
```

### Motion Vector Requirements

AppSW requires accurate motion vectors to extrapolate frames. Ensure:
- URP motion vector pass is enabled
- All moving objects have correct motion vector output
- Shader Graph materials have motion vector output enabled
- Skinned meshes output per-vertex motion vectors

## Dynamic Resolution

Dynamic resolution automatically adjusts the render target resolution to maintain a stable frame rate. When GPU load is high, it reduces resolution; when there is headroom, it increases resolution back toward the target.

### How to Enable

```csharp
// Enable in script
OVRManager.eyeResolutionScale = 1.0f;  // Start at full resolution
OVRManager.minDynamicResolutionScale = 0.7f;
OVRManager.maxDynamicResolutionScale = 1.2f;

// Or use Unity's built-in dynamic resolution
ScalableBufferManager.ResizeBuffers(0.8f, 0.8f); // 80% resolution
```

Combine with FFR for maximum GPU flexibility — FFR reduces edges while dynamic resolution adjusts overall pixel count.

## Compositor Layers

Compositor layers (also called overlay/underlay layers) are rendered directly by the VR compositor at the display's native resolution, bypassing the application's render pipeline entirely.

### Benefits

- **No lens distortion**: Text and UI rendered at native panel resolution
- **No aliasing from eye buffer sampling**: Crisp edges
- **No additional GPU cost**: Composited by the VR runtime

### Use Cases

- UI panels and menus
- Video players (360 and flat)
- Cockpit instruments
- Heads-up displays
- Loading screens

### Implementation

```csharp
// Add OVROverlay component to a quad
// Set the texture directly on the overlay
OVROverlay overlay = gameObject.AddComponent<OVROverlay>();
overlay.currentOverlayType = OVROverlay.OverlayType.Overlay;
overlay.compositionDepth = 0;
overlay.currentOverlayShape = OVROverlay.OverlayShape.Quad;
overlay.textures = new Texture[] { uiRenderTexture };
```

### Limitations

- Limited number of layers (16 maximum, recommend using < 4 for performance)
- Each layer has a small CPU cost for composition
- Cannot apply post-processing or custom rendering effects
- Overlay layers always render on top; underlay layers render behind the eye buffer

## Vulkan vs OpenGL ES

**Vulkan is recommended for Quest development.**

| Feature | Vulkan | OpenGL ES |
|---------|--------|-----------|
| CPU overhead | Lower | Higher |
| Multi-threaded rendering | Yes | Limited |
| Compute shaders | Full support | Limited |
| Memory management | Explicit (better control) | Driver-managed |
| AppSW support | Yes | Yes |
| Recommendation | Primary | Fallback only |

### How to Configure

1. Go to **Player Settings > Other Settings > Graphics APIs**
2. Remove "OpenGLES3" and ensure "Vulkan" is first in the list
3. Test thoroughly — some older plugins may not support Vulkan

## URP Settings for Quest

### URP Asset Configuration

```
Rendering:
  Render Scale: 1.0 (adjust with dynamic resolution)
  HDR: OFF (significant performance cost, not needed for most VR)
  MSAA: 4x (or 2x for performance-critical apps)
  Render Scale: 1.0

Quality:
  Anti Aliasing (MSAA): 4x
  SRP Batcher: ON
  Dynamic Batching: ON
  GPU Instancing: ON

Lighting:
  Main Light: Per Pixel
  Additional Lights: Per Vertex or Disabled
  Additional Lights Count: 2-4 maximum
  Cast Shadows: OFF for additional lights

Shadows:
  Max Distance: 20-30m (keep short for VR)
  Cascade Count: 1 (never use multiple cascades on Quest)
  Shadow Resolution: 1024 (2048 maximum)

Post-processing:
  Grading Mode: LDR
  Volume Update Mode: Via Scripting (to control when volumes update)
  Disable all post-processing effects unless absolutely necessary
```

### What to Disable

- **Depth Texture**: Disable unless required (adds a render pass)
- **Opaque Texture**: Disable unless required (adds a copy pass)
- **HDR**: Disable (expensive format conversion and tonemapping)
- **Screen-space shadows**: Never use on Quest
- **SSAO**: Never use on Quest
- **Bloom**: Avoid unless critical to art direction (use sparingly)

## Shader Guidelines

### Shader Selection

| Shader | Cost | Use Case |
|--------|------|----------|
| URP/Unlit | Cheapest | UI, skyboxes, pre-baked objects |
| URP/Simple Lit | Low | Most game objects, Blinn-Phong lighting |
| URP/Lit | Medium | Hero objects, characters needing PBR |
| URP/Baked Lit | Low | Lightmapped objects with no dynamic lighting |
| Standard (Built-in) | High | NEVER use on Quest |
| Custom | Varies | Optimize for mobile ALU budget |

### Custom Shader Best Practices

```hlsl
// DO: Keep ALU operations minimal
half4 frag(Varyings input) : SV_Target
{
    half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    half3 lighting = input.vertexLighting; // Per-vertex lighting
    return half4(baseColor.rgb * lighting, baseColor.a);
}

// DON'T: Complex per-pixel operations
half4 frag(Varyings input) : SV_Target
{
    // Avoid: multiple texture samples, complex math, branching
    half4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    half4 normal = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);
    half4 specular = SAMPLE_TEXTURE2D(_SpecMap, sampler_SpecMap, input.uv);
    half4 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv);
    half ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, input.uv).r;
    // ... expensive PBR calculations ...
}
```

### Rules for Quest Shaders

1. Limit texture samples to 3-4 per fragment shader
2. Use `half` precision instead of `float` wherever possible
3. Avoid branching (`if`, `switch`) in fragment shaders
4. Move calculations to the vertex shader when possible
5. Avoid `discard`/`clip` unless using alpha cutout (prevents early-Z)
6. Do not use procedural noise or complex math functions in fragment shaders

## MSAA (Multi-Sample Anti-Aliasing)

**4x MSAA is recommended for Quest. It is nearly free on tile-based GPUs when configured correctly.**

Quest uses a tile-based GPU architecture (Qualcomm Adreno). MSAA resolves within the tile memory and does not require an additional render pass, unlike post-process AA (FXAA, SMAA, TAA).

### Configuration

- Set MSAA to **4x** in the URP Asset
- Do NOT use post-process anti-aliasing (FXAA, SMAA, TAA) — they require additional full-screen passes
- If 4x MSAA is too expensive, use 2x as a fallback
- Ensure render textures used for off-screen rendering also have MSAA if they feed into the main view

### Why Not Post-Process AA?

| Method | Cost | Quality on Quest |
|--------|------|-----------------|
| MSAA 4x | Near-zero on tile GPU | Excellent edge AA |
| FXAA | 1 full-screen pass | Blurs text and fine detail |
| SMAA | 2-3 full-screen passes | Too expensive for Quest |
| TAA | 1+ passes + history buffer | Ghosting, too expensive |
