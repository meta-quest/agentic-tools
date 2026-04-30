---
name: hz-iwsdk-webxr
description: Builds WebXR experiences for Meta Quest and Horizon OS using the Immersive Web SDK (IWSDK) — ECS architecture, Three.js integration, spatial UI. Use when creating web-based VR/MR apps for Quest Browser.
allowed-tools:
  - Bash(hzdb:*)
---

# IWSDK WebXR Skill

Build immersive WebXR experiences for Meta Quest using Meta's Immersive Web
SDK (IWSDK). This skill covers the current package layout, ECS architecture,
Three.js integration, spatial UI development, XR input handling, and the
recommended Vite-based development loop.

## When to Use This Skill

Use this skill when you need to:

- Build a WebXR experience targeting Meta Quest using IWSDK
- Create 3D scenes using the ECS architecture on top of Three.js
- Design spatial UI panels with UIKitML and `PanelUI`
- Handle XR input from controllers, hands, and world-space pointers
- Run a closed-loop edit, reload, observe, and fix workflow for IWSDK apps
- Optimize WebXR performance for Quest hardware
- Debug and test WebXR applications with IWSDK's Vite dev tooling and Quest
  Browser

This skill applies to all Meta Quest headsets with Quest Browser.

## What Is IWSDK

The Immersive Web SDK (IWSDK) is Meta's framework for building WebXR
experiences. It provides:

- **Entity Component System (ECS)**: a data-oriented architecture built on
  typed component storage and query-driven systems
- **Three.js integration**: IWSDK manages the renderer, scene, camera, and XR
  session lifecycle so app code can focus on content
- **Spatial UI toolkit**: UIKitML plus `PanelUI`, `PanelDocument`, and
  `UIKitDocument` for in-world and screen-space UI
- **XR input management**: unified handling of controllers, hand tracking,
  pointers, and grabbing
- **Locomotion and interaction systems**: opt-in grabbing, teleport, slide, and
  turning features
- **Asset management**: manifest-based preloading and keyed access through
  `AssetManager`

IWSDK runs in the browser via the WebXR Device API. Applications are standard
web pages that are usually served by Vite during development and opened in
Quest Browser for on-device validation.

## Current Packaging And Tooling

These details are important because older pre-release guidance is now wrong:

- The main runtime package is `@iwsdk/core`
- The official scaffold command is `npm create @iwsdk@latest`
- The default dev loop uses `@iwsdk/vite-plugin-dev`
- Local HTTPS is typically handled with `vite-plugin-mkcert`
- Current IWSDK packages require Node.js `>=20.19.0`
- `@iwsdk/core` re-exports `@iwsdk/xr-input` and `@iwsdk/locomotor`

## Quick Start

### 1. Create A New Project

```bash
npm create @iwsdk@latest my-project
cd my-project
```

If you are starting from the official create flow, it can also install
dependencies and initialize git for you.

### 2. Install Dependencies

```bash
npm install
```

If you are setting up manually, use `@iwsdk/core`, `three`,
`@iwsdk/vite-plugin-dev`, and `vite-plugin-mkcert` instead of the old
`@meta-quest/iwsdk` package.

### 3. Start The Dev Server

```bash
npm run dev
```

The standard IWSDK Vite setup gives you a secure local dev server, hot module
replacement, and an emulator/dev browser workflow through
`@iwsdk/vite-plugin-dev`.

### 4. Test In XR

There are two normal validation paths:

- **Quest Browser**: open the secure dev URL on the headset and launch XR there
- **IWSDK dev tooling**: use the Vite plugin's emulator/dev browser path for
  quick desktop iteration

Quest Browser testing requires HTTPS. If the app works on desktop but will not
enter XR on-device, confirm that the dev server URL is secure and headset
reachable.

## AI-Assisted Development Loop

IWSDK is a strong fit for coding agents because the development loop is already
web-native: edit source, reload the page, observe behavior, and iterate.

Recommended loop:

1. Verify API and platform details against current docs before coding
2. Edit the code and run the Vite dev server
3. Reload in Quest Browser or the IWSDK emulator/dev browser
4. Observe logs, screenshots, and runtime state
5. Iterate until runtime behavior matches the request

If you are pairing a Quest-native frontend with a host-side coding agent, keep
the frontend thin. Let the host machine own the repository, file edits,
dependency installs, tests, build steps, and hzdb tool calls.

## Project Structure

A typical IWSDK project looks like this:

```text
my-project/
  src/
    index.ts              # Entry point: World.create, scene setup, XR controls
    systems/              # Optional ECS systems
      movement.ts
    components/           # Optional ECS components
      velocity.ts
  public/
    models/               # GLTF / GLB assets
    textures/             # Textures and images
    audio/                # Audio assets
    ui/                   # Compiled UIKitML JSON output
  ui/
    welcome.uikitml       # Source UIKitML files
  vite.config.ts
  package.json
  tsconfig.json
```

## Key Patterns

### Creating A Component

```typescript
import { Types, createComponent } from '@iwsdk/core';

export const Velocity = createComponent(
  'Velocity',
  {
    x: { type: Types.Float32, default: 0 },
    y: { type: Types.Float32, default: 0 },
    z: { type: Types.Float32, default: 0 },
  },
  'Linear velocity in meters per second',
);
```

### Writing A System

```typescript
import { createSystem } from '@iwsdk/core';
import { Velocity } from './components/velocity';

export class MovementSystem extends createSystem({
  moving: { required: [Velocity] },
}) {
  update(delta: number) {
    this.queries.moving.entities.forEach((entity) => {
      if (!entity.object3D) {
        return;
      }
      entity.object3D.position.x += Velocity.data.x[entity.index] * delta;
      entity.object3D.position.y += Velocity.data.y[entity.index] * delta;
      entity.object3D.position.z += Velocity.data.z[entity.index] * delta;
    });
  }
}
```

### Spawning An Entity With A Three.js Mesh

```typescript
import * as THREE from 'three';
import { Velocity } from './components/velocity';

function spawnCube(world) {
  const mesh = new THREE.Mesh(
    new THREE.BoxGeometry(0.5, 0.5, 0.5),
    new THREE.MeshStandardMaterial({ color: 0x00aaff }),
  );

  const entity = world.createTransformEntity(mesh);
  entity.object3D!.position.set(0, 1.5, -2);
  entity.addComponent(Velocity, { x: 0, y: 0, z: 0 });
}
```

### Creating The World And Launching XR

```typescript
import {
  ReferenceSpaceType,
  SessionMode,
  World,
} from '@iwsdk/core';

const container = document.getElementById('scene-container') as HTMLDivElement;
const world = await World.create(container, {
  xr: {
    sessionMode: SessionMode.ImmersiveVR,
    referenceSpace: ReferenceSpaceType.LocalFloor,
    offer: 'none',
  },
});

world.registerSystem(MovementSystem);
world.launchXR();
```

`World.create(...)` already wires the renderer, core systems, asset loading, and
render loop. Do not teach agents to use `new World()`, `createWorld()`,
`world.start()`, or the older `enterXR(...)` path.

## Gotchas

These are common pitfalls that cause unexpected failures or wasted debugging time when building IWSDK WebXR apps for Quest.

- **HTTPS required for WebXR on device** -- The WebXR Device API only works over HTTPS (or localhost). If you serve your dev build over plain HTTP and open it on the Quest Browser, the "Enter VR" button will not appear. Use `ngrok`, a self-signed cert, or the `--https` flag in Vite to tunnel HTTPS during development.
- **Quest Browser is Chromium, not desktop Chrome** -- Feature support differs. `SharedArrayBuffer`, certain WebGL2 extensions, and `OffscreenCanvas` may behave differently or be unavailable. Always test on actual Quest Browser, not just desktop Chrome with the IWER emulator.
- **72 Hz default, not 90 Hz** -- Quest Browser defaults to 72 Hz for WebXR sessions. You must explicitly request a higher framerate via `xrSession.updateTargetFrameRate(90)` after session start. Forgetting this is the most common reason a WebXR app feels "sluggish" compared to a native build.
- **Draw call budget is much lower than native** -- Aim for under 50-80 draw calls on Quest 2 and under 100-120 on Quest 3. JavaScript overhead per draw call is higher than native. Batch geometries, use instanced rendering, and merge materials aggressively.
- **Texture memory limits** -- Quest Browser has a lower GPU memory ceiling than native apps. Large uncompressed textures (4K+) can cause silent OOM crashes or visual corruption. Use compressed textures (KTX2 with Basis Universal or ETC2) and keep total texture memory under 256 MB.
- **`requestAnimationFrame` vs. XR frame loop** -- Once an XR session starts, you must use `xrSession.requestAnimationFrame`, not `window.requestAnimationFrame`. Using the wrong one causes the scene to freeze or render at the wrong rate.
- **Hand tracking pinch threshold** -- The WebXR hand tracking API reports pinch as a continuous value (0.0-1.0). Many developers threshold at 0.5, but on Quest the reliable activation threshold is closer to 0.7-0.8. Lower thresholds cause false positives, especially during pointing gestures.
- **HMR can break XR sessions** -- Hot module replacement sometimes fails to properly tear down and recreate the WebXR session. If your scene goes black after an HMR update, do a full page reload rather than debugging phantom rendering issues.
- **CORS blocks asset loading in VR** -- When loading GLTF/GLB models or textures from a CDN, ensure proper CORS headers are set. Quest Browser enforces CORS strictly. Missing headers cause silent asset load failures that are hard to diagnose without DevTools connected.
## Development Tips

- Use `World.create(...)`, `world.launchXR()`, and `world.exitXR()` as the
  runtime control surface
- Keep HTTPS enabled for Quest Browser development because WebXR is gated on
  secure contexts
- Start with `@iwsdk/vite-plugin-dev` and Quest Browser validation, then layer
  in emulator/dev browser workflows for faster iteration
- Search and fetch Meta docs before coding against APIs you have not verified
  recently
- Test on a real Quest device early and often because desktop emulation does not
  capture all input, comfort, and performance issues
- Keep draw calls low (roughly under 100) for smooth performance on Quest
  hardware

## References

### Skill References

- [AI-Assisted Development](references/ai-assisted-development.md) -- closed-loop edit/reload/observe workflows, Quest Browser testing, host-agent architecture
- [ECS Architecture](references/ecs-architecture.md) -- Components, entities, systems, queries, and the World
- [Spatial UI](references/spatial-ui.md) -- UIKitML, PanelUI, PanelDocument, layout, and UI best practices
- [Input Handling](references/input-handling.md) -- Controllers, hands, pointers, grabbing, and locomotion
- [Performance Tips](references/performance-tips.md) -- Frame rate targets, optimization techniques, and profiling tools
