# Web / IWSDK / WebXR Project Setup For Meta Quest

This guide walks through creating a new web-based XR project using the current
Immersive Web SDK (IWSDK) toolchain for Meta Quest.

## Requirements

- **Node.js**: 20.19.0 or newer
- **npm**: 9+ (bundled with Node.js)
- **A modern browser**: Quest Browser for device validation, plus a desktop
  browser for quick iteration
- **HTTPS**: required for WebXR API access
- **hzdb CLI**: optional, for preview/open/test workflows and device-side
  debugging

## Step 1: Create The Project

### Option A: Use The Official IWSDK Create Tool

```bash
npm create @iwsdk@latest my-quest-app
cd my-quest-app
```

The official `create-iwsdk` flow can also install dependencies, initialize git,
and scaffold XR feature choices for you.

### Option B: Manual Setup

Create the project directory and initialize it:

```bash
mkdir my-quest-app
cd my-quest-app
npm init -y
```

Install dependencies:

```bash
# Core dependencies
npm install @iwsdk/core three

# Development dependencies
npm install --save-dev \
  @iwsdk/vite-plugin-dev \
  vite-plugin-mkcert \
  vite \
  typescript \
  @types/three
```

Optional plugins you will often add next:

```bash
npm install --save-dev \
  @iwsdk/vite-plugin-uikitml \
  @iwsdk/vite-plugin-metaspatial
```

## Step 2: Add TypeScript Configuration

Create `tsconfig.json`:

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"]
  },
  "include": ["src/**/*"]
}
```

## Step 3: Configure Vite

Create `vite.config.ts`:

```typescript
import { iwsdkDev } from '@iwsdk/vite-plugin-dev';
import { defineConfig } from 'vite';
import mkcert from 'vite-plugin-mkcert';

export default defineConfig({
  plugins: [
    mkcert(),
    iwsdkDev({
      emulator: {
        device: 'metaQuest3',
      },
      verbose: true,
    }),
  ],
  server: {
    host: '0.0.0.0',
    port: 8081,
    strictPort: true,
  },
  build: {
    target: 'esnext',
    outDir: 'dist',
    sourcemap: true,
  },
  esbuild: {
    target: 'esnext',
  },
  publicDir: 'public',
  base: './',
});
```

If the project uses UIKitML, add the compiler plugin too:

```typescript
import { compileUIKit } from '@iwsdk/vite-plugin-uikitml';

export default defineConfig({
  plugins: [
    compileUIKit({ sourceDir: 'ui', outputDir: 'public/ui', verbose: true }),
  ],
});
```

Why HTTPS matters: Quest Browser requires a secure context for WebXR. Local TLS
is not optional if you want headset sessions to launch reliably.

## Step 4: Organize The Project

```text
my-quest-app/
  src/
    index.ts                  # Entry point: World.create, scene setup, XR controls
    systems/                  # Optional ECS systems
      movement.ts
  public/
    models/                   # GLTF / GLB models
    textures/                 # Texture files
    audio/                    # Audio files
    ui/                       # Compiled UIKitML JSON output
  ui/
    welcome.uikitml           # Source UIKitML files
  index.html                  # HTML entry point
  vite.config.ts
  tsconfig.json
  package.json
```

## Step 5: Create The Entry Point

### `index.html`

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>My Quest App</title>
  <style>
    html, body {
      margin: 0;
      height: 100%;
      overflow: hidden;
      background: #040b14;
      color: #f4f2ec;
      font-family: ui-sans-serif, system-ui, sans-serif;
    }

    #scene-container {
      width: 100%;
      height: 100%;
    }

    #enter-xr {
      position: fixed;
      left: 50%;
      bottom: 20px;
      transform: translateX(-50%);
      border: 0;
      border-radius: 999px;
      padding: 14px 24px;
      background: #d2ff72;
      color: #051015;
      font-size: 16px;
      font-weight: 700;
      cursor: pointer;
      z-index: 10;
    }

    #status {
      position: fixed;
      top: 20px;
      left: 20px;
      z-index: 10;
      max-width: 340px;
      padding: 12px 14px;
      border-radius: 18px;
      background: rgba(7, 18, 28, 0.86);
      border: 1px solid rgba(210, 255, 114, 0.18);
      backdrop-filter: blur(14px);
    }
  </style>
</head>
<body>
  <div id="scene-container"></div>
  <div id="status">
    <strong>My Quest App</strong><br />
    Open this URL in Quest Browser and tap Enter XR.
  </div>
  <button id="enter-xr">Enter XR</button>
  <script type="module" src="/src/index.ts"></script>
</body>
</html>
```

### `src/index.ts`

```typescript
import * as THREE from 'three';
import {
  ReferenceSpaceType,
  SessionMode,
  VisibilityState,
  World,
} from '@iwsdk/core';

async function main() {
  const container = document.getElementById('scene-container');
  const status = document.getElementById('status');
  const xrButton = document.getElementById('enter-xr');

  if (!(container instanceof HTMLDivElement)) {
    throw new Error('Missing #scene-container');
  }

  const world = await World.create(container, {
    xr: {
      sessionMode: SessionMode.ImmersiveVR,
      referenceSpace: ReferenceSpaceType.LocalFloor,
      offer: 'none',
      features: {
        handTracking: true,
        hitTest: true,
        layers: true,
      },
    },
  });

  world.camera.position.set(0, 1.6, 2.4);
  world.camera.lookAt(0, 1.35, -1.6);

  world.scene.add(new THREE.AmbientLight(0xffffff, 1.2));

  const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
  directionalLight.position.set(5, 10, 5);
  world.scene.add(directionalLight);

  const ground = new THREE.Mesh(
    new THREE.PlaneGeometry(10, 10),
    new THREE.MeshStandardMaterial({
      color: 0x1a2634,
      roughness: 0.92,
    }),
  );
  ground.rotation.x = -Math.PI / 2;
  world.createTransformEntity(ground, { persistent: true });

  const cube = new THREE.Mesh(
    new THREE.BoxGeometry(0.45, 0.45, 0.45),
    new THREE.MeshStandardMaterial({
      color: 0xd2ff72,
      metalness: 0.1,
      roughness: 0.35,
    }),
  );
  const cubeEntity = world.createTransformEntity(cube, { persistent: true });
  cubeEntity.object3D!.position.set(0, 1.35, -1.6);

  const updateUi = (state: VisibilityState) => {
    if (xrButton) {
      xrButton.textContent =
        state === VisibilityState.NonImmersive ? 'Enter XR' : 'Exit To Browser';
    }
    if (status) {
      status.innerHTML =
        state === VisibilityState.NonImmersive
          ? '<strong>My Quest App</strong><br />Open this URL in Quest Browser and tap Enter XR.'
          : '<strong>XR Session Active</strong><br />Use the headset to validate scene layout, input, and comfort.';
    }
  };

  world.visibilityState.subscribe(updateUi);
  updateUi(world.visibilityState.value);

  xrButton?.addEventListener('click', () => {
    if (world.visibilityState.value === VisibilityState.NonImmersive) {
      world.launchXR();
    } else {
      world.exitXR();
    }
  });
}

main().catch((error: unknown) => {
  console.error(error);
  const status = document.getElementById('status');
  if (status) {
    status.textContent = `Startup failed: ${String(error)}`;
  }
});
```

The important current API choices are:

- `World.create(...)` is the entry point
- `world.launchXR()` starts the session
- `world.exitXR()` ends it
- `world.visibilityState` is the right hook for UI state
- `world.createTransformEntity(...)` is the simplest way to attach scene meshes

## Step 6: Add Package Scripts

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  },
  "engines": {
    "node": ">=20.19.0"
  }
}
```

The default IWSDK Vite setup typically serves on `https://localhost:8081`.

## Step 7: Test It

### Desktop Iteration

`@iwsdk/vite-plugin-dev` provides the current built-in emulator/dev browser
path. Use it for fast iteration, but do not treat it as a substitute for Quest
hardware validation.

### Quest Browser Via Network

To test on an actual Quest device:

1. Ensure your Quest and development machine are on the same network.
2. Find your machine's local IP address, for example `192.168.1.100`.
3. Open Quest Browser on the headset.
4. Navigate to `https://192.168.1.100:8081`.
5. Trust or accept the local certificate flow.
6. Click `Enter XR` to start the immersive session.

### Testing Checklist

- [ ] `npm run build` succeeds
- [ ] the dev server is reachable from Quest Browser
- [ ] the XR button launches a session
- [ ] the headset shows the scene at the intended scale
- [ ] controllers and/or hands work as expected
- [ ] logs and screenshots stay clean during a basic interaction pass

## Practical Notes

- Keep HTTPS enabled. Quest Browser will not grant WebXR from an insecure local
  page.
- Prefer the public `@iwsdk/*` packages over any old `@meta-quest/iwsdk`
  references.
- Treat `npm create @iwsdk@latest` as the canonical starting point unless you
  have a strong reason to hand-roll the project.
