---
name: hz-unreal-code-review
license: Apache-2.0
description: Reviews Unreal Engine 5 code targeting Meta Quest and Horizon OS for mobile-VR performance issues, covering frame budget, renderer config, per-frame Tick cost, Blueprint hotpaths, draw-call/overdraw on tiled mobile GPUs, per-frame allocations, foveation, and Multi-View stereo. Use during code review of UE5 gameplay/rendering code that targets Meta Quest, or when diagnosing frame drops or jank.
---

# UE5 Code Review for Meta Quest / Horizon OS Performance

Review checklist and diagnostic guidance for UE5 code running on Meta Quest's mobile-VR hardware.

## Frame budget context

Meta Quest headsets commonly run at **72Hz**, giving a **~13.8ms** total frame budget (CPU + GPU combined, since the mobile pipeline is far more bandwidth/compute constrained than desktop). Some devices/modes go higher (90/120Hz), which only shrinks the budget further. Treat 13.8ms as the conservative default when reviewing.

Any single system consistently costing more than a low single-digit percentage of that budget deserves scrutiny.

## 1. Renderer configuration sanity check

Before reviewing logic, confirm project-level settings aren't undermining it (see `hz-unreal-project-setup` for full detail):

- Mobile Multi-View **on**, Mobile HDR **off**, Forward+MSAA renderer.
- If these are wrong, no amount of code-level optimization will fix the frame time — flag it first.

## 2. Per-frame Tick cost

`Tick`/`Event Tick` on Actors and Components runs every frame for every instance — it's the most common source of silent cost creep.

**Flag on sight:**
- `Event Tick` doing **line traces / sweeps** (`LineTraceSingle`, `SphereTraceMulti`, etc.) — move to timers, event-driven triggers, or reduced-frequency ticking (`SetActorTickInterval`).
- `Event Tick` calling **`GetAllActorsOfClass`** or similar world-scanning queries — these are O(n) over all actors and should never run per-frame; cache results or use an event/registry pattern instead.
- Tick enabled on actors/components that don't need it — check `PrimaryActorTick.bCanEverTick` / `bStartWithTickEnabled` defaults weren't left on by scaffolding.

## 3. Blueprint VM hotpaths

The Blueprint VM has meaningfully higher per-op overhead than native C++. Acceptable for setup/config/rare-event logic; a liability in per-frame or per-object hot paths.

**Flag:**
- Blueprint graphs running every Tick with nontrivial branching/math/loops.
- Blueprint-implemented logic operating over collections (arrays of actors, inventory loops, per-vertex/per-bone processing).
- **Recommendation:** port hot loops to C++ (or a `BlueprintCallable` native function called from a thin BP wrapper), keep BP for the parts that actually benefit from iteration speed/designer access.

## 4. Draw calls, batching, and overdraw

Quest GPUs are **tile-based mobile GPUs** — overdraw and draw-call count matter more than on desktop discrete GPUs.

**Flag:**
- High unique material/mesh counts preventing batching (encourage material instancing + mesh merging/Nanite-mobile where applicable, HISM/ISM for repeated meshes).
- **Translucency** used where masked/opaque would do — translucent materials disable early-Z and can force multiple overlapping passes, which is expensive on a tile renderer. Check for translucent UI panels, VFX, or foliage that could be masked instead.
- Stacked full-screen or large-screen-space translucent effects (particle systems, UI backgrounds) — each layer multiplies overdraw cost.

## 5. Per-frame allocations and GC pressure

- Watch for **per-frame heap allocations**: `NewObject` calls, dynamic array growth, string concatenation/formatting, or Blueprint "Make Array"/"Append" patterns inside Tick or frequently-called events.
- These generate garbage that triggers UE's GC, which can cause visible frame spikes (especially bad in VR, where a spike reads as nausea-inducing judder, not just a dropped frame).
- Prefer object pooling, pre-sized/reused containers, and caching over per-frame allocation.

## 6. Foveation and stereo rendering

- Confirm **Fixed Foveated Rendering (FFR)** is configured appropriately for the app's visual/performance tradeoff (not left at a default that's wrong for the content).
- Confirm rendering uses **single-pass stereo via Mobile Multi-View** rather than two independent per-eye passes — check no code path (custom render passes, post-process materials) forces a Multi-View-incompatible fallback.

## 7. Confirm with real device data, not desktop PIE

Desktop **Play-In-Editor is not representative** of Quest's mobile GPU/CPU characteristics — timings, overdraw cost, and thermal throttling behavior are all different. Any performance conclusion from this review should be confirmed on-device.

```bash
metavr device list                 # confirm target headset connected
metavr app install <path-to-apk>
metavr capture perfetto            # or the project's equivalent perf-capture command
```

Use the captured trace to confirm suspected hotspots (Tick cost, GPU time, GC pauses) actually show up as claimed before treating a review comment as validated.

## Review checklist

- [ ] Renderer config (Multi-View/HDR/Forward+MSAA) verified correct at project level
- [ ] No `Event Tick` running line traces, sweeps, or `GetAllActorsOfClass`
- [ ] Tick disabled/interval-reduced on actors/components that don't need per-frame updates
- [ ] No per-frame-hot Blueprint logic that should be C++
- [ ] Material/mesh instancing and batching used where repetition exists
- [ ] Translucency usage justified; not substituting for masked/opaque
- [ ] No per-frame heap allocations / GC-pressure patterns in hot paths
- [ ] FFR configured intentionally; single-pass stereo (Multi-View) confirmed active
- [ ] Findings confirmed via `metavr` on-device perf capture, not desktop PIE alone

## Last verified

This file's content (frame budget, Tick/Blueprint-VM cost, draw-call/overdraw, GC pressure, foveation) is general UE5 mobile-VR performance engineering guidance rather than versioned Meta SDK claims — it was not checked against specific developers.meta.com pages in this verification pass, so no deprecation-style citations apply here. If a PR reviewer needs sourced backing for any specific line (e.g. the 72Hz/13.8ms budget figure, or Mobile Multi-View recommendation), run a `meta_docs_search` pass on it before submission; none of these claims were flagged as disputed in this round of verification.
