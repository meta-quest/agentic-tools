# Web / IWSDK / WebXR Project Setup for Meta Quest

This guide walks through creating a new web-based XR project using the Immersive Web SDK (IWSDK) and WebXR for Meta Quest, from setup through deployment.

## Requirements

- **Node.js**: 18 or newer (LTS recommended)
- **npm**: 9+ (bundled with Node.js)
- **A modern browser**: Quest Browser, Chrome, or Firefox for testing
- **HTTPS**: Required for WebXR API access (dev server handles this automatically)
- **hzdb CLI**: Optional, for deploying PWA-wrapped builds to device

## Step 1: Create the Project

### Option A: Using the IWSDK Create Tool (Recommended)

```bash
npm create @meta-quest/iwsdk my-quest-app
cd my-quest-app
npm install
```

This scaffolds a project with the recommended structure, Vite configuration, and IWSDK dependencies pre-configured.

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
npm install @meta-quest/iwsdk three

# Development dependencies
npm install --save-dev vite typescript @types/three
```

### TypeScript Configuration

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
    "forceConsistentCasingInFileNames": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "jsx": "preserve",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "outDir": "./dist",
    "rootDir": "./src",
    "declaration": true,
    "declarationMap": true,
    "sourceMap": true
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist"]
}
```

## Step 2: Vite Configuration

Create `vite.config.ts`:

```typescript
import { defineConfig } from 'vite';
import fs from 'fs';
import path from 'path';

export default defineConfig({
  server: {
    // HTTPS is required for WebXR API access
    https: {
      // Vite can generate self-signed certs automatically,
      // or provide your own:
      // key: fs.readFileSync('./certs/key.pem'),
      // cert: fs.readFileSync('./certs/cert.pem'),
    },
    // Allow access from Quest Browser on local network
    host: '0.0.0.0',
    port: 3000,
  },
  build: {
    target: 'esnext',
    outDir: 'dist',
    sourcemap: true,
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

For HTTPS without custom certificates, install the `@vitejs/plugin-basic-ssl` plugin:

```bash
npm install --save-dev @vitejs/plugin-basic-ssl
```

Then update `vite.config.ts`:

```typescript
import { defineConfig } from 'vite';
import basicSsl from '@vitejs/plugin-basic-ssl';

export default defineConfig({
  plugins: [basicSsl()],
  server: {
    host: '0.0.0.0',
    port: 3000,
  },
  build: {
    target: 'esnext',
    outDir: 'dist',
    sourcemap: true,
  },
});
```

## Step 3: Project Structure

```
my-quest-app/
  src/
    index.ts                  # Entry point: create World, start loop
    components/
      spin.ts                 # ECS component definitions
      health.ts
    systems/
      spin-system.ts          # ECS system logic
      input-system.ts
    entities/
      player.ts               # Entity factory functions
      environment.ts
    utils/
      math.ts                 # Utility functions
  public/
    models/
      scene.glb               # 3D models (GLTF/GLB format)
    textures/
      ground.jpg              # Texture files
    audio/
      ambient.mp3             # Audio files
    manifest.json             # Web App Manifest (for PWA)
    sw.js                     # Service worker (for PWA)
  index.html                  # HTML entry point
  vite.config.ts
  tsconfig.json
  package.json
```

## Step 4: Entry Point

### `index.html`

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>My Quest App</title>
  <style>
    body { margin: 0; overflow: hidden; }
    canvas { display: block; width: 100%; height: 100%; }
    #enter-vr {
      position: fixed;
      bottom: 20px;
      left: 50%;
      transform: translateX(-50%);
      padding: 12px 24px;
      font-size: 18px;
      cursor: pointer;
      z-index: 10;
    }
  </style>
</head>
<body>
  <button id="enter-vr">Enter VR</button>
  <script type="module" src="/src/index.ts"></script>
</body>
</html>
```

### `src/index.ts`

```typescript
import * as THREE from 'three';
import { World } from '@meta-quest/iwsdk';
import { SpinSystem } from './systems/spin-system';

async function main() {
  // Create the IWSDK World
  const world = new World({
    renderer: {
      antialias: true,
      alpha: true,
    },
    xr: {
      enabled: true,
      referenceSpaceType: 'local-floor',
    },
  });

  // Register ECS systems
  world.registerSystem(new SpinSystem());

  // Set up the scene
  setupScene(world.scene);

  // Set up the VR entry button
  const enterVRButton = document.getElementById('enter-vr');
  if (enterVRButton) {
    enterVRButton.addEventListener('click', async () => {
      await world.enterXR('immersive-vr', {
        requiredFeatures: ['local-floor'],
        optionalFeatures: ['hand-tracking', 'hit-test'],
      });
    });
  }

  // Start the render loop
  world.start();
}

function setupScene(scene: THREE.Scene) {
  // Add ambient light
  const ambientLight = new THREE.AmbientLight(0x404040, 2);
  scene.add(ambientLight);

  // Add directional light
  const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
  directionalLight.position.set(5, 10, 5);
  scene.add(directionalLight);

  // Add a ground plane
  const groundGeometry = new THREE.PlaneGeometry(10, 10);
  const groundMaterial = new THREE.MeshStandardMaterial({
    color: 0x808080,
    roughness: 0.8,
  });
  const ground = new THREE.Mesh(groundGeometry, groundMaterial);
  ground.rotation.x = -Math.PI / 2;
  scene.add(ground);

  // Add a sample cube
  const cubeGeometry = new THREE.BoxGeometry(0.5, 0.5, 0.5);
  const cubeMaterial = new THREE.MeshStandardMaterial({ color: 0x00aaff });
  const cube = new THREE.Mesh(cubeGeometry, cubeMaterial);
  cube.position.set(0, 1.0, -1.5);
  scene.add(cube);
}

main().catch(console.error);
```

### `src/systems/spin-system.ts`

```typescript
import { System, World } from '@meta-quest/iwsdk';

export class SpinSystem extends System {
  execute(delta: number, world: World) {
    // Query entities with a Spin component and rotate them
    const entities = world.query(['Spin', 'Transform']);
    for (const entity of entities) {
      const spin = entity.getComponent('Spin');
      const transform = entity.getComponent('Transform');
      transform.rotation.y += spin.speed * delta;
    }
  }
}
```

## Step 5: Development Server

Start the development server:

```bash
npm run dev
```

Add the dev script to `package.json` if not already present:

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

The server starts at `https://localhost:3000` with HTTPS enabled for WebXR.

## Step 6: Testing

### Browser Emulator (IWER)

For desktop testing without a headset, use the Immersive Web Emulator:

1. Install the [Immersive Web Emulator](https://chromewebstore.google.com/detail/immersive-web-emulator/) Chrome extension.
2. Open your app at `https://localhost:3000`.
3. Open Chrome DevTools and navigate to the **WebXR** tab.
4. Use the emulator controls to simulate headset movement, controller input, and hand tracking.

### Quest Browser via Network

To test on an actual Quest device:

1. Ensure your Quest and development machine are on the same Wi-Fi network.
2. Find your machine's local IP address (e.g., `192.168.1.100`).
3. Open **Quest Browser** on the headset.
4. Navigate to `https://192.168.1.100:3000`.
5. Accept the self-signed certificate warning.
6. Click "Enter VR" to start the immersive session.

### Testing Checklist

- [ ] Scene loads without errors in browser console
- [ ] "Enter VR" button initiates WebXR session
- [ ] Head tracking works correctly
- [ ] Controller input is detected (if applicable)
- [ ] Hand tracking works (if using `hand-tracking` feature)
- [ ] Performance is smooth at 72 Hz on Quest

## Step 7: PWA Setup for Store Distribution

To distribute your WebXR app through the Meta Quest Store, package it as a Progressive Web App (PWA).

### Web App Manifest

Create `public/manifest.json`:

```json
{
  "name": "My Quest App",
  "short_name": "QuestApp",
  "description": "An immersive WebXR experience for Meta Quest",
  "start_url": "/",
  "display": "standalone",
  "orientation": "landscape",
  "background_color": "#000000",
  "theme_color": "#000000",
  "icons": [
    {
      "src": "/icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ],
  "categories": ["games", "entertainment"],
  "xr": {
    "enabled": true,
    "sessionMode": "immersive-vr"
  }
}
```

Add the manifest link to `index.html`:

```html
<link rel="manifest" href="/manifest.json" />
```

### Service Worker

Create `public/sw.js`:

```javascript
const CACHE_NAME = 'quest-app-v1';
const ASSETS_TO_CACHE = [
  '/',
  '/index.html',
  '/src/index.ts',
  '/manifest.json',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(ASSETS_TO_CACHE))
  );
});

self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request).then((response) => {
      return response || fetch(event.request);
    })
  );
});
```

Register the service worker in `src/index.ts`:

```typescript
if ('serviceWorker' in navigator) {
  navigator.serviceWorker.register('/sw.js');
}
```

### APK Packaging with Bubblewrap

To create an APK from your PWA for Quest Store submission:

```bash
# Install Bubblewrap CLI
npm install -g @nickalcala/nickalcala

# Initialize Bubblewrap project
nickalcala init --manifest https://your-deployed-url.com/manifest.json

# Build the APK
nickalcala build

# Install on Quest for testing
hzdb app install ./app-release-signed.apk
```

## Step 8: Build and Deploy

### Build for Production

```bash
npm run build
```

This outputs optimized static files to the `dist/` directory.

### Deploy to Hosting

Deploy the `dist/` directory to any static hosting service:

**GitHub Pages:**

```bash
npm install --save-dev gh-pages

# Add to package.json scripts:
# "deploy": "gh-pages -d dist"

npm run deploy
```

**Vercel:**

```bash
npx vercel --prod
```

**Netlify:**

```bash
npx netlify deploy --prod --dir=dist
```

Ensure your hosting service supports HTTPS, as WebXR requires a secure context.

## Three.js Integration Patterns

### Loading GLTF Models

```typescript
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';

const loader = new GLTFLoader();

loader.load('/models/scene.glb', (gltf) => {
  const model = gltf.scene;
  model.position.set(0, 0, -2);
  model.scale.setScalar(0.5);
  scene.add(model);
});
```

### Controller Input

```typescript
function setupControllers(renderer: THREE.WebGLRenderer) {
  const controller1 = renderer.xr.getController(0);
  const controller2 = renderer.xr.getController(1);

  controller1.addEventListener('selectstart', () => {
    // Trigger pressed
  });

  controller1.addEventListener('selectend', () => {
    // Trigger released
  });

  scene.add(controller1);
  scene.add(controller2);

  // Add visible controller models
  const geometry = new THREE.CylinderGeometry(0.005, 0.005, 0.2);
  const material = new THREE.MeshStandardMaterial({ color: 0xffffff });
  const ray = new THREE.Mesh(geometry, material);
  ray.rotation.x = -Math.PI / 2;
  controller1.add(ray.clone());
  controller2.add(ray.clone());
}
```

### Hand Tracking

```typescript
const hand1 = renderer.xr.getHand(0);
const hand2 = renderer.xr.getHand(1);

// Add hand models using the XRHandModelFactory
import { XRHandModelFactory } from 'three/examples/jsm/webxr/XRHandModelFactory';

const handModelFactory = new XRHandModelFactory();
hand1.add(handModelFactory.createHandModel(hand1, 'mesh'));
hand2.add(handModelFactory.createHandModel(hand2, 'mesh'));

scene.add(hand1);
scene.add(hand2);
```

## Performance Considerations

- **Draw calls**: Minimize by merging geometries and using instanced meshes.
- **Texture size**: Keep textures small (512x512 or 1024x1024 max for most objects).
- **Model complexity**: Target under 50K-100K triangles total for smooth 72 Hz.
- **Shader complexity**: Use `MeshStandardMaterial` or `MeshBasicMaterial`. Avoid complex custom shaders.
- **Asset loading**: Load assets asynchronously and show a loading indicator.
- **Garbage collection**: Minimize allocations in the render loop to avoid GC pauses.

## Next Steps

- Explore the **IWSDK documentation** for the full ECS API, physics, and networking features.
- Add **spatial audio** using Three.js `PositionalAudio` with the Web Audio API.
- Implement **hit testing** for mixed reality placement using the WebXR Hit Test API.
- Set up **multiplayer** using WebSockets or WebRTC for real-time communication.
- Package as a **PWA** and submit to the Meta Quest Store for distribution.
