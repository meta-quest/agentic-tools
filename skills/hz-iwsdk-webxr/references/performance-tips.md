# Performance Tips

This reference covers performance optimization techniques for WebXR applications built with IWSDK running on Meta Quest hardware.

## Frame Rate Targets

Meta Quest headsets require consistent frame delivery to maintain presence and comfort:

| Headset       | Supported Refresh Rates     | Frame Budget    |
| ------------- | --------------------------- | --------------- |
| Quest 2       | 72Hz, 80Hz, 90Hz, 120Hz    | 13.9ms at 72Hz  |
| Quest 3       | 72Hz, 80Hz, 90Hz, 120Hz    | 13.9ms at 72Hz  |
| Quest 3S      | 72Hz, 80Hz, 90Hz            | 13.9ms at 72Hz  |
| Quest Pro     | 72Hz, 80Hz, 90Hz            | 13.9ms at 72Hz  |

The frame budget is the total time available for both CPU and GPU work. At 72Hz, you have 13.9ms per frame. At 90Hz, only 11.1ms. Exceeding the budget causes frame drops, judder, and motion sickness.

## Fixed Foveated Rendering (FFR)

FFR reduces the resolution at the edges of each eye's view, where the user is less likely to look. This significantly reduces fragment shader workload.

```typescript
// Enable FFR at the start of the XR session
const session = world.session;
if (session && 'fixedFoveatedRendering' in session) {
  session.fixedFoveatedRendering.level = 'high'; // 'none', 'low', 'medium', 'high'
}
```

FFR levels:

| Level    | Peripheral Resolution Reduction | Use Case                          |
| -------- | ------------------------------- | --------------------------------- |
| `none`   | 0%                              | Maximum visual quality            |
| `low`    | ~15%                            | Slight performance gain           |
| `medium` | ~25%                            | Balanced quality and performance  |
| `high`   | ~40%                            | Maximum performance gain          |

Recommendation: Start with `medium` and adjust based on visual quality needs.

## Application SpaceWarp

WebXR Application SpaceWarp (ASW) allows you to render at half the target frame rate while the compositor reprojects frames to maintain visual smoothness. This effectively doubles the time budget per frame.

```typescript
// Enable Application SpaceWarp
if (navigator.xr && 'enableSpaceWarp' in navigator.xr) {
  navigator.xr.enableSpaceWarp(true);
}
```

Caveats:

- SpaceWarp adds latency and can produce artifacts on fast-moving objects
- It works best for applications with relatively static scenes
- UI text may shimmer -- consider using compositor layers for important UI
- Not all content types benefit equally from SpaceWarp

## Draw Call Optimization

Draw calls are one of the biggest performance bottlenecks on Quest. Each unique material/geometry combination is a separate draw call.

### Batching Geometries

Combine multiple meshes that share the same material:

```typescript
import * as THREE from 'three';
import { mergeGeometries } from 'three/examples/jsm/utils/BufferGeometryUtils.js';

const geometries = [];
for (let i = 0; i < 100; i++) {
  const geo = new THREE.BoxGeometry(0.1, 0.1, 0.1);
  geo.translate(Math.random() * 10, 0, Math.random() * 10);
  geometries.push(geo);
}

const mergedGeometry = mergeGeometries(geometries);
const material = new THREE.MeshStandardMaterial({ color: 0x888888 });
const batchedMesh = new THREE.Mesh(mergedGeometry, material);
```

### Instanced Rendering

For many identical objects, use `THREE.InstancedMesh`:

```typescript
const geometry = new THREE.BoxGeometry(0.2, 0.2, 0.2);
const material = new THREE.MeshStandardMaterial({ color: 0x44aa88 });
const instanceCount = 1000;
const instancedMesh = new THREE.InstancedMesh(geometry, material, instanceCount);

const matrix = new THREE.Matrix4();
for (let i = 0; i < instanceCount; i++) {
  matrix.setPosition(
    Math.random() * 20 - 10,
    Math.random() * 5,
    Math.random() * 20 - 10
  );
  instancedMesh.setMatrixAt(i, matrix);
}
instancedMesh.instanceMatrix.needsUpdate = true;
```

This renders 1000 cubes in a single draw call.

### Draw Call Budget

Aim for fewer than 100 draw calls total on Quest. Monitor draw calls with:

```typescript
console.log('Draw calls:', world.renderer.info.render.calls);
console.log('Triangles:', world.renderer.info.render.triangles);
```

## Texture Optimization

- **Compress textures**: Use KTX2 with Basis Universal compression. Three.js supports this via `KTX2Loader`.
- **Use mipmaps**: Always enable mipmaps for textures viewed at varying distances. Three.js generates them by default for power-of-two textures.
- **Limit resolution**: Quest GPU has limited memory bandwidth. Avoid textures larger than 2048x2048. Use 1024x1024 or smaller where possible.
- **Atlas textures**: Combine multiple small textures into a single texture atlas to reduce material count and draw calls.

```typescript
import { KTX2Loader } from 'three/examples/jsm/loaders/KTX2Loader.js';

const ktx2Loader = new KTX2Loader()
  .setTranscoderPath('/basis/')
  .detectSupport(renderer);

const texture = await ktx2Loader.loadAsync('/textures/environment.ktx2');
```

## Asset Loading

Use `AssetManifest` plus `World.create(...)` for startup preloading, then query
assets from `AssetManager` by key:

```typescript
import { AssetManager, AssetType, World } from '@iwsdk/core';

const world = await World.create(container, {
  assets: {
    lobby: {
      url: '/models/lobby.gltf',
      type: AssetType.GLTF,
      priority: 'critical',
    },
  },
});

const lobby = AssetManager.getGLTF('lobby')?.scene.clone();
if (lobby) {
  world.createTransformEntity(lobby);
}
```

Best practices:

- Load assets asynchronously during a loading screen
- Show a loading UI with progress indication
- Preload critical assets before entering VR
- Use compressed formats (glTF binary `.glb` with Draco/meshopt compression, KTX2 textures)

## Profiling Tools

### Chrome DevTools

Connect to the Quest Browser via `chrome://inspect` on your desktop Chrome:

1. Connect the Quest via USB
2. Open `chrome://inspect` in desktop Chrome
3. Find the Quest Browser tab running your app
4. Click "inspect" to open DevTools
5. Use the Performance tab to record and analyze CPU traces

### OVR Metrics Tool

An on-device overlay that displays real-time performance metrics:

- FPS (actual vs target)
- CPU and GPU frame times
- CPU and GPU utilization levels
- Thermal state

Install it from the Meta Quest Developer Hub or via ADB:

```bash
hzdb adb logcat --tag OVRMetrics
```

### Three.js Renderer Info

Log renderer statistics each frame:

```typescript
export const DebugStatsSystem = createSystem({
  name: 'debugStats',
  queries: [],
  execute: (world) => {
    if (world.time.frame % 300 === 0) { // Log every 300 frames
      const info = world.renderer.info;
      console.log(`Draw calls: ${info.render.calls}`);
      console.log(`Triangles: ${info.render.triangles}`);
      console.log(`Textures: ${info.memory.textures}`);
      console.log(`Geometries: ${info.memory.geometries}`);
    }
  },
});
```

## Media Layers

For video playback or important UI elements, use WebXR Layers. These are composited by the VR runtime at the final stage, providing better visual quality (no aliasing, no double-sampling) and improved performance.

```typescript
// Request a quad layer for video playback
const session = world.session;
const layerFactory = new XRMediaBinding(session);

const videoElement = document.createElement('video');
videoElement.src = '/videos/intro.mp4';
await videoElement.play();

const quadLayer = layerFactory.createQuadLayer(videoElement, {
  space: xrReferenceSpace,
  layout: 'mono',
  width: 2.0,   // 2 meters wide
  height: 1.125, // 16:9 aspect ratio
  transform: new XRRigidTransform(
    { x: 0, y: 1.5, z: -3, w: 1 },
    { x: 0, y: 0, z: 0, w: 1 }
  ),
});

session.updateRenderState({ layers: [quadLayer, session.renderState.baseLayer] });
```

Benefits of compositor layers:

- Video is sampled directly by the compositor at full resolution
- No lens-correction double-sampling artifacts
- GPU cost is lower than rendering video as a textured quad in the scene
- Text on layers is crisper than text rendered in-scene

## Three.js Tips

### Dispose Unused Resources

Three.js does not automatically garbage-collect GPU resources. Dispose them explicitly:

```typescript
function removeEntity(entity) {
  const object3D = entity.getObject3D();
  if (object3D) {
    object3D.traverse((child) => {
      if (child.geometry) child.geometry.dispose();
      if (child.material) {
        if (Array.isArray(child.material)) {
          child.material.forEach((m) => m.dispose());
        } else {
          child.material.dispose();
        }
      }
    });
  }
  entity.destroy();
}
```

### Object Pooling

Reuse objects instead of creating and destroying them:

```typescript
class BulletPool {
  private pool: Entity[] = [];

  acquire(world: World): Entity {
    if (this.pool.length > 0) {
      const entity = this.pool.pop()!;
      entity.getObject3D().visible = true;
      return entity;
    }
    return this.createBullet(world);
  }

  release(entity: Entity): void {
    entity.getObject3D().visible = false;
    entity.getMutable(Velocity).x = 0;
    entity.getMutable(Velocity).y = 0;
    entity.getMutable(Velocity).z = 0;
    this.pool.push(entity);
  }

  private createBullet(world: World): Entity {
    const mesh = new THREE.Mesh(bulletGeometry, bulletMaterial);
    return world
      .createEntity()
      .addComponent(Transform, { position: { x: 0, y: 0, z: 0 } })
      .addComponent(Velocity, { x: 0, y: 0, z: 0 })
      .addObject3D(mesh);
  }
}
```

### Avoid Allocations in the Render Loop

Pre-allocate temporary vectors and quaternions:

```typescript
// Bad: creates new Vector3 every frame
execute: (world, queries) => {
  for (const entity of queries.get(movingEntities)) {
    const direction = new THREE.Vector3(1, 0, 0); // Allocation every frame
  }
};

// Good: reuse pre-allocated vector
const _tempVec3 = new THREE.Vector3();

execute: (world, queries) => {
  for (const entity of queries.get(movingEntities)) {
    _tempVec3.set(1, 0, 0); // Reuse, no allocation
  }
};
```

## Multiview Rendering

Multiview rendering draws both eyes in a single pass, significantly reducing draw call overhead. Enable it via the WebGL extension:

```typescript
const gl = renderer.getContext();
const ext = gl.getExtension('OCULUS_multiview');
if (ext) {
  console.log('Multiview rendering supported');
  renderer.xr.useMultiview = true;
}
```

Multiview effectively halves the draw call count for stereo rendering. It is supported on all Quest headsets and should be enabled whenever possible.

## Performance Checklist

Use this checklist when optimizing a WebXR application for Quest:

- [ ] Draw calls under 100
- [ ] Triangle count under 750K per eye
- [ ] Textures compressed (KTX2/Basis)
- [ ] No textures larger than 2048x2048
- [ ] FFR enabled at medium or high
- [ ] Multiview rendering enabled
- [ ] No allocations in the render loop (no `new` in `execute`)
- [ ] Unused geometries and materials disposed
- [ ] Object pooling for frequently created/destroyed entities
- [ ] Asset loading with progress UI
- [ ] Video playback using compositor layers
- [ ] Tested on-device at target refresh rate
- [ ] No thermal throttling warnings in logs
