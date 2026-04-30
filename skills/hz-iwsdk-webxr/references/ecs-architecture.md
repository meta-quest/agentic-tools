# ECS Architecture

This reference covers the current ECS model used by IWSDK. The runtime surface
is exported from `@iwsdk/core` and is built on top of Elics. The main pattern
is:

- define typed component data with `createComponent(...)`
- define query-driven systems by extending `createSystem(...)`
- attach `Object3D` instances with `world.createTransformEntity(...)`
- bootstrap everything from `World.create(...)`

## Components

Components are pure data containers. In current IWSDK, component fields are
stored on typed arrays at `Component.data.<field>`, indexed by `entity.index`.

### Defining A Component

```typescript
import { Types, createComponent } from '@iwsdk/core';

export const Health = createComponent(
  'Health',
  {
    current: { type: Types.Float32, default: 100 },
    max: { type: Types.Float32, default: 100 },
    regenerationRate: { type: Types.Float32, default: 1 },
  },
  'Simple health component',
);
```

### Common Field Types

| Type | Example Use |
| ---- | ----------- |
| `Types.Boolean` | flags and tags |
| `Types.Float32` | speeds, timers, scalar values |
| `Types.Int8` | enum-like state values |
| `Types.String` | labels, ids, file paths |
| `Types.Vec3` | positions, offsets, scales |
| `Types.Vec4` | quaternions and 4D data |
| `Types.Object` | config objects or opaque handles |
| `Types.Enum` | finite enum values backed by an enum map |

### Vector-Shaped Data

```typescript
import { Types, createComponent } from '@iwsdk/core';

export const SpawnPoint = createComponent('SpawnPoint', {
  position: { type: Types.Vec3, default: [0, 1.5, -2] },
  rotation: { type: Types.Vec4, default: [0, 0, 0, 1] },
});
```

In practice, world-space transforms still usually live on the entity's
`object3D`. Use ECS fields for authored data or gameplay state, then apply them
inside systems.

## Entities

Entities are lightweight containers identified by an index. They gain behavior
through attached components and, optionally, an attached `Object3D`.

### Creating An Entity

```typescript
const entity = world.createEntity();
```

### Adding And Removing Components

```typescript
entity.addComponent(Health, {
  current: 100,
  max: 100,
  regenerationRate: 1,
});

entity.removeComponent(Health);
```

### Attaching Three.js Objects

The normal scene-content path is to create a transform-backed entity directly
from an `Object3D`:

```typescript
import * as THREE from 'three';

const mesh = new THREE.Mesh(
  new THREE.SphereGeometry(0.5),
  new THREE.MeshStandardMaterial({ color: 0xff0000 }),
);

const entity = world.createTransformEntity(mesh);
entity.object3D!.position.set(0, 1.5, -2);
```

### Destroying An Entity

```typescript
entity.destroy();
entity.dispose(); // if GPU resources should also be released
```

Destroying an entity removes it from all queries and detaches its `Object3D`.

## Systems

Systems contain the logic that operates on entities each frame. In current
IWSDK, you usually define a class that extends `createSystem(...)`.

### Defining A System

```typescript
import { createSystem } from '@iwsdk/core';
import { Health } from '../components/health';

export class HealthRegenSystem extends createSystem({
  damaged: { required: [Health] },
}) {
  update(delta: number) {
    this.queries.damaged.entities.forEach((entity) => {
      const next = Math.min(
        Health.data.current[entity.index] +
          Health.data.regenerationRate[entity.index] * delta,
        Health.data.max[entity.index],
      );
      Health.data.current[entity.index] = next;
    });
  }
}
```

### Lifecycle Hooks

```typescript
export class SpawnSystem extends createSystem({
  players: { required: [Health] },
}) {
  init() {
    this.queries.players.subscribe('qualify', (entity) => {
      console.log('player appeared', entity.index);
    });
  }

  update(_delta: number, _time: number) {
    // frame loop
  }

  destroy() {
    // cleanup side effects here if needed
  }
}
```

`subscribe('qualify', ...)` and `subscribe('disqualify', ...)` are important
for reactive workflows such as loading panels, wiring event listeners, or
tracking entities entering and leaving a query.

## Queries

Queries are declared inline when you call `createSystem(...)`. Each query name
is available under `this.queries.<name>`.

### Basic Query

```typescript
export class MovementSystem extends createSystem({
  moving: { required: [Velocity] },
}) {}
```

### Excluding Components

```typescript
export class MovementSystem extends createSystem({
  moving: {
    required: [Velocity],
    excluded: [Frozen],
  },
}) {}
```

### Filtering With `where`

```typescript
import { PanelDocument, PanelUI, createSystem, eq } from '@iwsdk/core';

export class PanelSystem extends createSystem({
  welcomePanel: {
    required: [PanelUI, PanelDocument],
    where: [eq(PanelUI, 'config', './ui/welcome.json')],
  },
}) {}
```

## World

The `World` is the top-level container for entities, systems, scene objects, XR
state, and feature systems.

### Creating A World

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
```

### Registering Systems

Systems execute in registration order:

```typescript
world.registerSystem(InputSystem);
world.registerSystem(MovementSystem);
world.registerSystem(CollisionSystem);
world.registerSystem(RenderSystem);
```

### Launching XR

```typescript
world.launchXR();
world.exitXR();
```

`World.create(...)` already wires the render loop. Do not add a separate
`world.start()` call unless the SDK reintroduces one in a future version.

## Best Practices

- **Keep components small and focused.** Prefer composable pieces of data over a
  single monolithic component.
- **Keep systems focused.** Each system should own one responsibility.
- **Treat `Component.data` as the hot path.** Current IWSDK component data lives
  in typed storage arrays keyed by `entity.index`.
- **Use `world.createTransformEntity(...)` for scene content.** It keeps
  `Object3D` ownership aligned with the ECS world.
- **Use query subscriptions for side effects.** They are the cleanest way to
  respond when entities enter or leave a query.
- **Avoid allocations in `update(...)`.** Reuse vectors and temp objects to
  reduce GC pressure on Quest hardware.
- **Destroy entities explicitly.** Call `entity.destroy()` or `entity.dispose()`
  when content is no longer needed.
