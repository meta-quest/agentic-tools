# ECS Architecture

This reference covers the Entity Component System (ECS) architecture used by IWSDK. ECS is a data-oriented pattern that separates data (components) from logic (systems) and uses lightweight containers (entities) to compose behaviors.

## Components

Components are pure data containers defined with a typed schema. Each field has a specific type that determines how the data is stored and accessed.

### Defining a Component

```typescript
import { createComponent } from '@meta-quest/iwsdk';

export const Health = createComponent({
  name: 'health',
  schema: {
    current: 'f32',
    max: 'f32',
    regenerationRate: 'f32',
  },
});
```

### Supported Field Types

| Type          | Description                                | Example Use               |
| ------------- | ------------------------------------------ | ------------------------- |
| `'f32'`       | 32-bit floating point number               | position, speed, health   |
| `'f64'`       | 64-bit floating point number               | high-precision timestamps |
| `'i32'`       | 32-bit signed integer                      | score, count              |
| `'u32'`       | 32-bit unsigned integer                    | flags, bitmasks           |
| `'bool'`      | Boolean value                              | isActive, isVisible       |
| `'string'`    | String value                               | name, label               |
| `'vec3'`      | 3D vector (x, y, z)                        | position, velocity        |
| `'quaternion'`| Quaternion (x, y, z, w)                    | rotation, orientation     |
| `'entity'`    | Reference to another entity                | target, parent            |

### Vector and Quaternion Fields

For spatial data, use the built-in vector and quaternion types:

```typescript
export const Transform = createComponent({
  name: 'transform',
  schema: {
    position: 'vec3',
    rotation: 'quaternion',
    scale: 'vec3',
  },
});
```

These types integrate directly with Three.js `Vector3` and `Quaternion` objects.

### Entity References

Components can reference other entities, allowing you to build relationships:

```typescript
export const Follow = createComponent({
  name: 'follow',
  schema: {
    target: 'entity',
    speed: 'f32',
    minDistance: 'f32',
  },
});
```

## Entities

Entities are lightweight containers identified by a unique ID. They have no data or logic of their own -- they gain behavior by having components attached to them.

### Creating an Entity

```typescript
const entity = world.createEntity();
```

### Adding Components

```typescript
entity.addComponent(Health, { current: 100, max: 100, regenerationRate: 1.0 });
entity.addComponent(Transform, {
  position: { x: 0, y: 1.5, z: -3 },
  rotation: { x: 0, y: 0, z: 0, w: 1 },
  scale: { x: 1, y: 1, z: 1 },
});
```

### Removing Components

```typescript
entity.removeComponent(Health);
```

### Attaching Three.js Objects

Entities can have a Three.js `Object3D` attached. IWSDK automatically adds the object to the scene graph and syncs its transform:

```typescript
import * as THREE from 'three';

const mesh = new THREE.Mesh(
  new THREE.SphereGeometry(0.5),
  new THREE.MeshStandardMaterial({ color: 0xff0000 })
);

entity.addObject3D(mesh);
```

### Destroying an Entity

```typescript
entity.destroy();
```

Destroying an entity removes it from all queries, detaches its Three.js object from the scene, and frees its component data.

## Systems

Systems contain the logic that operates on entities each frame. A system declares which entities it cares about through queries, then processes matching entities in its `execute` function.

### Defining a System

```typescript
import { createSystem, createQuery } from '@meta-quest/iwsdk';
import { Health } from '../components/health';
import { Transform } from '../components/transform';

const damagedEntities = createQuery({ all: [Health, Transform] });

export const HealthRegenSystem = createSystem({
  name: 'healthRegen',
  queries: [damagedEntities],
  execute: (world, queries) => {
    const delta = world.time.delta;
    for (const entity of queries.get(damagedEntities)) {
      const health = entity.getMutable(Health);
      if (health.current < health.max) {
        health.current = Math.min(
          health.current + health.regenerationRate * delta,
          health.max
        );
      }
    }
  },
});
```

### System Lifecycle Hooks

Systems can define `init` and `cleanup` hooks in addition to `execute`:

```typescript
export const AudioSystem = createSystem({
  name: 'audio',
  queries: [audioSources],

  init: (world) => {
    // Called once when the system is registered
    // Set up audio context, load sounds, etc.
  },

  execute: (world, queries) => {
    // Called every frame
  },

  cleanup: (world) => {
    // Called when the system is removed or the world is destroyed
    // Release audio resources, close connections, etc.
  },
});
```

### Read-Only vs Mutable Access

Use `entity.get(Component)` for read-only access and `entity.getMutable(Component)` when you need to modify data. Read-only access enables internal optimizations:

```typescript
// Read-only -- does not trigger change detection
const health = entity.get(Health);
console.log(health.current);

// Mutable -- marks the component as changed
const healthMut = entity.getMutable(Health);
healthMut.current -= 10;
```

## Queries

Queries define which entities a system processes. They filter entities based on which components are present or absent.

### Basic Query

```typescript
import { createQuery } from '@meta-quest/iwsdk';

// Match entities that have BOTH Position and Velocity
const movingEntities = createQuery({
  all: [Position, Velocity],
});
```

### Excluding Components

```typescript
// Match entities with Position but NOT Static
const dynamicEntities = createQuery({
  all: [Position],
  none: [Static],
});
```

### Optional Components

```typescript
// Match entities with Position; optionally read Velocity if present
const positionedEntities = createQuery({
  all: [Position],
  any: [Velocity],
});
```

### Changed Queries

You can query for entities whose components changed since the last frame:

```typescript
const changedHealth = createQuery({
  all: [Health],
  changed: [Health],
});
```

This is useful for reactive systems that only need to act when data actually changes (updating UI, triggering effects).

### Added and Removed Queries

Track when entities gain or lose components:

```typescript
const newlySpawned = createQuery({
  added: [Transform],
});

const recentlyDestroyed = createQuery({
  removed: [Health],
});
```

## World

The `World` is the top-level container that holds all entities, systems, and global state.

### Creating a World

```typescript
import { createWorld } from '@meta-quest/iwsdk';

const world = createWorld();
```

### Registering Systems

Systems are registered on the world and execute in the order they are registered:

```typescript
world.registerSystem(InputSystem);
world.registerSystem(MovementSystem);
world.registerSystem(CollisionSystem);
world.registerSystem(RenderSystem);
```

Order matters. Input should be processed before movement, movement before collision, and rendering last.

### Accessing Time

The world provides timing information:

```typescript
world.time.delta;    // Time since last frame in seconds
world.time.elapsed;  // Total elapsed time in seconds
world.time.frame;    // Current frame number
```

### Starting the Loop

```typescript
world.start();
```

This begins the render loop. IWSDK uses `requestAnimationFrame` (or the XR session's frame callback when in VR) to drive the loop.

## Best Practices

- **Keep components small and focused.** A component should represent a single concept. Prefer `Position` + `Velocity` over a monolithic `Physics` component.
- **Keep systems focused.** Each system should handle one responsibility. A `MovementSystem` moves entities; a `CollisionSystem` detects collisions. Do not combine them.
- **Use queries efficiently.** Define queries at module scope, not inside the `execute` function. Queries are evaluated once and updated incrementally.
- **Prefer read-only access.** Use `entity.get()` instead of `entity.getMutable()` when you do not need to modify data. This avoids unnecessary change tracking overhead.
- **Avoid creating objects in the execute loop.** Pre-allocate temporary vectors and reuse them to avoid garbage collection pressure.
- **Use entity references for relationships.** Instead of storing IDs as numbers, use the `'entity'` field type so the ECS can track the relationship.
- **Destroy entities explicitly.** When an entity is no longer needed, call `entity.destroy()` to free memory and remove it from queries.
