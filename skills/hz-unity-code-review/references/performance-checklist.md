# Performance Checklist for Unity on Meta Quest

This checklist covers the critical performance areas to review when developing Unity applications for Meta Quest headsets. Each section includes targets, techniques, and code examples.

## Draw Calls

**Budget: 50-100 draw calls per frame on Quest 2, up to 150 on Quest 3.**

Every draw call has CPU overhead for state setup and GPU command submission. Exceeding the budget causes frame drops and reprojection.

### Optimization Techniques

- **GPU Instancing**: Renders multiple copies of the same mesh in a single draw call. Enable on materials and use `Graphics.DrawMeshInstanced` for programmatic placement.
- **Static Batching**: Combines non-moving meshes at build time. Mark GameObjects as "Batching Static" in the Inspector.
- **Dynamic Batching**: Combines small meshes (< 300 vertices) at runtime. Enable in URP asset settings.
- **SRP Batcher**: Groups materials that share the same shader variant. Enable in URP asset settings — this is the most impactful batcher for URP.
- **Texture Atlasing**: Combine multiple textures into one atlas so objects can share a material and batch together.

### How to Verify

```csharp
// Use the Frame Debugger (Window > Analysis > Frame Debugger) to inspect draw calls.
// At runtime, check the stats overlay:
//   OVRManager.SetPerformanceLevel(...)
//   Enable developer mode stats overlay on device
```

Monitor using Unity Profiler or OVR Metrics Tool:
- Batches (draw calls) per frame
- SetPass calls (shader state changes)
- Tris and Verts per frame

## Triangle Count

**Target: < 750K triangles per frame on Quest 2, < 1M on Quest 3.**

### Optimization Techniques

- **LOD Groups**: Use 3-4 LOD levels with aggressive transitions for Quest.
  - LOD0: Full detail (< 5m distance)
  - LOD1: 50% triangles (5-15m)
  - LOD2: 25% triangles (15-30m)
  - LOD3: 10% triangles or billboard (> 30m)
  - Culled: beyond max distance

```csharp
// Configure LOD transitions in code
LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
LOD[] lods = new LOD[4];
lods[0] = new LOD(0.6f, new Renderer[] { highDetailRenderer });
lods[1] = new LOD(0.3f, new Renderer[] { midDetailRenderer });
lods[2] = new LOD(0.1f, new Renderer[] { lowDetailRenderer });
lods[3] = new LOD(0.01f, new Renderer[] { billboardRenderer });
lodGroup.SetLODs(lods);
lodGroup.RecalculateBounds();
```

- **Occlusion Culling**: Bake occlusion data to skip rendering objects hidden behind others.
  - Set cell sizes appropriate for your scene scale
  - Use Occlusion Areas to define high-detail zones
  - Use Occlusion Portals for doorways and openings

- **Frustum Culling**: Unity does this automatically, but ensure object bounds are accurate. Oversized bounds prevent culling.

- **Mesh Simplification**: Use tools like UnityMeshSimplifier or Blender's Decimate modifier to reduce polygon counts.

## Texture Sizes

**Target: 2K max for most textures, 4K only for large environment textures or skyboxes.**

Texture memory is a major constraint on Quest. Oversized textures waste VRAM and cause loading stalls.

### Guidelines

| Texture Type | Recommended Max Size | Compression |
|-------------|---------------------|-------------|
| Character diffuse | 1024x1024 | ASTC 6x6 |
| Environment | 2048x2048 | ASTC 6x6 |
| UI elements | 512x512 or atlas | ASTC 4x4 |
| Normal maps | 1024x1024 | ASTC 6x6 |
| Skybox | 2048x2048 per face | ASTC 6x6 |
| Lightmaps | 1024x1024 | ASTC 6x6 |

### Optimization Techniques

- **ASTC compression**: Required for Quest. Use ASTC 6x6 as default, ASTC 4x4 for UI/text that needs higher quality.
- **Mipmaps**: Enable for all 3D textures (reduces aliasing and improves cache performance). Disable only for UI textures rendered at native resolution.
- **Texture Atlasing**: Pack multiple small textures into one larger atlas to reduce material count and enable batching.
- **Streaming Mipmaps**: Enable to load only the mip levels needed based on camera distance, reducing peak memory usage.
- **Power of Two**: Use power-of-two texture dimensions (256, 512, 1024, 2048) for optimal GPU memory alignment.

```csharp
// Check texture import settings programmatically
TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
importer.textureCompression = TextureImporterCompression.Compressed;

var platformSettings = importer.GetPlatformTextureSettings("Android");
platformSettings.overridden = true;
platformSettings.format = TextureImporterFormat.ASTC_6x6;
platformSettings.maxTextureSize = 2048;
importer.SetPlatformTextureSettings(platformSettings);
```

## Overdraw

**Target: Minimize overdraw to < 2x average across the frame.**

Overdraw occurs when fragments are shaded but then overwritten by closer geometry. The Quest GPU is fill-rate limited, making overdraw particularly expensive.

### Optimization Techniques

- **Render opaque objects front-to-back**: URP does this automatically for opaque geometry.
- **Minimize transparent objects**: Each transparent object adds a full layer of overdraw.
- **Alpha cutout over alpha blend**: Cutout (clip) allows early depth rejection; alpha blend does not.
- **Reduce particle overdraw**: Limit particle count and size, use additive blending sparingly.
- **Avoid full-screen effects**: Post-processing adds at least one full-screen overdraw pass.

```csharp
// Prefer alpha cutout for vegetation, fences, etc.
// In shader:
//   clip(texColor.a - _Cutoff);
// This allows the GPU to reject fragments early via depth testing.
```

## Batching Deep Dive

### Static Batching

Best for objects that never move, rotate, or scale at runtime.

```csharp
// Mark objects as static in the Inspector, or at runtime:
StaticBatchingUtility.Combine(parentGameObject);
```

Caveats:
- Increases build size and runtime memory (combined mesh is stored)
- Objects cannot move after batching
- All objects must share the same material

### Dynamic Batching

For small meshes (< 300 vertices after all shader inputs are accounted for).

- Enable in URP Renderer Asset settings
- Most useful for small decorative objects, particles
- Minimal benefit if SRP Batcher is also active

### GPU Instancing

For rendering many copies of the same mesh with different transforms or material properties.

```csharp
// Enable GPU Instancing on the material
material.enableInstancing = true;

// For per-instance data, use MaterialPropertyBlock
MaterialPropertyBlock props = new MaterialPropertyBlock();
props.SetColor("_Color", instanceColor);
renderer.SetPropertyBlock(props);
```

## Occlusion Culling

### Setup

1. Mark static occluders (walls, floors, large objects) as "Occluder Static"
2. Mark static occludees (objects that can be hidden) as "Occludee Static"
3. Set cell sizes in Window > Rendering > Occlusion Culling:
   - Smallest Occluder: size of your smallest wall/blocker
   - Smallest Hole: smallest gap the camera might see through
4. Bake occlusion data

### Runtime Considerations

- Occlusion culling has CPU cost — balance against rendering savings
- Only useful for scenes with solid occluders (walls, buildings)
- Not effective for open outdoor scenes with few occluders

## Memory Budget

| Resource | Quest 2 | Quest 3 |
|----------|---------|---------|
| Total available RAM | ~1.5 GB | ~2 GB |
| Texture memory | ~300-500 MB | ~500-700 MB |
| Mesh memory | ~100-200 MB | ~150-300 MB |
| Audio memory | ~50-100 MB | ~50-100 MB |
| Mono/IL2CPP heap | ~100-200 MB | ~100-200 MB |

### Monitoring Memory

```csharp
// Log memory usage
long totalMemory = Profiler.GetTotalAllocatedMemoryLong();
long textureMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();
Debug.Log($"Total: {totalMemory / 1024 / 1024} MB, GPU: {textureMemory / 1024 / 1024} MB");
```

Use OVR Metrics Tool or `adb shell dumpsys meminfo <package>` to monitor device-side memory consumption.

## Audio

### Compression Settings

| Clip Type | Load Type | Compression | Sample Rate |
|-----------|-----------|-------------|-------------|
| Short SFX (< 1s) | Decompress On Load | ADPCM | 22050 Hz |
| Medium SFX (1-5s) | Compressed In Memory | Vorbis (70%) | 44100 Hz |
| Music/Ambient | Streaming | Vorbis (50%) | 44100 Hz |
| Voice/Dialog | Streaming | Vorbis (70%) | 22050 Hz |

### Best Practices

- Limit simultaneous audio sources to 16 or fewer
- Use audio source pooling instead of creating/destroying sources
- Set `AudioSource.priority` to ensure important sounds are not culled
- Spatialize 3D audio using Meta's Audio SDK for accurate HRTF positioning
- Disable `AudioListener` on inactive cameras
