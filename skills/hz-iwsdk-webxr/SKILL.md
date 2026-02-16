---
name: hz-iwsdk-webxr
description: Builds WebXR experiences for Meta Quest and Horizon OS using the Immersive Web SDK (IWSDK) — ECS architecture, Three.js integration, spatial UI. Use when creating web-based VR/MR apps for Quest Browser.
allowed-tools:
  - Bash(hzdb:*)
---

# IWSDK WebXR Skill

Build immersive WebXR experiences for Meta Quest using Meta's Immersive Web SDK (IWSDK). This skill covers the ECS architecture, Three.js integration, spatial UI development, XR input handling, and performance optimization for web-based VR/MR applications.

## When to Use This Skill

Use this skill when you need to:

- Build a WebXR experience targeting Meta Quest using IWSDK
- Create 3D scenes using the Entity Component System (ECS) architecture on top of Three.js
- Design spatial UI panels with UIKit or UIKitML markup
- Handle XR input from controllers, hands, and gaze
- Optimize WebXR performance for Quest hardware
- Debug and test WebXR applications with the IWER emulator

This skill applies to all Meta Quest headsets with the Quest Browser (Quest 2, Quest 3, Quest 3S, Quest Pro).

## What is IWSDK

The Immersive Web SDK (IWSDK) is Meta's framework for building WebXR experiences. It provides:

- **Entity Component System (ECS)**: A data-oriented architecture for organizing game logic, inspired by modern game engine patterns
- **Three.js integration**: IWSDK manages the Three.js scene graph, camera, and WebXR renderer so you can focus on content
- **Spatial UI toolkit**: UIKit and UIKitML for building 3D interface panels using a familiar HTML/CSS-like workflow
- **XR input management**: Unified handling of controllers, hand tracking, gaze, and grabbing interactions
- **Locomotion**: Built-in teleport, smooth locomotion, and snap/smooth turn systems
- **Asset management**: Async loading of models, textures, audio, and other assets with progress tracking

IWSDK runs in the browser via the WebXR Device API. Applications are standard web pages that can be hosted anywhere and accessed through the Quest Browser.

## Key Concepts

### Entity Component System (ECS)

IWSDK uses an ECS architecture where:

- **Entities** are lightweight containers with a unique ID -- they hold no data or logic themselves
- **Components** are typed data containers attached to entities (position, velocity, health, mesh reference)
- **Systems** contain the logic that runs each frame, operating on entities that match specific component queries
- **Queries** define which entities a system cares about by specifying required and excluded components

This separation of data and logic makes it straightforward to compose behaviors and keeps code modular.

### Three.js Integration

IWSDK sits on top of Three.js and manages the core rendering pipeline:

- The `THREE.Scene`, `THREE.WebGLRenderer`, and `THREE.PerspectiveCamera` are created and managed by IWSDK
- You attach `THREE.Object3D` instances to entities and IWSDK handles adding them to the scene graph
- You can use any Three.js feature (geometries, materials, lights, post-processing) within the ECS framework
- The XR session lifecycle (requesting, entering, exiting) is handled automatically

### Spatial UI

IWSDK provides two approaches for building UI in 3D space:

- **UIKit**: Programmatic API for creating UI panels with Flexbox layout
- **UIKitML**: Declarative markup language similar to HTML/CSS, compiled by a Vite plugin into spatial UI panels

### XR Input

The `XRInputManager` provides a unified interface for all input sources:

- Head (gaze direction)
- Left and right controllers (buttons, thumbsticks, triggers)
- Left and right hands (pinch, grab, point gestures)
- Ray pointers for UI interaction and object selection
- Grab pointers for picking up and manipulating objects

## Quick Start

### 1. Create a New Project

```bash
npm create @meta-quest/iwsdk my-project
cd my-project
```

### 2. Install Dependencies

```bash
npm install
```

### 3. Start the Dev Server

```bash
npm run dev
```

The dev server starts with hot module replacement. You can access the app in a desktop browser for initial development.

### 4. Test in VR

There are two options for testing:

- **Quest Browser**: Navigate to the dev server URL on your Quest (use the same local network, or tunnel with `ngrok` or similar). Tap the "Enter VR" button.
- **IWER Emulator**: The Immersive Web Emulator Runtime (IWER) lets you test WebXR on desktop by emulating a headset and controllers. Install the browser extension or configure it in your project.

## Project Structure

A typical IWSDK project looks like this:

```
my-project/
  src/
    index.ts              # Entry point -- creates the World, registers systems
    components/           # Component definitions
      player.ts
      enemy.ts
    systems/              # System definitions
      movement.ts
      combat.ts
      rendering.ts
    ui/                   # UIKitML files for spatial UI
      hud.xml
      menu.xml
    assets/               # 3D models, textures, audio
  public/                 # Static assets served directly
  vite.config.ts          # Vite configuration with IWSDK plugin
  iwsdk.config.ts         # IWSDK-specific configuration
  package.json
  tsconfig.json
```

## Key Patterns

### Creating a Component

```typescript
import { createComponent } from '@meta-quest/iwsdk';

export const Velocity = createComponent({
  name: 'velocity',
  schema: {
    x: 'f32',
    y: 'f32',
    z: 'f32',
  },
});
```

### Writing a System

```typescript
import { createSystem, createQuery } from '@meta-quest/iwsdk';
import { Position } from './components/position';
import { Velocity } from './components/velocity';

const movingEntities = createQuery({ all: [Position, Velocity] });

export const MovementSystem = createSystem({
  name: 'movement',
  queries: [movingEntities],
  execute: (world, queries) => {
    const delta = world.time.delta;
    for (const entity of queries.get(movingEntities)) {
      const pos = entity.get(Position);
      const vel = entity.get(Velocity);
      pos.x += vel.x * delta;
      pos.y += vel.y * delta;
      pos.z += vel.z * delta;
    }
  },
});
```

### Spawning an Entity with a Three.js Mesh

```typescript
import * as THREE from 'three';
import { Position } from './components/position';
import { Velocity } from './components/velocity';

function spawnCube(world) {
  const geometry = new THREE.BoxGeometry(0.5, 0.5, 0.5);
  const material = new THREE.MeshStandardMaterial({ color: 0x00aaff });
  const mesh = new THREE.Mesh(geometry, material);

  world
    .createEntity()
    .addComponent(Position, { x: 0, y: 1.5, z: -2 })
    .addComponent(Velocity, { x: 0, y: 0, z: 0 })
    .addObject3D(mesh);
}
```

### Registering Systems in the World

```typescript
import { createWorld } from '@meta-quest/iwsdk';
import { MovementSystem } from './systems/movement';
import { RenderSystem } from './systems/rendering';

const world = createWorld();

world.registerSystem(MovementSystem);
world.registerSystem(RenderSystem);

world.start();
```

## Development Tips

- Use the IWER emulator during early development to iterate quickly on desktop without a headset
- Enable hot module replacement in the Vite config for fast feedback loops
- Use Chrome DevTools to inspect the Three.js scene with the three-devtools extension
- Test on a real Quest device early and often -- desktop behavior can differ from on-device WebXR
- Keep draw calls low (under 100) for smooth performance on Quest hardware

## References

### Skill References

- [ECS Architecture](references/ecs-architecture.md) -- Components, entities, systems, queries, and the World
- [Spatial UI](references/spatial-ui.md) -- UIKit, UIKitML, layout, text rendering, and UI best practices
- [Input Handling](references/input-handling.md) -- Controllers, hands, pointers, grabbing, and locomotion
- [Performance Tips](references/performance-tips.md) -- Frame rate targets, optimization techniques, and profiling tools

