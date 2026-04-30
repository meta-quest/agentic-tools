# CPU Hotspot Analysis

## Overview

CPU hotspot recording uses hardware cycle counters via `simpleperf record` to identify which functions consume the most CPU time. Unlike Perfetto's software instrumentation, simpleperf samples the program counter at a fixed frequency and attributes cycles to the function being executed.

## Recording a Profile

```bash
# Basic recording — profiles foreground app for 10 seconds at 4000 Hz
hzdb perf simpleperf record

# Custom settings
hzdb perf simpleperf record --app com.example.myapp --frequency 4000 --duration 10
```

### Sampling Frequency Guidelines

| Frequency | Use Case | Overhead |
|-----------|----------|----------|
| 1000 Hz | Long recordings (>30s), minimal perturbation | Very low |
| 4000 Hz | Default — good balance of accuracy and overhead | Low |
| 8000 Hz | Short recordings (<5s), need precise attribution | Moderate |

On Quest ARM SoCs, frequencies above 8000 Hz can measurably perturb the workload. Stick to 4000 Hz unless you need fine-grained attribution on a very short recording.

## Interpreting Hotspot Results

### Top Functions by CPU Cycles

The output lists functions sorted by percentage of total CPU cycles:

```
Hotspot Profile (10.0s, 40000 samples)

  %    Cycles      Function                              Module
 12.3  4,920       Physics::Simulate                     libPhysX.so
  8.7  3,480       Renderer::DrawBatch                   libunity.so
  6.2  2,480       GarbageCollector::CollectGeneration    libmonobdwgc.so
  5.1  2,040       memcpy                                libc.so
  4.8  1,920       LZ4_decompress_fast                   libunity.so
  3.2  1,280       Mesh::UpdateVertexData                 libunity.so
  ...
```

### Common VR Hotspot Patterns

| Pattern | % Budget | What It Means | Action |
|---------|----------|---------------|--------|
| Physics functions > 15% | High | Physics dominating frame | Simplify colliders, increase fixed timestep, reduce rigidbody count |
| GC/malloc > 5% | Moderate | Memory allocation pressure | Pool objects, avoid per-frame `new`, use structs over classes |
| `memcpy`/`memmove` > 5% | Moderate | Large data copies | Use references, zero-copy APIs, reduce buffer sizes |
| Compression > 3% | Notable | Runtime decompression | Pre-decompress assets, cache decompressed data |
| String operations > 3% | Notable | String formatting/parsing | Cache strings, avoid per-frame string ops |
| Shader compilation > any% | Critical | Runtime shader compilation | Pre-warm shader variants during loading |

### Engine-Specific Function Names

**Unity (IL2CPP builds):**
- Game logic: Mangled C++ names like `YourScript_Update_m12345`
- Physics: `PhysicsManager::Simulate`, `PhysX::*`
- Rendering: `GfxDeviceVulkan::*`, `Renderer::DrawBatch`
- GC: `GarbageCollector::*`, `bdwgc::*`
- Scripting: `il2cpp::vm::*`

**Unity (Mono builds):**
- C# methods appear with full namespace: `MyNamespace.MyClass:Update()`
- JIT overhead: `mono_jit_*` functions

**Unreal Engine:**
- Game logic: `AActor::Tick`, `UGameplayStatics::*`
- Rendering: `FDeferredShadingSceneRenderer::Render*`
- Blueprints: `UObject::ProcessEvent` (high counts = too many Blueprints)
- Animation: `USkeletalMeshComponent::*`

**Native OpenXR:**
- App functions appear directly with their symbol names
- OpenXR runtime: `xr*` functions (these are system overhead, not optimizable)
- Vulkan: `vk*` functions from the driver

## Optimization Priority

Focus on the **top 3-5 functions** that together account for the most cycles. VR optimization follows the 80/20 rule — a small number of hot functions typically account for most of the CPU time.

**Don't optimize:**
- Functions below 1% of total cycles (noise)
- System/runtime functions you can't change (`libc.so`, `libvulkan.so`)
- PhaseSync / xrWaitFrame idle (intentional frame pacing)

**Do optimize:**
- Application functions above 5% of cycles
- Library functions where you control the call frequency (e.g., reducing physics calls)
- Allocation functions (reduce allocation count, not the allocator itself)
